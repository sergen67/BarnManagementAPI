using BarnManagementAPI.Data;
using BarnManagementAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarnManagementAPI.Services
{
    public class ProductionService : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<ProductionService> _logger;

        public ProductionService(IServiceProvider sp, ILogger<ProductionService> logger)
        {
            _sp = sp;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProductionService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _sp.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<BarnDbContext>();
                        var now = DateTime.UtcNow;

                        var toDie = await db.Animals
                            .Where(a => a.IsAlive && now >= a.BornAt.AddDays(a.LifeSpanDays))
                            .ToListAsync(stoppingToken);

                        foreach (var a in toDie)
                            a.IsAlive = false;


                        var toProduce = await db.Animals
                            .Where(a => a.IsAlive && now >= a.NextProductionAt)
                            .ToListAsync(stoppingToken);

                        foreach (var a in toProduce)
                        {
                            var inferred = InferProduct(a);

                            db.Products.Add(new Product
                            {
                                AnimaId = a.Id,
                                ProductType = inferred,
                                Quantity = 1,
                                ProductAt = now,
                                Issold = false
                            });
                            a.NextProductionAt = now.AddDays(a.ProductionIntervalDays);
                        }

                        if (toDie.Count > 0 || toProduce.Count > 0)
                        {
                            await db.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation("Tick → dead:{dead}, produced:{prod}", toDie.Count, toProduce.Count);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ProductionService tick failed");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // shutdown
                }
            }

            _logger.LogInformation("ProductionService stopped");
        }

       
        private static string InferProduct(Animal a)
        {
            var s = ((a.Type ?? a.Type) ?? string.Empty).Trim().ToLowerInvariant();

            if (s.Contains("tavuk") || s.Contains("chicken") || s.Contains("hen"))
                return "egg";

            if (s.Contains("inek") || s.Contains("cow"))
                return "milk";

            if (s.Contains("koyun") || s.Contains("sheep"))
                return "wool";

            return "meat";
        }
    }
}

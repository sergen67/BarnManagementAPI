using BarnManagementAPI.Data;
using BarnManagementAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<BarnDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("BarnDb")));


var jwtSection = builder.Configuration.GetSection("Jwt");


var jwtKey = jwtSection["Key"]
             ?? throw new InvalidOperationException("Jwt:Key missing");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key too short (>= 32 bytes).");


var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
         
            ValidateIssuer = false,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = false,
            ValidAudience = jwtSection["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };


        o.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                var msg = (ctx.Exception?.Message ?? "invalid_token")
                    .Replace("\r", " ").Replace("\n", " ").Replace("\"", "'");
                ctx.Response.Headers["WWW-Authenticate"] =
                    $"Bearer error=\"invalid_token\", error_description=\"{msg}\"";
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();


builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.AddScoped<JwtServices>();


builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});


builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Barn API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});
builder.Services.AddHostedService<ProductionService>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

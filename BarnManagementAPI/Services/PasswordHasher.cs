using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Barn.Api.Services;
public static class PasswordHasher
{
    public static byte[] NewSalt(int size = 16)
    {
        var salt = new byte[size];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    public static byte[] Hash(string password, byte[] salt, int iter = 150_000)
    {
        return KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, iter, 32);
    }
}

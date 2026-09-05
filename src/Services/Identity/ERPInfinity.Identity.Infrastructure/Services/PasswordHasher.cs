using System.Security.Cryptography;
using ERPInfinity.Identity.Application.Abstractions;

namespace ERPInfinity.Identity.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32;  // 256 bits
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        byte[] result = new byte[SaltSize + KeySize];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, result, SaltSize, KeySize);

        return Convert.ToBase64String(result);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            byte[] decoded = Convert.FromBase64String(hashedPassword);
            if (decoded.Length != SaltSize + KeySize) return false;

            byte[] salt = new byte[SaltSize];
            byte[] hash = new byte[KeySize];

            Buffer.BlockCopy(decoded, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(decoded, SaltSize, hash, 0, KeySize);

            byte[] testHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
        catch
        {
            return false;
        }
    }
}

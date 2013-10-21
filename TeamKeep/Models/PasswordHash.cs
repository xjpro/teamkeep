using System;
using System.Security.Cryptography;

namespace Teamkeep.Models
{
    // http://csharptest.net/470/another-example-of-how-to-store-a-salted-password-hash/
    public class PasswordHash
    {
        private const int SaltSize = 16;
        private const int HashSize = 20;
        private const int HashIterations = 10000;

        private readonly byte[] _salt, _hash;

        public byte[] Salt { get { return (byte[])_salt.Clone(); } }
        public byte[] Hash { get { return (byte[])_hash.Clone(); } }

        public PasswordHash(string password)
        {
            new RNGCryptoServiceProvider().GetBytes(_salt = new byte[SaltSize]);
            _hash = new Rfc2898DeriveBytes(password, _salt, HashIterations).GetBytes(HashSize);
        }

        public PasswordHash(byte[] hashBytes)
        {
            Array.Copy(hashBytes, 0, _salt = new byte[SaltSize], 0, SaltSize);
            Array.Copy(hashBytes, SaltSize, _hash = new byte[HashSize], 0, HashSize);
        }

        public byte[] ToArray()
        {
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(_salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(_hash, 0, hashBytes, SaltSize, HashSize);
            return hashBytes;
        }

        public bool Verify(string password)
        {
            byte[] test = new Rfc2898DeriveBytes(password, _salt, HashIterations).GetBytes(HashSize);
            for (int i = 0; i < HashSize; i++)
            {
                if (test[i] != _hash[i]) return false;
            }
            return true;
        }
    }
}
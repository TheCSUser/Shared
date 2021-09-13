//BASED ON: https://docs.microsoft.com/en-GB/dotnet/api/system.security.cryptography.hashalgorithm.computehash?view=netframework-3.5

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace com.github.TheCSUser.Shared.Common
{
    public static class Hash
    {
        public static string GetHash(HashAlgorithm hashAlgorithm, string fileName)
        {
            if (!File.Exists(fileName)) return null;

            byte[] data;
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                data = hashAlgorithm.ComputeHash(stream);
            }

            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static bool VerifyHash(HashAlgorithm hashAlgorithm, string fileName, string hash)
        {
            if (!File.Exists(fileName)) return false;

            var hashOfInput = GetHash(hashAlgorithm, fileName);

            var comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInput, hash) == 0;
        }
    }
}

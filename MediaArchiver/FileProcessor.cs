using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Numerics;

namespace MediaArchiver
{
    public class FileProcessor
    {
        public FileProcessor()
        {
        }

        public static IEnumerable<string> EnumerateSpecificFiles(string directory, List<string> extensions, SearchOption searchOption)
        {
            var validExtensions = extensions.Select(e => e.ToLower());
            var nonDotFiles = Directory.EnumerateFiles(directory, "*.*", searchOption).Where(fn => !Path.GetFileName(fn).StartsWith(".", StringComparison.InvariantCultureIgnoreCase));
            foreach (var file in nonDotFiles)
            {
                var fileExtension = Path.GetExtension(file).ToLower();
                if (extensions.Contains(fileExtension)) { yield return file; }
            }
        }

        public static string CalculateHash(string fileName)
        {
            byte[] hashValue;
            using (var hasher = SHA256.Create())
            using (var fileStrm = File.OpenRead(fileName))
            {
                fileStrm.Position = 0;
                hashValue = hasher.ComputeHash(fileStrm);
            }
            //Console.WriteLine(new BigInteger(hashValue));
            return Convert.ToBase64String(hashValue);
        }
        public static string MakeFileNameSafeHash(string fileHash)
        {
            var hash = fileHash;
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                hash = hash.Replace(invalidChar, '_');
            }
            return hash;
        }

    }
}

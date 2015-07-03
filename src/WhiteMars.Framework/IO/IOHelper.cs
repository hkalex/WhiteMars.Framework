using System;
using System.IO;
using System.Threading.Tasks;

namespace WhiteMars.Framework
{
    public static class IOHelper
    {
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public static string ReadAllText(string filepath)
        {
            return File.ReadAllText(filepath);
        }

        public static string CombinePath(string[] paths)
        {
            return Path.Combine(paths);
        }
    }
}


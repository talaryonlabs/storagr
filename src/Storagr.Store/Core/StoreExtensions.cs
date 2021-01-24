using System.IO;

namespace Storagr.Store
{
    public static class StoreExtensions
    {
        public static DirectoryInfo CombineWith(this DirectoryInfo a, DirectoryInfo directory)
            => CombineWith(a, directory.FullName);
        
        public static DirectoryInfo CombineWith(this DirectoryInfo a, string path) =>
            new(Path.Combine(a.FullName, path));
    }
}
using System.Collections.Concurrent;

namespace AlumniStaticWeb.IO
{
    public class Cache
    {
        public static ConcurrentDictionary<string,WebFile> CachedFiles = new ConcurrentDictionary<string, WebFile>();

        public static WebFile GetFile(string name)
        {
            if (!CachedFiles.TryGetValue(name, out var data))
            {
                var file = new WebFile(name);
                CachedFiles.TryAdd(name, file);
                data = file;
            }
            return data;
        }
    }
}
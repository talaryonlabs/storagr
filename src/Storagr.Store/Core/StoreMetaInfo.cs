using System.IO;
using Newtonsoft.Json;

namespace Storagr.Store
{
    [JsonObject]
    public class StoreMetaInfo
    {
        [JsonProperty("name")] public string Name { get; set; }

        public static StoreMetaInfo FromFile(string path)
        {
            var file = new FileInfo(path);

            if (!file.Exists)
                throw new FileNotFoundException();

            using var reader = file.OpenText();
            return (StoreMetaInfo)new JsonSerializer().Deserialize(reader, typeof(StoreMetaInfo));
        } 
        
        public static void ToFile(string path, StoreMetaInfo metaInfo)
        {
            var file = new FileInfo(path);

            if (!file.Exists)
                throw new FileNotFoundException();

            using var writer = file.CreateText();
            new JsonSerializer().Serialize(writer, metaInfo);
        } 
    }
}
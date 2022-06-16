using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace IRO.FilesBackuper.MainLogic.Models
{
    [JsonConverter(typeof(ToStringJsonConverter))]
    public struct FileSizeInfo 
    {
        public string Path { get; set; }

        public long BytesSize { get; set; }

        public override string ToString()
        {
            return $"{BytesSize / 1024} KB | {Path}";
        }
    }

    public class ToStringJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value?.ToString());
            t.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}

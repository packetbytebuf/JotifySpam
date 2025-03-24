using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam.Messages
{
    public class ResponseObject
    {
        public T? ParseMessage<T>()
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(message_raw)?.ToString() ?? "{}");
        }
        [JsonProperty("message")]
        private object? message_raw;
        public string? type;
        public long timestamp;
    }
}

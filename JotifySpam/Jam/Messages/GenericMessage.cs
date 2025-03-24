using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam.Messages
{
    public abstract class GenericMessage
    {
        [JsonIgnore]
        public abstract string Type { get; }
    }
}

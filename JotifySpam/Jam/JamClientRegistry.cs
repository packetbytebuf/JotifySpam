using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam
{
    public class JamClientRegistry
    {
        public static List<JamClient> Clients { get; internal set; } = new List<JamClient>();
    }
}

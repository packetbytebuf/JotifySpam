using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam
{
    public class ClientRegistry
    {
        public static List<JamClient> JamClients { get; internal set; } = new List<JamClient>();
        public static List<DesktopClient> DesktopClients { get; internal set; } = new List<DesktopClient>();
    }
}

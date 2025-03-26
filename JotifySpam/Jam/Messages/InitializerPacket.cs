using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam.Messages
{
    public class InitializerPacket : GenericMessage
    {
        public InitializerPacket(bool desktop)
        {
            this.desktop = desktop;
        }
        public bool desktop;
        public override string Type => "INIT";
    }
}

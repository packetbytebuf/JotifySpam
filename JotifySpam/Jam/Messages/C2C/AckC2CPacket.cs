using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam.Messages.C2C
{
    public class AckC2CPacket : GenericMessage
    {
        public override string Type => "ACK";
    }
}

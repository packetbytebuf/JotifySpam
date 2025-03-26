using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam.Messages
{
    public class SetTimePosition : GenericMessage
    {
        public SetTimePosition(int position, long? synctime=null)
        {
            this.position = position;
            this.synctime = synctime ?? JamClient.UTCNow() + 1000;
        }

        public int position;
        public long synctime;
        public override string Type => "SETTIME";
    }
}

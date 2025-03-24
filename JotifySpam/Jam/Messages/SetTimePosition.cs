using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam.Messages
{
    public class SetTimePosition : GenericMessage
    {
        public SetTimePosition(int position)
        {
            this.position = position;
            this.synctime = JamClient.UTCNow() + 1000;
        }

        public int position;
        public long synctime;
        public override string Type => "SETTIME";
    }
}

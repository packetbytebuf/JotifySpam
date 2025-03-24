using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam.Messages
{
    //public struct Message
    //{
    //    public enum Type
    //    {
    //        Unknown,
    //        Ack,
    //        QueueSong,
    //        SetTimePosition,
    //        Play,
    //        Pause,
    //        Loop1,
    //        LoopAlbum,
    //        LoopOff,
    //        PlaySong,
    //        Clear,
    //        ApplyQueue
    //    }
    //    public string timestamp;
    //}

    
    public class Ack : GenericMessage
    {
        public string message = "Server Response";
        public override string Type => "ACK";
    }
}

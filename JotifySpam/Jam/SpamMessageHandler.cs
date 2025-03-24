using JotifySpam.Jam.Messages;
using JotifySpam.Jam.Messages.C2C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam
{
    internal class SpamMessageHandler
    {
        public SpamMessageHandler() { }

        private static Dictionary<string, Action<ResponseObject>> Handlers = new Dictionary<string, Action<ResponseObject>>() {
            { "ACK", (response) => Ack(response) }
        };
        public static bool HandleMessage(ResponseObject? response)
        {
            if (response == null)
            {
                JamServer.Logger.Error("ResponseObject passed into HandleMessage was null.");
                return false;
            }

            if (Handlers.TryGetValue(response.type ?? "", out Action<ResponseObject>? impl))
            {
                impl.Invoke(response);
                return true;
            }
            else
            {
                JamServer.Logger.Error("Couldn't find a handler in JamMessageHandler.Handlers that handles message type", response.type);
            }


            return false;
        }

        public static void Ack(ResponseObject response)
        {
            AckC2CPacket? message = response.ParseMessage<AckC2CPacket>();
            Program.logger.Info("IM PICKLE RIIIIIICK!!!!");
        }
    }
}

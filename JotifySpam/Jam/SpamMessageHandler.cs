using JotifySpam.Jam.Messages;
using JotifySpam.Jam.Messages.C2C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam
{
    // TODO: also replace with generic MessageHandler. sooo much repetition
    public class SpamMessageHandler
    {
        private DesktopClient client;
        public SpamMessageHandler(DesktopClient client) { this.client = client; }

        private static Dictionary<string, Action<SpamMessageHandler, ResponseObject>> Handlers = new Dictionary<string, Action<SpamMessageHandler, ResponseObject>>() {
            { "ACK", (handler, message) => handler.Ack(message) },
            { "SETTIME", (handler, message) => handler.SetTimePosition(message) }
        };
        public bool HandleMessage(ResponseObject? response)
        {
            if (response == null)
            {
                JamServer.Logger.Error("ResponseObject passed into HandleMessage was null.");
                return false;
            }

            if (Handlers.TryGetValue(response.type ?? "", out Action<SpamMessageHandler, ResponseObject>? impl))
            {
                impl.Invoke(this, response);
                return true;
            }
            else
            {
                JamServer.Logger.Error("Couldn't find a handler in JamMessageHandler.Handlers that handles message type", response.type);
            }


            return false;
        }

        public void Ack(ResponseObject response)
        {
            AckC2CPacket? message = response.ParseMessage<AckC2CPacket>();
            Program.logger.Info("IM PICKLE RIIIIIICK!!!!");
        }

        public void SetTimePosition(ResponseObject response)
        {
            //SetTimePositionC2CPacket? message = response.ParseMessage<SetTimePositionC2CPacket>();
            //foreach(DesktopClient client in ClientRegistry.DesktopClients)
            //{
            //    client.SendMessage(new SetTimePositionC2CPacket());
            //}

            SetTimePosition? message = response.ParseMessage<SetTimePosition>();

            if (message == null)
            {
                client.Logger.Error("Failed to parse SetTimePosition body.");
                return;
            }

            foreach (JamClient client in ClientRegistry.JamClients)
            {
                client.SendMessage(new SetTimePosition(message.position, message.synctime));
            }
        }
    }
}

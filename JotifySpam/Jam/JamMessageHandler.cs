using JotifySpam.Jam.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam
{
    public class JamMessageHandler
    {
        private JamClient client;
        public JamMessageHandler(JamClient client) { this.client = client; }

        private static Dictionary<string, Action<JamMessageHandler, ResponseObject>> Handlers = new Dictionary<string, Action<JamMessageHandler, ResponseObject>>() { 
            { "ACK", (handler, response) => handler.Ack(response) } 
        };
        public void HandleMessage(ResponseObject? response)
        {
            if(response == null) {
                client.Logger.Error("ResponseObject passed into HandleMessage was null.");
                return;
            }

            if (Handlers.TryGetValue(response.type ?? "", out Action<JamMessageHandler, ResponseObject>? impl))
            {
                impl.Invoke(this, response);
            }
            else
            {
                client.Logger.Error("Couldn't find a handler in JamMessageHandler.Handlers that handles message type", response.type);
            }
        }

        public void Ack(ResponseObject response)
        {
            Ack? message = response.ParseMessage<Ack>();
            client.Logger.Info("Recieved ack. Message:", message?.message, "| Ping (ms):", JamClient.UTCNow() - response.timestamp);
            client.Ack();
        }

        public void SetTimePosition(ResponseObject response)
        {
            SetTimePosition? message = response.ParseMessage<SetTimePosition>();

            if(message == null)
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

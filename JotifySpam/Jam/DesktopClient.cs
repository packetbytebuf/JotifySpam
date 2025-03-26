using JotifySpam.Jam.Messages;
using Newtonsoft.Json;
using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace JotifySpam.Jam
{
    // TODO: JamClient and DesktopClient can be thrown under a generic class. 
    public class DesktopClient
    {
        internal Logger Logger;
        public bool IsValid { get; private set; } = false;
        public WebSocket WebSocket { get; private set; }
        internal Thread HandlerThread;
        private CancellationTokenSource cts = new CancellationTokenSource();
        public readonly SpamMessageHandler MessageHandler;

        public DesktopClient(WebSocket webSocket)
        {
            Logger = new Logger($"DesktopClient {ConsoleExtensions.Pastel(Guid.NewGuid().ToString(), Color.FloralWhite)}", Color.Cornsilk);
            WebSocket = webSocket;
            IsValid = webSocket.State == WebSocketState.Open;

            Logger.Info("Initialized");
            if (IsValid)
                Logger.Info("Client is active.");

            HandlerThread = new Thread(() => { Task.Run(() => ClientHandler()); });
            HandlerThread.Start();

            ClientRegistry.DesktopClients.Add(this);
            MessageHandler = new SpamMessageHandler(this);
        }
        internal byte[] messageBuffer = new byte[2048];
        public async Task ClientHandler()
        {
            try
            {
                while (WebSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(messageBuffer), cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Disconnect();
                        break;
                    }

                    string receivedMessage = Encoding.UTF8.GetString(messageBuffer, 0, result.Count);
                    ResponseObject? response = JsonConvert.DeserializeObject<ResponseObject>(receivedMessage);
                    MessageHandler.HandleMessage(response);

                    //byte[] responseBuffer = Encoding.UTF8.GetBytes(JamClient.PackageMessage(new Ack()));
                    //await WebSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, cts.Token);
                }
            }
            catch (WebSocketException ex)
            {
                Logger.Error($"WebSocket error: {ex.InnerException.Message} {WebSocket.State.ToString()}");
                Disconnect();
            }
            catch (Exception ex)
            {
                Logger.Error($"ClientHandler error: {ex.Message}");
            }
        }
        public void SendMessage(GenericMessage message)
        {
            byte[] responseBuffer = Encoding.UTF8.GetBytes(JamClient.PackageMessage(message));
            WebSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, cts.Token);
        }

        public void Disconnect(string reason = "Closing (No reason provided)")
        {
            WebSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);

            cts?.Cancel();
            cts?.Dispose();

            WebSocket?.Dispose();

            ClientRegistry.DesktopClients.Remove(this);

            Logger.Info("Client disconnected.");
        }
    }
}

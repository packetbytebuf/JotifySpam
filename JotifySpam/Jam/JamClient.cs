using JotifySpam.Jam.Messages;
using Newtonsoft.Json;
using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JotifySpam.Jam
{
    /*
     * NOTE: connection loop (acking in its entirety) was just for debugging. im leaving the code cause it was a pain in the ASS to setup (cause it came with the implementation of the message system) but now that i have the framework its really not needed. was just debug. MIGHT come into play but honestly it doesnt matter that much.
     */
    public class JamClient
    {
        internal Logger Logger;
        public bool IsValid { get; private set; } = false;
        public WebSocket WebSocket { get; private set; }

        //internal Thread ConnectionLoop;
        internal Thread HandlerThread;
        private CancellationTokenSource cts = new CancellationTokenSource();
        public readonly JamMessageHandler MessageHandler;

        public JamClient(WebSocket webSocket)
        {
            Logger = new Logger($"JamClient {ConsoleExtensions.Pastel(Guid.NewGuid().ToString(), Color.FloralWhite)}", Color.CornflowerBlue);
            WebSocket = webSocket;
            IsValid = webSocket.State == WebSocketState.Open;

            Logger.Info("Initialized");
            if (IsValid)
                Logger.Info("Client is active.");

            //ConnectionLoop = new Thread(() => { Task.Run(() => ConnectionCheck(cts.Token)); });
            //ConnectionLoop.Start();
            HandlerThread = new Thread(() => { Task.Run(() => ClientHandler()); });
            HandlerThread.Start();

            MessageHandler = new JamMessageHandler(this);
        }

        internal bool acked = false;
        internal int missed_acks = 0;

        //public async Task ConnectionCheck(CancellationToken token)
        //{
        //    try
        //    {
        //        await Task.Delay(1000, token); // 1 second buffer to wait for client to start sending acks
        //        while (!token.IsCancellationRequested)
        //        {
        //            if (!IsValid)
        //                continue;

        //            await Task.Delay(5000, token);

        //            if (acked)
        //            {
        //                acked = false;
        //                continue;
        //            }

        //            if (missed_acks >= 5)
        //            {
        //                Logger.Error("Client missed 5 acks. Disconnecting.");
        //                Disconnect();
        //                break;
        //            }

        //            missed_acks++;
        //            Logger.Info("missed an ack!");
        //        }
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        Logger.Info("ConnectionCheck task was cancelled.");
        //    }
        //}

        internal byte[] messageBuffer = new byte[2048];

        public static long UTCNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static string PackageMessage(GenericMessage message)
        {
            return JsonConvert.SerializeObject(new Dictionary<string, object> { { "message", message }, { "type", message.Type }, { "timestamp", UTCNow().ToString() } });
        }

        public void SendMessage(GenericMessage message)
        {
            byte[] responseBuffer = Encoding.UTF8.GetBytes(PackageMessage(message));
            WebSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task ClientHandler()
        {
            try
            {
                while (WebSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(messageBuffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Disconnect();
                        break;
                    }

                    string receivedMessage = Encoding.UTF8.GetString(messageBuffer, 0, result.Count);
                    //Logger.Info($"Received: {receivedMessage}");
                    ResponseObject? response = JsonConvert.DeserializeObject<ResponseObject>(receivedMessage);
                    //Logger.Info("Recieved a message of type", response?.type);
                    MessageHandler.HandleMessage(response);
                    //switch (response?.type)
                    //{
                    //    case "ACK":
                    //        Logger.Info("Ack message:", response.ParseMessage<Ack>()?.message);
                    //        Ack();
                    //        break;
                    //}

                    byte[] responseBuffer = Encoding.UTF8.GetBytes(PackageMessage(new Ack()));
                    await WebSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (WebSocketException ex)
            {
                Logger.Error($"WebSocket error: {ex.Message}");
                Disconnect();
            }
            catch (Exception ex)
            {
                Logger.Error($"ClientHandler error: {ex.Message}");
            }
        }

        public void Ack()
        {
            acked = true;
            IsValid = true;
            missed_acks = 0;
        }

        public void Disconnect(string reason="Closing (No reason provided)")
        {
            WebSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);

            JamClientRegistry.Clients.Remove(this);

            cts?.Cancel();
            cts?.Dispose();

            WebSocket?.Dispose();

            Logger.Info("Client disconnected.");
        }
    }
}

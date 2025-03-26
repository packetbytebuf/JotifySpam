using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace JotifySpam.Jam
{
    using JotifySpam.Jam.Messages;
    using Newtonsoft.Json;
    using System;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Net;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Results;

    class JamServer
    {
        public static Logger Logger = new Logger("JamServer", Color.FromArgb(197, 65, 255));
        public static long UTCNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        private static void HandleHttp(HttpListenerContext context)
        {
            Logger.Warn("HTTP request received. BAD!!! dont do that!!!");

            //// Get the content length from the request header (default to 0 if not present)
            //long contentLength = context.Request.ContentLength64;

            //// Check if the content length is larger than 0
            //if (contentLength > 0)
            //{
            //    byte[] buffer = new byte[contentLength];

            //    try
            //    {
            //        // Read the full content from the InputStream
            //        int bytesRead = 0;
            //        while (bytesRead < contentLength)
            //        {
            //            int read = context.Request.InputStream.Read(buffer, bytesRead, (int)(contentLength - bytesRead));
            //            if (read == 0) break; // End of stream
            //            bytesRead += read;
            //        }

            //        // Convert the buffer into a string
            //        string body = Encoding.UTF8.GetString(buffer);

            //        // Deserialize the body into a ResponseObject
            //        ResponseObject? response = JsonConvert.DeserializeObject<ResponseObject>(body);

            //        // Handle the message
            //        Message.HandleMessage(response);
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Error($"Error processing request: {ex.Message}");
            //        context.Response.StatusCode = 500; // Internal Server Error
            //    }
            //}
            //else
            //{
            //    Logger.Warn("Received request with no content.");
            //}

            // Send a successful response
            context.Response.StatusCode = 400;
            context.Response.Close();
        }

        public static async Task StartAsync(string uri)
        {
            HttpListener listener = new HttpListener();
            //listener.Prefixes.Add("http://127.0.0.1:5000");
            //listener.Prefixes.Add("http://127.0.0.1:5000");
            listener.Prefixes.Add(uri);
            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error starting server: {ex.Message}");
            }
            Logger.Info($"WebSocket server started at {uri}");

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest) // JamClients using the Spotify application use WebSockets
                {
                    ProcessWebSocketRequest(context);
                    Logger.Info("post PWSR call");
                }
                else // JotifySpam clients use regular HTTP communication
                {
                    HandleHttp(context);
                }
            }
        }
        public static void Start(string uri)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(uri);
            listener.Start();
            Logger.Info($"WebSocket server started at {uri}");

            while (true)
            {
                var task = listener.GetContextAsync();
                task.Wait();
                HttpListenerContext context = task.Result;
                if (context.Request.IsWebSocketRequest)
                {
                    ProcessWebSocketRequest(context);
                }
                else
                {
                    HandleHttp(context);
                }
            }
        }

        private static async Task ProcessWebSocketRequest(HttpListenerContext context)
        {
            WebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = wsContext.WebSocket;
            Logger.Info("New Client Connected");

            try
            {
                byte[] buffer = new byte[512];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ResponseObject? response = JsonConvert.DeserializeObject<ResponseObject>(receivedMessage);
                InitializerPacket? initpacket = response?.ParseMessage<InitializerPacket>();
                if (initpacket == null)
                {
                    Logger.Error("Expected InitializerPacket. Either didn't recieve one, or it was invalid and could not be parsed.");
                    return;
                }

                if (initpacket.desktop)
                    ClientRegistry.DesktopClients.Add(new DesktopClient(webSocket));
                else
                    ClientRegistry.JamClients.Add(new JamClient(webSocket));
            }
            catch (Exception ex)
            {
                Program.logger.Error($"WebSocket error: {ex.Message}");
            }
        }
    }
}

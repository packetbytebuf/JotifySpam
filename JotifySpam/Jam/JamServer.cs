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
    using System.Drawing;
    using System.Net;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class JamServer
    {
        public static Logger Logger = new Logger("JamServer", Color.FromArgb(197, 65, 255));
        public static long UTCNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        public static async Task StartAsync(string uri)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(uri);
            listener.Start();
            Logger.Info($"WebSocket server started at {uri}");

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest) // JamClients using the Spotify application use WebSockets
                {
                    ProcessWebSocketRequest(context);
                }
                else // JotifySpam clients use regular HTTP communication
                {
                    byte[] buffer = new byte[1024];
                    if(context.Request.InputStream.Length > buffer.Length)
                    {
                        Program.logger.Warn("Another client attempted to send data over 1024 bytes. Skipping.");
                        continue;
                    }

                    context.Request.InputStream.Read(buffer, 0, buffer.Length);
                    string body = Encoding.UTF8.GetString(buffer);
                    ResponseObject? response = JsonConvert.DeserializeObject<ResponseObject>(body);

                    SpamMessageHandler.

                    context.Response.StatusCode = 200;
                    context.Response.Close();
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
                    context.Response.StatusCode = 400;
                    context.Response.Close();
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
                JamClientRegistry.Clients.Add(new JamClient(webSocket));
            }
            catch (Exception ex)
            {
                Program.logger.Error($"WebSocket error: {ex.Message}");
            }
        }
    }
}

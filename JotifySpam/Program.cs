using JotifySpam.Jam;
using JotifySpam.Jam.Messages;
using JotifySpam.Jam.Messages.C2C;
using Newtonsoft.Json;
using System.Drawing;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;

namespace JotifySpam
{
    internal class Program
    {
        public static Logger logger = new Logger("JotifySpam", Color.FromArgb(30, 215, 96)); // spotify green
        static void Main(string[] args)
        {
            AsyncMain(args);
            while (true) { Console.ReadKey(true); }
        }
        static async Task AsyncMain(string[] args)
        {
            logger.Info("Starting Websocket Server");
            _ = JamServer.StartAsync("http://*:5000/");

            //while (true)
            //{
            //    string? input = Console.ReadLine();

            //    if (string.IsNullOrWhiteSpace(input))
            //        continue;

            //    string[] arguments = input.Split(' ');
            //    string command = arguments[0];
            //    arguments = arguments.Where((item, index) => index != 0).ToArray();

            //    switch (command)
            //    {
            //        case "settime":
            //            JamClientRegistry.Clients[0].SendMessage(new SetTimePosition(int.Parse(arguments[0])));
            //            break;
            //    }
            //}

            logger.Newline();
            logger.LogNewline(Logger.LogStyle.Info, false, "Provide IP Address of server (127.0.0.1 if self): ");
            string? ip = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ip))
            {
                logger.Error("Didn't recieve an input.");
                return;
            }

            logger.Info("Spawning connection with", ip);
            ClientWebSocket socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri($"ws://{ip}:5000"), CancellationToken.None);
            logger.Info("Connected");
            //DesktopClient sync = new DesktopClient(socket); // handle messages to us < this method was STUPID and DUMB because the server connection reciever thingy handler already creates a new instance. this made 2.

            // ^ (cont.) this led to me having to fucking reuse code which i SHOULDNT HAVE TO DO EVER BUT IM LAZY OKAY and just manually send the message thru the socket
            byte[] responseBuffer = Encoding.UTF8.GetBytes(JamClient.PackageMessage(new InitializerPacket(true)));
            _ = socket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);

            while (true)
            {
                string? input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string[] arguments = input.Split(' ');
                string command = arguments[0];
                arguments = arguments.Where((item, index) => index != 0).ToArray();

                switch (command)
                {
                    case "settime":
                        foreach (DesktopClient client in ClientRegistry.DesktopClients)
                        {
                            client.SendMessage(new SetTimePosition(int.Parse(arguments[0]), JamClient.UTCNow() + long.Parse(arguments[0])));
                        }
                        break;
                    case "ping":
                        logger.Info("pong");
                        break;
                }
            }
        }
    }

}

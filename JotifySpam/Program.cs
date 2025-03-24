using JotifySpam.Jam;
using JotifySpam.Jam.Messages;
using System.Drawing;

namespace JotifySpam
{
    internal class Program
    {
        public static Logger logger = new Logger("JotifySpam", Color.FromArgb(30, 215, 96)); // spotify green
        static void Main(string[] args)
        {
            logger.LogNewline(Logger.LogStyle.Info, false, "Connect to existing server? Choice: ");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                logger.Newline();
                logger.LogNewline(Logger.LogStyle.Info, false, "Provide IP Address of server: ");
                string? ip = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(ip))
                {
                    logger.Error("Didn't recieve an input.");
                    return;
                }

                logger.Info(ip);
            }
            else
            {
                logger.Newline();

                logger.Info("Starting Websocket Server");
                JamServer.StartAsync("http://127.0.0.1:5000/");

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
                            JamClientRegistry.Clients[0].SendMessage(new SetTimePosition(int.Parse(arguments[0])));
                            break;
                    }
                }
            }
        }
    }
}

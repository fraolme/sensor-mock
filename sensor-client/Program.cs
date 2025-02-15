using System.Net.Sockets;
using System.Text;

namespace Program {
    class Program {
        public static async Task Main(string[] args) {
            var client = new SensorClient();
            if (args.Length == 0) {
                Console.WriteLine(
                    @"USAGE: sensor-client command
Available Commands: 
    GET_STATUS
    GET_TEMP
    GET_PRESSURE
    GET_RPM");
                return;
            }

            await client.SendCommand(args[0]);
        }
    }

    class SensorClient {

        private readonly Logger logger;
        public SensorClient() {
            logger = new Logger();
        }

        public async Task SendCommand(string command) {
            var host = "127.0.0.1";
            var port = 5611;
            logger.Log("Conecting to server...", LogLevel.Info);
            try {
                
                using var server = new TcpClient(host, port);
                using NetworkStream stream = server.GetStream();
                logger.Log($"Sending Command: {command}", LogLevel.Info);
                var bytes = Encoding.ASCII.GetBytes(command);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var response = Encoding.ASCII.GetString(buffer.Take(bytesRead).ToArray());    

                Console.WriteLine($"RESPONSE FROM SERVER: {response}");
            } catch(SocketException) {
                logger.Log($"Unable to Connect to server: Make sure the server is running at {host}:{port}", LogLevel.Error);
            } catch(Exception e) {
                logger.Log($"UNKNOWN ERROR: {e}", LogLevel.Error);
            }
        }
    }

    class Logger {
        public Logger(){
        }

        public void Log(string message, LogLevel level) {    
            switch(level) {
                case LogLevel.Info:
                    Console.WriteLine($"[INFO]: {message}");
                    break;
                case LogLevel.Error:
                    Console.WriteLine($"[ERROR]: {message}");
                    break;
            }
        }     
    }

    enum LogLevel {
        Info,
        Error,
    } 
}

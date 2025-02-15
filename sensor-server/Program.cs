using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Program {
    class Program {
        public static async Task Main(string[] args) {
            var server = new SensorServer();
            await server.Start();
        } 
    }

    class SensorServer {

        private readonly Logger logger;

        public SensorServer() {
            logger = new Logger();
        }

        public async Task Start() {
            var port = 5611;
            logger.Log($"Server Started Listening on Port : {port}");

            var listener = new TcpListener(IPAddress.Loopback, port);   
            listener.Start();

            Console.CancelKeyPress += delegate {
                listener.Stop();
                logger.Log($"Server Exiting");
            };
            
            while(true) {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Task.Run(async () => await HandleClient(client));
            }
        }
    
        async Task HandleClient(TcpClient client)
        {
            logger.Log($"Accepted New Connection", client.Client.RemoteEndPoint, LogLevel.Info);
            try {
                var buffer = new byte[1024];

                using NetworkStream stream = client.GetStream();
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                var command = Encoding.ASCII.GetString(buffer.Take(bytesRead).ToArray());          

                await HandleCommand(command.ToString(), stream, client.Client.RemoteEndPoint);
            } catch(Exception ex) {
                logger.Log(ex, client.Client.RemoteEndPoint);
            } finally {
                client.Close();
            }
        }

        async Task HandleCommand(string command, NetworkStream stream, EndPoint endPoint) {
            logger.Log($"Sending Response to Command : {command}",  endPoint, LogLevel.Info);
            string response;
            switch (command) {
                case "GET_STATUS":
                    response = "Active";                
                    break;
                case "GET_TEMP":
                    response = "35.6C";
                    break;
                case "GET_PRESSURE":
                    response = "450P";
                    break;
                case "GET_RPM":
                    response = "341RPM";
                    break;
                default:
                    response = $"UNKNOWN COMMAND: {command}";
                    logger.Log(response ,endPoint, LogLevel.Error);
                    break;
            }

            var bytes = Encoding.ASCII.GetBytes(response);
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }

    class Logger {
        public Logger(){
        }

        public void Log(string message, EndPoint endPoint, LogLevel level) {    
            switch(level) {
                case LogLevel.Info:
                    Console.WriteLine($"[INFO]: {endPoint}: {message}");
                    break;
                case LogLevel.Error:
                    Console.WriteLine($"[ERROR]: {endPoint}: {message}");
                    break;
            }
        }     

        public void Log(Exception ex, EndPoint endPoint) {    
            Log($"{ex.Message} \n {ex.StackTrace}", endPoint, LogLevel.Error);
        }    

        public void Log(string message) {
            Console.WriteLine($"[INFO]: {message}");
        }
    }

    enum LogLevel {
        Info,
        Error,
    } 
}
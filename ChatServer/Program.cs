using System.Net;
using System.Net.Sockets;

TcpListener listener = new TcpListener(IPAddress.Loopback, 5050);
listener.Start();
Console.WriteLine("Serveur démarré sur 127.0.0.1:5050. En attente d'un client...");

Server server = new Server();
Logger logger = new Logger("server.log");
logger.Attach(server);

while (true)
{
    TcpClient client = listener.AcceptTcpClient();
    ClientHandler handler = new ClientHandler(client, server);
    Thread thread = new Thread(handler.Run);
    thread.IsBackground = true;
    thread.Start();

}


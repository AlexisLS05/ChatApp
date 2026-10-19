using System.Net;
using System.Net.Sockets;

TcpListener listener = new TcpListener(IPAddress.Loopback, 5050);
listener.Start();
Console.WriteLine("Serveur démarré sur 127.0.0.1:5050. En attente d'un client...");

while (true)
{
    TcpClient client = listener.AcceptTcpClient();
    Console.WriteLine("Client connecté.");
    ClientHandler handler = new ClientHandler(client);
    Thread thread = new Thread(handler.Run);
    thread.IsBackground = true;
    thread.Start();
    

}


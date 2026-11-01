using System.Net;
using System.Net.Sockets;

Server server = new Server();
Logger logger = new Logger("server.log");
logger.Attach(server);

TcpListener listener = new TcpListener(IPAddress.Loopback, 5050);
try
{
    listener.Start();
}
catch (SocketException ex)
{
    server.ReportError($"Démarrage impossible sur 127.0.0.1:5050 : {ex.Message}");
    return;
}
Console.WriteLine("Serveur démarré sur 127.0.0.1:5050. Tapez 'stop' pour l'arrêter.");

// Boucle Accept dans son propre thread : la console reste libre pour 'stop'
Thread acceptThread = new Thread(() =>
{
    try
    {
        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            ClientHandler handler = new ClientHandler(client, server);
            Thread thread = new Thread(handler.Run);
            thread.IsBackground = true;
            thread.Start();
        }
    }
    catch (SocketException ex)
    {
        // listener.Stop() débloque AcceptTcpClient par une exception
        if (!server.IsStopping)
            server.ReportError($"Boucle Accept interrompue : {ex.Message}");
    }
});
acceptThread.IsBackground = true;
acceptThread.Start();

// Console du serveur : 'stop' (ou fin de l'entrée) arrête proprement
while (true)
{
    string? command = Console.ReadLine();
    if (command == null || command.Trim().Equals("stop", StringComparison.OrdinalIgnoreCase)) break;
    Console.WriteLine("Commande inconnue. Tapez 'stop' pour arrêter le serveur.");
}

Console.WriteLine("Arrêt du serveur...");
server.IsStopping = true;
listener.Stop();   // plus de nouvelles connexions
server.StopAll("*** Le serveur s'arrête. Au revoir !");
acceptThread.Join(2000);

// Attend (2 s max) que chaque ClientHandler ait fini son finally et journalisé son départ
DateTime limit = DateTime.Now.AddSeconds(2);
while (server.GetUserNames().Count > 0 && DateTime.Now < limit)
    Thread.Sleep(50);
logger.Detach(server);
Console.WriteLine("Serveur arrêté.");

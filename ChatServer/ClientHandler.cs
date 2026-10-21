using System.Net.Sockets;
public class ClientHandler
{
    private TcpClient _client;
    private Server _server;
    private StreamWriter? _writer;
    private object _writeLock = new object();

    public ClientHandler(TcpClient client, Server server) 
    { 
        _client = client;
        _server = server; 
    }
    public void Run()
    {
        NetworkStream stream = _client.GetStream();
        StreamReader reader = new StreamReader(stream);
        _writer = new StreamWriter(stream);
        _writer.AutoFlush = true;
        _server.MessageBroadcast += OnBroadcast;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            Console.WriteLine($"Reçu : {line}");
            _server.Broadcast($"{_client.Client.RemoteEndPoint} : {line}");
        }
        Console.WriteLine("Client déconnecté.");
        _server.MessageBroadcast -= OnBroadcast;
        _client.Close(); 
    }

      private void OnBroadcast(object sender, MessageEventArgs e)
    {
        lock (_writeLock)
        {
            try{
                _writer.WriteLine(e.Text);
            }
            catch (IOException)
            {
                Console.WriteLine("Erreur d'envoi à un client (déconnecté ?)");   // sera remplacé par le Logger
            }
        }

    }
}
using System.Net.Sockets;
public class ClientHandler
{
    private TcpClient _client;
    public ClientHandler(TcpClient client) 
    { 
        _client = client; 
    }
    public void Run()
    {
        // stream + reader/writer sur _client (AutoFlush = true),
        // boucle while ReadLine() != null de l'étape 1, puis "Client déconnecté." + _client.Close()
        NetworkStream stream = _client.GetStream();
        StreamReader reader = new StreamReader(stream);
        StreamWriter writer = new StreamWriter(stream);
        writer.AutoFlush = true;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            Console.WriteLine($"Reçu : {line}");
            writer.WriteLine($"Echo : {line}");
        }
        Console.WriteLine("Client déconnecté.");
        _client.Close(); 
    }
}
using System.Net.Sockets;

// 1. Se connecter au serveur
TcpClient client = new TcpClient("127.0.0.1", 5050);
Console.WriteLine("Connecté. Tape un message (ligne vide pour quitter).");

// 2. Mêmes flux que côté serveur : la connexion TCP est symétrique
NetworkStream stream = client.GetStream();
StreamReader reader = new StreamReader(stream);
StreamWriter writer = new StreamWriter(stream);
writer.AutoFlush = true;

// 3. Envoyer une ligne, attendre la réponse, l'afficher
while (true)
{
    string? input = Console.ReadLine();
    if (string.IsNullOrEmpty(input)) break;

    writer.WriteLine(input);
    string? response = reader.ReadLine();
    Console.WriteLine(response);
}

client.Close();
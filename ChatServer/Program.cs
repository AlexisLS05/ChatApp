using System.Net;
using System.Net.Sockets;

// 1. Écouter sur 127.0.0.1:5050 (IPAddress.Loopback = 127.0.0.1, comme dans le cours)
TcpListener listener = new TcpListener(IPAddress.Loopback, 5050);
listener.Start();
Console.WriteLine("Serveur démarré sur 127.0.0.1:5050. En attente d'un client...");

// 2. Attendre un client. BLOQUANT : le programme reste figé ici jusqu'à une connexion
TcpClient client = listener.AcceptTcpClient();
Console.WriteLine("Client connecté.");

// 3. Créer les flux de lecture/écriture sur la connexion
NetworkStream stream = client.GetStream();
StreamReader reader = new StreamReader(stream);
StreamWriter writer = new StreamWriter(stream);
writer.AutoFlush = true;   // envoie immédiatement chaque WriteLine (sinon ça reste en mémoire tampon)

// 4. Boucle : lire une ligne, la renvoyer. ReadLine est BLOQUANT et renvoie null
//    quand le client ferme la connexion proprement.
string? line;
while ((line = reader.ReadLine()) != null)
{
    Console.WriteLine($"Reçu : {line}");
    writer.WriteLine($"Echo : {line}");
}

Console.WriteLine("Client déconnecté.");
client.Close();
listener.Stop();
using System.Net.Sockets;

TcpClient client;
try
{
    client = new TcpClient("127.0.0.1", 5050);
}
catch (SocketException)
{
    Console.WriteLine("Impossible de joindre le serveur 127.0.0.1:5050 (est-il démarré ?).");
    return 1;
}
Console.WriteLine("Connecté au serveur. Tapez /quit pour quitter.");

NetworkStream stream = client.GetStream();
StreamReader reader = new StreamReader(stream);
StreamWriter writer = new StreamWriter(stream);
writer.AutoFlush = true;

// Vrai quand l'utilisateur quitte lui-même : la fin de connexion est alors attendue
bool quitting = false;

// Thread de lecture réseau : affiche tout ce que le serveur envoie
Thread readThread = new Thread(() =>
{
    try
    {
        string? l;
        while ((l = reader.ReadLine()) != null)
            Console.WriteLine(l);
    }
    catch (IOException) { }
    catch (ObjectDisposedException) { }

    if (Volatile.Read(ref quitting)) return;
    // Coupure côté serveur : le main est bloqué sur Console.ReadLine, on quitte le processus
    Console.WriteLine("Connexion perdue avec le serveur.");
    Environment.Exit(1);
});
readThread.IsBackground = true;
readThread.Start();

// Thread principal : clavier -> serveur
while (true)
{
    string? input = Console.ReadLine();
    if (input == null) input = "/quit"; // fin de l'entrée (Ctrl+Z)
    if (input.Trim().Length == 0) continue;

    bool quit = input.Trim().Equals("/quit", StringComparison.OrdinalIgnoreCase);
    if (quit) Volatile.Write(ref quitting, true);
    try
    {
        writer.WriteLine(input);
    }
    catch (IOException)
    {
        Console.WriteLine("Connexion perdue avec le serveur.");
        return 1;
    }
    if (quit) break;
}

// Laisse le temps d'afficher le « Au revoir » du serveur
readThread.Join(2000);
client.Close();
Console.WriteLine("Déconnecté.");
return 0;

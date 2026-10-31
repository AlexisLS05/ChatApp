using System.Net.Sockets;

public class ClientHandler
{
    private TcpClient _client;
    private Server _server;
    private StreamWriter? _writer;
    private object _writeLock = new object();

    // Pseudo ; null tant que le login n'a pas réussi
    public string? Name { get; private set; }

    public ClientHandler(TcpClient client, Server server)
    {
        _client = client;
        _server = server;
    }

    public void Run()
    {
        try
        {
            NetworkStream stream = _client.GetStream();
            StreamReader reader = new StreamReader(stream);
            _writer = new StreamWriter(stream);
            _writer.AutoFlush = true;

            if (!Login(reader)) return;

            _server.MessageBroadcast += OnBroadcast;
            _server.Broadcast($"*** {Name} a rejoint le chat.");

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Trim().Length == 0) continue;
                _server.Broadcast(Server.Format(Name!, line));
            }
        }
        catch (IOException)
        {
            // Déconnexion brutale du client
        }
        catch (ObjectDisposedException)
        {
            // Socket déjà fermée
        }
        finally
        {
            _server.MessageBroadcast -= OnBroadcast;
            if (Name != null)
            {
                _server.Unregister(Name);
                _server.Broadcast($"*** {Name} a quitté le chat.");
            }
            Close();
        }
    }

    // 1re ligne = pseudo ; on redemande tant qu'il est invalide ou pris
    private bool Login(StreamReader reader)
    {
        Send("Bienvenue ! Entrez votre pseudo :");
        while (true)
        {
            string? name = reader.ReadLine();
            if (name == null) return false;
            name = name.Trim();
            if (!IsValidName(name))
                Send("Pseudo invalide (1 à 20 caractères, sans espace ni '/'). Réessayez :");
            else if (!_server.TryRegister(name, this))
                Send($"Le pseudo '{name}' est déjà pris. Réessayez :");
            else
            {
                Name = name;
                Send($"Connecté en tant que {name}. Tapez /help pour l'aide.");
                return true;
            }
        }
    }

    public static bool IsValidName(string name)
    {
        return name.Length > 0 && name.Length <= 20 && !name.Contains(' ') && !name.StartsWith('/');
    }

    // Écriture directe vers ce client, protégée par le lock du writer
    public void Send(string text)
    {
        lock (_writeLock)
        {
            try
            {
                _writer?.WriteLine(text);
            }
            catch (IOException)
            {
                Console.WriteLine("Erreur d'envoi à un client (déconnecté ?)");
            }
            catch (ObjectDisposedException)
            {
                // Client déjà fermé
            }
        }
    }

    private void OnBroadcast(object sender, MessageEventArgs e)
    {
        Send(e.Text);
    }

    public void Close()
    {
        _client.Close();
    }
}

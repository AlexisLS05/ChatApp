using System.Net.Sockets;

public class ClientHandler
{
    private TcpClient _client;
    private Server _server;
    private StreamWriter? _writer;
    private object _writeLock = new object();
    private string _endPoint;

    // Pseudo ; null tant que le login n'a pas réussi
    public string? Name { get; private set; }

    public ClientHandler(TcpClient client, Server server)
    {
        _client = client;
        _server = server;
        _endPoint = client.Client.RemoteEndPoint?.ToString() ?? "?";
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
            _server.NotifyConnected($"{Name} ({_endPoint})");
            _server.Broadcast($"*** {Name} a rejoint le chat.");

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Trim().Length == 0) continue;
                if (line.StartsWith('/'))
                {
                    if (!HandleCommand(line)) break; // /quit
                }
                else
                {
                    _server.NotifyMessage($"{Name} : {line}");
                    _server.Broadcast(Server.Format(Name!, line));
                }
            }
        }
        catch (IOException ex)
        {
            // Déconnexion brutale du client
            if (!_server.IsStopping)
                _server.ReportError($"Connexion perdue avec {Name ?? _endPoint} : {ex.Message}");
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
                _server.NotifyDisconnected($"{Name} ({_endPoint})");
                _server.Broadcast($"*** {Name} a quitté le chat.");
                _server.Unregister(Name); // en dernier : l'arrêt attend la liste vide
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

    // Traite une commande ; renvoie false pour terminer la session (/quit)
    private bool HandleCommand(string line)
    {
        string[] parts = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        string cmd = parts[0].ToLowerInvariant();

        switch (cmd)
        {
            case "/msg":
                if (parts.Length < 3)
                {
                    Send("Usage : /msg pseudo texte");
                    break;
                }
                ClientHandler? target = _server.Find(parts[1]);
                if (target == null || target.Name == null)
                {
                    Send($"Erreur : utilisateur '{parts[1]}' inconnu. Tapez /users.");
                    break;
                }
                // Écriture directe dans le writer du destinataire + écho à l'expéditeur
                _server.NotifyMessage($"{Name} -> {target.Name} (privé)");
                target.Send($"[{DateTime.Now:HH:mm}] (privé) {Name} : {parts[2]}");
                Send($"[{DateTime.Now:HH:mm}] (privé à {target.Name}) {Name} : {parts[2]}");
                break;

            case "/users":
                List<string> users = _server.GetUserNames();
                Send($"Connectés ({users.Count}) : {string.Join(", ", users)}");
                break;

            case "/name":
                if (parts.Length < 2 || !IsValidName(parts[1]))
                {
                    Send("Usage : /name nouveau (1 à 20 caractères, sans espace ni '/')");
                    break;
                }
                string oldName = Name!;
                string newName = parts[1];
                if (!_server.TryRename(oldName, newName, this))
                {
                    Send($"Le pseudo '{newName}' est déjà pris.");
                    break;
                }
                Name = newName;
                _server.NotifyMessage($"{oldName} renommé en {newName}");
                _server.Broadcast($"*** {oldName} s'appelle maintenant {newName}.");
                break;

            case "/quit":
                Send("Au revoir !");
                return false;

            case "/joke":
                try
                {
                    string joke = JokeService.GetJoke();
                    _server.NotifyMessage($"{Name} a demandé une blague");
                    _server.Broadcast(Server.Format("Chuck Norris", joke));
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException
                                           || ex is System.Text.Json.JsonException || ex is KeyNotFoundException
                                           || ex is IOException || ex is InvalidOperationException)
                {
                    _server.ReportError($"/joke : {ex.Message}");
                    Send("Service de blagues indisponible.");
                }
                break;

            case "/help":
                Send("Commandes : /msg pseudo texte | /users | /name nouveau | /joke | /quit | /help");
                break;

            default:
                Send($"Commande inconnue : {cmd}. Tapez /help.");
                break;
        }
        return true;
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
            catch (IOException ex)
            {
                if (!_server.IsStopping)
                    _server.ReportError($"Envoi impossible à {Name ?? _endPoint} : {ex.Message}");
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

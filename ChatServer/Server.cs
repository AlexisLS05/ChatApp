public delegate void MessageEventHandler(object sender, MessageEventArgs e);

public class MessageEventArgs : EventArgs
{
    public string Text { get; }
    public MessageEventArgs(string text) { Text = text; }
}

public class Server
{
    public event MessageEventHandler? MessageBroadcast;

    // Clients connectés, indexés par pseudo ; toujours accédé sous _clientsLock
    private readonly Dictionary<string, ClientHandler> _clients = new Dictionary<string, ClientHandler>(StringComparer.OrdinalIgnoreCase);
    private readonly object _clientsLock = new object();

    protected virtual void OnMessageBroadcast(MessageEventArgs e)
    {
        MessageBroadcast?.Invoke(this, e);
    }

    public void Broadcast(string text)
    {
        MessageEventArgs args = new MessageEventArgs(text);
        OnMessageBroadcast(args);
    }

    // Événements d'activité, écoutés par le Logger (même modèle que le broadcast)
    public event LogEventHandler? ClientConnected;
    public event LogEventHandler? ClientDisconnected;
    public event LogEventHandler? MessageReceived;
    public event LogEventHandler? ErrorOccurred;

    protected virtual void OnClientConnected(LogEventArgs e) { ClientConnected?.Invoke(this, e); }
    protected virtual void OnClientDisconnected(LogEventArgs e) { ClientDisconnected?.Invoke(this, e); }
    protected virtual void OnMessageReceived(LogEventArgs e) { MessageReceived?.Invoke(this, e); }
    protected virtual void OnErrorOccurred(LogEventArgs e) { ErrorOccurred?.Invoke(this, e); }

    public void NotifyConnected(string text) { OnClientConnected(new LogEventArgs(text)); }
    public void NotifyDisconnected(string text) { OnClientDisconnected(new LogEventArgs(text)); }
    public void NotifyMessage(string text) { OnMessageReceived(new LogEventArgs(text)); }
    public void ReportError(string text) { OnErrorOccurred(new LogEventArgs(text)); }

    // Message horodaté côté serveur
    public static string Format(string name, string text)
    {
        return $"[{DateTime.Now:HH:mm}] {name} : {text}";
    }

    // Enregistre le pseudo s'il est libre
    public bool TryRegister(string name, ClientHandler handler)
    {
        lock (_clientsLock)
        {
            if (_clients.ContainsKey(name)) return false;
            _clients.Add(name, handler);
            return true;
        }
    }

    public void Unregister(string name)
    {
        lock (_clientsLock)
        {
            _clients.Remove(name);
        }
    }

    // Change le pseudo ; vérification d'unicité et remplacement sous le même lock
    public bool TryRename(string oldName, string newName, ClientHandler handler)
    {
        lock (_clientsLock)
        {
            bool sameUser = string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase);
            if (!sameUser && _clients.ContainsKey(newName)) return false;
            _clients.Remove(oldName);
            _clients.Add(newName, handler);
            return true;
        }
    }

    public ClientHandler? Find(string name)
    {
        lock (_clientsLock)
        {
            return _clients.TryGetValue(name, out ClientHandler? handler) ? handler : null;
        }
    }

    // Vrai pendant l'arrêt : les erreurs d'envoi attendues ne sont plus journalisées
    public volatile bool IsStopping;

    // Arrêt : copie sous lock, puis prévient et ferme HORS du lock
    // (les finally des ClientHandler appellent Unregister qui reprend le lock)
    public void StopAll(string message)
    {
        IsStopping = true;
        List<ClientHandler> handlers;
        lock (_clientsLock)
        {
            handlers = _clients.Values.ToList();
        }
        foreach (ClientHandler handler in handlers)
        {
            handler.Send(message);
            handler.Close();
        }
    }

    public List<string> GetUserNames()
    {
        lock (_clientsLock)
        {
            return _clients.Keys.OrderBy(n => n).ToList();
        }
    }
}

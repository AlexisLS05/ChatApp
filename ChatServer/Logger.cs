// Delegate et arguments des événements de journalisation
public delegate void LogEventHandler(object sender, LogEventArgs e);

public class LogEventArgs : EventArgs
{
    public string Text { get; }
    public LogEventArgs(string text) { Text = text; }
}

// Abonné aux événements du Server ; écrit dans la console et dans server.log
public class Logger
{
    private readonly string _path;
    private readonly object _logLock = new object();

    public Logger(string path)
    {
        _path = path;
    }

    // Abonnement aux événements du serveur
    public void Attach(Server server)
    {
        server.ClientConnected += OnClientConnected;
        server.ClientDisconnected += OnClientDisconnected;
        server.MessageReceived += OnMessageReceived;
        server.ErrorOccurred += OnErrorOccurred;
    }

    public void Detach(Server server)
    {
        server.ClientConnected -= OnClientConnected;
        server.ClientDisconnected -= OnClientDisconnected;
        server.MessageReceived -= OnMessageReceived;
        server.ErrorOccurred -= OnErrorOccurred;
    }

    private void OnClientConnected(object sender, LogEventArgs e) { Write("CONNEXION", e.Text); }
    private void OnClientDisconnected(object sender, LogEventArgs e) { Write("DECONNEXION", e.Text); }
    private void OnMessageReceived(object sender, LogEventArgs e) { Write("MESSAGE", e.Text); }
    private void OnErrorOccurred(object sender, LogEventArgs e) { Write("ERREUR", e.Text); }

    // Un seul thread à la fois écrit (console + fichier)
    private void Write(string category, string text)
    {
        string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{category}] {text}";
        lock (_logLock)
        {
            Console.WriteLine(line);
            try
            {
                File.AppendAllText(_path, line + Environment.NewLine);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"(écriture de {_path} impossible : {ex.Message})");
            }
        }
    }
}

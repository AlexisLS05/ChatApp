public delegate void MessageEventHandler(object sender, MessageEventArgs e);

public class MessageEventArgs : EventArgs
{
    public string Text { get; }
    public MessageEventArgs(string text) { Text = text; }
}

public class Server
{
    public event MessageEventHandler? MessageBroadcast;

    protected virtual void OnMessageBroadcast(MessageEventArgs e)
    {
        MessageBroadcast?.Invoke(this, e);
    }

    public void Broadcast(string text)
    {
        MessageEventArgs args = new MessageEventArgs(text);
        OnMessageBroadcast(args);
    }
}
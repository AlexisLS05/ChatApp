using System.Net.Sockets;


TcpClient client = new TcpClient("127.0.0.1", 5050);
Console.WriteLine("Connecté. Tape un message (ligne vide pour quitter).");


NetworkStream stream = client.GetStream();
StreamReader reader = new StreamReader(stream);
StreamWriter writer = new StreamWriter(stream);
writer.AutoFlush = true;


Thread readThread = new Thread(() =>
{
    string? l;
    while ((l = reader.ReadLine()) != null)
        Console.WriteLine(l);
});
readThread.IsBackground = true; 
readThread.Start();

while (true)
{
    string? input = Console.ReadLine();
    if (string.IsNullOrEmpty(input)) break;
    writer.WriteLine(input);
}

client.Close();
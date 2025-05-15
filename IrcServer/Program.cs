using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IrcServer;

class Program
{
    private static readonly List<(TcpClient Client, string Nick)> clients = new();
    private const string Channel = "#mychannel";

    static async Task Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 6667);
        listener.Start();
        Console.WriteLine("IRC Server running on port 6667");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            clients.Add((client, ""));
            _ = HandleClientAsync(client);
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        string nick = "";
        try
        {
            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Send welcome message
            await writer.WriteLineAsync($":server 001 {nick} :Welcome to the IRC Server!");
            await writer.WriteLineAsync($":server 376 {nick} :End of MOTD");

            while (true)
            {
                string? line = await reader.ReadLineAsync();
                if (line == null) break;

                Console.WriteLine($"Received: {line}");

                // Parse IRC commands
                if (line.StartsWith("NICK"))
                {
                    nick = line.Split(' ')[1].Trim();
                    clients[clients.FindIndex(c => c.Client == client)] = (client, nick);
                    await writer.WriteLineAsync($":server NOTICE {nick} :Nickname set to {nick}");
                }
                else if (line.StartsWith("USER"))
                {
                    await writer.WriteLineAsync($":server NOTICE {nick} :User registered");
                }
                else if (line.StartsWith("PRIVMSG"))
                {
                    string message = line.Substring(line.IndexOf(':') + 1);
                    string target = line.Split(' ')[1];
                    if (target == Channel)
                    {
                        Console.WriteLine("a");
                        await BroadcastAsync($":{nick}!user@host PRIVMSG {Channel} :{message}", client);
                        Console.WriteLine($"<{nick} in {Channel}> {message}");
                    }
                }
                else if (line.StartsWith("PING"))
                {
                    await writer.WriteLineAsync($"PONG {line.Split(' ')[1]}");
                }
                else if (line.StartsWith("QUIT"))
                {
                    break;
                }
                else if (line.StartsWith("LIST"))
                {
                    StringBuilder list = new StringBuilder();
                    foreach (var (c, n) in clients)
                    {
                        if (n != "")
                        {
                            list.AppendLine($"{n}");
                        }
                    }
                    await writer.WriteLineAsync($":server 353 {nick} = {Channel}: {string.Join(" ", list)}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Client error: {e.Message}");
        }

        clients.RemoveAll(c => c.Client == client);
        client.Close();
        Console.WriteLine($"Client {nick} disconnected");
    }

    static async Task BroadcastAsync(string message, TcpClient sender)
    {
        Console.WriteLine("a");
        foreach (var (client, _) in clients)
        {
            Console.WriteLine($"Client {client.Client.RemoteEndPoint} Sender: {sender.Client.RemoteEndPoint}");
            if (!client.Client.RemoteEndPoint.Equals(sender.Client.RemoteEndPoint) && client.Connected)
            {
                try
                {
                    var stream = client.GetStream();
                    var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
                    await writer.WriteLineAsync(message);
                }
                catch { }
            }
        }
    }
}
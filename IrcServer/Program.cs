using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IrcServer;

class Program
{
private static readonly List<(TcpClient Client, string nick)> _clients = new();
    private const string Channel = "#mainC";
    static async Task Main(string[] args)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 6667);
        listener.Start();
        Console.WriteLine("Server started on port 6667. Waiting for clients...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _clients.Add((client, "Guest"));
            _ = HandleClientAsync(client);
        }
    }

    static async Task HandleClientAsync(TcpClient client)
    {
        string nick = "Guest";
        try
        {
            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            await writer.WriteLineAsync($"Welcome to the IRC server! Your nickname is {nick}.");
            await writer.WriteLineAsync($"To change your nickname, use the command: /nick <new_nick>");
            await writer.WriteLineAsync("End of MOTD.");
            while (true)
            {
                string message = await reader.ReadLineAsync();
                if (message == null) break;

                if (message.StartsWith("/nick "))
                {
                    string newNick = message.Substring(6);
                    nick = newNick;
                    _clients[_clients.FindIndex(c => c.Client == client)] = (client, newNick);
                    await writer.WriteLineAsync($"Your nickname has been changed to {newNick}.");
                }
                else if (message.StartsWith("/quit"))
                {
                    await writer.WriteLineAsync("Goodbye!");
                    break;
                }
                else if (message.StartsWith("/list"))
                {
                    StringBuilder sb = new StringBuilder("Connected users:");
                    foreach (var (c, n) in _clients)
                    {
                        sb.AppendLine($" - {n}");
                    }
                    await writer.WriteLineAsync(sb.ToString());
                }
                else if (message.StartsWith("/msg "))
                {
                    string[] parts = message.Split(' ', 3);
                    if (parts.Length < 3)
                    {
                        await writer.WriteLineAsync("Usage: /msg <nick> <message>");
                        continue;
                    }
                    string targetNick = parts[1];
                    string targetMessage = parts[2];
                    var targetClient = _clients.FirstOrDefault(c => c.nick == targetNick).Client;
                    if (targetClient != null)
                    {
                        StreamWriter targetWriter = new StreamWriter(targetClient.GetStream(), Encoding.UTF8) { AutoFlush = true };
                        await targetWriter.WriteLineAsync($"{nick} (private): {targetMessage}");
                    }
                    else
                    {
                        await writer.WriteLineAsync($"User {targetNick} not found.");
                    }
                }
                else
                {
                    foreach (var (c, n) in _clients)
                    {
                        if (c != client)
                        {
                            StreamWriter clientWriter = new StreamWriter(c.GetStream(), Encoding.UTF8) { AutoFlush = true };
                            await clientWriter.WriteLineAsync($"{nick}: {message}");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
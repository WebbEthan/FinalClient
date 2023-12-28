using System.Net.Sockets;

public class Messager : Client
{
    public Messager()
    {
        // setup handles
        Handles = new Dictionary<int, _packetScripts>()
        {
            { 0, _msg }
        };
        // connect to server
        Connect("127.0.0.1", 25578);
    }
    private bool _active = true;
    protected override void OnAuthentication()
    {
        // Joins a match
        JoinMatch();
    }
    protected override void AddClient(string clientID)
    {
        Console.WriteLine($"{clientID} joined the chat");
    }
    protected override void OnMatchKicked()
    {
        // Joins a new Match
        JoinMatch();
    }
    protected override void OnDisconnect()
    {
        _active = false;
        ConsoleApp.Start();
    }
    protected override void OnMatchJoin()
    {
        Console.WriteLine("Write a message or type /quit to disconnect.");
        while (_active)
        {
            string msg = Console.ReadLine();
            if (!string.IsNullOrEmpty(msg))
            {
                if (msg == "/quit")
                {
                    Disconnect();
                    return;
                }
                // sends msg
                using (Packet packet = new Packet(0))
                {
                    packet.Write(msg);
                    SendData(packet, ProtocolType.Tcp);
                }
            }
        }
    }
    private void _msg(Packet packet, ProtocolType protocolType)
    {
        Console.WriteLine(packet.ReadString());
    }
}
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
    protected override void OnAuthentication()
    {
        // Joins a match
        JoinMatch();
    }
    protected override void OnMatchJoin()
    {
        Console.WriteLine("Write a message or type /quit to disconnect and quit.");
        while (true)
        {
            string msg = Console.ReadLine();
            if (!string.IsNullOrEmpty(msg))
            {
                if (msg == "/quit")
                {
                    // closes program
                    ThreadManager.StopThreads();
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
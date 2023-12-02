using System.Net.Sockets;

public class RPSClient : Client
{
    public RPSClient()
    {
        Handles = new Dictionary<int, _packetScripts>()
        {
            { 0, _msg },
            { 1, _startGame },
            { 2, _winner }
        };
        Connect("127.0.0.1", 25579);
    }
    protected override void OnAuthentication()
    {
        JoinMatch();
    }
    protected override void OnMatchJoin()
    {
        Console.WriteLine("Waiting for other player");
    }
    private void _startGame(Packet packet, ProtocolType protocolType)
    {
        Console.WriteLine("Select One");
        Console.WriteLine("1. Rock");
        Console.WriteLine("2. Paper");
        Console.WriteLine("3. Scissors");
        int.TryParse(Console.ReadLine(), out int submition);
        if (submition <= 3 && submition > 0)
        {
            using (Packet packet1 = new Packet(1))
            {
                packet1.Write((byte)(submition - 1));
                SendData(packet, protocolType);
            }
        }
    }
    private void _winner(Packet packet, ProtocolType protocolType)
    {
        switch(packet.ReadByte())
        {
            case 0:
                Console.WriteLine("draw");
                break;
            case 1:
                Console.WriteLine();
                break;
            case 2:
                Console.WriteLine();
                break;
        }
    }
    private void _msg(Packet packet, ProtocolType protocolType)
    {
        Console.WriteLine(packet.ReadString());
    }
}
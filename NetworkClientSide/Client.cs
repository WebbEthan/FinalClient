using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Reflection.Metadata;
public class Client
{
    private string _ip;
    private int _port;
    public delegate void _packetScripts(Packet packet, ProtocolType protocolType);
    // The store of methods defined by the client
    public Dictionary<int, _packetScripts> Handles;
    #region PublicMethods
    public string Username = "";
    public bool IsHost = false;
    public string CurrenctMatchCode;
    public void Connect(string ip, int port)
    {
        _ip = ip;
        _port = port;
        _tcpProtocal = new _tcp(ip, port, this);
    }
    public void SendData(Packet packet, ProtocolType protocolType)
    {
        if (protocolType == ProtocolType.Tcp && _tcpProtocal != null)
        {
            packet.Insert(0, BitConverter.GetBytes(packet.Data.Count));
            packet.PrepForSending();
            _tcpProtocal.SendData(packet);
        }
        else if (_udpProtocal != null)
        {
            packet.PrepForSending();
            _udpProtocal.SendData(packet);
        }
    }
    // A method for when a client joins the match you are currently in
    protected virtual void AddClient(string clientID) {}
    // A method fow when a client leave the match you are currently in
    protected virtual void RemoveClient(string clientID) {}
    // Called when you successfully authenticate to the server
    protected virtual void OnAuthentication() {}
    // Called when you join a match
    protected virtual void OnMatchJoin() {}
    // Called when you disconnect from the server
    protected virtual void OnDisconnect() {}
    // Tells the server to put you in a match, "0" for random match
    public void JoinMatch(string code = "0")
    {
        using (Packet packet = new Packet(255))
        {
            packet.Write(code);
            packet.Write(Username);
            SendData(packet, ProtocolType.Tcp);
        }
    }
    // Tells the server to create a match with you as the host
    public void CreateMatch()
    {
        using (Packet packet = new Packet(255))
        {
            packet.Write("-1");
            packet.Write(Username);
            SendData(packet, ProtocolType.Tcp);
        }
    }
    // Tells the server to remove you from your current match
    public void LeaveMatch()
    {
        using (Packet packet = new Packet(252))
        {
            SendData(packet, ProtocolType.Tcp);
        }
    }
    // Disconnects from the server
    public void Disconnect()
    {
        _tcpProtocal.Disconnect();
        _udpProtocal.Disconnect();
        _tcpProtocal = null;
        _udpProtocal = null;
        Console.WriteLine("Disconnected");
        OnDisconnect();
    }
    #endregion
    private void _connectionCallback(int partialClientID)
    {
        Console.WriteLine("Setting Up UDP");
        _udpProtocal = new _udp(_ip, _port, partialClientID, this);
    }
    #region Sockets
    private _tcp? _tcpProtocal;
    private _udp? _udpProtocal;
    private class _tcp
    {
        private Client _reference;
        private bool _active = true;
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private byte[] _buffer = new byte[4096];
        public _tcp(string ip, int port, Client reference)
        {
            _reference = reference;
            // Calls connect
            _socket.Connect(ip, port);
            // starts listening to data
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(_handleData), null);
        }
        private void _handleData(IAsyncResult result)
        {
            if (_active)
            {
                if (!_socket.Connected)
                {
                    _reference.Disconnect();
                    return;
                }
                int recieveLength = _socket.EndReceive(result);
                if (recieveLength > _buffer.Length)
                {
                    _reference.Disconnect();
                }
                if (recieveLength > 0)
                {
                    byte[] data = new byte[recieveLength];
                    Array.Copy(_buffer, data, recieveLength);
                    // Moves of the network thread
                    ThreadManager.ExecuteOnMainThread = new List<Action>() {() => 
                    {
                        try
                        {
                            while(true)
                            {
                                using (Packet packet = new Packet(data))
                                {
                                    if (_reference._udpProtocal == null)
                                    {
                                        Console.WriteLine(packet.ReadString());
                                        _reference._connectionCallback(packet.ReadInt());
                                        break;
                                    }
                                    else
                                    {
                                        if (packet.PacketType == 255)
                                        {
                                            // Reads data when match is joined
                                            _reference.CurrenctMatchCode = packet.ReadString();
                                            _reference.IsHost = packet.ReadBool();
                                            int connectedUsers = packet.ReadInt();
                                            for (int i = 0; i < connectedUsers; i++)
                                            {
                                                _reference.AddClient(packet.ReadString());
                                            }
                                            Console.WriteLine($"Joined Match {_reference.CurrenctMatchCode}");
                                            ThreadManager.ExecuteOnApplicationThread = new List<Action>() {() => {_reference.OnMatchJoin();}};
                                        }
                                        else if (packet.PacketType == 254)
                                        {
                                            // New Client in match data
                                            _reference.AddClient(packet.ReadString());
                                        }
                                        else if (packet.PacketType == 253)
                                        {
                                            // Authentication callback
                                            Console.WriteLine("Succesfully Authenticated");
                                            ThreadManager.ExecuteOnApplicationThread = new List<Action>() {() => {_reference.OnAuthentication();}};
                                        }
                                        else if (packet.PacketType == 252)
                                        {
                                            // Client left your match
                                            _reference.RemoveClient(packet.ReadString());
                                        }
                                        else
                                        {
                                            // Runs what is scripted in the handle
                                            _reference.Handles[packet.PacketType](packet, ProtocolType.Tcp);
                                        }
                                        // Runs packets recieved in rececion
                                        data = packet.UnreadData();
                                        if (data.Length == 0)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }};
                    // Begins listening for more data
                    _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(_handleData), null);
                }
                else
                {
                    _reference.Disconnect();
                }
            }
        }
        public void SendData(Packet packet)
        {
            _socket.BeginSend(packet.Data.ToArray(), 0, packet.Data.Count, SocketFlags.None, new AsyncCallback(_endSend), null);
        }
        private void _endSend(IAsyncResult result) { _socket.EndSend(result); }
        public void Disconnect()
        {
            _active = false;
            _socket.Close();
        }
    }
    private class _udp
    {
        private Client _reference;
        private UdpClient _socket;
        private IPEndPoint _endpoint;
        public _udp(string ip, int port, int partialClientID, Client reference)
        {
            _reference = reference;
            _endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            _socket = new UdpClient();
            // calls connect
            _socket.Connect(_endpoint);
            _socket.BeginReceive(new AsyncCallback(_handleData), null);

            using (Packet packet = new Packet(0))
            {
                packet.Write(partialClientID);
                packet.PrepForSending();
                SendData(packet);
            }
        }
        private void _handleData(IAsyncResult result)
        {
            if (_socket.Client == null)
            {
                return;
            }
            byte[] data = _socket.EndReceive(result, ref _endpoint);
            try
            {
                using (Packet packet = new Packet(data))
                {
                    _reference.Handles[packet.PacketType](packet, ProtocolType.Udp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            _socket.BeginReceive(new AsyncCallback(_handleData), null);
        }
        public void SendData(Packet packet)
        {
            _socket.BeginSend(packet.Data.ToArray(), packet.Data.Count, null, null);
        }
        public void Disconnect()
        {
            _socket.Close();
        }
    }
    #endregion
}
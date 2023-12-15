using System.Text;
public class Packet : IDisposable
{
    // A number to differenciate packet types
    public byte PacketType;
    public List<byte> Data;
    private byte[] _readingData;
    private int _readerPos = 0;
    #region  Constructors
    public Packet(byte[] _data)
    {
        PacketType = _data[0];
        List<byte> Readable = new List<byte>();
        Readable.AddRange(_data);
        Data = Readable.GetRange(1, _data.Length - 1);
        _readingData = Data.ToArray();
    }
    public Packet(byte type)
    {
        Data = new List<byte>();
        PacketType = type;
    }
    #endregion
    public byte[] UnreadData()
    {
        return Data.GetRange(_readerPos, Data.Count - _readerPos).ToArray();
    }
    #region WriteingMethods
    public void Write(byte _data)
    {
        Data.Add(_data);
    }
    public void Write(byte[] _data)
    {
        Data.AddRange(_data);
    }
    public void Write(int value)
    {
        Data.AddRange(BitConverter.GetBytes(value));
    }
    public void Write(float value)
    {
        Data.AddRange(BitConverter.GetBytes(value));
    }
    public void Write(string value)
    {
        byte[] _data = Encoding.ASCII.GetBytes(value);
        Write(_data.Length);
        Data.AddRange(_data);
    }
    public void Write(bool value)
    {
        Data.AddRange(BitConverter.GetBytes(value));
    }
    #endregion
    #region ReadMethods
    public byte ReadByte()
    {
        _readerPos += 1;
        return Data[_readerPos - 1];
    }
    public int ReadInt()
    {
        _readerPos += 4;
        return BitConverter.ToInt32(_readingData, _readerPos - 4);
    }
    public string ReadString()
    {
        int length = ReadInt();
        byte[] value = Data.GetRange(_readerPos, length).ToArray();
        _readerPos += length;
        return Encoding.ASCII.GetString(value);
    }
    public bool ReadBool()
    {
        _readerPos += 1;
        return _readingData[_readerPos - 1] == 1;
    }
    #endregion
    public void Insert(int index, byte[] _data)
    {
        Data.InsertRange(index, _data);
    }
    public void PrepForSending()
    {
        Data.Insert(0, PacketType);
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
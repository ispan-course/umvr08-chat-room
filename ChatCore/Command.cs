using System;
using System.Text;

namespace ChatCore
{
  public class Command
  {
    public enum Type
    {
      EXIT = 0,
      LOGIN = 1,
      MESSAGE
    }

    private int m_Command;
    public int CommandID => m_Command;

    private int m_Length;

    private byte[] m_PacketBuffer = new byte[1024];
    private int m_BeginPos;
    private int m_Pos;

    protected Command(int command)
    {
      m_Command = command;
    }

    public static void FetchHeader(out int length, out int command, byte[] packetData, int beginPos)
    {
      var header = new Command(0);

      header.UnSealPacketBuffer(packetData, beginPos);
      header.Unserialize();

      length = header.m_Length;
      command = header.m_Command;
    }

    public virtual void Serialize()
    {
      m_Pos = m_BeginPos;

      _WriteToBuffer(m_Length);
      _WriteToBuffer(m_Command);
    }

    public virtual void Unserialize()
    {
      m_Pos = m_BeginPos;

      _ReadFromBuffer(out m_Length);
      _ReadFromBuffer(out m_Command);
    }

    public byte[] SealPacketBuffer(out int iLength)
    {
      m_Length = m_Pos;

      var curPos = m_Pos;
      m_Pos = m_BeginPos;
      _WriteToBuffer(m_Length);
      m_Pos = curPos;

      iLength = m_Length;
      return m_PacketBuffer;
    }

    public void UnSealPacketBuffer(byte[] packetData, int beginPos)
    {
      m_PacketBuffer = packetData;
      m_BeginPos = beginPos;
    }

    protected bool _WriteToBuffer(int i)
    {
      // convert int to byte array
      var bytes = BitConverter.GetBytes(i);
      _WriteToBuffer(bytes);
      return true;
    }

    protected bool _ReadFromBuffer(out int i)
    {
      if (BitConverter.IsLittleEndian)
      {
        var byteData = new byte[sizeof(int)];
        Buffer.BlockCopy(m_PacketBuffer, m_BeginPos + m_Pos, byteData, 0, byteData.Length);
        Array.Reverse(byteData);
        i = BitConverter.ToInt32(byteData, 0);
      }
      else
      {
        i = BitConverter.ToInt32(m_PacketBuffer, m_BeginPos + m_Pos);
      }

      m_Pos += sizeof(int);
      return true;
    }

    protected bool _WriteToBuffer(string str)
    {
      // convert string to byte array
      var bytes = Encoding.Unicode.GetBytes(str);

      // write byte array length to packet's byte array
      if (_WriteToBuffer(bytes.Length) == false)
      {
        return false;
      }

      _WriteToBuffer(bytes);
      return true;
    }

    // read a string from packet's byte array
    protected bool _ReadFromBuffer(out string str)
    {
      // read string length
      _ReadFromBuffer(out int length);

      if (BitConverter.IsLittleEndian)
      {
        var byteData = new byte[length];
        Buffer.BlockCopy(m_PacketBuffer, m_BeginPos + m_Pos, byteData, 0, length);
        Array.Reverse(byteData);
        str = Encoding.Unicode.GetString(byteData, 0, length);
      }
      else
      {
        str = Encoding.Unicode.GetString(m_PacketBuffer, m_BeginPos + m_Pos, length);
      }

      m_Pos += length;
      return true;
    }

    // write byte array to packet's byte array
    private void _WriteToBuffer(byte[] byteData)
    {
      // converter little-endian to network's big-endian
      if (BitConverter.IsLittleEndian)
      {
        Array.Reverse(byteData);
      }

      byteData.CopyTo(m_PacketBuffer, m_BeginPos + m_Pos);
      m_Pos += byteData.Length;
    }
  }
}

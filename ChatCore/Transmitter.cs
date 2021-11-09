using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ChatCore
{
  public class Transmitter
  {
    private string m_ClientID;
    public string ClientID => m_ClientID;
    
    private TcpClient m_Client;
    private string m_Address;
    private int m_Port;

    private readonly Dictionary<int, Type> m_CommandTypes = new Dictionary<int, Type>();
    private readonly Dictionary<int, Delegate> m_CommandActions = new Dictionary<int, Delegate>();

    public Transmitter()
    {
    }

    public Transmitter(string clientID, TcpClient client = null)
    {
      m_ClientID = clientID;
      m_Client = client;
    }

    public bool Connect(string address, int port)
    {
      m_Address = address;
      m_Port = port;

      m_Client = new TcpClient();

      try
      {
        Console.WriteLine("Connecting to server {0}:{1}", m_Address, m_Port);
        m_Client.Connect(m_Address, m_Port);
        Console.WriteLine("Connected to server");

        return true;
      }
      catch (ArgumentNullException e)
      {
        Console.WriteLine("ArgumentNullException happened: {0}", e);
        return false;
      }
      catch (SocketException e)
      {
        Console.WriteLine("SocketException happened: {0}", e);
        return false;
      }
      catch (Exception e)
      {
        Console.WriteLine("Exception happened: {0}", e);
        return false;
      }
    }

    public bool IsConnected()
    {
      if (m_Client == null || m_Client.Connected == false)
      {
        return false;
      }

      try
      {
        if (m_Client.Client.Poll(0, SelectMode.SelectRead))
        {
          var buff = new byte[1];
          if (m_Client.Client.Receive(buff, SocketFlags.Peek) == 0)
          {
            return false;
          }
        }
      }
      catch (Exception)
      {
        return false;
      }

      return true;
    }

    public void Disconnect()
    {
      m_Client.Close();
    }

    public void Register<T>(Action<Transmitter, T> action) where T : Command, new()
    {
      var cmd = new T();
      m_CommandTypes.Add(cmd.CommandID, cmd.GetType());
      m_CommandActions.Add(cmd.CommandID, action);
    }

    public void Send(Command command)
    {
      command.Serialize();
      var buffer = command.SealPacketBuffer(out var length);

      try
      {
        m_Client.GetStream().Write(buffer, 0, length);
      }
      catch (Exception e)
      {
        Console.WriteLine("Client {0} Send Failed: {1}", ClientID, e.Message);
      }
    }

    public void Refresh()
    {
      if (m_Client.Available > 0)
      {
        HandleReceiveMessages();
      }
    }

    private void HandleReceiveMessages()
    {
      var numBytes = m_Client.Available;
      var buffer = new byte[numBytes];

      var bytesRead = m_Client.GetStream().Read(buffer, 0, numBytes);

      if (bytesRead != numBytes)
      {
        Console.WriteLine("Error reading stream buffer...");
        return;
      }

      var pos = 0;

      while (pos < bytesRead)
      {
        Command.FetchHeader(out var length, out var commandId, buffer, pos);

        var t = m_CommandTypes[commandId];
        var msg = (Command)Activator.CreateInstance(t);
        msg.UnSealPacketBuffer(buffer, pos);
        msg.Unserialize();

        var actionObj = m_CommandActions[commandId];
        actionObj.DynamicInvoke(new object[] { this, msg });

        pos += length;
      }
    }
  }
}

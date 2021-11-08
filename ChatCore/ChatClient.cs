using System;
using System.Net.Sockets;

namespace ChatCore
{
  public class ChatClient
  {
    private TcpClient m_client;

    public ChatClient()
    {
    }

    public bool Connect(string address, int port)
    {
      m_client = new TcpClient();

      try
      {
        Console.WriteLine("Connecting to chat server {0}:{1}", address, port);
        m_client.Connect(address, port);

        Console.WriteLine("Connected to chat server");
        return m_client.Connected;
      }
      catch (ArgumentNullException e)
      {
        Console.WriteLine("ArgumentNullException: {0}", e);
        return false;
      }
      catch (SocketException e)
      {
        Console.WriteLine("SocketException: {0}", e);
        return false;
      }
    }

    public void Disconnect()
    {
      m_client.Close();
      Console.WriteLine("Disconnected");
    }

    public void Refresh()
    {
      if (m_client.Available > 0)
      {
        HandleReceiveMessages(m_client);
      }
    }

    private void HandleReceiveMessages(TcpClient client)
    {
      var stream = client.GetStream();

      var numBytes = client.Available;
      var buffer = new byte[numBytes];
      var bytesRead = stream.Read(buffer, 0, numBytes);
      var request = System.Text.Encoding.ASCII.GetString(buffer).Substring(0, bytesRead);

      if (request.StartsWith("MESSAGE:", StringComparison.OrdinalIgnoreCase))
      {
        var tokens = request.Split(':');
        var sender = tokens[1];
        var message = tokens[2];
        Console.WriteLine("{0}: {1}", sender, message);
      }
    }

    public void SetName(string name)
    {
      var data = "LOGIN:" + name;
      SendData(data);
    }

    public void SendMessage(string message)
    {
      var data = "MESSAGE:" + message;
      SendData(data);
    }

    private void SendData(string data)
    {
      var requestBuffer = System.Text.Encoding.ASCII.GetBytes(data);
      m_client.GetStream().Write(requestBuffer, 0, requestBuffer.Length);
    }
  }
}

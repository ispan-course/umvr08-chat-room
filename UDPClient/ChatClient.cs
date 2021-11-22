using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Net;

namespace UDPClient
{
  internal class ChatClient
  {
    private string m_userName = "";
    private UdpClient m_client = null;
    private IPEndPoint m_serverEndPoint = null;

    public ChatClient()
    {
    }

    public bool Connect(string address, int port)
    {
      m_client = new UdpClient();

      try
      {
        var ipAddress = IPAddress.Parse(address);
        m_serverEndPoint = new IPEndPoint(ipAddress, port);

        return true;
      }
      catch (Exception e)
      {
        Console.WriteLine("Exception: {0}", e);
        return false;
      }
    }

    public void SetName(string name)
    {
      m_userName = name;
      var request = "LOGIN:" + m_userName;
      var buffer = System.Text.Encoding.ASCII.GetBytes(request);

      m_client.Send(buffer, buffer.Length, m_serverEndPoint);
    }

    public void SendMessage(string sMessage)
    {
      var request = "MESSAGE:" + sMessage;
      var buffer = System.Text.Encoding.ASCII.GetBytes(request);

      m_client.Send(buffer, buffer.Length, m_serverEndPoint);
    }

    public void Refresh()
    {
      if (m_client.Available > 0)
      {
        HandleReceiveMessages();
      }
    }

    private void HandleReceiveMessages()
    {
      IPEndPoint ep = null;
      var buffer = m_client.Receive(ref ep);
      var request = System.Text.Encoding.ASCII.GetString(buffer);

      if (request.StartsWith("MESSAGE:", StringComparison.OrdinalIgnoreCase))
      {
        var tokens = request.Split(':');
        var name = tokens[1];
        var message = tokens[2];
        Console.WriteLine("{0}: {1}", name, message);
      }
    }
  }
}

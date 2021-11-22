using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace UDPServer
{
  internal class ChatServer
  {
    private UdpClient m_listener;
    private readonly Dictionary<string, IPEndPoint> m_clients = new Dictionary<string, IPEndPoint>();
    private readonly Dictionary<string, string> m_userNames = new Dictionary<string, string>();

    public ChatServer()
    {
    }

    public void Bind(int port)
    {
      m_listener = new UdpClient(port);
      Console.WriteLine("Server start at port {0}", port);
    }

    public void Run()
    {
      while (true)
      {
        var numBytes = m_listener.Available;
        if (numBytes > 0)
        {
          HandleReceiveMessages();
        }
      }
    }

    private void HandleReceiveMessages()
    {
      IPEndPoint peerEndPoint = null;
      var buffer = m_listener.Receive(ref peerEndPoint);

      var request = System.Text.Encoding.ASCII.GetString(buffer);

      if (request.StartsWith("LOGIN:", StringComparison.OrdinalIgnoreCase))
      {
        var tokens = request.Split(':');
        var clientId = peerEndPoint.ToString();

        m_clients.Add(clientId, peerEndPoint);
        m_userNames.Add(clientId, tokens[1]);
        Console.WriteLine("Client Login: {0}", tokens[1]);
      }
      else if (request.StartsWith("MESSAGE:", StringComparison.OrdinalIgnoreCase))
      {
        var tokens = request.Split(':');
        var message = tokens[1];

        var peerId = peerEndPoint.ToString();

        Console.WriteLine("Receive: {0}:{1}", m_userNames[peerId], tokens[1]);

        foreach (var clientId in m_userNames.Keys)
        {
          if (clientId == peerId)
          {
            continue;
          }
          
          var data = "MESSAGE:" + m_userNames[peerId] + ":" + message;
          var dataBuffer = System.Text.Encoding.ASCII.GetBytes(data);

          m_listener.Send(dataBuffer, dataBuffer.Length, m_clients[clientId]);
        }
      }
    }
  }
}

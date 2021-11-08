using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatCore
{
  public class ChatServer
  {
    private int m_port;
    private TcpListener m_listener;
    private Thread m_handleThread;
    private readonly Dictionary<string, TcpClient> m_clients = new Dictionary<string, TcpClient>();
    private readonly Dictionary<string, string> m_userNames = new Dictionary<string, string>();

    public ChatServer()
    {
    }

    public void Bind(int port)
    {
      m_port = port;

      m_listener = new TcpListener(IPAddress.Any, port);
      Console.WriteLine("Server start at port {0}", port);
      m_listener.Start();
    }

    public void Start()
    {
      m_handleThread = new Thread(ClientsHandler);
      m_handleThread.Start();

      while (true)
      {
        Console.WriteLine("Waiting for a connection... ");
        var client = m_listener.AcceptTcpClient();

        var clientId = client.Client.RemoteEndPoint.ToString();
        Console.WriteLine("Client has connected from {0}", clientId);

        lock (m_clients)
        {
          m_clients.Add(clientId, client);
          m_userNames.Add(clientId, "Unknown");
        }
      }
    }

    private void ClientsHandler()
    {
      while (true)
      {
        var disconnectedClients = new List<string>();

        lock (m_clients)
        {
          foreach (var clientId in m_clients.Keys)
          {
            var client = m_clients[clientId];

            try
            {
              if (!client.Connected)
              {
                disconnectedClients.Add(clientId);
              }
              if (client.Available > 0)
              {
                ReceiveMessage(clientId);
              }
            }
            catch (Exception e)
            {
              Console.WriteLine("Client {0} Receive Error: {1}", clientId, e.Message);
            }
          }

          foreach (var clientId in disconnectedClients)
          {
            RemoveClient(clientId);
          }
        }
      }
    }

    private void RemoveClient(string clientId)
    {
      Console.WriteLine("Client {0} has disconnected...", clientId);
      var client = m_clients[clientId];
      m_clients.Remove(clientId);
      m_userNames.Remove(clientId);
      client.Close();
    }

    private void ReceiveMessage(string clientId)
    {
      var client = m_clients[clientId];
      var stream = client.GetStream();

      var numBytes = client.Available;
      var buffer = new byte[numBytes];
      var bytesRead = stream.Read(buffer, 0, numBytes);
      var request = System.Text.Encoding.ASCII.GetString(buffer).Substring(0, bytesRead);

      if (request.StartsWith("LOGIN:", StringComparison.OrdinalIgnoreCase))
      {
        var tokens = request.Split(':');
        m_userNames[clientId] = tokens[1];
        Console.WriteLine("Client {0} Login from {1}", m_userNames[clientId], clientId);
        return;
      }

      if (request.StartsWith("MESSAGE:", StringComparison.OrdinalIgnoreCase))
      {
        var tokens = request.Split(':');
        var message = tokens[1];
        Console.WriteLine("Text: {0} from {1}", message, m_userNames[clientId]);
        Broadcast(clientId, message);
      }
    }

    private void Broadcast(string senderId, string message)
    {
      var data = $"MESSAGE:{m_userNames[senderId]}:{message}";
      var buffer = System.Text.Encoding.ASCII.GetBytes(data);

      foreach (var clientId in m_clients.Keys)
      {
        if (clientId != senderId)
        {
          try
          {
            m_clients[clientId].GetStream().Write(buffer, 0, buffer.Length);
          }
          catch (Exception e)
          {
            Console.WriteLine("Client {0} Send Failed: {1}", clientId, e.Message);
          }
        }
      }
    }
  }
}

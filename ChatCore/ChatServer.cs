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
        }
      }
    }

    private void ClientsHandler()
    {
      while (true)
      {
        lock (m_clients)
        {
          foreach (var clientId in m_clients.Keys)
          {
            var client = m_clients[clientId];

            try
            {
              if (client.Available > 0)
              {
                ReceiveMessage(clientId);
              }
            }
            catch (Exception e)
            {
              Console.WriteLine("Client {0} Error: {1}", clientId, e.Message);
            }
          }
        }
      }
    }

    private void ReceiveMessage(string clientId)
    {
      var client = m_clients[clientId];
      var stream = client.GetStream();

      var numBytes = client.Available;
      var buffer = new byte[numBytes];
      var bytesRead = stream.Read(buffer, 0, numBytes);

      var request = System.Text.Encoding.ASCII.GetString(buffer).Substring(0, bytesRead);
      Console.WriteLine("Text: {0} from {1}", request, clientId);
    }
  }
}

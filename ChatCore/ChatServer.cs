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
    private readonly Dictionary<string, string> m_accounts = new Dictionary<string, string>();

    public ChatServer()
    {
      m_accounts.Add("arthur", "1111");
      m_accounts.Add("jojo", "1111");
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

    private void SendData(TcpClient client, string data)
    {
      var requestBuffer = System.Text.Encoding.ASCII.GetBytes(data);
      client.GetStream().Write(requestBuffer, 0, requestBuffer.Length);
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
        if (tokens.Length != 3)
        {
          Console.WriteLine("Client({0}) Login failed: parameters incorrect", clientId);
          SendData(client, "LOGIN:0");
          return;
        }

        var username = tokens[1];
        var password = tokens[2];

        if (!m_accounts.ContainsKey(username))
        {
          Console.WriteLine("Client({0}) {1} Login failed: unknown client", clientId, username);
          SendData(client, "LOGIN:0");
          return;
        }
        
        if (m_accounts[username] != password)
        {
          Console.WriteLine("Client({0}) {1} Login failed: password incorrect", clientId, username);
          SendData(client, "LOGIN:0");
          return;
        }
        
        m_userNames[clientId] = username;
        Console.WriteLine("Client({0}) {1} Login success", clientId, username);

        SendData(client, "LOGIN:1");
        return;
      }

      if (request.StartsWith("MESSAGE:", StringComparison.OrdinalIgnoreCase))
      {
        var tokens = request.Split(':');
        var message = tokens[1];

        if (!m_userNames.ContainsKey(clientId))
        {
          Console.WriteLine("Text: {0} from unauthenticated user", message);
        }
        else
        {
          Console.WriteLine("Text: {0} from {1}", message, m_userNames[clientId]);
        }
      }
    }
  }
}

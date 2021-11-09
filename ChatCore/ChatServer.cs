using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly Dictionary<string, Transmitter> m_transmitters = new Dictionary<string, Transmitter>();
    private readonly Dictionary<string, string> m_userNames = new Dictionary<string, string>();

    public ChatServer()
    {
    }

    public void Bind(int port)
    {
      m_port = port;

      m_listener = new TcpListener(IPAddress.Any, port);
      Console.WriteLine("Server start at port {0}", m_port);
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

        var transmitter = new Transmitter(clientId, client);
        transmitter.Register<LoginCommand>(OnLoginCommand);
        transmitter.Register<ExitCommand>(OnExitCommand);
        transmitter.Register<MessageCommand>(OnMessageCommand);

        lock (m_transmitters)
        {
          m_transmitters.Add(clientId, transmitter);
          m_userNames.Add(clientId, "Unknown");
        }
      }
      // ReSharper disable once FunctionNeverReturns
    }

    private void ClientsHandler()
    {
      while (true)
      {
        Dictionary<string, Transmitter> transmitters;
        lock (m_transmitters)
        {
          transmitters = new Dictionary<string, Transmitter>(m_transmitters);
        }

        foreach (var clientId in transmitters.Keys)
        {
          var transmitter = transmitters[clientId];
          var isDisconnected = false;

          if (transmitter.IsConnected() == false)
          {
            isDisconnected = true;
          }
          else
          {
            try
            {
              transmitter.Refresh();
            }
            catch (Exception e)
            {
              Console.WriteLine("Client {0} Refresh Error: {1}", clientId, e.Message);
              isDisconnected = true;
            }
          }

          if (isDisconnected)
          {
            lock (m_transmitters)
            {
              if (m_transmitters.Keys.Contains(clientId))
              {
                Console.WriteLine("Client {0} has disconnected...", clientId);
                m_transmitters.Remove(clientId);
                m_userNames.Remove(clientId);
                transmitter.Disconnect();
              }
            }
          }
        }
      }
    }

    public void OnLoginCommand(Transmitter transmitter, LoginCommand cmd)
    {
      m_userNames[transmitter.ClientID] = cmd.m_Name;
      Console.WriteLine("Client {0} Login from {1}",
        m_userNames[transmitter.ClientID], transmitter.ClientID);
    }

    public void OnExitCommand(Transmitter transmitter, ExitCommand cmd)
    {
      transmitter.Disconnect();

      Console.WriteLine("Client {0} leave", m_userNames[transmitter.ClientID]);

      lock (m_transmitters)
      {
        m_transmitters.Remove(transmitter.ClientID);
        m_userNames.Remove(transmitter.ClientID);
      }
    }

    public void OnMessageCommand(Transmitter sender, MessageCommand cmd)
    {
      Console.WriteLine(cmd.m_UserName + " say: " + cmd.m_Message);

      Broadcast(sender, cmd.m_Message);
    }

    private void Broadcast(Transmitter sender, string message)
    {
      var command = new MessageCommand
      {
        m_UserName = m_userNames[sender.ClientID],
        m_Message = message
      };

      Dictionary<string, Transmitter> transmitters;
      lock (m_transmitters)
      {
        transmitters = new Dictionary<string, Transmitter>(m_transmitters);
      }

      foreach (var transmitter in transmitters.Values)
      {
        if (transmitter != sender)
        {
          transmitter.Send(command);
        }
      }
    }
  }
}

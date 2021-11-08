using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatServer
{
  internal class Program
  {
    static HashSet<TcpClient> clients = new HashSet<TcpClient>();

    public static void Main(string[] args)
    {
      const int port = 4099;

      Console.WriteLine("====================================");
      var listener = new TcpListener(IPAddress.Any, port);

      var theThread = new Thread(HandleMessages);
      theThread.Start();

      try
      {
        Console.WriteLine("Server start at port {0}", port);
        listener.Start();

        while (true)
        {
          Console.WriteLine("Waiting for a connection... ");
          var client = listener.AcceptTcpClient();

          var address = client.Client.RemoteEndPoint.ToString();
          Console.WriteLine("Client has connected from {0}", address);

          lock (clients)
          {
            clients.Add(client);
          }
        }
      }
      catch (SocketException e)
      {
        Console.WriteLine("SocketException: {0}", e);
      }
      finally
      {
        // Stop listening for new clients.
        listener.Stop();
        Console.WriteLine("Server shutdown");
      }
    }

    private static void HandleMessages()
    {
      while (true)
      {
        lock (clients)
        {
          foreach (var client in clients)
          {
            try
            {
              if (client.Available > 0)
              {
                Receive(client);
              }
            }
            catch (Exception e)
            {
              Console.WriteLine("Error: {0}", e);
            }
          }
        }
      }
    }

    private static void Receive(TcpClient client)
    {
      var stream = client.GetStream();
      var address = client.Client.RemoteEndPoint.ToString();

      var numBytes = client.Available;
      if (numBytes == 0)
      {
        return;
      }

      var buffer = new byte[numBytes];
      var bytesRead = stream.Read(buffer, 0, numBytes);

      var request = System.Text.Encoding.ASCII.GetString(buffer).Substring(0, bytesRead);
      Console.WriteLine("Text: {0} from {1}", request, address);
    }
  }
}

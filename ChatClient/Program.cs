using System;
using System.Net.Sockets;

namespace ChatClient
{
  internal class Program
  {
    public static void Main(string[] args)
    {
      const string hostIP = "127.0.0.1";
      const int port = 4099;

      Console.WriteLine("====================================");
      var client = new TcpClient();

      try
      {
        Console.WriteLine("Connecting to chat server {0}:{1}", hostIP, port);
        client.Connect(hostIP, port);

        if (!client.Connected)
        {
          Console.WriteLine("Can't Connect to chat server");
          return;
        }

        Console.WriteLine("Connected to chat server");

        var counter = 0;
        while (counter < 5)
        {
          counter++;
          var msg = "go-go-go-" + counter;
          Send(client, msg);
          Console.WriteLine("message sent: " + msg);
          System.Threading.Thread.Sleep(1000);
        }
      }
      catch (ArgumentNullException e)
      {
        Console.WriteLine("ArgumentNullException: {0}", e);
      }
      catch (SocketException e)
      {
        Console.WriteLine("SocketException: {0}", e);
      }
      finally
      {
        client.Close();
        Console.WriteLine("Disconnected");
      }
    }

    private static void Send(TcpClient client, string msg)
    {
      var requestBuffer = System.Text.Encoding.ASCII.GetBytes(msg);

      client.GetStream().Write(requestBuffer, 0, requestBuffer.Length);
    }
  }
}

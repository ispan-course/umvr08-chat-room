using System;

namespace UDPServer
{
  internal class Program
  {
    public static void Main(string[] args)
    {
      Console.WriteLine("====================================");
      var server = new ChatServer();
      server.Bind(4099);
      server.Run();
    }
  }
}

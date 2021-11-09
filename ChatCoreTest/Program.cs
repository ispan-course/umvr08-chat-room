using System;
using System.Text;
using ChatCore;

namespace ChatCoreTest
{
  internal class Program
  {
    public static void Main(string[] args)
    {
      var loginCommand = new LoginCommand { m_Name = "Arthur" };
      var messageCommand = new MessageCommand { m_UserName = "JoJo", m_Message = "Hello!" };

      var length1 = SerializeCommand(loginCommand, out var buffer1);
      printBuffer(buffer1, length1);
      var command1 = UnserializeBuffer(buffer1);
      printCommand(command1);

      var length2 = SerializeCommand(messageCommand, out var buffer2);
      printBuffer(buffer2, length2);
      var command2 = UnserializeBuffer(buffer2);
      printCommand(command2);
    }

    private static int SerializeCommand(Command command, out byte[] buffer)
    {
      command.Serialize();
      buffer = command.SealPacketBuffer(out var length);

      return length;
    }

    private static Command UnserializeBuffer(byte[] buffer)
    {
      Command.FetchHeader(out var length, out var commandType, buffer, 0);
      Console.WriteLine("Command: {0}, Length: {1}", (Command.Type)commandType, length);

      Command command;

      switch (commandType)
      {
        case (int)Command.Type.LOGIN:
          command = new LoginCommand();
          break;
        case (int)Command.Type.MESSAGE:
          command = new MessageCommand();
          break;
        default:
          // invalid command type
          return null;
      }

      command.UnSealPacketBuffer(buffer, 0);
      command.Unserialize();

      return command;
    }

    private static void printBuffer(byte[] buffer, int length)
    {
      Console.Write($"Output Byte array(length:{length}): ");
      for (var i = 0; i < length; i++)
      {
        Console.Write(buffer[i] + ", ");
      }

      Console.WriteLine("");
    }

    private static void printCommand(Command command)
    {
      if (command == null)
      {
        return;
      }

      switch (command.CommandID)
      {
        case (int)Command.Type.LOGIN:
          Console.WriteLine("Login Name: {0}", ((LoginCommand)command).m_Name);
          break;
        case (int)Command.Type.MESSAGE:
          Console.WriteLine("{0} say: {1}", ((MessageCommand)command).m_UserName, ((MessageCommand)command).m_Message);
          break;
      }
    }
  }
}

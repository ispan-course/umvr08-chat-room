using System;

namespace ChatCore
{
  public class ChatClient
  {
    string m_UserName = "";
    private Transmitter m_Transmitter = new Transmitter();

    public event EventHandler<MessageCommand> OnReceiveMessage;
    
    public ChatClient()
    {
    }

    public bool Connect(string address, int port)
    {
      if (!m_Transmitter.Connect(address, port))
      {
        Console.WriteLine("Connect to server failed");
        return false;
      }

      m_Transmitter.Register<MessageCommand>(OnMessageCommand);

      var loginCommand = new LoginCommand
      {
        m_Name = m_UserName
      };
      m_Transmitter.Send(loginCommand);

      return true;
    }

    public void Disconnect()
    {
      m_Transmitter.Disconnect();
      Console.WriteLine("Disconnected");
    }

    public void Refresh()
    {
      m_Transmitter.Refresh();
    }

    public void SetName(string name)
    {
      m_UserName = name;
    }

    public void SendMessage(string message)
    {
      var command = new MessageCommand
      {
        m_UserName = m_UserName,
        m_Message = message
      };

      m_Transmitter.Send(command);
    }

    public void Bye()
    {
      var command = new ExitCommand();
      m_Transmitter.Send(command);
      m_Transmitter.Disconnect();
    }

    public void OnMessageCommand(Transmitter sender, MessageCommand command)
    {
      Console.WriteLine("{0}: {1}", command.m_UserName, command.m_Message);

      OnReceiveMessage?.Invoke(this, command);
    }
  }
}

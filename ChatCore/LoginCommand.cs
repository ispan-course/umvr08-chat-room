namespace ChatCore
{
  public class LoginCommand : Command
  {
    public string m_Name = "";

    public LoginCommand() : base((int)Type.LOGIN)
    {
    }

    public override void Serialize()
    {
      base.Serialize();

      // write command data
      _WriteToBuffer(m_Name);
    }

    public override void Unserialize()
    {
      base.Unserialize();

      // read command data
      _ReadFromBuffer(out m_Name);
    }
  }
}

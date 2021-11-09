namespace ChatCore
{
  public class MessageCommand : Command
  {
    public string m_UserName = "";
    public string m_Message = "";

    public MessageCommand() : base( (int)Type.MESSAGE )
    {
    }

    public override void Serialize()
    {
      base.Serialize();

      // write command data
      _WriteToBuffer( m_UserName );
      _WriteToBuffer( m_Message );
    }
    public override void Unserialize()
    {
      base.Unserialize();

      // read command data
      _ReadFromBuffer( out m_UserName );
      _ReadFromBuffer( out m_Message );
    }
  }
}

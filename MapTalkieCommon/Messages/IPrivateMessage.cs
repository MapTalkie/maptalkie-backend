namespace MapTalkieCommon.Messages
{
    public interface IPrivateMessage : IPrivateMessageBase
    {
        string Text { get; }
    }
}
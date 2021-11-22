namespace MapTalkieCommon.Messages
{
    public interface IPrivateMessageUpdate : IPrivateMessageBase
    {
        string NewText { get; }
    }
}
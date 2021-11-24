namespace MapTalkie.Common.Messages.PrivateMessages
{
    public interface IPrivateMessage : IPrivateMessageBase
    {
        string Text { get; }
    }
}
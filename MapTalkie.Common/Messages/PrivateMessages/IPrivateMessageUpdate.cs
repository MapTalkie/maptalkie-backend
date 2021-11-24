namespace MapTalkie.Common.Messages.PrivateMessages
{
    public interface IPrivateMessageUpdate : IPrivateMessageBase
    {
        string NewText { get; }
    }
}
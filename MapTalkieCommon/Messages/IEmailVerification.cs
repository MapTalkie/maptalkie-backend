namespace MapTalkieCommon.Messages
{
    public enum VerificationType
    {
        NewEmail,
        AccountCreated
    }

    public interface IEmailVerification
    {
        string Email { get; }
        string UserId { get; }
        string UserName { get; }
        VerificationType VerificationType { get; }
    }
}
namespace MapTalkie.Common.Messages.Email
{
    public interface IEmailVerification
    {
        string Email { get; }
        string UserId { get; }
        string UserName { get; }
        VerificationType VerificationType { get; }
    }

    public enum VerificationType
    {
        NewEmail,
        AccountCreated
    }
}
using System.Text.Json.Serialization;

namespace MapTalkie.Domain.Messages.User;

public record EmailVerification
{
    [JsonConstructor]
    public EmailVerification(string email, string userId, string userName)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
    }

    public string Email { get; }
    public string UserId { get; }
    public string UserName { get; }
}

public record InitialEmailVerification : EmailVerification
{
    public InitialEmailVerification(string email, string userId, string userName) : base(email, userId, userName)
    {
    }
}
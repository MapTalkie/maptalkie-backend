using MapTalkieCommon.Messages;

namespace MapTalkie.MessagesImpl
{
    public class EmailVerification : IEmailVerification
    {
        public string Email { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public VerificationType VerificationType { get; set; }
    }
}
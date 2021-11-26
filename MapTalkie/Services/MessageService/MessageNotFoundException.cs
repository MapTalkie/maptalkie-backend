namespace MapTalkie.Services.MessageService
{
    public class MessageNotFoundException : MessageServiceException
    {
        public MessageNotFoundException(long id) : base($"PrivateMessage with id={id} could'nt be found")
        {
        }
    }
}
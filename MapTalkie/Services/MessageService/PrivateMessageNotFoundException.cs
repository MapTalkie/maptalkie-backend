using System;

namespace MapTalkie.Services.MessageService
{
    public class PrivateMessageNotFoundException : Exception
    {
        public PrivateMessageNotFoundException(long id) : base($"PrivateMessage with id={id} could'nt be found")
        {
        }
    }
}
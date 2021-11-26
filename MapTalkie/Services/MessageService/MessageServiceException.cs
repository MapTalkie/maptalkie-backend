using System;
using System.Runtime.Serialization;

namespace MapTalkie.Services.MessageService
{
    [Serializable]
    public class MessageServiceException : Exception
    {
        public MessageServiceException()
        {
        }

        public MessageServiceException(string message) : base(message)
        {
        }

        public MessageServiceException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MessageServiceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
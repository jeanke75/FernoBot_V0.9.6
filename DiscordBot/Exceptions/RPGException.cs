using System;
using System.Runtime.Serialization;

namespace DiscordBot.Exceptions
{
    public class RPGException : Exception
    {
        public RPGException() { }
        public RPGException(string message) : base(message) { }
        public RPGException(string message, Exception inner) : base(message, inner) { }
        protected RPGException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

using System;
using System.Runtime.Serialization;

namespace Exemple.Domain.Models
{
    [Serializable]
    internal class InvalidClientRegistrationNameException : Exception
    {
        public InvalidClientRegistrationNameException()
        {
        }

        public InvalidClientRegistrationNameException(string? message) : base(message)
        {
        }

        public InvalidClientRegistrationNameException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidClientRegistrationNameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
using System;

namespace SecureNotebook.Services.Exceptions
{
    public class EntityValidationException : Exception
    {
        public EntityValidationException() : base() { }

        public EntityValidationException(string message) : base(message) { }
    }
}
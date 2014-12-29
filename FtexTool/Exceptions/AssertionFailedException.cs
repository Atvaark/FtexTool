using System;
using System.Runtime.Serialization;

namespace FtexTool.Exceptions
{
    [Serializable]
    public class AssertionFailedException : Exception
    {
        public AssertionFailedException()
        {
        }

        public AssertionFailedException(string message) : base(message)
        {
        }

        protected AssertionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AssertionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
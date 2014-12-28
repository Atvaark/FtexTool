using System;
using System.Runtime.Serialization;

namespace FtexTool.Exceptions
{
    [Serializable]
    public class AssertionFailedException : System.Exception
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

        public AssertionFailedException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}
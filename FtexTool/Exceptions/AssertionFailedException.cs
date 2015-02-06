using System;

namespace FtexTool.Exceptions
{
    [Serializable]
    public class AssertionFailedException : FtexToolException
    {
        public AssertionFailedException()
        {
        }

        public AssertionFailedException(string message) : base(message)
        {
        }

        public AssertionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

using System;

namespace FtexTool.Exceptions
{
    [Serializable]
    public abstract class FtexToolException : ApplicationException
    {
        protected FtexToolException()
        {
        }

        protected FtexToolException(string message) : base(message)
        {
        }

        protected FtexToolException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

using System;

namespace FtexTool.Exceptions
{
    [Serializable]
    public class MissingFtexsFileException : FtexToolException
    {
        public MissingFtexsFileException()
        {
        }

        public MissingFtexsFileException(string message) : base(message)
        {
        }

        public MissingFtexsFileException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

namespace FtexTool.Ftexs
{
    internal static class FtexsFileFactory
    {
        public static FtexsFile CreateFtexsFile(byte fileNumber)
        {
            FtexsFile result;
            if (fileNumber == 1)
            {
                // TODO: Create a different type here
                result = new FtexsFile();
            }
            else
            {
                result = new FtexsFile();
            }
            result.FileNumber = fileNumber;
            return result;
        }
    }
}
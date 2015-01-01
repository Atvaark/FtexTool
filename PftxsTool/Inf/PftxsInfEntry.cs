namespace PftxsTool.Inf
{
    public class PftxsInfEntry
    {
        public PftxsInfEntry()
        {
        }

        private PftxsInfEntry(string archiveName, string fileDirectory, string fileName, int subFileCount)
        {
            ArchiveName = archiveName;
            FileDirectory = fileDirectory;
            FileName = fileName;
            SubFileCount = subFileCount;
        }

        public string ArchiveName { get; set; }
        public string FileDirectory { get; set; }
        public string FileName { get; set; }
        public int SubFileCount { get; set; }

        public static PftxsInfEntry Create(string archiveName, string fileDirectory, string fileName, int subFileCount)
        {
            return new PftxsInfEntry(archiveName, fileDirectory, fileName, subFileCount);
        }
    }
}

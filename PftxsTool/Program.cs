using System;
using System.IO;
using System.Linq;
using PftxTool.Pftx;

namespace PftxTool
{
    internal class Program
    {
        private const string UsageInfo = "PftxsTool.exe path.pftxs|path.inf";

        private static void Main(string[] args)
        {
            if (args.Count() == 1)
            {
                string path = args[0];
                if (path.EndsWith(".pftxs"))
                {
                    UnpackPftxFile(path);
                    return;
                }
                if (path.EndsWith(".inf"))
                {
                    PackPftxFile(path);
                    return;
                }
            }
            Console.WriteLine(UsageInfo);
        }

        private static void PackPftxFile(string path)
        {
            throw new NotImplementedException();
        }

        private static void UnpackPftxFile(string path)
        {
            string archiveName = Path.GetFileNameWithoutExtension(path);
            string archiveDirectory = Path.GetDirectoryName(path);

            PftxsFile pftxsFile;
            using (FileStream input = new FileStream(path, FileMode.Open))
            {
                pftxsFile = PftxsFile.ReadPftxsFile(input);
            }

            string fileDirectory = "";
            foreach (var file in pftxsFile.Files)
            {
                var fileName = "";
                if (file.FileName.StartsWith("@"))
                {
                    fileName = file.FileName.Remove(0, 1);
                }
                else if (file.FileName.StartsWith("/"))
                {
                    fileDirectory = Path.GetDirectoryName(file.FileName.Remove(0, 1));
                    fileName = Path.GetFileName(file.FileName);
                }
                string outputDirectory = Path.Combine(archiveDirectory, archiveName, fileDirectory);
                string fullPath = Path.Combine(outputDirectory, fileName);
                Directory.CreateDirectory(outputDirectory);
                using (FileStream fileOutputStream = new FileStream(fullPath, FileMode.Create))
                {
                    fileOutputStream.Write(file.Data, 0, file.Data.Length);
                }
                int subFileNumber = 1;
                foreach (var subFileIndex in file.PsubFile.Indices)
                {
                    string subFilePath = String.Format("{0}.{1}", fullPath, subFileNumber);

                    using (FileStream subFileOutputStream = new FileStream(subFilePath, FileMode.Create))
                    {
                        subFileOutputStream.Write(subFileIndex.Data, 0, subFileIndex.Data.Length);
                    }
                    subFileNumber += 1;
                }
            }
        }
    }
}

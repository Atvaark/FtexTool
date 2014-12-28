using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FtexTool.Dds;
using FtexTool.Ftex;
using FtexTool.Ftexs;

namespace FtexTool
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                ShowUsageInfo();
                return;
            }

            string path = args[0];
            FileAttributes attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryInfo fileDirectory = new DirectoryInfo(path);
                var files = GetFileList(fileDirectory, true, ".ftex");
                foreach (var file in files)
                {
                    // TODO: Remove the try catch block when all files can be unpacked.
                    try
                    {
                        UnpackFtexFile(file.FullName);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error extracting: " + file.FullName + " " + e);
                    }
                }
            }
            else
            {
                if (path.EndsWith(".ftex"))
                {
                    UnpackFtexFile(path);
                    return;
                }
                if (path.EndsWith(".dds"))
                {
                    PackDdsFile(path);
                }
            }
            ShowUsageInfo();
        }

        private static void ShowUsageInfo()
        {
            Console.WriteLine("Usage:\r\nFtexTool.exe path");
        }

        private static void PackDdsFile(string filePath)
        {
            string fileDirectory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            DdsFile ddsFile = GetDdsFile(filePath);
            FtexFile ftexFile = FtexDdsConverter.ConvertToFtex(ddsFile);

            int ftexsFileNumber = 1;
            foreach (var ftexsFile in ftexFile.FtexsFiles)
            {
                string ftexsFileName = String.Format("{0}.{1}.ftexs", fileName, ftexsFileNumber);
                string ftexsFilePath = Path.Combine(fileDirectory, ftexsFileName);

                using (FileStream ftexsStream = new FileStream(ftexsFilePath, FileMode.Create))
                    ftexsFile.Write(ftexsStream);
                ++ftexsFileNumber;
            }

            // TODO: Calculate the offsets before saving.
            ftexFile.UpdateOffsets();

            string ftexFileName = String.Format("{0}.ftex", fileName);
            string ftexFilePath = Path.Combine(fileDirectory, ftexFileName);

            using (FileStream ftexStream = new FileStream(ftexFilePath, FileMode.Create))
                ftexFile.Write(ftexStream);
        }

        private static DdsFile GetDdsFile(string filePath)
        {
            DdsFile ddsFile;
            using (FileStream ddsStream = new FileStream(filePath, FileMode.Open))
                ddsFile = DdsFile.Read(ddsStream);
            return ddsFile;
        }

        private static void UnpackFtexFile(string filePath)
        {
            string fileDirectory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            FtexFile ftexFile = GetFtexFile(filePath);
            DdsFile ddsFile = FtexDdsConverter.ConvertToDds(ftexFile);

            LogFtexFileValues(filePath, ftexFile);

            string ddsFileName = String.Format("{0}.dds", fileName);
            string ddsFilePath = Path.Combine(fileDirectory, ddsFileName);

            using (FileStream outputStream = new FileStream(ddsFilePath, FileMode.Create))
                ddsFile.Write(outputStream);
        }

        private static FtexFile GetFtexFile(string filePath)
        {
            // TODO: Refactor this method. Too many brackets.
            string fileDirectory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);


            FtexFile ftexFile;
            using (FileStream ftexStream = new FileStream(filePath, FileMode.Open))
            {
                ftexFile = FtexFile.Read(ftexStream);
            }

            for (int i = 0; i < ftexFile.FtexsFileCount; i++)
            {
                ftexFile.FtexsFiles.Add(new FtexsFile());
            }

            for (int i = 0; i < ftexFile.MipMapCount; i++)
            {
                FtexFileMipMapInfo fileMipMapInfo = ftexFile.MipMapInfos[i];

                string ftexsName = String.Format("{0}.{1}.ftexs", fileName, fileMipMapInfo.FtexsFileNr);
                string ftexsFilePath = Path.Combine(fileDirectory, ftexsName);
                using (FileStream ftexsStream = new FileStream(ftexsFilePath, FileMode.Open))
                {
                    ftexsStream.Position = fileMipMapInfo.Offset;
                    FtexsFile ftexsFile = ftexFile.FtexsFiles[fileMipMapInfo.FtexsFileNr - 1];

                    if (fileMipMapInfo.FtexsFileNr == 1)
                    {
                        FtexsFile.Read(ftexsFile, ftexsStream, fileMipMapInfo.ChunkCount, false);
                    }
                    else
                    {
                        FtexsFile.Read(ftexsFile, ftexsStream, fileMipMapInfo.ChunkCount, true);
                    }
                }
            }
            return ftexFile;
        }


        private static List<FileInfo> GetFileList(DirectoryInfo fileDirectory, bool recursively, string extension)
        {
            List<FileInfo> files = new List<FileInfo>();
            if (recursively)
            {
                foreach (var directory in fileDirectory.GetDirectories())
                {
                    files.AddRange(GetFileList(directory, recursively, extension));
                }
            }
            files.AddRange(fileDirectory.GetFiles().Where(f => f.Extension == extension));
            return files;
        }

        private static void LogFtexFileValues(string filePath, FtexFile ftexFile)
        {
            using (FileStream log = new FileStream("E:\\loggw.csv", FileMode.Append))
            {
                StreamWriter sw = new StreamWriter(log, Encoding.Default);

                string s = "";
                s += filePath;
                s += ";";
                s += ftexFile.Height;
                s += ";";
                s += ftexFile.Width;
                s += ";";
                s += ftexFile.DtxType;
                s += ";";
                s += ftexFile.FtexsFileCount;
                s += ";";
                s += ftexFile.MipMapCount;
                s += ";";
                s += ftexFile.UnknownCount;
                s += ";";


                foreach (var ftexFileMipMap in ftexFile.MipMapInfos)
                {
                    s += ";";
                    s += ftexFileMipMap.FtexsFileNr;
                    s += ";";
                    s += ftexFileMipMap.FileSize1;
                    s += ";";
                    s += ftexFileMipMap.FileSize2;
                    s += ";";
                    s += ftexFileMipMap.ChunkCount;
                    s += ";";
                }
                sw.WriteLine(s);
                sw.Flush();
            }
        }
    }
}
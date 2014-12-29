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
                return;
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
                    return;
                }
            }
            ShowUsageInfo();
        }

        private static void ShowUsageInfo()
        {
            Console.WriteLine("FtexTool by Atvaark\n" +
                              "Description:\n" +
                              "  Converting between Fox Engine texture (.ftex) and DirectDraw Surface (.dds).\n" +
                              "Usage:\n" +
                              "  FtexTool.exe directory -Unpacks every ftex file in the directory\n" +
                              "  FtexTool.exe file.ftex -Unpacks a single ftex file\n" +
                              "  FtexTool.exe file.dds  -Packs a single dds file");
        }

        private static void PackDdsFile(string filePath)
        {
            string fileDirectory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            DdsFile ddsFile = GetDdsFile(filePath);
            FtexFile ftexFile = FtexDdsConverter.ConvertToFtex(ddsFile);

            foreach (var ftexsFile in ftexFile.FtexsFiles)
            {
                string ftexsFileName = String.Format("{0}.{1}.ftexs", fileName, ftexsFile.FileNumber);
                string ftexsFilePath = Path.Combine(fileDirectory, ftexsFileName);

                using (FileStream ftexsStream = new FileStream(ftexsFilePath, FileMode.Create))
                    ftexsFile.Write(ftexsStream);
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
                ftexFile = FtexFile.ReadFtexFile(ftexStream);
            }

            for (byte fileNumber = 1; fileNumber <= ftexFile.FtexsFileCount; fileNumber++)
            {
                FtexsFile ftexsFile = FtexsFileFactory.CreateFtexsFile(fileNumber);
                ftexFile.AddFtexsFile(ftexsFile);
            }

            foreach (var mipMapInfo in ftexFile.MipMapInfos)
            {
                string ftexsName = String.Format("{0}.{1}.ftexs", fileName, mipMapInfo.FtexsFileNumber);
                string ftexsFilePath = Path.Combine(fileDirectory, ftexsName);
                using (FileStream ftexsStream = new FileStream(ftexsFilePath, FileMode.Open))
                {
                    ftexsStream.Position = mipMapInfo.Offset;
                    FtexsFile ftexsFile;
                    ftexFile.TryGetFtexsFile(mipMapInfo.FtexsFileNumber, out ftexsFile);

                    if (mipMapInfo.FtexsFileNumber == 1)
                    {
                        ftexsFile.Read(ftexsStream, mipMapInfo.ChunkCount, false);
                    }
                    else
                    {
                        ftexsFile.Read(ftexsStream, mipMapInfo.ChunkCount, true);
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
    }
}
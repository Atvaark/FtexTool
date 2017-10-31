using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FtexTool.Dds;
using FtexTool.Exceptions;
using FtexTool.Ftex;
using FtexTool.Ftex.Enum;
using FtexTool.Ftexs;

namespace FtexTool
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            FtexToolArguments arguments = FtexToolArguments.Parse(args);
            if (arguments.Errors.Any())
            {
                foreach (var error in arguments.Errors)
                {
                    Console.WriteLine(error);
                }
                return;
            }
            if (arguments.DisplayHelp)
            {
                ShowUsageInfo();
                return;
            }

            if (arguments.DirectoryInput)
            {
                DirectoryInfo fileDirectory = new DirectoryInfo(arguments.InputPath);
                var files = GetFileList(fileDirectory, true, ".ftex");
                foreach (var file in files)
                {
                    try
                    {
                        UnpackFtexFile(file.FullName, arguments.OutputPath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error while unpacking '{0}': {1}", file.FullName, e.Message);
                    }
                }
            }
            else
            {
                if (arguments.InputPath.EndsWith(".ftex", StringComparison.OrdinalIgnoreCase))
                {
                    UnpackFtexFile(arguments.InputPath, arguments.OutputPath);
                }
                else if (arguments.InputPath.EndsWith(".dds", StringComparison.OrdinalIgnoreCase))
                {
                    PackDdsFile(
                        arguments.InputPath,
                        arguments.OutputPath,
                        arguments.TextureType,
                        arguments.UnknownFlags,
                        arguments.FtexsFileCount);
                }
                else
                {
                    Console.WriteLine("Input file is not ending in ftex or dds.");
                }
            }
        }

        private static void ShowUsageInfo()
        {
            Console.WriteLine("FtexTool by Atvaark\n" +
                              "Description:\n" +
                              "  Converting between Fox Engine texture (.ftex) and DirectDraw Surface (.dds).\n" +
                              "Usage:\n" +
                              "  FtexTool [options] input [output]\n" +
                              "Options:\n" +
                              "  -h|help  Displays the help message\n" +
                              "  -t|type  type_name\n" +
                              "           d|diffuse (default)\n" +
                              "           m|material\n" +
                              "           n|normal\n" +
                              "           c|cube\n" +
                              "  -fl|flags flag_name\n" +
                              "            Default (default)\n" +
                              "            Clp\n" +
                              "            Unknown\n" +
                              "  -f|ftexs number\n" +
                              "  -i|input file_name|folder_Name\n" +
                              "  -o|output folder_name\n" +
                              "Examples:\n" +
                              "  FtexTool folder        Unpacks every ftex file in the folder\n" +
                              "  FtexTool file.ftex     Unpacks an ftex file\n" +
                              "  FtexTool file.dds      Packs a dds file\n" +
                              "  FtexTool -t n file.dds Packs a dds file as a normal map\n");
        }

        private static void PackDdsFile(string filePath, string outputPath, FtexTextureType textureType, FtexUnknownFlags flags, int? ftexsFileCount)
        {
            string fileDirectory = String.IsNullOrEmpty(outputPath) ? Path.GetDirectoryName(filePath) ?? string.Empty : outputPath;
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            DdsFile ddsFile = GetDdsFile(filePath);
            FtexFile ftexFile = FtexDdsConverter.ConvertToFtex(
                ddsFile,
                textureType,
                flags,
                ftexsFileCount);
            
            string ftexFileName = $"{fileName}.ftex";
            string ftexFilePath = Path.Combine(fileDirectory, ftexFileName);

            using (FileStream ftexStream = new FileStream(ftexFilePath, FileMode.Create))
            {
                if (ftexsFileCount == 0)
                {
                    ftexStream.Seek(ftexFile.Size, SeekOrigin.Begin);
                }

                foreach (var ftexsFile in ftexFile.FtexsFiles)
                {
                    if (ftexsFileCount == 0)
                    {
                        ftexsFile.Write(ftexStream);
                    }
                    else
                    {
                        string ftexsFileName = $"{fileName}.{ftexsFile.FileNumber}.ftexs";
                        string ftexsFilePath = Path.Combine(fileDirectory, ftexsFileName);
                        using (FileStream ftexsStream = new FileStream(ftexsFilePath, FileMode.Create))
                        {
                            ftexsFile.Write(ftexsStream);
                        }
                    }

                }

                ftexFile.UpdateOffsets();
                ftexStream.Seek(0, SeekOrigin.Begin);
                ftexFile.Write(ftexStream);
            }
        }

        private static DdsFile GetDdsFile(string filePath)
        {
            DdsFile ddsFile;
            using (FileStream ddsStream = new FileStream(filePath, FileMode.Open))
            {
                ddsFile = DdsFile.Read(ddsStream);
            }
            return ddsFile;
        }

        private static void UnpackFtexFile(string filePath, string outputPath)
        {
            string fileDirectory = string.IsNullOrEmpty(outputPath) ? Path.GetDirectoryName(filePath) ?? string.Empty : outputPath;
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            FtexFile ftexFile = GetFtexFile(filePath);
            DdsFile ddsFile = FtexDdsConverter.ConvertToDds(ftexFile);

            string ddsFileName = $"{fileName}.dds";
            string ddsFilePath = Path.Combine(fileDirectory, ddsFileName);

            using (FileStream outputStream = new FileStream(ddsFilePath, FileMode.Create))
            {
                ddsFile.Write(outputStream);
            }
        }

        private static FtexFile GetFtexFile(string filePath)
        {
            string fileDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            
            FtexFile ftexFile;
            using (FileStream ftexStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                ftexFile = FtexFile.ReadFtexFile(ftexStream);
            }

            for (byte fileNumber = 0; fileNumber <= ftexFile.FtexsFileCount; fileNumber++)
            {
                FtexsFile ftexsFile = new FtexsFile
                {
                    FileNumber = fileNumber
                };
                ftexFile.AddFtexsFile(ftexsFile);
            }

            foreach (var mipMapInfo in ftexFile.MipMapInfos)
            {
                string ftexsName;
                ftexsName = mipMapInfo.FtexsFileNumber == 0
                    ? Path.GetFileName(filePath)
                    : $"{fileName}.{mipMapInfo.FtexsFileNumber}.ftexs";

                string ftexsFilePath = Path.Combine(fileDirectory, ftexsName);
                FtexsFile ftexsFile;
                if (ftexFile.TryGetFtexsFile(mipMapInfo.FtexsFileNumber, out ftexsFile)
                    && File.Exists(ftexsFilePath))
                {
                    using (FileStream ftexsStream = new FileStream(ftexsFilePath, FileMode.Open, FileAccess.Read))
                    {
                        ftexsStream.Position = mipMapInfo.Offset;
                        ftexsFile.Read(
                            ftexsStream,
                            mipMapInfo.ChunkCount,
                            mipMapInfo.Offset,
                            mipMapInfo.DecompressedFileSize);
                    }
                }
                else
                {
                    throw new MissingFtexsFileException($"{ftexsName} not found");
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
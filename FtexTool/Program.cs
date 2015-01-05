using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FtexTool.Dds;
using FtexTool.Ftex;
using FtexTool.Ftex.Enum;
using FtexTool.Ftexs;

namespace FtexTool
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            FtexToolArguments arguments = ParseArguments(args);
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
                        Console.WriteLine("Error while unpacking {0}", file.FullName);
                    }
                }
            }
            else
            {
                if (arguments.InputPath.EndsWith(".ftex"))
                {
                    UnpackFtexFile(arguments.InputPath, arguments.OutputPath);
                }
                else if (arguments.InputPath.EndsWith(".dds"))
                {
                    PackDdsFile(arguments.InputPath, arguments.OutputPath, arguments.TextureType);

                }
                else
                {
                    Console.WriteLine("Input file is not ending in ftex or dds.");
                }
            }
        }


        private static FtexToolArguments ParseArguments(string[] args)
        {
            FtexToolArguments arguments = new FtexToolArguments
            {
                DisplayHelp = false,
                TextureType = FtexTextureType.DiffuseMap,
                InputPath = "",
                OutputPath = ""
            };
            if (args.Length == 0)
            {
                arguments.DisplayHelp = true;
                return arguments;
            }

            bool expectType = false;
            bool expectInput = false;
            bool expectOutput = false;

            int argIndex = 0;
            while(argIndex < args.Length)
            {
                string arg = args[argIndex];
                argIndex++;
                if (expectType)
                {
                    arguments.ReadType(arg);
                    expectType = false;
                }
                else if (expectInput)
                {
                    arguments.ReadInput(arg);
                    expectInput = false;
                }
                else if (expectOutput)
                {
                    arguments.ReadOutput(arg);
                    expectOutput = false;
                }
                else if (arg.StartsWith("-"))
                {
                    switch (arg)
                    {
                        case "-h":
                        case "-help":
                            arguments.DisplayHelp = true;
                            break;
                        case "-t":
                        case "-type":
                            expectType = true;
                            break;
                        case "-i":
                        case "-input":
                            expectInput = true;
                            break;
                        case "-o":
                        case "-output":
                            expectOutput = true;
                            break;
                        default:
                            arguments.Errors.Add("Unknown option" );
                            break;
                    }
                }
                else
                {
                    expectInput = true;
                    expectOutput = true;
                    argIndex--;
                }
            }
            return arguments;
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
                              "  -i|input file_name|folder_Name\n" +
                              "  -o|output folder_name\n" + 
                              "Examples:\n" +
                              "  FtexTool folder        Unpacks every ftex file in the folder\n" +
                              "  FtexTool file.ftex     Unpacks an ftex file\n" +
                              "  FtexTool file.dds      Packs a dds file\n" +
                              "  FtexTool -t n file.dds Packs a dds file as a normal map\n");
        }

        private static void PackDdsFile(string filePath, string outputPath, FtexTextureType textureType)
        {
            string fileDirectory = String.IsNullOrEmpty(outputPath) ? Path.GetDirectoryName(filePath) : outputPath;
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            DdsFile ddsFile = GetDdsFile(filePath);
            FtexFile ftexFile = FtexDdsConverter.ConvertToFtex(ddsFile, textureType);

            foreach (var ftexsFile in ftexFile.FtexsFiles)
            {
                string ftexsFileName = String.Format("{0}.{1}.ftexs", fileName, ftexsFile.FileNumber);
                string ftexsFilePath = Path.Combine(fileDirectory, ftexsFileName);

                using (FileStream ftexsStream = new FileStream(ftexsFilePath, FileMode.Create))
                    ftexsFile.Write(ftexsStream);
            }

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

        private static void UnpackFtexFile(string filePath, string outputPath)
        {
            string fileDirectory = String.IsNullOrEmpty(outputPath) ? Path.GetDirectoryName(filePath) : outputPath;
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
            string fileDirectory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);


            FtexFile ftexFile;
            using (FileStream ftexStream = new FileStream(filePath, FileMode.Open))
            {
                ftexFile = FtexFile.ReadFtexFile(ftexStream);
            }

            for (byte fileNumber = 1; fileNumber <= ftexFile.FtexsFileCount; fileNumber++)
            {
                FtexsFile ftexsFile = new FtexsFile
                {
                    FileNumber = fileNumber
                };
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
                    ftexsFile.Read(ftexsStream, mipMapInfo.ChunkCount);
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

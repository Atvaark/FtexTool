using System;
using System.Collections.Generic;
using System.IO;
using FtexTool.Ftex.Enum;

namespace FtexTool
{
    internal class FtexToolArguments
    {
        private readonly List<string> _errors;

        public FtexToolArguments()
        {
            _errors = new List<string>();
        }

        public bool DisplayHelp { get; set; }
        public FtexTextureType TextureType { get; set; }
        public string InputPath { get; set; }
        public bool DirectoryInput { get; set; }
        public string OutputPath { get; set; }
        public int? FtexsFileCount { get; set; }

        public List<string> Errors
        {
            get { return _errors; }
        }

        public void ReadType(string type)
        {
            switch (type)
            {
                case "d":
                case "diffuse":
                    TextureType = FtexTextureType.DiffuseMap;
                    break;
                case "m":
                case "material":
                    TextureType = FtexTextureType.MaterialMap;
                    break;
                case "n":
                case "normal":
                    TextureType = FtexTextureType.NormalMap;
                    break;
                case "c":
                case "cube":
                    TextureType = FtexTextureType.CubeMap;
                    break;
                default:
                    Errors.Add(string.Format("{0} is not a valid texture type.", type));
                    break;
            }
        }

        public void ReadInput(string inputPath)
        {
            InputPath = inputPath;
            try
            {
                FileAttributes attributes = File.GetAttributes(inputPath);
                DirectoryInput = (attributes & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch (Exception)
            {
                Errors.Add(string.Format("File/Directory {0} not found", inputPath));
            }
        }

        public void ReadOutput(string outputPath)
        {
            OutputPath = outputPath;
        }

        public void ReadFtexsCount(string ftexsFileCount)
        {
            int count;
            if (int.TryParse(ftexsFileCount, out count) && count > 0)
            {
                FtexsFileCount = count;
            }
            else
            {
                Errors.Add(string.Format("Invalid ftexs file count {0}", ftexsFileCount));
            }
        }
    }
}

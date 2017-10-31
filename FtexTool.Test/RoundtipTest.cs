using System;
using System.Collections.Generic;
using System.IO;
using FtexTool.Dds;
using FtexTool.Dds.Enum;
using Xunit;
using Xunit.Abstractions;

namespace FtexTool.Test
{
    public class RoundtipTest : IClassFixture<RoundtipTest.DdsRoundtipFixture>
    {
        public class DdsRoundtipFixture : IDisposable
        {
            public string TempDdsFileDirectory { get; }

            public DdsRoundtipFixture()
            {
                TempDdsFileDirectory = Path.Combine(Path.GetTempPath(), "FtexTool");
                Directory.CreateDirectory(TempDdsFileDirectory);
            }

            public void Dispose()
            {
                Directory.Delete(TempDdsFileDirectory, true);
            }
        }

        public class DdsTestCase
        {
            public DdsTestCase(DdsOptions options, string description)
            {
                Options = options;
                Description = description;
            }

            public DdsFile File { get; set; }

            public DdsOptions Options { get; }

            public string Description { get; }

            public byte[] Buffer { get; set; }

            public string DdsPath { get; set; }

            public string FtexPath { get; set; }

            public override string ToString()
            {
                return Description;
            }
        }

        public class DdsOptions
        {
            public DdsOptions(DdsPixelFormat pixelFormat, short width, short height, short depth, short mipMapCount)
            {
                PixelFormat = pixelFormat;
                Width = width;
                Height = height;
                Depth = depth;
                MipMapCount = mipMapCount;
            }

            public DdsOptions(DdsPixelFormat pixelFormat, short width, short height, short mipMapCount)
                : this(pixelFormat, width, height, depth: 0, mipMapCount: mipMapCount)
            {
            }

            public DdsPixelFormat PixelFormat { get; }
            public short Width { get; }
            public short Height { get; }
            public short Depth { get; }
            public short MipMapCount { get; }
        }

        private readonly DdsRoundtipFixture _fixture;
        private readonly ITestOutputHelper _output;

        public RoundtipTest(DdsRoundtipFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        private static DdsFile GenerateDds(DdsOptions options)
        {
            DdsFile ddsFile = new DdsFile();
            ddsFile.Header = new DdsFileHeader
            {
                Size = DdsFileHeader.DefaultHeaderSize,
                Flags = DdsFileHeaderFlags.Texture,
                Width = options.Width,
                Height = options.Height,
                Depth = options.Depth,
                MipMapCount = options.MipMapCount,
                Caps = DdsSurfaceFlags.Texture,
                PixelFormat = options.PixelFormat
            };

            if (ddsFile.Header.Depth > 1)
            {
                ddsFile.Header.Flags |= DdsFileHeaderFlags.Volume;
            }

            int mipMapCount = ddsFile.Header.MipMapCount;
            if (ddsFile.Header.MipMapCount == 1)
            {
                ddsFile.Header.MipMapCount = 0;
            }
            else if (ddsFile.Header.MipMapCount > 1)
            {
                ddsFile.Header.Flags |= DdsFileHeaderFlags.MipMap;
                ddsFile.Header.Caps |= DdsSurfaceFlags.MipMap;
            }

            const int minimumWidth = 4;
            const int minimumHeight = 4;
            int mipMapHeight = ddsFile.Header.Height;
            int mipMapWidth = ddsFile.Header.Width;
            int mipMapDepth = ddsFile.Header.Depth == 0 ? 1 : ddsFile.Header.Depth;
            int mipMapBufferSize = 0;
            for (int i = 0; i < mipMapCount; i++)
            {
                int mipMapSize = DdsPixelFormat.CalculateImageSize(ddsFile.Header.PixelFormat, mipMapWidth, mipMapHeight, mipMapDepth);
                mipMapBufferSize += mipMapSize;
                mipMapWidth = Math.Max(mipMapWidth / 2, minimumWidth);
                mipMapHeight = Math.Max(mipMapHeight / 2, minimumHeight);
            }

            ddsFile.Data = new byte[mipMapBufferSize];
            for (int i = 0; i < mipMapBufferSize; i++)
            {
                ddsFile.Data[i] = (byte)(i % 0xFF);
            }


            return ddsFile;
        }

        private static void SetupTestCase(DdsTestCase testCase, string tempDdsFileDirectory)
        {
            var ddsFileMemoryStream = new MemoryStream();
            testCase.File = GenerateDds(testCase.Options);
            testCase.File.Write(ddsFileMemoryStream);
            byte[] ddsFileBuffer = ddsFileMemoryStream.ToArray();

            string ddsFilePath = Path.Combine(
                tempDdsFileDirectory,
                Guid.NewGuid() + ".dds");
            File.WriteAllBytes(ddsFilePath, ddsFileBuffer);

            testCase.Buffer = ddsFileBuffer;
            testCase.DdsPath = ddsFilePath;
            testCase.FtexPath = Path.ChangeExtension(ddsFilePath, "ftex");
        }

        private void RunDdsTestCase(DdsTestCase[] testCases)
        {
            foreach (var testCase in testCases)
            {
                TestDdsFtexRoundtip(testCase);
            }
        }

        [Theory]
        [MemberData(nameof(AllUsedCombinations))]
        public void TestDdsFtexRoundtip(DdsTestCase testCase)
        {
            _output.WriteLine("Test '{0}' running", testCase.Description);

            // Setup
            SetupTestCase(testCase, _fixture.TempDdsFileDirectory);

            // Convert the DDS to FTEX
            Program.Main(new[] { testCase.DdsPath });
            // Convert the FTEX back to DDS
            Program.Main(new[] { testCase.FtexPath });

            // Assert
            byte[] expectedBuffer = testCase.Buffer;
            byte[] actualBuffer = File.ReadAllBytes(testCase.DdsPath);

            ////for (int i = 0; i < expectedBuffer.Length; i++)
            ////{
            ////    byte expected = expectedBuffer[i];
            ////    byte actual = actualBuffer[i];
            ////    Assert.Equal(expected, actual);
            ////}

            Assert.Equal(expectedBuffer.Length, actualBuffer.Length);
            Assert.Equal(expectedBuffer, actualBuffer);

            testCase.File = null;
            testCase.Buffer = null;

            _output.WriteLine("Test '{0}' OK", testCase.Description);
        }

        /// <summary>
        /// The format/width/height/mip combinations used by the game.
        /// </summary>
        public static IEnumerable<object[]> AllUsedCombinations
        {
            get
            {
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 8, 8, 2), "A8R8G8B8 8x8 (2 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 16, 16, 1), "A8R8G8B8 16x16 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 128, 128, 1), "A8R8G8B8 128x128 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 256, 2, 1), "A8R8G8B8 256x2 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 16, 16, 16, 1), "A8R8G8B8 16x16x16 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 512, 512, 1), "Luminance 512x512 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 128, 64, 6), "dxt1 128x64 (6 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 128, 128, 6), "dxt1 128x128 (6 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 128, 256, 7), "dxt1 128x256 (7 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 128, 512, 8), "dxt1 128x512 (8 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 256, 64, 7), "dxt1 256x64 (7 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 256, 128, 7), "dxt1 256x128 (7 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 256, 256, 1), "dxt1 256x256 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 256, 256, 7), "dxt1 256x256 (7 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 256, 512, 8), "dxt1 256x512 (8 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 256, 1024, 9), "dxt1 256x1024 (9 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 512, 128, 1), "dxt1 512x128 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 512, 128, 8), "dxt1 512x128 (8 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 512, 256, 8), "dxt1 512x256 (8 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 512, 512, 8), "dxt1 512x512 (8 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 512, 1024, 9), "dxt1 512x1024 (9 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 512, 2048, 10), "dxt1 512x2048 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 256, 9), "dxt1 1024x256 (9 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 512, 9), "dxt1 1024x512 (9 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 1024, 1), "dxt1 1024x1024 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 1024, 9), "dxt1 1024x1024 (9 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 2048, 10), "dxt1 1024x2048 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 2048, 512, 1), "dxt1 2048x512 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 2048, 512, 10), "dxt1 2048x512 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 2048, 1024, 1), "dxt1 2048x1024 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 2048, 1024, 10), "dxt1 2048x1024 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 2048, 2048, 10), "dxt1 2048x2048 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 4096, 2048, 11), "dxt1 4096x2048 (11 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 4096, 4096, 11), "dxt1 4096x4096 (11 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 8192, 8192, 12), "dxt1 8192x8192 (12 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 32, 32, 4), "dxt5 32x32 (4 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 64, 64, 1), "dxt5 64x64 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 64, 64, 5), "dxt5 64x64 (5 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 64, 256, 7), "dxt5 64x256 (7 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 256, 64, 7), "dxt5 256x64 (7 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 256, 128, 7), "dxt5 256x128 (7 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 256, 256, 1), "dxt5 256x256 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 256, 256, 7), "dxt5 256x256 (7 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 256, 512, 8), "dxt5 256x512 (8 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 256, 1024, 1), "dxt5 256x1024 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 256, 1024, 9), "dxt5 256x1024 (9 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 512, 64, 8), "dxt5 512x64 (8 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 512, 128, 8), "dxt5 512x128 (8 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 512, 256, 8), "dxt5 512x256 (8 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 512, 512, 8), "dxt5 512x512 (8 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 512, 1024, 9), "dxt5 512x1024 (9 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 512, 2048, 10), "dxt5 512x2048 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 256, 9), "dxt5 1024x256 (9 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 512, 9), "dxt5 1024x512 (9 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 1024, 1), "dxt5 1024x1024 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 1024, 9), "dxt5 1024x1024 (9 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 2048, 10), "dxt5 1024x2048 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 4096, 11), "dxt5 1024x4096 (11 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 2048, 256, 10), "dxt5 2048x256 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 2048, 512, 1), "dxt5 2048x512 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 2048, 512, 10), "dxt5 2048x512 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 2048, 1024, 1), "dxt5 2048x1024 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 2048, 1024, 10), "dxt5 2048x1024 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 2048, 2048, 1), "dxt5 2048x2048 (1 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 2048, 2048, 10), "dxt5 2048x2048 (10 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 4096, 2048, 11), "dxt5 4096x2048 (11 mip)") };
                yield return new object[] { new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 4096, 4096, 11), "dxt5 4096x4096 (11 mip)") };
            }
        }

        [Fact(Skip = "Use the data driven test instead.")]
        public void TestDxt1()
        {
            DdsTestCase[] testCases = {
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 1024, 1), "dxt1 1 mip"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 1024, 2), "dxt1 2 mip"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 1024, 5), "dxt1 5 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 1024, 10), "dxt1 10 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 1024, 15), "dxt1 15 mip" ),

                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1, 1, 1), "dxt1 1x1"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 2, 2, 2), "dxt1 2x2"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 4, 4, 3), "dxt1 4x4"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 8, 8, 4), "dxt1 8x8"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 16, 16, 5), "dxt1 16x16"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 32, 32, 6), "dxt1 32x32"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 64, 64, 7), "dxt1 64x63"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 128, 128, 8), "dxt1 128x128"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 256, 256, 9), "dxt1 256x256"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 512, 512, 10), "dxt1 512x512"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 1024, 1024, 11), "dxt1 1024x1024"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 2048, 2048, 12), "dxt1 2048x2048"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 4096, 4096, 13), "dxt1 4096x4096"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt1(), 8192, 8192, 14), "dxt1 8192x8192"),
            };

            RunDdsTestCase(testCases);
        }

        [Fact(Skip = "Use the data driven test instead.")]
        public void TestDxt5()
        {
            DdsTestCase[] testCases = {
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 1024, 1), "dxt5 1 mip"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 1024, 2), "dxt5 2 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 1024, 5), "dxt5 5 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 1024, 10), "dxt5 10 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 1024, 15), "dxt5 15 mip" ),

                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1, 1, 1), "dxt1 1x1"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 2, 2, 2), "dxt1 2x2"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 4, 4, 3), "dxt1 4x4"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 8, 8, 4), "dxt1 8x8"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 16, 16, 5), "dxt1 16x16"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 32, 32, 6), "dxt1 32x32"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 64, 64, 7), "dxt1 64x63"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 128, 128, 8), "dxt1 128x128"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 256, 256, 9), "dxt1 256x256"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 512, 512, 10), "dxt1 512x512"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 1024, 1024, 11), "dxt1 1024x1024"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 2048, 2048, 12), "dxt1 2048x2048"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 4096, 4096, 13), "dxt1 4096x4096"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfDxt5(), 8192, 8192, 14), "dxt1 8192x8192"),
            };

            RunDdsTestCase(testCases);
        }

        [Fact(Skip = "Use TestAllUsedCombinations instead.")]
        public void TestPfA8R8G8B8()
        {
            DdsTestCase[] testCases = {
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 1024, 1024, 1), "A8R8G8B8 1 mip"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 1024, 1024, 2), "A8R8G8B8 2 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 1024, 1024, 5), "A8R8G8B8 5 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 1024, 1024, 10), "A8R8G8B8 10 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 1024, 1024, 15), "A8R8G8B8 15 mip" ),

                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 1, 1, 1), "A8R8G8B8 1x1"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 2, 2, 2), "A8R8G8B8 2x2"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 4, 4, 3), "A8R8G8B8 4x4"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 8, 8, 4), "A8R8G8B8 8x8"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 16, 16, 5), "A8R8G8B8 16x16"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 32, 32, 6), "A8R8G8B8 32x32"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 64, 64, 7), "A8R8G8B8 64x63"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 128, 128, 8), "A8R8G8B8 128x128"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 256, 256, 9), "A8R8G8B8 256x256"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 512, 512, 10), "A8R8G8B8 512x512"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 1024, 1024, 11), "A8R8G8B8 1024x1024"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 2048, 2048, 12), "A8R8G8B8 2048x2048"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 4096, 4096, 13), "A8R8G8B8 4096x4096"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsPfA8R8G8B8(), 8192, 8192, 14), "A8R8G8B8 8192x8192")
            };

            RunDdsTestCase(testCases);
        }

        [Fact(Skip = "Use the data driven test instead.")]
        public void TestLuminance()
        {
            DdsTestCase[] testCases = {
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 1024, 1024, 1), "Luminance 1 mip"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 1024, 1024, 2), "Luminance 2 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 1024, 1024, 5), "Luminance 5 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 1024, 1024, 10), "Luminance 10 mip" ),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 1024, 1024, 15), "Luminance 15 mip" ),

                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 1, 1, 1), "Luminance 1x1"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 2, 2, 2), "Luminance 2x2"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 4, 4, 3), "Luminance 4x4"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 8, 8, 4), "Luminance 8x8"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 16, 16, 5), "Luminance 16x16"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 32, 32, 6), "Luminance 32x32"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 64, 64, 7), "Luminance 64x63"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 128, 128, 8), "Luminance 128x128"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 256, 256, 9), "Luminance 256x256"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 512, 512, 10), "Luminance 512x512"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 1024, 1024, 11), "Luminance 1024x1024"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 2048, 2048, 12), "Luminance 2048x2048"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 4096, 4096, 13), "Luminance 4096x4096"),
                new DdsTestCase(new DdsOptions(DdsPixelFormat.DdsLuminance(), 8192, 8192, 14), "Luminance 8192x8192")
            };

            RunDdsTestCase(testCases);
        }
    }
}

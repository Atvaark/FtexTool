using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace FtexTool
{
    internal static class ZipUtility
    {
        internal static byte[] Inflate(byte[] buffer)
        {
            InflaterInputStream inflaterStream = new InflaterInputStream(new MemoryStream(buffer));
            MemoryStream outputStream = new MemoryStream();
            inflaterStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }

        internal static byte[] Deflate(byte[] buffer)
        {
            DeflaterOutputStream deflaterStream = new DeflaterOutputStream(new MemoryStream(buffer));
            MemoryStream outputStream = new MemoryStream();
            deflaterStream.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }
}
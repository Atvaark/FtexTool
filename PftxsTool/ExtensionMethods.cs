using System;
using System.IO;
using System.Linq;
using System.Text;

namespace PftxTool
{
    internal static class ExtensionMethods
    {
        internal static string ReadNullTerminatedString(this BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();
            char nextCharacter;
            while ((nextCharacter = reader.ReadChar()) != 0)
            {
                builder.Append(nextCharacter);
            }
            return builder.ToString();
        }

        internal static void WriteNullTerminatedString(this BinaryWriter writer, string text)
        {
            byte[] data = Encoding.Default.GetBytes(text + '\0') ;
            writer.Write(data, 0, data.Length);
        }


        internal static void AlignRead(this Stream input, int alignment)
        {
            long alignmentRequired = input.Position%alignment;
            if (alignmentRequired > 0)
                input.Position += alignmentRequired;
        }

        internal static void AlignWrite(this Stream output, int alignment, byte data)
        {
            long alignmentRequired = output.Position % alignment;
            if (alignmentRequired > 0)
            {
                byte[] alignmentBytes = Enumerable.Repeat(data, (int) (alignment - alignmentRequired)).ToArray();
                output.Write(alignmentBytes, 0, alignmentBytes.Length);
                
            }
        }

    }
}

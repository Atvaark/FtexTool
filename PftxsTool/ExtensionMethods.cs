using System.IO;
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

        internal static void AlignRead(this Stream input, int alignment)
        {
            long alignmentRequired = input.Position%alignment;
            if (alignmentRequired > 0)
                input.Position += alignmentRequired;
        }
    }
}

using System;

namespace FtexTool.Dds.Enum
{
    [Flags]
    public enum DdsPixelFormatFlag : uint
    {
        Alpha = 0x00000002, // DDPF_ALPHA
        FourCc = 0x00000004, // DDPF_FOURCC
        Rgb = 0x00000040, // DDPF_RGB
        Rgba = 0x00000041, // DDPF_RGB | DDPF_ALPHAPIXELS
        Luminance = 0x00020000, // DDPF_LUMINANCE
        Normal = 0x80000000 // Nvidia custom DDPF_NORMA
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtexTool.Ftex.Enum
{
    public enum FtexTextureType
    {
        /// <summary>
        // SRM and MTM files
        // Each channel acts as a material parameter
        //      R = Ambient Occlusion
        //      G = Specular Albedo
        //      B = Roughness
        /// </summary>
        MaterialMap = 0x01000001,
        /// <summary>
        /// BSM files
        /// Albedo diffuse texture
        /// </summary>
        DiffuseMap = 0x01000003,
        /// <summary>
        /// CBM files
        /// CubeMap
        /// </summary>
        CubeMap = 0x01000007,
        /// <summary>
        /// NRM files
        /// NormalMap
        /// </summary>
        NormalMap = 0x01000009,

    }
}

FtexTool
========
[![Build status](https://ci.appveyor.com/api/projects/status/fyes86rw8kb6xcj5?svg=true)](https://ci.appveyor.com/project/Atvaark/ftextool)

A tool for converting files between Fox Engine texture (.ftex) and DirectDraw Surface (.dds).

Requirements
--------
```
Microsoft .NET Framework 4.5 
SharpZipLib
```

Usage
--------
```
FtexTool [options] input [output_folder]
```

Options:
--------
```
  -h|help  Displays the help message
  -t|type  type_name
           d|diffuse (default)
           m|material
           n|normal
           c|cube
  -f|ftexs positive_number *Max ftexs file count* 
  -i|input file_name|folder_Name
  -o|output folder_name
```

Examples
--------

Converting an .ftex file to .dds:
```
FtexTool file_name.ftex
```

Converting all .ftex files in a folder to .dds:
```
FtexTool folder_name
```

Converting a .dds file to .ftex:
```
FtexTool file_name.dds
```

Converting a .dds file to an .ftex and 3 .ftexs files:
```
FtexTool -f 3 file_name.dds
```

Converting a .dds normalmap file to .ftex
```
FtexTool -t n file_name.dds
```

Important
--------
* When repacking custom textures remember to also repack all .pftxs files that contain the changed files with the [GzsTool](https://github.com/Atvaark/GzsTool)
* Fox Engine uses the pixel format DXT1 for textures without alpha channel and DXT5 for textures with alpha channel. Changing the pixel format could lead to unexpected results.

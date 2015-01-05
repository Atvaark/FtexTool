FtexTool
========
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
```
  -h|help  Displays the help message
  -t|type  type_name
           d|diffuse (default)
           m|material
           n|normal
           c|cube
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

Converting a .dds normalmap file to .ftex
```
FtexTool -t n file_name.dds
```

Important
--------
When repacking custom textures remember to also repack all .pftxs files that contain the changed files.

PftxsTool
========
A tool for unpacking and repacking Fox Engine texture pack (.pftxs) file

Requirements
--------
```
Microsoft .NET Framework 4.5 
```
Usage
--------

Unpacking all Fox Engine texure files (.ftex) and Fox Engine sub texture files (.ftexs) in a .pftxs file.
This will create the file 'filename_pftxs.inf' and the output folder 'filename'.
```
PftxsTool.exe filename.pftxs
```

Repacking all Fox Engine texure files (.ftex) and Fox Engine sub texture files (.ftexs) 
```
PftxsTool.exe filename_pftxs.inf
```

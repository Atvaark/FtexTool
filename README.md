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

Converting a .ftex file to .dds:
```
FtexTool.exe filename.ftex
```

Converting all .ftex files in a directory to .dds:
```
FtexTool.exe directoryname
```

Converting a .dds file to .ftex:
```
FtexTool.exe filename.dds
```

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

All source codes in this directory are written to be read by flat assembler. Therefore, the files can be assembled using the following command line:

fasm.exe file.asm

The source files should opt for a 32-bit binary file. Therefore every file should start with the following line of code:

use32

As said before, all output files should be in binary format (*.bin). This takes away the need of calculating or finding the exact address of the user code in a hex-editor or debugger.
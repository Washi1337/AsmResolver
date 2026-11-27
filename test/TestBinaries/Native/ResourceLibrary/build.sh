#!/bin/bash
# Build ResourceLibrary.dll - a native PE containing all Win32 resource types.
# Requires: mingw-w64-binutils, mingw-w64-gcc, ImageMagick (for test.bmp)
#
# Source files:
#   test.bmp       - 16x16 red bitmap (create with: magick -size 16x16 xc:red BMP3:test.bmp)
#   test.ani       - Real animated cursor (RIFF/ACON format)
#   test.fnt       - Real Windows FNT v3.00 raster font (extracted from Wine's courer.fon)
#   test.html      - Simple HTML document
#   test.mc        - Message compiler source
#   resource.h     - Header file for RT_DLGINCLUDE
#   resources.rc   - Main resource script
#
# Usage: ./build.sh [--install]
# Output: ResourceLibrary.dll

set -e
cd "$(dirname "$0")"

# Generate test bitmap if missing
if [ ! -f test.bmp ]; then
    echo "Generating test.bmp..."
    magick -size 16x16 xc:red BMP3:test.bmp 2>/dev/null || \
    convert -size 16x16 xc:red BMP3:test.bmp
fi

# Compile message table
echo "Compiling message table..."
x86_64-w64-mingw32-windmc test.mc

# Compile resources
echo "Compiling resources..."
x86_64-w64-mingw32-windres resources.rc resources.o

# Link into DLL
echo "Linking ResourceLibrary.dll..."
x86_64-w64-mingw32-gcc -shared -o ResourceLibrary.dll -nostartfiles \
    -Wl,--subsystem,windows resources.o

echo "Done: ResourceLibrary.dll ($(wc -c < ResourceLibrary.dll) bytes)"

# Clean intermediate files
rm -f resources.o MSG00409.bin MSG00409.rc test.h test_messages.h test.rc

# Copy to test resources if requested
if [ "$1" = "--install" ]; then
    DEST_ABS="$(cd "$(dirname "$0")/../../.." && pwd)/test/AsmResolver.PE.Win32Resources.Tests/Resources/ResourceLibrary.dll"
    cp ResourceLibrary.dll "$DEST_ABS"
    echo "Installed to $DEST_ABS"
fi

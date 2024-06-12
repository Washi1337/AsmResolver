# Overview

Win32 resources are additional files embedded into the PE image, and are organized by their resource type in directories typically stored in the `.rsrc` section.

While `AsmResolver.PE` provides basic traversal of these win32 resource directories (see [Win32 Resources](../peimage/win32resources.md)), it stops at exposing the raw data of each of the embedded files.
The `AsmResolver.PE.Win32Resources` package is an extension that provides a richer API for reading and writing resource data for various resource types.

The following resource types are supported by this extension package:

- [RT_CURSOR](icons.md)
- [RT_GROUP_CURSOR](icons.md)
- [RT_GROUP_ICON](icons.md)
- [RT_ICON](icons.md)
- [RT_VERSIONINFO](version.md)
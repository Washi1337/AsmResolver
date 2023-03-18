Overview
========

The Program Database (PDB) file format is a format developed by Microsoft for storing debugging information about binary files. 
PDBs are typically constructed based on the original source code the binary was compiled with, and lists various symbols that the source code defines and/or references.

Since version 5.0, AsmResolver provides a work-in-progress implementation for reading (and sometimes writing) PDB files to allow for better analysis of compiled binaries. This implementation is fully managed, and thus does not depend on libraries such as the Debug Interface Access (DIA) that only work on the Windows platform.
Furthermore, this project also aims to provide additional documentation on the file format, to make it more accessible to other developers.

.. warning:: 

    As the PDB file format is not very well documented, and mostly is reverse engineered from the official implementation provided by Microsoft, not everything in this API is finalized or stable yet. 
    As such, this part of AsmResolver's API is still likely to undergo some breaking changes as development continues.
AsmResolver
===========

This is the documentation of the AsmResolver project. AsmResolver is a set of libraries allowing .NET programmers to read, modify and write executable files. This includes .NET as well as native native images. The library exposes high-level representations of the PE, while still allowing the user to access low-level structures.

Table of Contents:
------------------

.. toctree::
   :maxdepth: 1
   :caption: General
   :name: sec-general

   overview

.. toctree::
   :maxdepth: 1
   :caption: Abstraction Layer 1: PE Files
   :name: sec-pefile

   pefile/index
   pefile/basics


.. toctree::
   :maxdepth: 1
   :caption: Abstraction Layer 2: PE Images
   :name: sec-peimage

   peimage/index
   peimage/basics
   peimage/imports
   peimage/win32resources
   peimage/dotnet


.. toctree::
   :maxdepth: 1
   :caption: Abstraction Layer 3: .NET assemblies
   :name: sec-peimage

   dotnet/index
   dotnet/basics
   dotnet/traversal
   dotnet/importing
   dotnet/cloning
   dotnet/managed-method-bodies
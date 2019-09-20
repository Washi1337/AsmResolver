AsmResolver
===========

This is the documentation of the AsmResolver project. AsmResolver is a set of libraries allowing .NET programmers to read, modify and write executable files. This includes .NET as well as native native images. The library exposes high-level representations of the PE, while still allowing the user to access low-level structures.

Overview
--------

AsmResolver provides three levels of abstraction of a portable executable (PE) file. This may sound complicated at first, but a typical use-case of the library might only need one of them. The three levels are, in increasing level of abstraction:

* **PEFile:** The lowest level of abstraction. This layer exposes the raw top-level PE headers, as well as section headers and raw section contents.
* **PEImage:** This layer exposes interpretations of data directories, such as import and export directories, raw .NET metadata structures and Win32 resources.
* **NetImage:** (Only relevant to PE files with .NET metadata) Provides a high-level representation of the .NET metadata that is somewhat similar to *System.Reflection*.

The higher level of abstraction you go, the easier the library is to use and typically the less things you have to worry about. Most people that use AsmResolver to edit .NET applications will probably never even touch the *PEFile* or *PEImage* class, as *NetImage* is most likely enough for a lot of use cases. The *PEFile* and *PEImage* classes are for users that know what they are doing and want to make changes on a lower level.


Table of Contents:

.. toctree::
   :maxdepth: 2

   index
   pefile

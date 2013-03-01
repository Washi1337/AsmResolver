AsmResolver
===========

AsmResolver is a library that allows you to read and edit any kind of PE file, including native and .net applications, using any kind of .NET language such as C#. It is designed to be as stable as possible and should crash or fail that fast. You are able to use AsmResolver by accepting the Terms and Conditions of the **GPL License**.

Usage
=====

To use AsmResolver in Visual Studio, simply add the library to your references.

Structure
=========

The library consists of the following namespaces, each with a special purpose:

* TUP.AsmResolver. 
  * PE
     * Readers
	 * Writers
  * ASM
  * NET
     * Specialized
         * MSIL
  * Exceptions


**TUP.AsmResolver**  
This is the main namespace containing classes that represent the standard structures of the executable, such as headers and resources. The *Win32Assembly* class is the main class which represent an entire executable. From here, you will load the file and access headers, properties and methods.

**TUP.AsmResolver.PE**  
The PE namespace is the "core" of AsmResolver. It contains classes that are reading or writing directly to the assembly. The *PeImage* class is the only public class and is comparable to a stream. 

**TUP.AsmResolver.PE.Readers**  
This namespace contains all classes that have something to do with reading the raw structure of the PE. All these classes are internal.

**TUP.AsmResolver.PE.Writers**  
This namespace contains all classes that have something to do with writing a new executable. These classes are being used for rebuilding the application. All these classes are internal and are in pre-pre-pre-alpha stage and need a lot of work.

**TUP.AsmResolver.ASM**  
In this namespace you will find classes that work with the x86 assembly instruction set. You can use the assembler and disassembler to read and write x86 instructions with the help of the *x86Instruction* and *x86OpCodes* class. This namespace is far from done and needs a lot of work. Don't expect perfect outputs.

**TUP.AsmResolver.NET**  
This contains all classes that have something to do with the .NET assembly. You will find here classes that represent Metadata headers, streams and heaps.

**TUP.AsmResolver.NET.Specialized**  
Here you will find (almost) all possible metadata table members. These members are all based on the *MetaDataMember* class, which holds e.g. the *MetaDataToken* and *MetaDataRow* properties.

**TUP.AsmResolver.NET.Specialized.MSIL**  
In this namespace you will find classes that work with the MSIL instruction set. You can use the assembler and disassembler to read and write MSIL instructions in a *MethodBody* with the help of the *MSILInstruction* and *MSILOpCodes* class.

**TUP.AsmResolver.Exceptions**  
This namespace contains only custom created exceptions.
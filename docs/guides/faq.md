# Frequently Asked Questions

## Why are there so many libraries / packages instead of just one?

Not everyone will need everything from the code base of AsmResolver. 
An application only interested in the headers of a PE file does not require any of the functionality of the `AsmResolver.DotNet` package.

For this reason, AsmResolver is not a monolithic library but a collection of smaller packages.
This way, you can select the packages that you need and leave out what you do not intend to use. 
This can reduce the total deployment size significantly.    

If you do not wish to deploy multiple DLLs next to your application, consider deploying your application as a [single-file bundle](https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview?tabs=cli) or use a solution such as [ILRepack](https://github.com/gluck/il-repack) or [ILMerge](https://github.com/dotnet/ILMerge).


## How do I customize reader/writer error handling in AsmResolver?

By default, AsmResolver tries to ignore and recover from invalid data present in the input file, and throws exceptions when attempting to construct and write executable files with invalid data.
This ensures files produced by AsmResolver are by default conforming to standards that the underlying operating system and/or the CLR accepts, and that you do not make any mistakes in constructing invalid executable files.

AsmResolver can be configured to be more strict or more lax in this verification process.
See one of the following articles:
- [Advanced PE Image Reading - Custom Error Handling](peimage/advanced-pe-reading.md#custom-error-handling) (PE)
- [Advanced Module Reading - PE Image Reading Parameters](dotnet/advanced-module-reading.md#pe-image-reading-parameters) (.NET modules),
- [Advanced PE Image Building - Image Builder Diagnostics](dotnet/advanced-pe-image-building.md#image-builder-diagnostics) (.NET modules).

> [!WARNING]
> Be careful with suppressing PE construction errors. 
> Disabling verification can cause the output to not work anymore unless you know what you are doing.

If AsmResolver throws an exception that you think should not be thrown, please consider reporting an issue on the
[issues board](https://github.com/Washi1337/AsmResolver/issues).


## Why does the executable not work anymore after modifying it with AsmResolver?

One of the following may have happened:

- AsmResolver has a bug.
- You are changing something in the executable you are not supposed to change.
- You are changing something that results in the executable not function anymore.
- The target binary is actively trying to prevent you from applying any modifications (this happens a lot with obfuscated binaries).

With great power comes great responsibility. 
Changing the wrong things in the input executable file can result in the output stop working.

For .NET applications, make sure your application conforms with specification ([ECMA-335](https://www.ecma-international.org/publications/files/ECMA-ST/ECMA-335.pdf)).
To help you with finding problems in your final output, try reopening the executable in AsmResolver and look for errors reported by the reader. 
Alternatively, use the PE verification tools that ships with .NET's SDK:
- `peverify` for .NET Framework (accessible via Visual Studio Developer's command prompt).
- `dotnet ILVerify` for .NET Core / .NET 5+ applications (installable via `dotnet tool install -g dotnet-ilverify       `)

If you believe it is a bug in AsmResolver, please report it on the [issues board](https://github.com/Washi1337/AsmResolver/issues).


## My code works using Mono.Cecil/dnlib but not         in AsmResolver, why?

Essentially, two things can be happening here:

- AsmResolver has a bug.
- You are misusing AsmResolver.

It is important to remember that, while the public API of `AsmResolver.DotNet` resembles the ones from Mono.Cecil and dnlib, AsmResolver follows a very different design philosophy.
As such, a lot of classes in those libraries will not map one-to-one to AsmResolver classes directly.
Carefully read exceptions thrown and/or check the documentation to make sure you are not misusing the API.

If you believe it is a bug, please report it on the [issues board](https://github.com/Washi1337/AsmResolver/issues).


## Does AsmResolver use writer events (similar to dnlib)?

No.

Instead, to have more control over how the final output executable file will look like, AsmResolver works in layers of abstraction. 
For example, you can manually serialize a `ModuleDefinition` to a `PEImage` first, before writing it to the disk:

```csharp
ModuleDefinition inputModule = ...;

// Construct a PE image from the module.
var newImage = inputModule.ToPEImage();

/* ... Make any additional changes to the image ... */ 

// Construct a file and write to the disk.
var newFile = newImage.ToPEFile(new ManagedPEFileBuilder());
newFile.Write("output.exe");
```


For more details, refer to [Advanced PE Image Building](dotnet/advanced-pe-image-building.md)

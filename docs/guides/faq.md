# Frequently Asked Questions

## Why are there so many libraries / packages instead of just one?

Not everyone will need everything from the code base of AsmResolver. For
example, someone that is only interested in reading the PE headers of an
input file does not require any of the functionality of the
`AsmResolver.DotNet` package.

This is why AsmResolver\'s design philosophy is not to be one monolithic
library, but rather be a toolsuite of libraries. By splitting up in
multiple smaller libraries, the user can carefully select the packages
that they need and leave out what they do not need. This can easily
shave off 100s of kilobytes from the total size of code that is shipped.

## Why does AsmResolver throw so many errors on reading/writing?

AsmResolver does verification of the input file, and if it finds
anything that is out of place or not according to specification, it will
report this to the `IErrorListener` passed onto the reader parameters. A
similar thing happens when serializing the input application back to the
disk. By default, this translates to an exception being thrown (e.g. you
might have seen a `System.AggregateException` being thrown upon
writing).

AsmResolver often can ignore and recover from kinds of errors, but this
is not enabled by default. To enable these, please refer to
[Advanced PE Image Reading](peimage/advanced-pe-reading.md#custom-error-handling) (PE)
or [Advanced Module Reading](dotnet/advanced-module-reading.md#pe-image-reading-parameters) (.NET),
and [Image Builder Diagnostics](dotnet/advanced-pe-image-building.md#image-builder-diagnostics) (.NET).
Be careful with ignoring errors though. Especially for disabling writer
verification can cause the output to not work anymore unless you know
what you are doing.

If it still breaks and you believe it is a bug, please report it on the
[issues board](https://github.com/Washi1337/AsmResolver/issues).

## Why does the executable not work anymore after modifying it with AsmResolver?

A couple of things can be happening here:

-   AsmResolver´s PE builder has a bug.
-   You are changing something in the executable you are not supposed to
    change.
-   You are changing something that results in the executable not
    function anymore.
-   The target binary is actively trying to prevent you from applying
    any modifications (this happens a lot with obfuscated binaries).

With great power comes great responsibility. Changing the wrong things
in the input executable file can result in the output stop working.

For .NET applications, make sure your application conforms with
specification
([ECMA-335](https://www.ecma-international.org/publications/files/ECMA-ST/ECMA-335.pdf)).
To help you with finding problems in your final output, try reopening
the executable in AsmResolver and look for errors reported by the
reader. Alternatively, using tools such as `peverify` for .NET Framework
applications (usually located in
`C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX X.X Tools.`)
or `dotnet ILVerify` for .NET Core / .NET 5+ applications, can also help
narrowing down what might have gone wrong.

If you believe it is a bug in AsmResolver, please report it on the
[issues board](https://github.com/Washi1337/AsmResolver/issues).

## In Mono.Cecil / dnlib my code works, but it does not work in AsmResolver, why?

Essentially, two things can be happening here:

-   AsmResolver´s code could have a bug.
-   You are misusing AsmResolver´s API.

It is important to remember that, while the public API of
`AsmResolver.DotNet` looks similar on face value to other libraries
(such as Mono.Cecil or dnlib), AsmResolver itself follows a very
different design philosophy than these libraries. As such, a lot of
classes in those libraries will not map one-to-one to AsmResolver
classes directly. Check the documentation to make sure you are not
misusing the API.

If you believe it is a bug, please report it on the [issues
board](https://github.com/Washi1337/AsmResolver/issues).

## Does AsmResolver have a concept similar to writer events in dnlib?

No.

Instead, to have more control over how the final output executable file
will look like, AsmResolver works in layers of abstraction. For example,
you can manually serialize a `ModuleDefinition` to a `PEImage` first,
before writing it to the disk. This class exposes more low level
structures of the executable file, which can all be changed before
writing to the disk.

For more details, refer to [Advanced PE Image Building](dotnet/advanced-pe-image-building.md)

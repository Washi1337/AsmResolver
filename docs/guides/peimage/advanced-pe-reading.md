# Advanced PE Image Reading

Advanced users might have the need to configure AsmResolver\'s PE image
reader. For example, instead of letting the PE reader throw exceptions
upon reading invalid data, errors should be ignored and recovered from.
Other uses might include a custom interpretation of .NET metadata
streams. These kinds of settings can be configured using the
`PEReaderParameters` class.

``` csharp
var parameters = new PEReaderParameters();
```

These parameters can then be passed on to any of the `PEImage.FromXXX`
methods.

``` csharp
var image = PEImage.FromFile(@"C:\Path\To\File.exe", parameters);
```

## Custom error handling 

By default, AsmResolver throws exceptions upon encountering invalid data
in the input file. To provide a custom method for handling parser
errors, set the `ErrorListener` property. There are a couple of default
implementations that AsmResolver provides.

-   `ThrowErrorListener`: Throws the recorded parser exception. This is
    the default.
-   `EmptyErrorListener`: Silently consumes any parser exception, and
    allows the reader to recover.
-   `DiagnosticBag`: Collects any parser exceptions, and allows the
    reader to recover.

Below an example on how to use the `DiagnosticBag` for collecting parser
errors and reporting them afterwards.

``` csharp
var bag = new DiagnosticBag();

var image = PEImage.FromFile("...", new PEReaderParameters(bag));

/* ... */

// Report any errors caught so far to the standard output.
foreach (var error in bag.Exceptions)
    Console.WriteLine(error);
```

> [!NOTE]
> The `PEImage` class and its derivatives are initialized lazily. As a
> result, parser errors might not immediately appear in the `Exceptions`
> property of the `DiagnosticBag` class.


## Custom metadata stream reading

Some .NET obfuscators insert custom metadata streams in the .NET
metadata directory. By default, AsmResolver creates a
`CustomMetadataStream` object for any metadata stream for which the name
is not recognized. To change this behaviour, you can provide a custom
implementation of the `IMetadataStreamReader` interface, or extend the
existing `DefaultMetadataStreamReader` class. Below an example of an
implementation that changes the way a stream with the name
`#CustomStream` is parsed:

``` csharp
public class CustomMetadataStreamReader : DefaultMetadataStreamReader
{
    public override IMetadataStream ReadStream(
        PEReaderContext context, 
        MetadataStreamHeader header,
        ref BinaryStreamReader reader)
    {
        if (header.Name == "#CustomStream")
        {
            // Do custom parsing here.
            /* ... */
        }
        else
        {
            // Forward to default stream parser.
            base.ReadStream(context, header, ref reader);
        }
    }
}
```

To let the reader use this implementation of the
`IMetadataStreamReader`, set the `MetadataStreamReader` property of the
reader parameters.

``` csharp
parameters.MetadataStreamReader = new CustomMetadataStreamReader();
```

> [!WARNING]
> Higher levels of abstractions (e.g. `AsmResolver.DotNet`) depend on the
> existence of certain default stream types like the `TablesStream` and
> `StringsStream`. When these are not provided by your custom
> implementation, these abstractions will stop working correctly.


## Custom debug data reading

Debug data directories can have arbitrary data stored in the PE image.
By default, AsmResolver creates for every entry an instance of
`CustomDebugDataSegment`. This can be configured by providing a custom
implementation of the `IDebugDataReader` interface:

``` csharp
public class CustomDebugDataReader : DefaultDebugDataReader
{
    public override IDebugDataSegment ReadDebugData(
        PEReaderContext context, 
        DebugDataType type, 
        ref BinaryStreamReader reader)
    {
        if (type == DebugDataType.Coff)
        {
            // Do custom parsing here.
            /* ... */
        }
        else
        {
            // Forward to default parser.
            return base.ReadDebugData(context, type, ref reader);
        }
    }
}
```

To let the reader use this implementation of the `IDebugDataReader`, set
the `DebugDataReader` property of the reader parameters.

``` csharp
parameters.DebugDataReader = new CustomDebugDataReader();
```

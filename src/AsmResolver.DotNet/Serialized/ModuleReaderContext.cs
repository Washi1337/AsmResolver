using System;
using AsmResolver.PE;
using AsmResolver.PE.DotNet.Metadata;

namespace AsmResolver.DotNet.Serialized
{
    /// <summary>
    /// Provides a context in which a .NET module parser exists in. This includes the original PE image, as well as the
    /// module reader parameters.
    /// </summary>
    public class ModuleReaderContext : IErrorListener
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ModuleReaderContext"/> class.
        /// </summary>
        /// <param name="image">The original PE image to read from.</param>
        /// <param name="parentModule">The root module object.</param>
        /// <param name="parameters">The module reader parameters.</param>
        public ModuleReaderContext(PEImage image, SerializedModuleDefinition parentModule, ModuleReaderParameters parameters)
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));
            ParentModule = parentModule ?? throw new ArgumentNullException(nameof(parentModule));
            Parameters = new ModuleReaderParameters(parameters) ?? throw new ArgumentNullException(nameof(parameters));

            // Both CLR and CoreCLR implement a slightly different loading procedure for EnC metadata.
            // While the difference is very subtle, it has a slight effect on which streams are selected
            // when multiple streams with the same name are present in the metadata directory. This only
            // really happens in post-processed binaries (e.g., obfuscated binaries). Any normal .NET
            // compiler only emits one stream for each stream type.
            //
            // For normal metadata (i.e., metadata with a #~ stream), every stream is loaded. This means that
            // if a stream has the same name as a previously loaded one, it will override the contents of the previous.
            // On the other hand, EnC metadata (i.e., metadata with a #- stream) looks up the first occurrence
            // of the stream of the provided name. The exception for this is the tables stream itself, for which both
            // the CLR and CoreCLR seem to always result in a file corruption error when there are multiple table streams.
            bool isEncMetadata = Metadata.IsEncMetadata;

            for (int i = 0; i < Metadata.Streams.Count; i++)
            {
                switch (Metadata.Streams[i])
                {
                    case TablesStream tablesStream when TablesStream is null:
                        TablesStream = tablesStream;
                        TablesStreamIndex = i;
                        break;
                    case BlobStream blobStream when BlobStream is null || !isEncMetadata:
                        BlobStream = blobStream;
                        BlobStreamIndex = i;
                        break;
                    case GuidStream guidStream when GuidStream is null || !isEncMetadata:
                        GuidStream = guidStream;
                        GuidStreamIndex = i;
                        break;
                    case StringsStream stringsStream when StringsStream is null || !isEncMetadata:
                        StringsStream = stringsStream;
                        StringsStreamIndex = i;
                        break;
                    case UserStringsStream userStringsStream when UserStringsStream is null || !isEncMetadata:
                        UserStringsStream = userStringsStream;
                        UserStringsStreamIndex = i;
                        break;
                }
            }

            // There should at least be a tables stream.
            if (TablesStream is null)
                throw new ArgumentException("Metadata directory does not contain a tables stream.");
        }

        /// <summary>
        /// Gets the original PE image to read from.
        /// </summary>
        public PEImage Image
        {
            get;
        }

        /// <summary>
        /// Gets the root module object that is being read.
        /// </summary>
        public SerializedModuleDefinition ParentModule
        {
            get;
        }

        /// <summary>
        /// Gets the original metadata directory.
        /// </summary>
        public IMetadata Metadata => Image.DotNetDirectory!.Metadata!;

        /// <summary>
        /// Gets the main tables stream in the metadata directory.
        /// </summary>
        public TablesStream TablesStream
        {
            get;
        }

        /// <summary>
        /// Gets the original index of the tables stream.
        /// </summary>
        public int TablesStreamIndex
        {
            get;
        } = -1;

        /// <summary>
        /// Gets the main blob stream in the metadata directory.
        /// </summary>
        public BlobStream? BlobStream
        {
            get;
        }

        /// <summary>
        /// Gets the original index of the blob stream.
        /// </summary>
        public int BlobStreamIndex
        {
            get;
        } = -1;

        /// <summary>
        /// Gets the main GUID stream in the metadata directory.
        /// </summary>
        public GuidStream? GuidStream
        {
            get;
        }

        /// <summary>
        /// Gets the original index of the GUID stream.
        /// </summary>
        public int GuidStreamIndex
        {
            get;
        } = -1;

        /// <summary>
        /// Gets the main strings stream in the metadata directory.
        /// </summary>
        public StringsStream? StringsStream
        {
            get;
        }

        /// <summary>
        /// Gets the original index of the strings stream.
        /// </summary>
        public int StringsStreamIndex
        {
            get;
        } = -1;

        /// <summary>
        /// Gets the main user-strings stream in the metadata directory.
        /// </summary>
        public UserStringsStream? UserStringsStream
        {
            get;
        }

        /// <summary>
        /// Gets the original index of the user-strings stream.
        /// </summary>
        public int UserStringsStreamIndex
        {
            get;
        } = -1;

        /// <summary>
        /// Gets the reader parameters.
        /// </summary>
        public ModuleReaderParameters Parameters
        {
            get;
        }

        /// <inheritdoc />
        public void MarkAsFatal() => Parameters.PEReaderParameters.ErrorListener.MarkAsFatal();

        /// <inheritdoc />
        public void RegisterException(Exception exception) =>
            Parameters.PEReaderParameters.ErrorListener.RegisterException(exception);
    }
}

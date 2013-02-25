using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PE.Writers
{
    internal class DataDirectoryWriter : IWriterTask
    {
        internal DataDirectoryWriter(PEWriter writer)
        {
            Writer = writer;
        }

        public PEWriter Writer
        {
            get;
            private set;
        }

        public void RunProcedure()
        {
            WriteDirectories(Writer.OriginalAssembly.NTHeader.OptionalHeader.DataDirectories);
            
        }

        private void WriteDirectories(DataDirectory[] directories)
        {
            foreach (DataDirectory dataDirectory in directories)
            {
                if (dataDirectory.TargetOffset.FileOffset != 0)
                {
                    Writer.MoveToOffset(dataDirectory.TargetOffset.FileOffset);
                    if (AllowedToWriteDirectory(dataDirectory))
                    {
                        Writer.BinWriter.Write(dataDirectory.GetBytes());
                        if (dataDirectory.Name == DataDirectoryName.Clr)
                        {
                            WriteDirectories(Writer.OriginalAssembly.NETHeader.DataDirectories);
                        }
                    }
                }
            }
        }

        private bool AllowedToWriteDirectory(DataDirectory directory)
        {
            switch (directory.Name)
            {
                case DataDirectoryName.Export:
                    return !Writer.Parameters.RebuildExportTable;
                case DataDirectoryName.Import:
                    return !Writer.Parameters.RebuildImportTable;
                case DataDirectoryName.Resource:
                    return !Writer.Parameters.RebuildResources;
                case DataDirectoryName.Clr:
                case DataDirectoryName.NETCodeManager:
                case DataDirectoryName.NETMetadata:
                case DataDirectoryName.NETResource:
                    return !Writer.Parameters.RebuildNETHeaders;
                default:
                    return true;
            }
        }


    }
}

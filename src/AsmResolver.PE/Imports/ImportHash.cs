using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using AsmResolver.Shims;

namespace AsmResolver.PE.Imports
{
    /// <summary>
    /// Provides an implementation of the import hash (ImpHash) introduced by Mandiant.
    ///
    /// Reference:
    /// https://www.fireeye.com/blog/threat-research/2014/01/tracking-malware-import-hashing.html
    /// </summary>
    public static class ImportHash
    {
        private static readonly string[] Extensions = { ".dll", ".sys", ".ocx" };

        /// <summary>
        /// Computes the hash of all imported symbols.
        /// </summary>
        /// <param name="image">The image to get the import hash from.</param>
        /// <returns>The hash.</returns>
        /// <remarks>
        /// This is the ImpHash as introduced by Mandiant.
        /// Reference: https://www.fireeye.com/blog/threat-research/2014/01/tracking-malware-import-hashing.html
        /// </remarks>
        public static byte[] GetImportHash(this PEImage image) => image.GetImportHash(DefaultSymbolResolver.Instance);

        /// <summary>
        /// Computes the hash of all imported symbols.
        /// </summary>
        /// <param name="image">The image to get the import hash from.</param>
        /// <param name="symbolResolver">The object responsible for resolving symbols imported by ordinal.</param>
        /// <returns>The hash.</returns>
        /// <remarks>
        /// This is the ImpHash as introduced by Mandiant.
        /// Reference: https://www.fireeye.com/blog/threat-research/2014/01/tracking-malware-import-hashing.html
        /// </remarks>
        public static byte[] GetImportHash(this PEImage image, ISymbolResolver symbolResolver)
        {
            var elements = new List<string>();

            for (int j = 0; j < image.Imports.Count; j++)
            {
                var module = image.Imports[j];

                string formattedModuleName = FormatModuleName(module);
                for (int i = 0; i < module.Symbols.Count; i++)
                    elements.Add($"{formattedModuleName}.{FormatSymbolName(module.Symbols[i], symbolResolver)}");
            }

            using var md5 = MD5.Create();
            return md5.ComputeHash(Encoding.ASCII.GetBytes(StringShim.Join(",", elements)));
        }

        private static string FormatModuleName(IImportedModule module)
        {
            string name = module.Name!;
            if (string.IsNullOrEmpty(name))
                return name;

            foreach (string extension in Extensions)
            {
                if (name.EndsWith(extension))
                {
                    name = name.Remove(name.Length - extension.Length);
                    break;
                }
            }

            return name.ToLowerInvariant();
        }

        private static string FormatSymbolName(ImportedSymbol symbol, ISymbolResolver symbolResolver)
        {
            if (symbol.IsImportByName)
                return symbol.Name.ToLowerInvariant();

            var resolvedSymbol = symbolResolver.Resolve(symbol);

            if (resolvedSymbol is null)
                throw new ArgumentException($"Failed to resolve {symbol}.");
            if (!resolvedSymbol.IsByName)
                throw new ArgumentException($"Resolved export for {symbol} has no name.");

            return resolvedSymbol.Name.ToLowerInvariant();
        }
    }
}

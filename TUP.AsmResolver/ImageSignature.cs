using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{
    /// <summary>
    /// Represents the type of a signature of a portable executable file.
    /// </summary>
    public enum ImageSignature
    {
        /// <summary>
        /// Indicates that the image is a DOS signature, containing a MZ header.
        /// </summary>
        DOS = 0x5A4D,     /* MZ   */
        /// <summary>
        /// Indicates that the image is a OS/2 signature, containing a NE header.
        /// </summary>
        OS2 = 0x454E,     /* NE   */
        /// <summary>
        /// Indicates that the image is a OS/2 LE signature, containing a LE header.
        /// </summary>
        OS2_LE = 0x454C,     /* LE   */
        /// <summary>
        /// Indicates that the image is a OS/2 LX signature, containing a LX header.
        /// </summary>
        OS2_LX = 0x584C,     /* LX */
        /// <summary>
        /// Indicates that the image is a VXD signature, containing a LE header.
        /// </summary>
        VXD = 0x454C,     /* LE   */
        /// <summary>
        /// Indicates that the image is a NT signature, containing a NT or PE header.
        /// </summary>
        NT = 0x00004550   /* PE00 */
    }
}

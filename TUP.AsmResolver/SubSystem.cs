using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver
{

    /// <summary>
    /// The Windows subsystem that will be invoked to run the executable.
    /// </summary>
    public enum SubSystem
    {
        /// <summary>
        /// The subsystem is unknown.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Doesn't require a subsystem.
        /// </summary>
        Native = 1,
        /// <summary>
        /// Runs in the Windows GUI subsystem.
        /// </summary>
        WindowsGraphicalUI = 2,
        /// <summary>
        /// Runs in the Windows character (console) subsystem.
        /// </summary>
        WindowsConsoleUI = 3,
        /// <summary>
        /// Runs in the OS/2 character (console) subsystem (OS/2 1.x apps only).
        /// </summary>
        OS2ConsoleUI = 5,
        /// <summary>
        /// Runs in the Posix character (console) subsystem.
        /// </summary>
        POSIXConsoleUI = 7,
        /// <summary>
        /// Runs as a native Win9x driver.
        /// </summary>
        NativeWindows = 8,
        /// <summary>
        /// Runs in the Windows Embedded Compact GUI subsystem.
        /// </summary>
        WindowsCEGUI = 9,
        /// <summary>
        /// Runs as an Extensible Firmware Interface (EFI) application
        /// </summary>
        EFIApplication = 10,
        /// <summary>
        /// Runs as an Extensible Firmware Interface (EFI) driver with boot services
        /// </summary>
        EFIBootServiceDriver = 11,
        /// <summary>
        /// Runs as an Extensible Firmware Interface (EFI) driver with run-time services
        /// </summary>
        EFIRuntimeDriver = 12,
        /// <summary>
        /// Runs as an Extensible Firmware Interface (EFI) ROM image
        /// </summary>
        EFIRom = 13,
        /// <summary>
        /// Runs in the Xbox subsystem
        /// </summary>
        Xbox = 14,
        /// <summary>
        /// Runs in the Windows boot subsystem.
        /// </summary>
        WindowsBootApplication = 16,
    }
}

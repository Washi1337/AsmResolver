using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class MethodBodySection
    {
        byte signature;
        internal List<ExceptionHandler> handlers;
        internal MethodBodySection(byte signature)
        {
            this.signature = signature;
            handlers = new List<ExceptionHandler>();
        }

        public bool IsFat
        {
            get
            {
                return (signature & 0x40) == 0x40;
            }
        }
        public bool IsExceptionHandler
        {
            get
            {
                return true;
            }
        }
        public bool HasMoreSections
        {
            get
            {
                return (signature & 0x80) == 0x80;
            }
        }
        public ExceptionHandler[] ExceptionHandlers
        { get { return handlers.ToArray(); } }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.NET.Specialized.MSIL
{
    public class MethodBodySection
    {
        private byte _signature;
        internal List<ExceptionHandler> _handlers;

        internal MethodBodySection(byte signature)
        {
            this._signature = signature;
            _handlers = new List<ExceptionHandler>();
        }

        public bool IsFat
        {
            get
            {
                return (_signature & 0x40) == 0x40;
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
                return (_signature & 0x80) == 0x80;
            }
        }
        public ExceptionHandler[] ExceptionHandlers
        { get { return _handlers.ToArray(); } }

    }
}

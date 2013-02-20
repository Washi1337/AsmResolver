using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TUP.AsmResolver.PreviewApplication
{
    public struct FlagItem<T>
    {
        public string Name;
        public T Value;
        public override string ToString()
        {
            return Name;
        }
        public FlagItem(string name, T value)
        {
            Name = name;
            Value = value;
        }
    }
}

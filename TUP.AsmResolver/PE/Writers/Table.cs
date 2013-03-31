using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TUP.AsmResolver.PE.Writers
{
    internal abstract class Table
    {
        public uint StartingRVA { get; set; }
        public abstract uint CalculateSize();
        public abstract void AddStartingRvas();
    }

    internal class Table<T> : Table
    {
        public Table()
        {
            Items = new Dictionary<uint, T>();
        }

        public Dictionary<uint, T> Items { get; private set; }

        public override uint CalculateSize()
        {
            uint size = 0;
            foreach (var item in Items)
            {
                size += GetSizeOfItem(item.Value);
            }
            return size;
        }

        public uint GetSizeOfItem(T item)
        {
            if (typeof(T) == typeof(HintName))
                return (item as HintName).GetSize();
            else if (typeof(T) == typeof(string))
                return (uint)Encoding.ASCII.GetBytes(item as string).Length + 1;
            else if (typeof(T) == typeof(uint?))
                return sizeof(uint);
            else
                return (uint)Marshal.SizeOf(typeof(T));
        }

        public void Add(T item)
        {
            uint offset = 0;
            if (Items.Count > 0)
            {
                var keyPair = Items.ToArray().Last();
                offset = keyPair.Key + GetSizeOfItem(keyPair.Value);
            }
            Items.Add(offset, item);
        }

        public override void AddStartingRvas()
        {
            var items = Items.ToArray();
            Items.Clear();

            foreach (var item in items)
                Items.Add(item.Key + StartingRVA, item.Value);
        }

    }

    internal class HintName
    {
        public ushort Hint;
        public string Name;

        public HintName(ushort hint, string name)
        {
            Hint = hint;
            Name = name;
        }

        public uint GetSize()
        {
            return (uint)(sizeof(uint) + Encoding.ASCII.GetBytes(Name).Length + 1);
        }

        public override string ToString()
        {
            return Hint.ToString("X4") + ": " + Name;
        }
    }
}

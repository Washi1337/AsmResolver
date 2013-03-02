using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AsmResolver
{
    public class IntToHexTypeDescriptionProvider<T> : TypeDescriptionProvider where T : struct
    {
        private IntToHexCustomTypeDescriptor<T> m_Descriptor = new IntToHexCustomTypeDescriptor<T>();

        public IntToHexTypeDescriptionProvider(TypeDescriptionProvider parent) :
            base(parent) { }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if (objectType == typeof(T))
            {
                return m_Descriptor;
            }
            else
            {
                return base.GetTypeDescriptor(objectType, instance);
            }
        }
    }

    public class IntToHexCustomTypeDescriptor<T> : CustomTypeDescriptor where T : struct
    {
        private IntToHexTypeConverter<T> m_Converter = new IntToHexTypeConverter<T>();
        public override TypeConverter GetConverter()
        {
            return m_Converter;
        }
    }


    public class IntToHexTypeConverter<T> : TypeConverter where T : struct
    {
        MethodInfo parseMethod;
        public IntToHexTypeConverter()
        {
            parseMethod = typeof(T).GetMethod("Parse", new Type[] { typeof(string), typeof(NumberStyles), typeof(IFormatProvider)});
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            else
            {
                return base.CanConvertFrom(context, sourceType);
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            else
            {
                return base.CanConvertTo(context, destinationType);
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value.GetType() == typeof(T))
            {
                return string.Format("0x{0:X" + (Marshal.SizeOf(typeof(T)) * 2).ToString() + "}", value);
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                string input = (string)value;

                if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    input = input.Substring(2);
                }
                
                T retVal = (T)parseMethod.Invoke(null, new object[] {input, NumberStyles.HexNumber, culture });
                return retVal;
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }
    }


}

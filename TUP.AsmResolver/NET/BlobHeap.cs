using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.NET.Specialized;
using TUP.AsmResolver.NET.Specialized.MSIL;
namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// Represents the blob heap stream containing various values of many metadata members.
    /// </summary>
    public class BlobHeap : Heap
    {
        internal MemoryStream stream;
        internal BinaryReader reader;
        IGenericParametersProvider provider;

        internal BlobHeap(MetaDataStream stream)
            : base(stream)
        {
            this.stream = new MemoryStream(Contents);
            this.reader = new BinaryReader(this.stream);
            
        }

        public static BlobHeap FromStream(MetaDataStream stream)
        {
            BlobHeap heap = new BlobHeap(stream);
            return heap;
        }

        /// <summary>
        /// Gets the blob value by it's signature/index.
        /// </summary>
        /// <param name="index">The index or signature to get the blob value from.</param>
        /// <returns></returns>
        public byte[] GetBlob(uint index)
        {

            stream.Seek(index, SeekOrigin.Begin);
            byte length = reader.ReadByte();

            byte[] bytes = reader.ReadBytes(length);

            return bytes;
            
        }
        public IMemberSignature ReadMemberRefSignature(uint sig, IGenericParametersProvider provider)
        {
            stream.Seek(sig, SeekOrigin.Begin);
            uint length = ReadCompressedUInt32();

            byte flag = reader.ReadByte();

            if (flag == 0x6)
            {
                FieldSignature signature = new FieldSignature();
                signature.ReturnType = ReadTypeReference((ElementType)reader.ReadByte(), null);
                return signature;
            }
            else
            {
                MethodSignature signature = new MethodSignature();

                if ((flag & 0x20) != 0)
                {
                    signature.HasThis = true;
                    flag = (byte)(flag & -33);
                }
                if ((flag & 0x40) != 0)
                {
                    signature.ExplicitThis = true;
                    flag = (byte)(flag & -65);
                }
                if ((flag & 0x10) != 0)
                {
                    uint genericsig = ReadCompressedUInt32();
                }
                signature.CallingConvention = (MethodCallingConvention)flag;

                //if ((flag & 0x10) != 0x0)
                //{
                //    uint num2 = ReadCompressedUInt32();
                //
                //    List<GenericParameter> generics = new List<GenericParameter>();
                //    for (int i = 0; i < num2; i++)
                //    {
                //    }
                //    
                //}

                uint num3 = ReadCompressedUInt32();
                signature.ReturnType = ReadTypeReference((ElementType)ReadCompressedUInt32(), provider);//ReadTypeSignature((uint)stream.Position);
                if (num3 != 0)
                {
                    ParameterReference[] parameters = new ParameterReference[num3];
                    for (int i = 0; i < num3; i++)
                    {
                        parameters[i] = new ParameterReference() { ParameterType = ReadTypeReference((ElementType)ReadCompressedUInt32(), provider) };
                    }
                    signature.Parameters = parameters;
                }


                return signature;
            }
        }
        public PropertySignature ReadPropertySignature(uint signature)
        {
            stream.Seek(signature, SeekOrigin.Begin);
            uint length = ReadCompressedUInt32();

            byte flag = reader.ReadByte();

            if ((flag & 8) == 0)
                throw new ArgumentException("Signature doesn't refer to a valid property signature.");

            PropertySignature propertySig = new PropertySignature();
            propertySig.HasThis = (flag & 0x20) != 0;
            ReadCompressedUInt32();
            propertySig.ReturnType = ReadTypeReference((ElementType)reader.ReadByte(), null);
            return propertySig;
        }
        public VariableDefinition[] ReadVariableSignature(uint signature, MethodDefinition parentMethod)
        {
            stream.Seek(signature, SeekOrigin.Begin);
            uint length = ReadCompressedUInt32();
            byte local_sig = reader.ReadByte();

            if (local_sig != 0x7)
                throw new ArgumentException("Signature doesn't refer to a valid local variable signature");

            uint count = ReadCompressedUInt32();

            if (count == 0)
                return null;
            
            VariableDefinition[] variables = new VariableDefinition[count];

            for (int i = 0; i < count; i++)
                variables[i] = new VariableDefinition(i, ReadTypeReference((ElementType)reader.ReadByte(), parentMethod));

            return variables;
        }
        public TypeReference ReadTypeSignature(uint signature)
        {
            stream.Seek(signature, SeekOrigin.Begin);
            uint length = ReadCompressedUInt32();
            return ReadTypeReference((ElementType)ReadCompressedUInt32(), provider);
        }
        public TypeReference ReadTypeSignature(uint signature, IGenericParametersProvider provider)
        {
            this.provider = provider;
            stream.Seek(signature, SeekOrigin.Begin);
            uint length = ReadCompressedUInt32();
            return ReadTypeReference((ElementType)ReadCompressedUInt32(), this.provider);
            
        }
        public TypeReference[] ReadGenericParametersSignature(uint signature, IGenericParametersProvider provider)
        {
            stream.Seek(signature, SeekOrigin.Begin);
            uint length = ReadCompressedUInt32();
            List<TypeReference> types = new List<TypeReference>();
            this.provider = provider;
            if (reader.ReadByte() == 0xa)
            {
                uint count = ReadCompressedUInt32();
                for (int i = 0; i < count; i++)
                    types.Add(ReadTypeReference((ElementType)reader.ReadByte(),provider));
            }
            this.provider = null;
            return types.ToArray();
        }
        public object ReadConstantValue(ElementType type, uint signature)
        {
            stream.Seek(signature, SeekOrigin.Begin);

            int length = reader.ReadByte();

            switch (type)
            {
                case ElementType.Boolean:
                    return reader.ReadByte() == 1;
                case ElementType.Char:
                    return (char)reader.ReadUInt16();
                case ElementType.String:
                    if ((length & 1) == 1)
                        length--;
                    return Encoding.Unicode.GetString(reader.ReadBytes(length));
                case ElementType.I1:
                    return reader.ReadSByte();
                case  ElementType.I2:
                    return reader.ReadInt16();
                case ElementType.I4:
                    return reader.ReadInt32();
                case ElementType.I8:
                    return reader.ReadInt64();
                case ElementType.U1:
                    return reader.ReadByte();
                case ElementType.U2:
                    return reader.ReadUInt16();
                case ElementType.U4:
                    return reader.ReadUInt32();
                case ElementType.U8:
                    return reader.ReadUInt64();
                case ElementType.R4:
                    return reader.ReadSingle();
                case ElementType.R8:
                    return reader.ReadDouble();
                default:
                    throw new ArgumentException("Invalid constant type", "type");
            }
        }
        public CustomAttributeSignature ReadCustomAttributeSignature(CustomAttribute parent, uint signature)
        {
            stream.Seek(signature, SeekOrigin.Begin);
            uint length = ReadCompressedUInt32();

            ushort sign = reader.ReadUInt16();
            if (sign != 0x0001)
                throw new ArgumentException("Signature doesn't refer to a valid Custom Attribute signature");

            
            int fixedArgCount = 0;

            if (parent.Constructor.Signature != null && parent.Constructor.Signature.Parameters != null)
                fixedArgCount = parent.Constructor.Signature.Parameters.Length;

            CustomAttributeArgument[] fixedArgs = new CustomAttributeArgument[fixedArgCount];

            for (int i = 0; i < fixedArgCount; i++)
                fixedArgs[i] = new CustomAttributeArgument(ReadArgumentValue(parent.Constructor.Signature.Parameters[i].ParameterType));

            int namedArgCount = 0;
            CustomAttributeArgument[] namedArgs = new CustomAttributeArgument[namedArgCount];

            return new CustomAttributeSignature(fixedArgs, namedArgs);
        }



        internal TypeReference ReadTypeReference(ElementType type, IGenericParametersProvider provider)
        {
            switch (type)
            {
                case ElementType.Void:
                    return netheader.TypeSystem.Void;
                case ElementType.I:
                    return netheader.TypeSystem.IntPtr;
                case ElementType.I1:
                    return netheader.TypeSystem.Int8;
                case ElementType.I2:
                    return netheader.TypeSystem.Int16;
                case ElementType.I4:
                    return netheader.TypeSystem.Int32;
                case ElementType.I8:
                    return netheader.TypeSystem.Int64;
                case ElementType.U:
                    return netheader.TypeSystem.UIntPtr;
                case ElementType.U1:
                    return netheader.TypeSystem.UInt8;
                case ElementType.U2:
                    return netheader.TypeSystem.UInt16;
                case ElementType.U4:
                    return netheader.TypeSystem.UInt32;
                case ElementType.U8:
                    return netheader.TypeSystem.UInt64;
                case ElementType.Object:
                    return netheader.TypeSystem.Object;
                case ElementType.R4:
                    return netheader.TypeSystem.Single;
                case ElementType.R8:
                    return netheader.TypeSystem.Double;
                case ElementType.String:
                    return netheader.TypeSystem.String;
                case ElementType.Char:
                    return netheader.TypeSystem.Char;
                case ElementType.Type:
                    return netheader.TypeSystem.Type;
                case ElementType.Boolean:
                    return netheader.TypeSystem.Boolean;
                case ElementType.Ptr:
                    return new PointerType(ReadTypeReference((ElementType)reader.ReadByte(), provider));
                case ElementType.MVar:
                    uint token = ReadCompressedUInt32();
                    if (provider != null && provider.GenericParameters != null && provider.GenericParameters.Length > token)
                        return provider.GenericParameters[token];
                    else
                        return new GenericParamReference((int)token, new TypeReference() { name = "MVar", elementType = ElementType.MVar, @namespace = "", netheader = this.netheader });


                case ElementType.Var:
                    token =ReadCompressedUInt32();
                    if (provider != null && provider is MemberReference)
                    {
                        var member = provider as MemberReference;
                        if (member.DeclaringType is IGenericParametersProvider)
                        {
                            var genericprovider = (IGenericParametersProvider)member.DeclaringType;
                            if (genericprovider.GenericParameters != null)
                                return genericprovider.GenericParameters[token];
                        }
                    }   
                    return new GenericParamReference((int)token, new TypeReference() { name = "Var", elementType = ElementType.Var, @namespace = "", netheader = this.netheader });

                    break;
                case ElementType.Array:

                    return ReadArrayType();


                case ElementType.SzArray:
                    return new ArrayType(ReadTypeReference((ElementType)reader.ReadByte(), provider));
                case ElementType.Class:
                    return (TypeReference)netheader.TablesHeap.tablereader.TypeDefOrRef.GetMember((int)ReadCompressedUInt32());
                case ElementType.ValueType:
                    TypeReference typeRef = (TypeReference)netheader.TablesHeap.tablereader.TypeDefOrRef.GetMember((int)ReadCompressedUInt32());
                    typeRef.IsValueType = true;
                    return typeRef;
                case ElementType.ByRef:
                    return new ByReferenceType(ReadTypeReference((ElementType)reader.ReadByte(), provider));
                case ElementType.Pinned:
                    return new PinnedType(ReadTypeReference((ElementType)reader.ReadByte(), provider));
                case ElementType.GenericInst:
                    bool flag = reader.ReadByte() == 0x11;
                    TypeReference reference2 = ReadTypeToken();
                    GenericInstanceType instance = new GenericInstanceType(reference2);
                    this.ReadGenericInstanceSignature(reference2, instance);
                    if (flag)
                    {
                        instance.IsValueType = true;
                        
                    }
                    return instance;
            }
            return new TypeReference() { name = type.ToString(), @namespace = "" , netheader = this.netheader};

        }
        private void ReadGenericInstanceSignature(IGenericParametersProvider provider, GenericInstanceType type)
        {
            uint number = ReadCompressedUInt32();

            //provider.GenericParameters = new GenericParameter[number];
            type.GenericArguments = new TypeReference[number];

            for (int i = 0; i < number; i++)
                type.GenericArguments[i] = ReadTypeReference((ElementType)reader.ReadByte(), provider);


        }

        private TypeReference ReadTypeToken()
        {
            return (TypeReference)netheader.tableheap.tablereader.TypeDefOrRef.GetMember((int)ReadCompressedUInt32());
        }
        private ArrayType ReadArrayType()
        {
            TypeReference arrayType = ReadTypeReference((ElementType)reader.ReadByte(), provider);
            uint rank = ReadCompressedUInt32();
            uint[] upperbounds = new uint[ReadCompressedUInt32()];

            for (int i = 0; i < upperbounds.Length; i++)
                upperbounds[i] = ReadCompressedUInt32();

            int[] lowerbounds = new int[ReadCompressedUInt32()];

            for (int i = 0; i < lowerbounds.Length; i++)
                lowerbounds[i] = ReadCompressedInt32();


            ArrayDimension[] dimensions = new ArrayDimension[rank];

            for (int i = 0; i < rank; i++)
            {
                int? lower = null;
                int? upper = null;

                if (i < lowerbounds.Length)
                    lower = new int?(lowerbounds[i]);

                if (i < upperbounds.Length)
                {
                    int x = (int)upperbounds[i];
                    upper = (lower.HasValue ? new int?(lower.GetValueOrDefault() + x) : 0) - 1;
                }
                ArrayDimension dimension = new ArrayDimension(lower,upper);
                dimensions[i] = dimension;

            }


         
            return new ArrayType(arrayType, (int)rank, dimensions);

        }



        private object ReadArgumentValue(TypeReference paramType)
        {  
            if (!paramType.IsArray || !(paramType as ArrayType).IsVector)
                return ReadElement(paramType);
            
           // throw new NotImplementedException("Array constructor values are not supported yet.");
            
            ushort elementcount = reader.ReadUInt16();
            object[] elements = new object[elementcount];
            for (int i = 0; i < elementcount; i++)
                elements[i] = ReadElement((paramType as ArrayType).OriginalType);

            return elements;
        }
        private object ReadElement(TypeReference paramType)
        {
            switch (paramType.elementType)
            {
                case ElementType.I1:
                    return reader.ReadSByte();
                case ElementType.I2:
                    return reader.ReadInt16();
                case ElementType.I4:
                    return reader.ReadInt32();
                case ElementType.I8:
                    return reader.ReadInt64();
                case ElementType.U1:
                    return reader.ReadByte();
                case ElementType.U2:
                    return reader.ReadInt16();
                case ElementType.U4:
                    return reader.ReadInt32();
                case ElementType.U8:
                    return reader.ReadInt64();
                case ElementType.R4:
                    return reader.ReadSingle();
                case ElementType.R8:
                    return reader.ReadDouble();
                case ElementType.Type:
                case ElementType.String:
                    uint size = ReadCompressedUInt32();
                    if (size == 0xFF)
                        return string.Empty;
                    byte[] rawdata = reader.ReadBytes((int)size);
                    return Encoding.UTF8.GetString(rawdata);
                case ElementType.Char:
                    return reader.ReadChar();
                    throw new NotSupportedException();
                case ElementType.Boolean:
                    return reader.ReadByte() == 1;
                

            }
            return null;
        }




        private uint ReadCompressedUInt32()
        {
           // stream.Seek(index, SeekOrigin.Begin);
            byte num = reader.ReadByte();
            if ((num & 0x80) == 0)
            {
                return num;
            }
            if ((num & 0x40) == 0)
            {
                return (uint)(((num & -129) << 8) | reader.ReadByte());
            }
            return (uint)(((((num & -193) << 0x18) | (reader.ReadByte() << 0x10)) | (reader.ReadByte() << 8)) | reader.ReadByte());
        }
        public int ReadCompressedInt32()
        {
            int num = (int)(this.ReadCompressedUInt32() >> 1);
            if ((num & 1) == 0)
            {
                return num;
            }
            if (num < 0x40)
            {
                return (num - 0x40);
            }
            if (num < 0x2000)
            {
                return (num - 0x2000);
            }
            if (num < 0x10000000)
            {
                return (num - 0x10000000);
            }
            return (num - 0x20000000);
        }

        public override void Dispose()
        {
            reader.BaseStream.Close();
            reader.BaseStream.Dispose();
            reader.Close();
            reader.Dispose();
        }
    }
}

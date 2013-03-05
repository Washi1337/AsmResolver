using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TUP.AsmResolver.NET.Specialized;
using TUP.AsmResolver.NET.Specialized.MSIL;
using TUP.AsmResolver.PE;
namespace TUP.AsmResolver.NET
{
    /// <summary>
    /// Represents the blob heap stream containing various values of many metadata members.
    /// </summary>
    public class BlobHeap : MetaDataStream
    {

        internal SortedDictionary<uint, byte[]> readBlobs = new SortedDictionary<uint, byte[]>();
        
        IGenericParametersProvider provider;

        //MetaDataStream stream)
            //: base(stream)
        internal BlobHeap(NETHeader netheader, int headeroffset, Structures.METADATA_STREAM_HEADER rawHeader, string name)
            : base(netheader, headeroffset, rawHeader, name)
        {
        }


        internal override void Initialize()
        {
        }

        internal void Reconstruct()
        {
            // will be removed once blobs are being serialized.

            MemoryStream newStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(newStream);
            writer.Write((byte)0);
            ReadAllBlobs();
        
            foreach (var blob in readBlobs)
            {
                NETGlobals.WriteCompressedUInt32(writer, (uint)blob.Value.Length);
                writer.Write(blob.Value);
            }
        
            mainStream.Dispose();
            binReader.Dispose();
            binWriter.Dispose();
            mainStream = newStream;
            binReader = new BinaryReader(newStream);
            binWriter = new BinaryWriter(newStream);
            this.streamHeader.Size = (uint)newStream.Length;
        }

        internal void ReadAllBlobs()
        {
            mainStream.Seek(1, SeekOrigin.Begin);
            while (mainStream.Position < mainStream.Length)
            {
                bool alreadyExisted = readBlobs.ContainsKey((uint)mainStream.Position);
                byte[] value = GetBlob((uint)mainStream.Position);

                int length = value.Length;
                if (length == 0)
                    break;
                if (alreadyExisted)
                    mainStream.Seek(length + NETGlobals.GetCompressedUInt32Size((uint)length), SeekOrigin.Current);

            }
        }

        public override void Dispose()
        {
            ClearCache();
            base.Dispose();
        }

        public override void ClearCache()
        {
            readBlobs.Clear();
        }

        /// <summary>
        /// Gets the blob value by it's signature/index.
        /// </summary>
        /// <param name="index">The index or signature to get the blob value from.</param>
        /// <returns></returns>
        public byte[] GetBlob(uint index)
        {
            byte[] bytes = null;
            if (readBlobs.TryGetValue(index, out bytes))
                return bytes;

            mainStream.Seek(index, SeekOrigin.Begin);
            int length = (int)NETGlobals.ReadCompressedUInt32(binReader);

            bytes = binReader.ReadBytes(length);
            readBlobs.Add(index, bytes);
            return bytes;
            
        }

        /// <summary>
        /// Gets the blob value by it's signature/index and creates a binary reader.
        /// </summary>
        /// <param name="index">The index or signature to get the blob value from.</param>
        /// <returns></returns>
        public BinaryReader GetBlobReader(uint index)
        {
            byte[] bytes = null;
            if (!readBlobs.TryGetValue(index, out bytes))
            {
                mainStream.Seek(index, SeekOrigin.Begin);
                uint length = NETGlobals.ReadCompressedUInt32(binReader);
                bytes = binReader.ReadBytes((int)length);
            }

            MemoryStream newStream = new MemoryStream(bytes);
            newStream.Seek(0, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(newStream);
            return reader;
        }

        public uint GetBlobIndex(byte[] blobValue)
        {
            ReadAllBlobs();

            if (readBlobs.ContainsValue(blobValue))
                return readBlobs.FirstOrDefault(b => b.Value == blobValue).Key;

            mainStream.Seek(0, SeekOrigin.End);
            uint index = (uint)mainStream.Position;
            NETGlobals.WriteCompressedUInt32(binWriter, (uint)blobValue.Length);
            binWriter.Write(blobValue);
            readBlobs.Add(index, blobValue);
            return index;
        }

        public IMemberSignature ReadMemberRefSignature(uint sig, IGenericParametersProvider provider)
        {
            IMemberSignature signature = null;
            using (BinaryReader reader = GetBlobReader(sig))
            {

                byte flag = reader.ReadByte();

                if (flag == 0x6)
                {
                    FieldSignature fieldsignature = new FieldSignature();
                    fieldsignature.ReturnType = ReadTypeReference(reader, (ElementType)reader.ReadByte(), null);
                    signature = fieldsignature;
                }
                else
                {
                    MethodSignature methodsignature = new MethodSignature();

                    if ((flag & 0x20) != 0)
                    {
                        methodsignature.HasThis = true;
                        flag = (byte)(flag & -33);
                    }
                    if ((flag & 0x40) != 0)
                    {
                        methodsignature.ExplicitThis = true;
                        flag = (byte)(flag & -65);
                    }
                    if ((flag & 0x10) != 0)
                    {
                        uint genericsig = NETGlobals.ReadCompressedUInt32(reader);
                    }
                    methodsignature.CallingConvention = (MethodCallingConvention)flag;

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

                    uint num3 = NETGlobals.ReadCompressedUInt32(reader);
                    methodsignature.ReturnType = ReadTypeReference(reader, (ElementType)NETGlobals.ReadCompressedUInt32(reader), provider);//ReadTypeSignature((uint)stream.Position);
                    if (num3 != 0)
                    {
                        ParameterReference[] parameters = new ParameterReference[num3];
                        for (int i = 0; i < num3; i++)
                        {
                            parameters[i] = new ParameterReference() { ParameterType = ReadTypeReference(reader, (ElementType)NETGlobals.ReadCompressedUInt32(reader), provider) };
                        }
                        methodsignature.Parameters = parameters;
                    }
                    signature = methodsignature;

                }
            }
            return signature;
        }
        
        public PropertySignature ReadPropertySignature(uint signature)
        {
            PropertySignature propertySig = null;
            using (BinaryReader reader = GetBlobReader(signature))
            {

                byte flag = reader.ReadByte();

                if ((flag & 8) == 0)
                    throw new ArgumentException("Signature doesn't refer to a valid property signature.");

                propertySig = new PropertySignature();
                propertySig.HasThis = (flag & 0x20) != 0;
                NETGlobals.ReadCompressedUInt32(reader);
                propertySig.ReturnType = ReadTypeReference(reader, (ElementType)reader.ReadByte(), null);
            }
            return propertySig;
        }
        
        public VariableDefinition[] ReadVariableSignature(uint signature, MethodDefinition parentMethod)
        {
            VariableDefinition[] variables = null;
            using (BinaryReader reader = GetBlobReader(signature))
            {

                byte local_sig = reader.ReadByte();

                if (local_sig != 0x7)
                    throw new ArgumentException("Signature doesn't refer to a valid local variable signature");

                uint count = NETGlobals.ReadCompressedUInt32(reader);

                if (count == 0)
                    return null;

                variables = new VariableDefinition[count];

                for (int i = 0; i < count; i++)
                    variables[i] = new VariableDefinition(i, ReadTypeReference(reader, (ElementType)reader.ReadByte(), parentMethod));
            }
            return variables;
        }
        
        public TypeReference ReadTypeSignature(uint signature)
        {
            TypeReference typeRef = null;
            using (BinaryReader reader = GetBlobReader(signature))
            {
                typeRef = ReadTypeReference(reader, (ElementType)NETGlobals.ReadCompressedUInt32(reader), provider);
            }
            return typeRef;
        }
        
        public TypeReference ReadTypeSignature(uint signature, IGenericParametersProvider provider)
        {
            this.provider = provider;
            TypeReference typeRef = null;
            using (BinaryReader reader = GetBlobReader(signature))
            {
                typeRef = ReadTypeReference(reader, (ElementType)NETGlobals.ReadCompressedUInt32(reader), this.provider);
            }

            return typeRef;
        }
        
        public TypeReference[] ReadGenericParametersSignature(uint signature, IGenericParametersProvider provider)
        {
            List<TypeReference> types = new List<TypeReference>();
            using (BinaryReader reader = GetBlobReader(signature))
            {
                
                this.provider = provider;
                if (reader.ReadByte() == 0xa)
                {
                    uint count = NETGlobals.ReadCompressedUInt32(reader);
                    for (int i = 0; i < count; i++)
                        types.Add(ReadTypeReference(reader,(ElementType)reader.ReadByte(), provider));
                }
                this.provider = null;
            }
            return types.ToArray();
        }
        
        public object ReadConstantValue(ElementType type, uint signature)
        {
            object value = null;
            using (BinaryReader reader = GetBlobReader(signature))
            {

                switch (type)
                {
                    case ElementType.Boolean:
                        value = reader.ReadByte() == 1;
                        break;
                    case ElementType.Char:
                        value = (char)reader.ReadUInt16();
                        break;
                    case ElementType.String:
                        value = Encoding.Unicode.GetString(reader.ReadBytes((int)reader.BaseStream.Length));
                        break;
                    case ElementType.I1:
                        value =  reader.ReadSByte();
                        break;
                    case ElementType.I2:
                        value =  reader.ReadInt16();
                        break;
                    case ElementType.I4:
                        value =  reader.ReadInt32();
                        break;
                    case ElementType.I8:
                        value =  reader.ReadInt64();
                        break;
                    case ElementType.U1:
                        value =  reader.ReadByte();
                        break;
                    case ElementType.U2:
                        value =  reader.ReadUInt16();
                        break;
                    case ElementType.U4:
                        value =  reader.ReadUInt32();
                        break;
                    case ElementType.U8:
                        value =  reader.ReadUInt64();
                        break;
                    case ElementType.R4:
                        value =  reader.ReadSingle();
                        break;
                    case ElementType.R8:
                        value =  reader.ReadDouble();
                        break;
                    default:
                        throw new ArgumentException("Invalid constant type", "type");
                }
            }
            return value;
        }
       
        public CustomAttributeSignature ReadCustomAttributeSignature(CustomAttribute parent, uint signature)
        {
            CustomAttributeSignature customAttrSig = null;
            using (BinaryReader reader = GetBlobReader(signature))
            {
                ushort sign = reader.ReadUInt16();
                if (sign != 0x0001)
                    throw new ArgumentException("Signature doesn't refer to a valid Custom Attribute signature");


                int fixedArgCount = 0;



                if (parent.Constructor.Signature != null && parent.Constructor.Signature.Parameters != null)
                    fixedArgCount = parent.Constructor.Signature.Parameters.Length;

                CustomAttributeArgument[] fixedArgs = new CustomAttributeArgument[fixedArgCount];


                for (int i = 0; i < fixedArgCount; i++)
                {
                    fixedArgs[i] = new CustomAttributeArgument(ReadArgumentValue(reader,parent.Constructor.Signature.Parameters[i].ParameterType));

                }

                int namedArgCount = 0;
                CustomAttributeArgument[] namedArgs = new CustomAttributeArgument[namedArgCount];

                customAttrSig = new CustomAttributeSignature(fixedArgs, namedArgs);
            }
            return customAttrSig;
        }

        internal TypeReference ReadTypeReference(BinaryReader reader, ElementType type, IGenericParametersProvider provider)
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
                    return new PointerType(ReadTypeReference(reader,(ElementType)reader.ReadByte(), provider));
                case ElementType.MVar:
                    uint token = NETGlobals.ReadCompressedUInt32(reader);
                    if (provider != null && provider.GenericParameters != null && provider.GenericParameters.Length > token)
                        return provider.GenericParameters[token];
                    else
                        return new GenericParamReference((int)token, new TypeReference() { name = "MVar", elementType = ElementType.MVar, @namespace = "", netheader = this.netheader });


                case ElementType.Var:
                    token = NETGlobals.ReadCompressedUInt32(reader);
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

                case ElementType.Array:
                    return ReadArrayType(reader);
                case ElementType.SzArray:
                    return new ArrayType(ReadTypeReference(reader,(ElementType)reader.ReadByte(), provider));
                case ElementType.Class:
                    return (TypeReference)netheader.TablesHeap.TypeDefOrRef.GetMember((int)NETGlobals.ReadCompressedUInt32(reader));
                case ElementType.ValueType:
                    TypeReference typeRef = (TypeReference)netheader.TablesHeap.TypeDefOrRef.GetMember((int)NETGlobals.ReadCompressedUInt32(reader));
                    typeRef.IsValueType = true;
                    return typeRef;
                case ElementType.ByRef:
                    return new ByReferenceType(ReadTypeReference(reader, (ElementType)reader.ReadByte(), provider));
                case ElementType.Pinned:
                    return new PinnedType(ReadTypeReference(reader, (ElementType)reader.ReadByte(), provider));
                case ElementType.GenericInst:
                    bool flag = reader.ReadByte() == 0x11;
                    TypeReference reference2 = ReadTypeToken(reader);
                    GenericInstanceType instance = new GenericInstanceType(reference2);
                     this.ReadGenericInstanceSignature(reader,reference2, instance);
                    if (flag)
                    {
                        instance.IsValueType = true;
                        
                    }
                    return instance;
            }
            return new TypeReference() { name = type.ToString(), @namespace = "" , netheader = this.netheader};

        }
        
        private void ReadGenericInstanceSignature(BinaryReader reader, IGenericParametersProvider provider, GenericInstanceType type)
        {
            uint number = NETGlobals.ReadCompressedUInt32(reader);

            //provider.GenericParameters = new GenericParameter[number];
            type.GenericArguments = new TypeReference[number];

            for (int i = 0; i < number; i++)
                type.GenericArguments[i] = ReadTypeReference(reader,(ElementType)reader.ReadByte(), provider);


        }

        private TypeReference ReadTypeToken(BinaryReader reader)
        {
            return (TypeReference)netheader.TablesHeap.TypeDefOrRef.GetMember((int)NETGlobals.ReadCompressedUInt32(reader));
        }
       
        private ArrayType ReadArrayType(BinaryReader reader)
        {
            TypeReference arrayType = ReadTypeReference(reader,(ElementType)reader.ReadByte(), provider);
            uint rank = NETGlobals.ReadCompressedUInt32(reader);
            uint[] upperbounds = new uint[NETGlobals.ReadCompressedUInt32(reader)];

            for (int i = 0; i < upperbounds.Length; i++)
                upperbounds[i] = NETGlobals.ReadCompressedUInt32(reader);

            int[] lowerbounds = new int[NETGlobals.ReadCompressedUInt32(reader)];

            for (int i = 0; i < lowerbounds.Length; i++)
                lowerbounds[i] = NETGlobals.ReadCompressedInt32(reader);


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
                
        private object ReadArgumentValue(BinaryReader reader, TypeReference paramType)
        {
            if (!paramType.IsArray || !(paramType as ArrayType).IsVector)
                return ReadElement(reader,paramType);
            
           // throw new NotImplementedException("Array constructor values are not supported yet.");

            ushort elementcount = reader.ReadUInt16();
            object[] elements = new object[elementcount];
            for (int i = 0; i < elementcount; i++)
                elements[i] = ReadElement(reader,(paramType as ArrayType).OriginalType);

            return elements;
        }
        
        private object ReadElement(BinaryReader reader, TypeReference paramType)
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
                    uint size = NETGlobals.ReadCompressedUInt32(reader);
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


    }
}

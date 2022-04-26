using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Serialized;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.IO;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using MethodAttributes = AsmResolver.PE.DotNet.Metadata.Tables.Rows.MethodAttributes;

namespace AsmResolver.DotNet.Dynamic
{
    /// <summary>
    /// Represents a single method in a type definition of a .NET module.
    /// </summary>
    public class DynamicMethodDefinition : MethodDefinition
    {
        /// <summary>
        /// Create a Dynamic Method Definition
        /// </summary>
        /// <param name="module">Target Module</param>
        /// <param name="dynamicMethodObj">Dynamic Method / Delegate / DynamicResolver</param>
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("Calls ResolveDynamicResolver and FromDynamicMethod")]
        public DynamicMethodDefinition(ModuleDefinition module, object dynamicMethodObj) :
            base(new MetadataToken(TableIndex.Method, 0))
        {
            dynamicMethodObj = DynamicMethodHelper.ResolveDynamicResolver(dynamicMethodObj);
            var methodBase = FieldReader.ReadField<MethodBase>(dynamicMethodObj, "m_method");
            if (methodBase is null)
            {
                throw new ArgumentException(
                    "Could not get the underlying method base in the provided dynamic method object.");
            }

            Module = module;
            Name = methodBase.Name;
            Attributes = (MethodAttributes)methodBase.Attributes;
            Signature = new ReferenceImporter(module).ImportMethodSignature(ResolveSig(methodBase, module));
            CilMethodBody = ToCilMethodBody(this, dynamicMethodObj);
        }

        private MethodSignature ResolveSig(MethodBase methodBase, ModuleDefinition module)
        {
            var imp = new ReferenceImporter(module);
            var returnType = methodBase is MethodInfo info
                ? imp.ImportTypeSignature(info.ReturnType)
                : module.CorLibTypeFactory.Void;

            var parameters = methodBase.GetParameters();

            var parameterTypes = new TypeSignature[parameters.Length];
            for (int i = 0; i < parameterTypes.Length; i++)
                parameterTypes[i] = imp.ImportTypeSignature(parameters[i].ParameterType);

            return new MethodSignature(
                methodBase.IsStatic ? 0 : CallingConventionAttributes.HasThis,
                returnType, parameterTypes);
        }

        /// <inheritdoc />
        public override ModuleDefinition Module { get; }

        /// <summary>
        ///     Creates a CIL method body from a dynamic method.
        /// </summary>
        /// <param name="method">The method that owns the method body.</param>
        /// <param name="dynamicMethodObj">The Dynamic Method/Delegate/DynamicResolver.</param>
        /// <returns>The method body.</returns>
        public static CilMethodBody ToCilMethodBody(MethodDefinition method, object dynamicMethodObj)
        {
            if (!(method.Module is SerializedModuleDefinition module))
                throw new ArgumentException("Method body should reference a serialized module.");

            var result = new CilMethodBody(method);
            dynamicMethodObj = DynamicMethodHelper.ResolveDynamicResolver(dynamicMethodObj);

            //Get Runtime Fields
            byte[] code = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_code")!;
            object scope = FieldReader.ReadField<object>(dynamicMethodObj, "m_scope")!;
            var tokenList = FieldReader.ReadField<List<object?>>(scope, "m_tokens")!;
            byte[] localSig = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_localSignature")!;
            byte[] ehHeader = FieldReader.ReadField<byte[]>(dynamicMethodObj, "m_exceptionHeader")!;
            var ehInfos = FieldReader.ReadField<IList<object>>(dynamicMethodObj, "m_exceptions")!;

            //Local Variables
            DynamicMethodHelper.ReadLocalVariables(result, method, localSig);

            // Read raw instructions.
            var reader = ByteArrayDataSource.CreateReader(code);
            var disassembler = new CilDisassembler(reader, new DynamicCilOperandResolver(module, result, tokenList));
            result.Instructions.AddRange(disassembler.ReadInstructions());

            //Exception Handlers
            DynamicMethodHelper.ReadReflectionExceptionHandlers(result, ehInfos, ehHeader, new ReferenceImporter(module));

            return result;
        }
    }
}

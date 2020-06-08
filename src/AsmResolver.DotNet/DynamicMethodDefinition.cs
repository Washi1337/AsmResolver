using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using MethodAttributes = AsmResolver.PE.DotNet.Metadata.Tables.Rows.MethodAttributes;
namespace AsmResolver.DotNet
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
        public DynamicMethodDefinition(ModuleDefinition module,object dynamicMethodObj) : 
            base(new MetadataToken(TableIndex.Method, 0))
        {
            dynamicMethodObj = DynamicMethodHelper.ResolveDynamicResolver(dynamicMethodObj);
            var methodBase = FieldReader.ReadField<MethodBase>(dynamicMethodObj, "m_method");

            Module = module;
            CilMethodBody = CilMethodBody.FromDynamicMethod(this,dynamicMethodObj);
            Name = methodBase.Name;
            Attributes = (MethodAttributes)methodBase.Attributes;
            Signature = new ReferenceImporter(module).ImportMethodSignature(ResolveSig(methodBase,module));
        }

        private MethodSignature ResolveSig(MethodBase methodBase,ModuleDefinition module)
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
        public override ModuleDefinition Module { get; }
        
    }
}
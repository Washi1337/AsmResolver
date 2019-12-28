using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using AsmResolver.DotNet.Collections;
using AsmResolver.Lazy;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides a base implementation for describing a self-describing .NET assembly hosted by a common language runtime (CLR).
    /// </summary>
    public abstract class AssemblyDescriptor : IHasCustomAttribute
    {
        private readonly LazyVariable<string> _name;
        private readonly LazyVariable<string> _culture;
        private IList<CustomAttribute> _customAttributes;
        
        /// <summary>
        /// Initializes a new empty assembly descriptor.
        /// </summary>
        /// <param name="token">The token of the assembly descriptor.</param>
        protected AssemblyDescriptor(MetadataToken token)
        {
            MetadataToken = token;
            _name = new LazyVariable<string>(GetName);
            _culture = new LazyVariable<string>(GetCulture);
        }

        /// <inheritdoc />
        public MetadataToken MetadataToken
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the name of the assembly.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Name column in the assembly table. 
        /// </remarks>
        public string Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }
        
        /// <summary>
        /// Gets or sets the version of the assembly.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the MajorVersion, MinorVersion, BuildNumber and RevisionNumber columns in
        /// the assembly table. 
        /// </remarks>
        public Version Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attributes associated to the assembly.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the Attributes column in the assembly table. 
        /// </remarks>
        public AssemblyAttributes Attributes
        {
            get;
            set;
        }       
        
        /// <summary>
        /// Gets or sets a value indicating whether the assembly holds the full (unhashed) public key.
        /// </summary>
        public bool HasPublicKey
        {
            get => (Attributes & AssemblyAttributes.PublicKey) != 0;
            set => Attributes = (Attributes & ~AssemblyAttributes.PublicKey)
                                | (value ? AssemblyAttributes.PublicKey : 0);
        }
        
        /// <summary>
        /// Gets or sets a value indicating just-in-time (JIT) compiler tracking is enabled for the assembly.
        /// </summary>
        /// <remarks>
        /// This attribute originates from the <see cref="DebuggableAttribute"/> attribute.
        /// </remarks>
        public bool EnableJitCompileTracking 
        {
            get => (Attributes & AssemblyAttributes.EnableJitCompileTracking) != 0;
            set => Attributes = (Attributes & ~AssemblyAttributes.EnableJitCompileTracking)
                                | (value ? AssemblyAttributes.EnableJitCompileTracking : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating any just-in-time (JIT) compiler optimization is disabled for the assembly.
        /// </summary>
        /// <remarks>
        /// This attribute originates from the <see cref="DebuggableAttribute"/> attribute.
        /// </remarks>
        public bool DisableJitCompileOptimizer
        {
            get => (Attributes & AssemblyAttributes.DisableJitCompileOptimizer) != 0;
            set => Attributes = (Attributes & ~AssemblyAttributes.DisableJitCompileOptimizer)
                                | (value ? AssemblyAttributes.DisableJitCompileOptimizer : 0);
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the assembly contains Windows Runtime (WinRT) code or not.
        /// </summary>
        public bool IsWindowsRuntime
        {
            get => (Attributes & AssemblyAttributes.ContentMask) == AssemblyAttributes.ContentWindowsRuntime;
            set => Attributes = (Attributes & ~AssemblyAttributes.ContentMask)
                                | (value ? AssemblyAttributes.ContentWindowsRuntime : 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the assembly can be retargeted (at runtime) to an assembly from
        /// a different publisher.
        /// </summary>
        public bool IsRetargetable
        {
            get => (Attributes & AssemblyAttributes.Retargetable) != 0;
            set => Attributes = (Attributes & ~AssemblyAttributes.Retargetable)
                                | (value ? AssemblyAttributes.Retargetable : 0);
        }

        /// <inheritdoc />
        public IList<CustomAttribute> CustomAttributes
        {
            get
            {
                if (_customAttributes is null)
                    Interlocked.CompareExchange(ref _customAttributes, GetCustomAttributes(), null);
                return _customAttributes;
            }
        }

        /// <summary>
        /// Obtains the list of custom attributes assigned to the member.
        /// </summary>
        /// <returns>The attributes</returns>
        /// <remarks>
        /// This method is called upon initialization of the <see cref="CustomAttributes"/> property.
        /// </remarks>
        protected virtual IList<CustomAttribute> GetCustomAttributes() =>
            new OwnedCollection<IHasCustomAttribute, CustomAttribute>(this);

        /// <summary>
        /// Gets or sets the locale string of the assembly (if available).
        /// </summary>
        /// <remarks>
        /// <para>If this value is set to <c>null</c>, the default locale will be used</para>
        /// <para>This property corresponds to the Culture column in the assembly table.</para> 
        /// </remarks>
        public string Culture
        {
            get => _culture.Value;
            set => _culture.Value = value;
        }

        /// <summary>
        /// When the application is signed with a strong name, obtains the public key token of the assembly 
        /// </summary>
        /// <returns>The token.</returns>
        public abstract byte[] GetPublicKeyToken();

        /// <summary>
        /// Obtains the name of the assembly definition.
        /// </summary>
        /// <returns>The name.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="Name"/> property.
        /// </remarks>
        protected virtual string GetName() => null;

        /// <summary>
        /// Obtains the locale string of the assembly definition.
        /// </summary>
        /// <returns>The locale string.</returns>
        /// <remarks>
        /// This method is called upon initializing the <see cref="Culture"/> property.
        /// </remarks>
        protected virtual string GetCulture() => null;

        /// <inheritdoc />
        public override string ToString()
        {
            var publicKeyToken = GetPublicKeyToken();
            string publicKeyTokenString = publicKeyToken != null
                ? string.Join(string.Empty, publicKeyToken.Select(x => x.ToString("x2")))
                : "null";
            
            return $"{Name}, Version={Version}, PublicKeyToken={publicKeyTokenString}";
        }
    }
}
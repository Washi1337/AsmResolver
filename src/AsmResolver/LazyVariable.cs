using System;
using System.Diagnostics.CodeAnalysis;

namespace AsmResolver
{
    /// <summary>
    /// Represents a variable that can be lazily initialized and/or assigned a new value.
    /// </summary>
    /// <typeparam name="T">The type of the values that the variable stores.</typeparam>
    public class LazyVariable<T>
    {
        private T? _value;
        private readonly Func<T?>? _getValue;
        private readonly object _lockObject = new();

        /// <summary>
        /// Creates a new lazy variable and initialize it with a constant.
        /// </summary>
        /// <param name="value">The value to initialize the variable with.</param>
        public LazyVariable(T value)
        {
            _value = value;
            IsInitialized = true;
        }

        /// <summary>
        /// Creates a new lazy variable and delays the initialization of the default value.
        /// </summary>
        /// <param name="getValue">The method to execute when initializing the default value.</param>
        public LazyVariable(Func<T?> getValue)
        {
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
        }

        /// <summary>
        /// Gets a value indicating the value has been initialized.
        /// </summary>
        [MemberNotNullWhen(false, nameof(_getValue))]
        public bool IsInitialized
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the value of the variable.
        /// </summary>
        public T Value
        {
            get
            {
                if (!IsInitialized)
                    InitializeValue();
                return _value!;
            }
            set
            {
                lock (_lockObject)
                {
                    _value = value;
                    IsInitialized = true;
                }
            }
        }

        private void InitializeValue()
        {
            lock (_lockObject)
            {
                if (!IsInitialized)
                {
                    _value = _getValue();
                    IsInitialized = true;
                }
            }
        }

    }
}

using System;
using System.Threading;

namespace AsmResolver.Lazy
{
    /// <summary>
    /// Represents a variable that can be lazily initialized and/or assigned a new value.
    /// </summary>
    /// <typeparam name="T">The type of the values that the variable stores.</typeparam>
    public class LazyVariable<T>
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private T _value;
        private bool _initialized;
        private readonly Func<T> _getValue;

        /// <summary>
        /// Creates a new lazy variable and initialize it with a constant.
        /// </summary>
        /// <param name="value">The value to initialize the variable with.</param>
        public LazyVariable(T value)
        {
            _value = value;
            _initialized = true;
        }

        /// <summary>
        /// Creates a new lazy variable and delays the initialization of the default value.
        /// </summary>
        /// <param name="getValue">The method to execute when initializing the default value.</param>
        public LazyVariable(Func<T> getValue)
        {
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
        }
        
        /// <summary>
        /// Gets or sets the value of the variable.
        /// </summary>
        public T Value
        {
            get
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    if (!_initialized) 
                        InitializeValue();

                    return _value;
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }
            set
            {
                _lock.EnterWriteLock();
                _value = value;
                _initialized = true;
                _lock.ExitWriteLock();
            }
        }

        private void InitializeValue()
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_initialized)
                {
                    _value = _getValue();
                    _initialized = true;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        
    }
}
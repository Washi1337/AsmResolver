using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsmResolver
{
    public class LazyValue<TValue>
    {
        private readonly Func<TValue> _getValue;
        private bool _isInitialized;
        private TValue _value;
        private TValue _originalValue;

        public LazyValue()
            : this(default(TValue))
        {
        }

        public LazyValue(TValue value)
            : this(() => value)
        {
            EnsureIsInitialized();
        }

        public LazyValue(Func<TValue> getValue)
        {
            _getValue = getValue;
        }

        public TValue Value
        {
            get
            {
                EnsureIsInitialized();
                return _value;
            }
            set
            {
                EnsureIsInitialized();
                _value = value;
            }
        }

        public TValue OriginalValue
        {
            get
            {
                EnsureIsInitialized();
                return _originalValue;
            }
        }

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public void EnsureIsInitialized()
        {
            if (!IsInitialized)
                _value = _originalValue = _getValue();
            _isInitialized = true;
        }

        public void Reset()
        {
            _isInitialized = false;
            _value = _originalValue = default(TValue);
        }
    }
}

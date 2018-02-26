using System;

namespace AsmResolver
{
    public class LazyValue<TValue>
    {
        private readonly Func<TValue> _getValue;
        private bool _isInitialized;
        private TValue _value;

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
                _value = value;
                _isInitialized = true;
            }
        }

        public bool IsInitialized
        {
            get { return _isInitialized; }
        }

        public void EnsureIsInitialized()
        {
            if (!IsInitialized)
                _value = _getValue();
            _isInitialized = true;
        }

        public void Reset()
        {
            _isInitialized = false;
            _value = default(TValue);
        }
    }
}

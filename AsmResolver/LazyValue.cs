using System;

namespace AsmResolver
{
    public class LazyValue<TValue>
    {
        protected Func<TValue> GetValue;
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
            GetValue = getValue;
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
                IsInitialized = true;
            }
        }

        public bool IsInitialized
        {
            get;
            protected set;
        }

        public void EnsureIsInitialized()
        {
            if (!IsInitialized)
                _value = GetValue();
            IsInitialized = true;
        }

        public void Reset()
        {
            IsInitialized = false;
            _value = default(TValue);
        }
    }

    public class TaggedLazyValue<TTag, TValue> : LazyValue<TValue>
    {
        private readonly Func<TTag, TValue> _getValue;

        public TaggedLazyValue()
        {
        }

        public TaggedLazyValue(TValue value)
            : base(value)
        {
        }

        public TaggedLazyValue(Func<TTag, TValue> getValue, Func<TTag> createTag)
        {
            _getValue = getValue;
            IsInitialized = false;
            base.GetValue = () => GetValue(createTag());
        }

        public TValue GetValue(TTag tag)
        {
            return _getValue(tag);
        }
    }
}

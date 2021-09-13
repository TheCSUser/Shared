using com.github.TheCSUser.Shared.Imports;
using System;

namespace com.github.TheCSUser.Shared.Common
{
    public sealed class Cached<T> : Lazy<T>
        where T : class
    {
        private int currentAttempts = 0;
        private readonly int _maxAttempts;

        public Cached(Func<T> createValue, int maxAttempts = 10) : base(createValue)
        {
            _maxAttempts = maxAttempts;
        }

        public override T Value
        {
            get
            {
                if (!isValueCreated || currentAttempts >= _maxAttempts)
                {
                    lock (padlock)
                    {
                        if (!isValueCreated)
                        {
                            currentAttempts++;
                            value = createValue();
                            isValueCreated = !(value is null);
                        }
                    }
                }
                return value;
            }
        }

        public override bool IsValueCreated
        {
            get
            {
                lock (padlock)
                {
                    return isValueCreated || currentAttempts >= _maxAttempts;
                }
            }
        }

        /// <summary>
        /// Remove the value of the current Cached{T} instance.
        /// </summary>
        public void Invalidate()
        {
            lock (padlock)
            {
                currentAttempts = 0;
                isValueCreated = false;
                value = null;
            }
        }
    }
}

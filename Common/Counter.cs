using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace com.github.TheCSUser.Shared.Common
{
    [SuppressMessage("Style", "IDE0019:Use pattern matching", Justification = "Personal preference")]
    public sealed class Counter : IComparable, IComparable<Counter>, ICloneable
    {
        public int Value { get => _value; }
        private int _value;

        public Counter() { _value = int.MinValue; }
        public Counter(int value) { _value = value; }

        public void Update() => Interlocked.Increment(ref _value);

        #region ICloneable
        public Counter Clone() => new Counter(_value);
        object ICloneable.Clone() => Clone();
        #endregion

        #region IComparable
        public int CompareTo(object obj)
        {
            if (obj is null) return 1;

            var other = obj as Counter;
            if (other is null)
            {
                throw new ArgumentException("A SettingsVersion object is required for comparison.", "obj");
            }

            return CompareTo(other);
        }
        public int CompareTo(Counter other)
        {
            if (other is null)
            {
                return 1;
            }

            if (_value >= int.MaxValue - 10000 && other._value < 0) return -1;
            if (_value < 0 && other._value >= int.MaxValue - 10000) return 1;
            return _value.CompareTo(other._value);
        }

        public static int Compare(Counter left, Counter right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }
            if (left is null)
            {
                return -1;
            }
            return left.CompareTo(right);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Counter;
            if (other is null)
            {
                return false;
            }
            return CompareTo(other) == 0;
        }

        public override int GetHashCode() => _value;

        public static bool operator ==(Counter left, Counter right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(Counter left, Counter right)
        {
            return !(left == right);
        }
        public static bool operator <(Counter left, Counter right)
        {
            return Compare(left, right) < 0;
        }
        public static bool operator >(Counter left, Counter right)
        {
            return Compare(left, right) > 0;
        }
        public static bool operator <=(Counter left, Counter right)
        {
            return Compare(left, right) <= 0;
        }
        public static bool operator >=(Counter left, Counter right)
        {
            return Compare(left, right) >= 0;
        }
        #endregion

        #region Casts
        public static implicit operator int(Counter obj) => obj._value;
        public static implicit operator Counter(int value) => new Counter(value);

        public static implicit operator string(Counter obj) => obj._value.ToString();
        #endregion

        public override string ToString() => _value.ToString();
    }
}
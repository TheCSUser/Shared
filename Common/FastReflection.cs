using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace com.github.TheCSUser.Shared.Common
{
    public static class FastReflectionExtensions
    {
        public static object GetField<T>(this T instance, string fieldName) => FastReflection<T>.GetOrCompileFieldGetter(fieldName)(instance);
        public static void SetField<T>(this T instance, string fieldName, object value) => FastReflection<T>.GetOrCompileFieldSetter(fieldName)(instance, value);
    }

    internal static class FastReflection<T>
    {
        private static readonly Dictionary<string, Func<object, object>> _fieldGetters = new Dictionary<string, Func<object, object>>();
        private static Func<object, object> MissingFieldGetter(string fieldName) => _ => throw new ArgumentException($"Field {fieldName} does not exist.");
        public static Func<object, object> GetOrCompileFieldGetter(string fieldName)
        {
            if (_fieldGetters.TryGetValue(fieldName, out var cachedGetter)) return cachedGetter;

            var field = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                var getter = MissingFieldGetter(fieldName);
                _fieldGetters[fieldName] = getter;
                return getter;
            }
            else
            {
                var objectParameter = Expression.Parameter(typeof(object), string.Empty);
                var fieldExpression = Expression.Field(Expression.Convert(objectParameter, typeof(T)), field);
                var getter = Expression.Lambda<Func<object, object>>(Expression.Convert(fieldExpression, typeof(object)), objectParameter).Compile();
                _fieldGetters[fieldName] = getter;
                return getter;
            }
        }

        private static readonly Dictionary<string, Action<object, object>> _fieldSetters = new Dictionary<string, Action<object, object>>();
        private static Action<object, object> MissingFieldSetter(string fieldName) => (_, __) => throw new ArgumentException($"Field {fieldName} does not exist.");
        public static Action<object, object> GetOrCompileFieldSetter(string fieldName)
        {
            if (_fieldSetters.TryGetValue(fieldName, out var cachedSetter)) return cachedSetter;

            var field = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                var setter = MissingFieldSetter(fieldName);
                _fieldSetters[fieldName] = setter;
                return setter;
            }
            else
            {
                var objectParameter = Expression.Parameter(typeof(object), string.Empty);
                var valueParameter = Expression.Parameter(typeof(object), string.Empty);
                var setterExpression = Expression.Lambda<Action<object, object>>(
                    ExpressionEx.Assign(
                        Expression.Field(Expression.Convert(objectParameter, typeof(T)), field),
                        Expression.Convert(valueParameter, field.FieldType)),
                    objectParameter,
                    valueParameter);
                var setter = setterExpression.Compile();
                _fieldSetters[fieldName] = setter;
                return setter;
            }
        }
    }

    internal static class ExpressionEx
    {
        public static BinaryExpression Assign(Expression left, Expression right)
        {
            var assign = typeof(Assigner<>).MakeGenericType(left.Type).GetMethod("Assign");
            var assignExpr = Expression.Add(left, right, assign);
            return assignExpr;
        }

        private static class Assigner<T>
        {
            public static T Assign(ref T left, T right)
            {
                return (left = right);
            }
        }
    }
}

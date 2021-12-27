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

        public static T GetStaticField<T>(this Type type, string fieldName) => FastReflection.GetOrCompileStaticFieldGetter<T>(type, fieldName)();
    }

    public static class FastReflection
    {
        private static readonly Dictionary<Type, Dictionary<string, Delegate>> _staticFieldGetters = new Dictionary<Type, Dictionary<string, Delegate>>();

        private static bool GetCachedGetter(Type type, string fieldName, out Delegate getter)
        {
            if (!_staticFieldGetters.TryGetValue(type, out var gettersDict)) _staticFieldGetters.Add(type, gettersDict = new Dictionary<string, Delegate>());
            if (gettersDict.TryGetValue(fieldName, out var cachedGetter))
            {
                getter = cachedGetter;
                return true;
            }
            getter = null;
            return false;
        }
        private static void SetCachedGetter(Type type, string fieldName, Delegate getter)
        {
            if (!_staticFieldGetters.TryGetValue(type, out var gettersDict)) _staticFieldGetters.Add(type, gettersDict = new Dictionary<string, Delegate>());
            gettersDict[fieldName] = getter;
        }

        public static Func<TField> GetOrCompileStaticFieldGetter<TField>(Type type, string fieldName)
        {
            if (GetCachedGetter(type, fieldName, out var func))
            {
                if (!(func is Func<TField> cachedGetter)) throw new InvalidCastException($"Field {fieldName} is of type {func.GetType().GetGenericTypeDefinition().GetGenericArguments()[0]}.");
                return cachedGetter;
            }

            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field is null) throw new ArgumentException($"Field {fieldName} does not exist.");

            var lambda = Expression.Lambda(Expression.Field(null, field));
            var getter = (Func<TField>)lambda.Compile();
            SetCachedGetter(type, fieldName, getter);
            return getter;
        }
    }

    public static class FastReflection<T>
    {
        private static readonly Dictionary<string, Func<object, object>> _fieldGetters = new Dictionary<string, Func<object, object>>();
        public static Func<object, object> GetOrCompileFieldGetter(string fieldName)
        {
            if (_fieldGetters.TryGetValue(fieldName, out var cachedGetter)) return cachedGetter;

            var field = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field is null) throw new ArgumentException($"Field {fieldName} does not exist.");

            var objectParameter = Expression.Parameter(typeof(object), string.Empty);
            var fieldExpression = Expression.Field(Expression.Convert(objectParameter, typeof(T)), field);
            var getter = Expression.Lambda<Func<object, object>>(Expression.Convert(fieldExpression, typeof(object)), objectParameter).Compile();
            _fieldGetters[fieldName] = getter;
            return getter;
        }

        private static readonly Dictionary<string, Action<object, object>> _fieldSetters = new Dictionary<string, Action<object, object>>();
        public static Action<object, object> GetOrCompileFieldSetter(string fieldName)
        {
            if (_fieldSetters.TryGetValue(fieldName, out var cachedSetter)) return cachedSetter;

            var field = typeof(T).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field is null) throw new ArgumentException($"Field {fieldName} does not exist.");

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

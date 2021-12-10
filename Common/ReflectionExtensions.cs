using System.Reflection;

namespace com.github.TheCSUser.Shared.Common
{
    public static class ReflectionExtensions
    {
        public static bool IsOverride(this MethodInfo m) => !(m is null) && m.GetBaseDefinition().DeclaringType != m.DeclaringType;
    }
}

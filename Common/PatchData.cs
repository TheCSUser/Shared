using com.github.TheCSUser.Shared.Imports;
using System;
using System.Reflection;

namespace com.github.TheCSUser.Shared.Common
{
    public sealed class PatchData
    {
        public readonly string PatchId;
        public readonly Lazy<MethodInfo> Target;
        public readonly Lazy<MethodInfo> Prefix;
        public readonly Lazy<MethodInfo> Postfix;
        public readonly Lazy<MethodInfo> Transpiler;
        public readonly Lazy<MethodInfo> Finalizer;

        public readonly Action OnUnpatch;
        public readonly Action OnPatch;

        public bool IsApplied { get; set; }

        public PatchData(
            string patchId,
            Func<MethodInfo> target,
            Func<MethodInfo> prefix = null,
            Func<MethodInfo> postfix = null,
            Func<MethodInfo> transpiler = null,
            Func<MethodInfo> finalizer = null,
            Action onPatch = null,
            Action onUnpatch = null)
        {
            PatchId = string.IsNullOrEmpty(patchId.Trim()) ? Guid.NewGuid().ToString() : patchId.Trim();
            Target = target is null ? null : new Lazy<MethodInfo>(target);
            Prefix = prefix is null ? null : new Lazy<MethodInfo>(prefix);
            Postfix = postfix is null ? null : new Lazy<MethodInfo>(postfix);
            Transpiler = transpiler is null ? null : new Lazy<MethodInfo>(transpiler);
            Finalizer = finalizer is null ? null : new Lazy<MethodInfo>(finalizer);
            OnPatch = onPatch;
            OnUnpatch = onUnpatch;
        }
    }
}
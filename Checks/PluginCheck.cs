using com.github.TheCSUser.Shared.Common;

namespace com.github.TheCSUser.Shared.Checks
{
    public abstract class PluginCheck : WithContext, IPluginCheck, IManagedLifecycle
    {
        public static readonly IPluginCheck Subscribed = new ConstantResultCheck(isSubscribed: true);
        public static readonly IPluginCheck NotSubscribed = new ConstantResultCheck(isNotSbuscribed: true);
        public static readonly IPluginCheck Enabled = new ConstantResultCheck(isSubscribed: true, isEnabled: true);
        public static readonly IPluginCheck Disabled = new ConstantResultCheck(isSubscribed: true, isDisabled: true);

        protected PluginCheck(IModContext context) : base(context) { }

        public abstract bool IsSubscribed { get; }
        public abstract bool IsNotSubscribed { get; }
        public abstract bool IsEnabled { get; }
        public abstract bool IsDisabled { get; }

        public abstract void Reset();

        #region ManagedLifecycle
        private IInitializable _lifecycleManager;
        public IInitializable GetLifecycleManager() => _lifecycleManager ?? (_lifecycleManager = new PluginCheckLifecycleManager(this));

        private sealed class PluginCheckLifecycleManager : LifecycleManager
        {
            private readonly PluginCheck _check;

            public PluginCheckLifecycleManager(PluginCheck check) : base(check.Context)
            {
                _check = check;
            }

            protected override bool OnInitialize() => true;

            protected override bool OnTerminate()
            {
                _check.Reset();
                return true;
            }
        }
        #endregion

        #region Dummy
        private sealed class ConstantResultCheck : IPluginCheck
        {
            public bool IsSubscribed { get; private set; }
            public bool IsNotSubscribed { get; private set; }
            public bool IsEnabled { get; private set; }
            public bool IsDisabled { get; private set; }

            public ConstantResultCheck(bool isSubscribed = false, bool isNotSbuscribed = false, bool isEnabled = false, bool isDisabled = false)
            {
                IsSubscribed = isSubscribed;
                IsNotSubscribed = isNotSbuscribed;
                IsEnabled = isEnabled;
                IsDisabled = isDisabled;
            }

            public void Reset() { }
        } 
        #endregion
    }
}

using ColossalFramework;
using ColossalFramework.Plugins;
using com.github.TheCSUser.Shared.Common;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.github.TheCSUser.Shared.Checks
{
    public class ModCheck : PluginCheck
    {
        private static Func<object, object> _pluginsFieldGetter;
        protected static Dictionary<string, PluginManager.PluginInfo> Plugins
        {
            get
            {
                if (!Singleton<PluginManager>.exists)
                {
#if DEV
                    Logging.Log.Shared.Warning($"{nameof(ModCheck)}.{nameof(Plugins)} {nameof(PluginManager)} does not exist");
#endif
                    return null;
                }
                if (_pluginsFieldGetter is null)
                {
                    _pluginsFieldGetter = FastReflection<PluginManager>.GetOrCompileFieldGetter("m_Plugins");
                }
                return (Dictionary<string, PluginManager.PluginInfo>)_pluginsFieldGetter(Singleton<PluginManager>.instance);
            }
        }

        private int _pluginsCount = -1;
        private readonly Func<PluginManager.PluginInfo, bool> _predicate;

        private PluginManager.PluginInfo _plugin = null;
        public PluginManager.PluginInfo Plugin
        {
            get
            {
                Check();
                return _plugin;
            }
        }
        public IUserMod ModInstance => _plugin?.userModInstance as IUserMod;

        public override bool IsSubscribed
        {
            get
            {
                Check();
                return !(_plugin is null);
            }
        }
        public override bool IsNotSubscribed
        {
            get
            {
                Check();
                return _plugin is null;
            }
        }
        public override bool IsEnabled
        {
            get
            {
                Check();
                return !(_plugin is null) && _plugin.isEnabled;
            }
        }
        public override bool IsDisabled
        {
            get
            {
                Check();
                return _plugin is null || !_plugin.isEnabled;
            }
        }

        public ModCheck(IModContext context, string name) : this(context, (p) => p.name == name) { }
        public ModCheck(IModContext context, string name1, string name2) : this(context, (p) => p.name == name1 || p.name == name2) { }
        public ModCheck(IModContext context, Func<string, bool> namePredicate) : this(context, (p) => namePredicate(p.name)) { }
        public ModCheck(IModContext context, Func<PluginManager.PluginInfo, bool> predicate) : base(context) { _predicate = predicate; }

        private void Check()
        {
            try
            {
                var plugins = Plugins;
                if (plugins is null)
                {
                    _pluginsCount = -1;
                    return;
                }
                if (plugins.Count == _pluginsCount) return;
                _pluginsCount = plugins.Count;
                _plugin = plugins.Values.FirstOrDefault(_predicate);
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(Check)} failed", e);
            }
        }

        public override void Reset()
        {
            _pluginsCount = -1;
            _plugin = null;
        }
    }
}

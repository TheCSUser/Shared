using ColossalFramework.Packaging;
using com.github.TheCSUser.Shared.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.github.TheCSUser.Shared.Checks
{
    public class AssetCheck : PluginCheck
    {
        private static Func<HashSet<Package>> _packagesFieldGetter;
        protected static ICollection<Package> Packages
        {
            get
            {
                if (_packagesFieldGetter is null)
                {
                    _packagesFieldGetter = FastReflection.GetOrCompileStaticFieldGetter<HashSet<Package>>(typeof(PackageManager), "m_PackagesIndexTable");
                }
                return _packagesFieldGetter();
            }
        }

        private int _packagesCount = -1;
        private readonly Func<Package, bool> _predicate;

        private Package _package = null;
        public Package Package
        {
            get
            {
                Check();
                return _package;
            }
        }

        private Package.Asset _mainAsset = null;
        public Package.Asset MainAsset
        {
            get
            {
                Check();
                return _mainAsset;
            }
        }

        public override bool IsSubscribed
        {
            get
            {
                Check();
                return !(_mainAsset is null);
            }
        }
        public override bool IsNotSubscribed
        {
            get
            {
                Check();
                return _mainAsset is null;
            }
        }
        public override bool IsEnabled
        {
            get
            {
                Check();
                return !(_mainAsset is null) && _mainAsset.isEnabled;
            }
        }
        public override bool IsDisabled
        {
            get
            {
                Check();
                return _mainAsset is null || !_mainAsset.isEnabled;
            }
        }

        public AssetCheck(IModContext context, string name) : this(context, (p) => p.packageName == name) { }
        public AssetCheck(IModContext context, string name1, string name2) : this(context, (p) => p.packageName == name1 || p.packageName == name2) { }
        public AssetCheck(IModContext context, Func<string, bool> namePredicate) : this(context, (p) => namePredicate(p.packageName)) { }
        public AssetCheck(IModContext context, Func<Package, bool> predicate) : base(context) { _predicate = predicate; }

        private void Check()
        {
            try
            {
                if ((Packages?.Count ?? 0) == _packagesCount) return;
                _packagesCount = (Packages?.Count ?? 0);

                _package = Packages?.FirstOrDefault(_predicate);
                if (_package is null)
                {
                    _mainAsset = null;
                }
                else
                {
                    _mainAsset = ((Dictionary<string, Package.Asset>)_package.GetField("m_IndexTable")).Values.FirstOrDefault(a => a.isMainAsset);
                }
            }
            catch (Exception e)
            {
                Log.Error($"{GetType().Name}.{nameof(Check)} failed", e);
            }
        }

        public override void Reset()
        {
            _packagesCount = -1;
            _package = null;
            _mainAsset = null;
        }
    }
}

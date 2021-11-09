using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Localization;
using System;

namespace com.github.TheCSUser.Shared.Common
{
    public abstract class WithContext : IWithContext
    {
        internal WithContext()
        {
            _context = ModContext.None;
        }
        public WithContext(IModContext context)
        {
            _context = context ?? ModContext.None;
        }

        #region Context
        internal IModContext _context;
        public IModContext Context => _context;

        protected IPatcher Patcher => _context.Patcher;
        protected ILogger Log => _context.Log;
        protected ILocaleLibrary LocaleLibrary => _context.LocaleLibrary;
        protected ILocaleManager LocaleManager => _context.LocaleManager;
        #endregion
    }
}
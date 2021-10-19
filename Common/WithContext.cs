using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Localization;
using System;

namespace com.github.TheCSUser.Shared.Common
{
    public abstract class WithContext<TContext> : IWithContext
        where TContext : IModContext
    {
        public WithContext(TContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _context = context;
        }

        #region Context
        protected readonly TContext _context;
        public IModContext Context => _context;

        protected IPatcher Patcher => _context.Patcher;
        protected ILogger Log => _context.Log;
        protected ILocaleLibrary LocaleLibrary => _context.LocaleLibrary;
        protected ILocaleManager LocaleManager => _context.LocaleManager;
        #endregion
    }

    public abstract class WithContext : IWithContext
    {
        public WithContext(IModContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            _context = context;
        }

        #region Context
        private readonly IModContext _context;
        public IModContext Context => _context;

        protected IPatcher Patcher => _context.Patcher;
        protected ILogger Log => _context.Log;
        protected ILocaleLibrary LocaleLibrary => _context.LocaleLibrary;
        protected ILocaleManager LocaleManager => _context.LocaleManager;
        #endregion
    }
}
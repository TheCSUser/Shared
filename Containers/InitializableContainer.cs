using com.github.TheCSUser.Shared.Common;
using com.github.TheCSUser.Shared.Logging;
using System;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.Containers
{
    internal sealed class InitializableContainer : List<IInitializable>, IInitializableContainer
    {
        public InitializableContainer(IModContext context) : base()
        {
            _context = context;
        }

        #region Context
        private readonly IModContext _context;
        #endregion

        public void Add(Action onInitialize) => Add(new DelegateInitializable(_context, onInitialize, null));
        public void Add(Action onInitialize, Action onTerminate) => Add(new DelegateInitializable(_context, onInitialize, onTerminate));

        private class DelegateInitializable : IInitializable
        {
            private readonly Action _onInitialize;
            private readonly Action _onTerminate;

            public DelegateInitializable(IModContext context, Action onInitialize, Action onTerminate)
            {
                _context = context;
                _onInitialize = onInitialize;
                _onTerminate = onTerminate;
            }

            #region Context
            private readonly IModContext _context;

            private ILogger Log => _context.Log;
            #endregion

            #region Initializable
            private bool _isInitialized;
            public bool IsInitialized => _isInitialized;

            public void Initialize()
            {
                if (IsInitialized) return;
                try
                {
                    if (!(_onInitialize is null)) _onInitialize();
                    _isInitialized = true;
                }
                catch (Exception e)
                {
                    _isInitialized = false;
                    Log.Error($"{nameof(InitializableContainer)}.{nameof(Initialize)} failed", e);
                }
            }
            public void Terminate()
            {
                if (!IsInitialized) return;
                try
                {
                    if (!(_onTerminate is null)) _onTerminate();
                    _isInitialized = false;
                }
                catch (Exception e)
                {
                    _isInitialized = false;
                    Log.Error($"{nameof(InitializableContainer)}.{nameof(Terminate)} failed", e);
                }
            }
            #endregion
        }
    }
}
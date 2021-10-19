using com.github.TheCSUser.Shared.Common;
using System;
using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.Containers
{
    internal sealed class InitializableContainer : List<IInitializable>, IInitializableContainer
    {
        private readonly IModContext _context;

        public InitializableContainer(IModContext context) : base()
        {
            _context = context;
        }

        public void Add(Action onInitialize) => Add(new DelegateInitializable(_context, onInitialize, null));
        public void Add(Action onInitialize, Action onTerminate) => Add(new DelegateInitializable(_context, onInitialize, onTerminate));

        private sealed class DelegateInitializable : WithContext, IInitializable
        {
            private readonly Action _onInitialize;
            private readonly Action _onTerminate;

            public DelegateInitializable(IModContext context, Action onInitialize, Action onTerminate) : base(context)
            {
                _onInitialize = onInitialize;
                _onTerminate = onTerminate;
            }

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
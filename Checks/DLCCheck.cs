using com.github.TheCSUser.Shared.Common;

namespace com.github.TheCSUser.Shared.Checks
{
    public class DLCCheck : WithContext, IDLCCheck
    {
        public static readonly IDLCCheck Available = new ConstantResultCheck(isAvailable: true);
        public static readonly IDLCCheck NotAvailable = new ConstantResultCheck(isNotAvailable: true);
        public static readonly IDLCCheck Enabled = new ConstantResultCheck(isAvailable: true, isEnabled: true);
        public static readonly IDLCCheck Disabled = new ConstantResultCheck(isAvailable: true, isDisabled: true);

        private readonly SteamHelper.DLC _dlc;

        public bool IsAvailable => SteamHelper.IsDLCAvailable(_dlc);
        public bool IsNotAvailable => !SteamHelper.IsDLCAvailable(_dlc);
        public bool IsEnabled => SteamHelper.IsDLCOwned(_dlc);
        public bool IsDisabled => !SteamHelper.IsDLCOwned(_dlc);

        public DLCCheck(IModContext context, SteamHelper.DLC dlc) : base(context)
        {
            _dlc = dlc;
        }

        #region Dummy
        private sealed class ConstantResultCheck : IDLCCheck
        {
            public bool IsAvailable { get; private set; }
            public bool IsNotAvailable { get; private set; }
            public bool IsEnabled { get; private set; }
            public bool IsDisabled { get; private set; }

            public ConstantResultCheck(bool isAvailable = false, bool isNotAvailable = false, bool isEnabled = false, bool isDisabled = false)
            {
                IsAvailable = isAvailable;
                IsNotAvailable = isNotAvailable;
                IsEnabled = isEnabled;
                IsDisabled = isDisabled;
            }

            public void Reset() { }
        } 
        #endregion
    }
}

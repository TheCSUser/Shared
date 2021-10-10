using com.github.TheCSUser.Shared.Logging;
using com.github.TheCSUser.Shared.UserInterface.Localization;

namespace com.github.TheCSUser.Shared.Common
{
    public interface IModContext
    {
        ILocaleLibrary LocaleLibrary { get; }
        ILocaleManager LocaleManager { get; }
        ILogger Log { get; }
        IMod Mod { get; }
        IPatcher Patcher { get; }
    }
}
using ICities;

namespace com.github.TheCSUser.Shared.Common
{
    public enum ApplicationMode : byte
    {
        MainMenu = 0,
        Game = 1,
        MapEditor = 2,
        AssetEditor = 3,
        ThemeEditor = 4,
        ScenarioEditor = 5,
    }

    public static class ApplicationModeExtensions
    {
        public static ApplicationMode ToApplicationMode(this AppMode appMode)
        {
            switch (appMode)
            {
                case AppMode.Game: return ApplicationMode.Game;
                case AppMode.MapEditor: return ApplicationMode.MapEditor;
                case AppMode.AssetEditor: return ApplicationMode.AssetEditor;
                case AppMode.ThemeEditor: return ApplicationMode.ThemeEditor;
                case AppMode.ScenarioEditor: return ApplicationMode.ScenarioEditor;
                default: return ApplicationMode.MainMenu;
            }
        }
    }
}

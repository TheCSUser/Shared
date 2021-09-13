using ColossalFramework.UI;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Components
{
    public interface ILocalizedGroup : ILocalizedComponent
    {
        UIPanel Panel { get; }
        UILabel Label { get; }
        ILocalizedUIBuilder Builder { get; }
    }
}
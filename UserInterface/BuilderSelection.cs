using com.github.TheCSUser.Shared.UserInterface.Localization;

namespace com.github.TheCSUser.Shared.UserInterface
{
    public class BuilderSelection
    {
        public UIBuilder Builder { get; }
        public LocalizedUIBuilder LocalizedBuilder { get; }

        public BuilderSelection(object builder)
        {
            if (builder is UIBuilder regularBuilder) Builder = regularBuilder;
            if (builder is LocalizedUIBuilder localizedBuilder) LocalizedBuilder = localizedBuilder;
        }
    }
}

using System.Collections.Generic;

namespace com.github.TheCSUser.Shared.UserInterface.Localization
{
    public interface ILanguageDictionary: IDictionary<string, string>
    {
        string Translate(string phrase, params string[] values);
    }
}
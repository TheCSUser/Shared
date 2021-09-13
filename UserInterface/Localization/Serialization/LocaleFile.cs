using System.Collections.Generic;
using System.Xml.Serialization;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Serialization
{
    [XmlType("resources")]
    public sealed class LocaleFile : List<SerializedPhrase> { }
}
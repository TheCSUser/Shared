using System.Xml.Serialization;

namespace com.github.TheCSUser.Shared.UserInterface.Localization.Serialization
{
    [XmlType("string")]
    public sealed class SerializedPhrase
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
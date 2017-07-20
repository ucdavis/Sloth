using System;
using System.Xml;
using System.Xml.Serialization;

namespace Sloth.Xml
{
    public abstract class KfsXmlElement
    {
        public const string KfsNamespace = "http://www.kuali.org/kfs/gl/collector";

        [XmlNamespaceDeclarations]
        public readonly XmlSerializerNamespaces Namespaces = new XmlSerializerNamespaces(new[]
        {
            new XmlQualifiedName(string.Empty, KfsNamespace), 
        });
    }
}

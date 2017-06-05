using System;
using System.Xml;
using System.Xml.Serialization;

namespace Sloth.Xml
{
    public abstract class KfsXmlElement
    {
        protected KfsXmlElement()
        {
            Namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName(string.Empty, KfsNamespace), 
            });
        }

        public const string KfsNamespace = "http://www.kuali.org/kfs/gl/collector";

        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Namespaces { get; }
    }
}

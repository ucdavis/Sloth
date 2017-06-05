using System;
using System.Xml.Serialization;

namespace Sloth.Xml
{
    public static class StaticHelpers<T>
    {
        public static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(T));
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Sloth.Xml.Types
{
    [XmlRoot("glEntry", Namespace = KfsNamespace, IsNullable = false)]
    public class EntryWithDetail : Entry
    {
        [XmlIgnore]
        public Detail Detail { get; set; }
    }
}

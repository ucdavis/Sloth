using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Sloth.Xml.Types;

namespace Sloth.Xml
{
    /// <remarks/>
    [XmlRoot("batch", Namespace = "http://www.kuali.org/kfs/gl/collector", IsNullable = false)]
    public class Batch : KfsXmlElement, IXmlSerializable
    {
        public Batch()
        {
            Entries = new List<Entry>();
        }

        /// <remarks/>
        [Required]
        public Header Header { get; set; }


        /// <remarks/>
        public List<Entry> Entries { get; set; }

        /// <remarks/>
        [Required]
        public Trailer Trailer { get; set; }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("header"))
                {
                    Header = DeserializeNode<Header>(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("trailer"))
                {
                    Trailer = DeserializeNode<Trailer>(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("glEntry"))
                {
                    var entry = DeserializeNode<EntryWithDetail>(reader);

                    if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("detail"))
                    {
                        var detail = DeserializeNode<Detail>(reader);
                        entry.Detail = detail;
                    }

                    Entries.Add(entry);
                }
                else
                {
                    reader.Read();
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            SerializeNode(writer, Header);

            foreach (var entry in Entries)
            {
                SerializeNode(writer, entry);
                if (entry is EntryWithDetail entryWithDetail && entryWithDetail.Detail != null)
                {
                    SerializeNode(writer, entryWithDetail.Detail);
                }
            }

            SerializeNode(writer, Trailer);
        }

        private T DeserializeNode<T>(XmlReader reader) where T : KfsXmlElement
        {
            var s = StaticHelpers<T>.XmlSerializer;
            return (T)s.Deserialize(reader);
        }

        private void SerializeNode<T>(XmlWriter writer, T node) where T : KfsXmlElement
        {
            var s = StaticHelpers<T>.XmlSerializer;
            s.Serialize(writer, node, Namespaces);
        }
    }
}

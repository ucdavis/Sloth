using System.Xml.Serialization;

namespace Sloth.Integrations.Cybersource {
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Report
    {
        public Batches Batches { get; set; }

        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute(DataType="NMTOKEN")]
        public string Version { get; set; }

        [XmlAttribute("MerchantID")]
        public string MerchantId { get; set; }

        [XmlAttribute()]
        public string ReportStartDate { get; set; }

        [XmlAttribute()]
        public string ReportEndDate { get; set; }
    }
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Batches
    {
        [XmlElement("Batch")]
        public Batch[] Batch { get; set; }
    }
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Batch
    {
        public Requests Requests { get; set; }

        [XmlAttribute("BatchID")]
        public string BatchId { get; set; }

        [XmlAttribute()]
        public string BatchDate { get; set; }
    }
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Requests
    {
        [XmlElement("Request")]
        public Request[] Request { get; set; }
    }
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Request
    {
        public string TransactionReferenceNumber { get; set; }

        public string TransactionId { get; set; }

        public string PaymentMethod { get; set; }

        public string CurrencyCode { get; set; }

        public string CustomerId { get; set; }

        public decimal Amount { get; set; }

        public LineItems LineItems { get; set; }

        public string Application { get; set; }

        public string WalletType { get; set; }

        public Channel Channel { get; set; }

        public string ProcessorTID { get; set; }

        public string NetworkTransactionID { get; set; }

        public string EffectiveDate { get; set; }

        [XmlAttribute()]
        public string RequestID { get; set; }

        [XmlAttribute()]
        public string MerchantReferenceNumber { get; set; }
    }
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class LineItems
    {
        [XmlElement("LineItem")]
        public LineItem[] LineItem { get; set; }
    }
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class LineItem
    {
        public string InvoiceNumber { get; set; }

        [XmlAttribute()]
        public string Number { get; set; }
    }
    
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Channel
    {
        public string Type { get; set; }

        public string SubType { get; set; }
    }
}

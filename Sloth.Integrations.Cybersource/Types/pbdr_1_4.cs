using System.Xml.Serialization;

namespace Sloth.Integrations.Cybersource {
    
    
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Report {
        
        private Batches batchesField;
        
        private string nameField;
        
        private string versionField;
        
        private string merchantIDField;
        
        private string reportStartDateField;
        
        private string reportEndDateField;
        
        
        public Batches Batches {
            get {
                return batchesField;
            }
            set {
                batchesField = value;
            }
        }
        
        
        [XmlAttribute()]
        public string Name {
            get {
                return nameField;
            }
            set {
                nameField = value;
            }
        }
        
        
        [XmlAttribute(DataType="NMTOKEN")]
        public string Version {
            get {
                return versionField;
            }
            set {
                versionField = value;
            }
        }
        
        
        [XmlAttribute()]
        public string MerchantID {
            get {
                return merchantIDField;
            }
            set {
                merchantIDField = value;
            }
        }
        
        
        [XmlAttribute()]
        public string ReportStartDate {
            get {
                return reportStartDateField;
            }
            set {
                reportStartDateField = value;
            }
        }
        
        
        [XmlAttribute()]
        public string ReportEndDate {
            get {
                return reportEndDateField;
            }
            set {
                reportEndDateField = value;
            }
        }
    }
    
    
    
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Batches {
        
        private Batch[] batchField;
        
        
        [XmlElementAttribute("Batch")]
        public Batch[] Batch {
            get {
                return batchField;
            }
            set {
                batchField = value;
            }
        }
    }
    
    
    
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Batch {
        
        private Requests requestsField;
        
        private string batchIDField;
        
        private string batchDateField;
        
        
        public Requests Requests {
            get {
                return requestsField;
            }
            set {
                requestsField = value;
            }
        }
        
        
        [XmlAttribute()]
        public string BatchID {
            get {
                return batchIDField;
            }
            set {
                batchIDField = value;
            }
        }
        
        
        [XmlAttribute()]
        public string BatchDate {
            get {
                return batchDateField;
            }
            set {
                batchDateField = value;
            }
        }
    }
    
    
    
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Requests {
        
        private Request[] requestField;
        
        
        [XmlElementAttribute("Request")]
        public Request[] Request {
            get {
                return requestField;
            }
            set {
                requestField = value;
            }
        }
    }
    
    
    
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Request {
        
        private string transactionReferenceNumberField;
        
        private string transactionIdField;
        
        private string paymentMethodField;
        
        private string currencyCodeField;
        
        private string customerIdField;
        
        private string amountField;
        
        private LineItems lineItemsField;
        
        private string applicationField;
        
        private string walletTypeField;
        
        private Channel channelField;
        
        private string processorTIDField;
        
        private string networkTransactionIDField;
        
        private string effectiveDateField;
        
        private string requestIDField;
        
        private string merchantReferenceNumberField;
        
        
        public string TransactionReferenceNumber {
            get {
                return transactionReferenceNumberField;
            }
            set {
                transactionReferenceNumberField = value;
            }
        }
        
        
        public string TransactionId {
            get {
                return transactionIdField;
            }
            set {
                transactionIdField = value;
            }
        }
        
        
        public string PaymentMethod {
            get {
                return paymentMethodField;
            }
            set {
                paymentMethodField = value;
            }
        }
        
        
        public string CurrencyCode {
            get {
                return currencyCodeField;
            }
            set {
                currencyCodeField = value;
            }
        }
        
        
        public string CustomerId {
            get {
                return customerIdField;
            }
            set {
                customerIdField = value;
            }
        }
        
        
        public string Amount {
            get {
                return amountField;
            }
            set {
                amountField = value;
            }
        }
        
        
        public LineItems LineItems {
            get {
                return lineItemsField;
            }
            set {
                lineItemsField = value;
            }
        }
        
        
        public string Application {
            get {
                return applicationField;
            }
            set {
                applicationField = value;
            }
        }
        
        
        public string WalletType {
            get {
                return walletTypeField;
            }
            set {
                walletTypeField = value;
            }
        }
        
        
        public Channel Channel {
            get {
                return channelField;
            }
            set {
                channelField = value;
            }
        }
        
        
        public string ProcessorTID {
            get {
                return processorTIDField;
            }
            set {
                processorTIDField = value;
            }
        }
        
        
        public string NetworkTransactionID {
            get {
                return networkTransactionIDField;
            }
            set {
                networkTransactionIDField = value;
            }
        }
        
        
        public string EffectiveDate {
            get {
                return effectiveDateField;
            }
            set {
                effectiveDateField = value;
            }
        }
        
        
        [XmlAttribute()]
        public string RequestID {
            get {
                return requestIDField;
            }
            set {
                requestIDField = value;
            }
        }
        
        
        [XmlAttribute()]
        public string MerchantReferenceNumber {
            get {
                return merchantReferenceNumberField;
            }
            set {
                merchantReferenceNumberField = value;
            }
        }
    }
    
    
    
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class LineItems {
        
        private LineItem[] lineItemField;
        
        
        [XmlElementAttribute("LineItem")]
        public LineItem[] LineItem {
            get {
                return lineItemField;
            }
            set {
                lineItemField = value;
            }
        }
    }
    
    
    
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class LineItem {
        
        private string invoiceNumberField;
        
        private string numberField;
        
        
        public string InvoiceNumber {
            get {
                return invoiceNumberField;
            }
            set {
                invoiceNumberField = value;
            }
        }
        
        
        [XmlAttribute()]
        public string Number {
            get {
                return numberField;
            }
            set {
                numberField = value;
            }
        }
    }
    
    
    
    
    [XmlType(AnonymousType=true)]
    [XmlRoot(IsNullable=false)]
    public class Channel {
        
        private string typeField;
        
        private string subTypeField;
        
        
        public string Type {
            get {
                return typeField;
            }
            set {
                typeField = value;
            }
        }
        
        
        public string SubType {
            get {
                return subTypeField;
            }
            set {
                subTypeField = value;
            }
        }
    }
}

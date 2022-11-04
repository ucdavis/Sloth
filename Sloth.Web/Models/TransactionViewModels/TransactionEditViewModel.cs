using System;
using System.Collections.Generic;

namespace Sloth.Web.Models.TransactionViewModels
{

    public class TransactionEditViewModel
    {
        public string Id { get; set; }
        public List<TransferEditModel> Transfers { get; set; } = new();

        public class TransferEditModel
        {
            public string Id { get; set; }
            public string FinancialSegmentString { get; set; }
        }
    }
}

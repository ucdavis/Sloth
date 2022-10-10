using System.Collections.Generic;
using Sloth.Core.Models;

namespace Sloth.Web.Models.BlobViewModels
{
    public class BlobDetailsViewModel
    {
        public Blob Blob { get; set; }
        public string Contents { get; set; }
    }
}

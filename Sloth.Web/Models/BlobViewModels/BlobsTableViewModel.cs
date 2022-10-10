using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sloth.Core.Models;

namespace Sloth.Web.Models.BlobViewModels
{
    public class BlobsTableViewModel
    {
        public BlobsTableViewModel()
        {
            Blobs = new List<Blob>();
        }
        public List<Blob> Blobs { get; set; }
        public string TeamSlug { get; set; }
    }
}

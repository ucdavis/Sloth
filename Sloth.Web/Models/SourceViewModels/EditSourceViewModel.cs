using System.ComponentModel.DataAnnotations;

namespace Sloth.Web.Models.SourceViewModels
{
    public class EditSourceViewModel : SourceViewModel
    {
        public EditSourceViewModel()
        {
            KfsFtpPasswordKeyDirty = false;
        }

        public bool KfsFtpPasswordKeyDirty { get; set; }
        public string Id { get; internal set; }
    }
}

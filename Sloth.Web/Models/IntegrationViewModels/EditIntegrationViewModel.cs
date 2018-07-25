using System;

namespace Sloth.Web.Models.IntegrationViewModels
{
    public class EditIntegrationViewModel : IntegrationViewModel
    {
        public EditIntegrationViewModel()
        {
            ReportPasswordDirty = false;
        }

        public bool ReportPasswordDirty { get; set; }
    }
}

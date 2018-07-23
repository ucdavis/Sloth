using System;
using System.Collections.Generic;
using Sloth.Core.Models;

namespace Sloth.Web.Models.SystemViewModels
{
    public class SystemViewModel
    {
        public IEnumerable<User> AdminUsers { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sloth.Core.Views.Shared
{
    public class EmailButtonModel
    {
        public string Text { get; set; }
        // host will be supplied by the notification service
        public string UrlPath { get; set; }
    }
}

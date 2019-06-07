using System;
using System.Collections.Generic;
using System.Text;

namespace Sloth.Integrations.Cybersource.Resources
{
    public static class ApplicationReplyTypes
    {
        public static readonly string Auth = "ics_auth";
        public static readonly string Bill = "ics_bill";
        public static readonly string Credit = "ics_credit";
        public static readonly string Sale = "ics_sale";
        public static readonly string CreateSubscription = "ics_pay_subscription_create";
    }
}

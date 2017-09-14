using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Sloth.Api.Attributes
{
    public class ListMinLengthAttribute : MinLengthAttribute
    {
        public ListMinLengthAttribute(int length) : base(length)
        {
        }

        public override bool IsValid(object value)
        {
            var list = value as IList;
            if (list == null)
                return base.IsValid(value);

            return list.Count >= Length;
        }
    }
}

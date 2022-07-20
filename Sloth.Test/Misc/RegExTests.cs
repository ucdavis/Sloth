using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Sloth.Core.Extensions;

namespace Sloth.Test.Misc
{
    public class RegExTests
    {
        [Theory]
        [InlineData("Aggie Enterprise", "AggieEnterprise")]
        [InlineData("Aggie*&^%&^%&^$&^%$Enterprise", "AggieEnterprise")]
        public void TestStripToGlReferenceField(string passed, string expected)
        {
            var rtValue = passed.StripToGlReferenceField(25);
            rtValue.ShouldBe(expected);
        }
    }
}

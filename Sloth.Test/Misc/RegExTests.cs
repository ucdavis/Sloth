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
        [InlineData("123456789012345678901234567890", "1234567890123456789012345")]
        [InlineData("abcdefghijklmnop", "abcdefghijklmnop")]
        [InlineData("ABCDEFGHIJKLMNOP", "ABCDEFGHIJKLMNOP")]
        [InlineData("qrstuvwkyz", "qrstuvwkyz")]
        [InlineData("QRSTUVWKYZ", "QRSTUVWKYZ")]
        [InlineData("__--'", "__--")]
        public void TestStripToGlReferenceField25(string passed, string expected)
        {
            var rtValue = passed.StripToGlReferenceField(25);
            rtValue.ShouldBe(expected);
        }
    }
}

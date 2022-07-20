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
        [InlineData("!@#$%^&*()+=~;:<>,.?[]{}|Meh'", "Meh")]
        public void TestStripToGlReferenceField25(string passed, string expected)
        {
            var rtValue = passed.StripToGlReferenceField(25);
            rtValue.ShouldBe(expected);
        }


        /// <summary>
        /// @"[^0-9a-zA-Z\ \.\-_]+"
        /// </summary>
        /// <param name="passed"></param>
        /// <param name="expected"></param>
        [Theory]
        [InlineData("Aggie Enterprise", "Aggie Enterprise")]
        [InlineData("Aggie*&^%&^%&^$&^%$Enterprise", "AggieEnterprise")]
        [InlineData("123456789012345678901234567890", "1234567890123456789012345")]
        [InlineData("abcdefghijklmnop", "abcdefghijklmnop")]
        [InlineData("ABCDEFGHIJKLMNOP", "ABCDEFGHIJKLMNOP")]
        [InlineData("qrstuvwkyz", "qrstuvwkyz")]
        [InlineData("QRSTUVWKYZ", "QRSTUVWKYZ")]
        [InlineData("__--.'", "__--.")]
        [InlineData("!@#$%^&*()+=~;:<>,.?[]{}|Meh'", ".Meh")]        
        public void TestStripToErpDescription25(string passed, string expected)
        {
            var rtValue = passed.StripToErpDescription(25);
            rtValue.ShouldBe(expected);
        }

        /// <summary>
        /// @"[^0-9a-zA-Z\ \-_]+"
        /// </summary>
        /// <param name="passed"></param>
        /// <param name="expected"></param>
        [Theory]
        [InlineData("Aggie Enterprise", "Aggie Enterprise")]
        [InlineData("Aggie*&^%&^%&^$&^%$Enterprise", "AggieEnterprise")]
        [InlineData("123456789012345678901234567890", "1234567890123456789012345")]
        [InlineData("abcdefghijklmnop", "abcdefghijklmnop")]
        [InlineData("ABCDEFGHIJKLMNOP", "ABCDEFGHIJKLMNOP")]
        [InlineData("qrstuvwkyz", "qrstuvwkyz")]
        [InlineData("QRSTUVWKYZ", "QRSTUVWKYZ")]
        [InlineData("__--.'", "__--.")]
        [InlineData("!@#$%^&*()+=~;:<>,.?[]{}|Meh'", "Meh")]
        public void TestStripToErpName25(string passed, string expected)
        {
            var rtValue = passed.StripToErpName(25);
            rtValue.ShouldBe(expected);
        }        
    }
}

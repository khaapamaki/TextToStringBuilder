namespace UnitTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TextToStringBuilder;

    // todo verbatim mode tests

    [TestClass]
    public class HelperUnitTest
    {
        [TestMethod]
        [DataRow("", "")]
        [DataRow("abc", "abc")]
        [DataRow("\t", "    ")]
        [DataRow("\tHello", "    Hello")]
        [DataRow("a\tHello", "a   Hello")]
        [DataRow("abc\tHello", "abc Hello")]
        [DataRow("abcd\tHello", "abcd    Hello")]
        [DataRow("\t Hello", "     Hello")]
        [DataRow("a\t Hello", "a    Hello")]
        [DataRow("abc\t Hello", "abc  Hello")]
        [DataRow("abcd\t Hello", "abcd     Hello")]
        public void ExpandTabs_TabSizeFour_ReturnsCorrectValues(string testString, string expected)
        {
            // ACT
            var result = Helper.ExpandTabs(testString, 4);

            // ASSERT
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("  .Append(\"text\").Append(\"TEXT\")  ", "textTEXT")]
        [DataRow("  .Append(\"text\")\n  .Append(\"TEXT\")  ", "textTEXT")]
        [DataRow("  .AppendLine(\"text\").Append(\"TEXT\")", "text\r\nTEXT")]
        [DataRow("  .AppendLine(\"text\").AppendLine(\"TEXT\")", "text\r\nTEXT\r\n")]
        [DataRow("  .AppendLine(\"text\\\").Append(\"TEXT\")", "text\").Append(\r\n")]
        [DataRow("  .AppendLine(\"text\\\") \n\n .Append(\"TEXT\")", "text\") \n\n .Append(\r\n")]
        [DataRow("  .Append()", "")]
        [DataRow("  .Append(\"\")", "")]
        [DataRow("  .AppendLine()", "\r\n")]
        [DataRow("  .AppendLine(\"\")", "\r\n")]
        [DataRow(".AppendLine(\"a\")", "a\r\n")]
        [DataRow(".AppendLine(\"a\")  ", "a\r\n")]
        [DataRow(".AppendLine(\" 'Ö'\\\" \")  ", " 'Ö'\" \r\n")]
        public void UnwrapAppends_Cases_ReturnCorrectValues(string testString, string expected)
        {
            string result = Helper.UnwrapAppends(testString);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("Neque porro quisquam est qui dolorem ipsum quia dolor sit amet", "porro", 6, true)]
        [DataRow("Neque porro quisquam est qui dolorem ipsum quia dolor sit amet", "porro", 5, false)]
        [DataRow("Neque porro quisquam est qui dolorem ipsum quia dolor sit amet", "Neque", 0, true)]
        [DataRow("Neque porro quisquam est qui dolorem ipsum quia dolor sit amet", "Neque", 1, false)]
        [DataRow("Neque", "e", 4, true)]
        [DataRow("Neque", "ee", 4, false)]
        [DataRow("Neque", "a", 4, false)]

        public void IsFollowedBy_Cases_ReturnCorrectValues(string data, string testString, int pos, bool expected)
        {
            bool result = Helper.IsFollowedBy(data, testString, pos);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("abc\"", 0, false, 3)]
        [DataRow("abc", 0, false, null)]
        [DataRow("ab\\\"cd\"ef\"", 0, false, 6)]
        [DataRow("ab\\\"cd\"ef\"", 3, false, 3)]
        [DataRow("ab\\\"cd\"ef\"", 4, false, 6)]
        [DataRow("abc\"cd\"ef\"x", 4, false, 6)]
        [DataRow("ab\\\"cd", 0, false, null)]
        [DataRow("ab\\\\\"cd", 0, false, 4)]
        [DataRow("ab\\\\\\\"cd", 0, false, null)]
        [DataRow("ab\\\\\\\\\"cd", 0, false, 6)]
        public void FindEndQuote_Cases_ReturnCorrectValues(string data, int pos, bool verbatim, int? expected)
        {
            int? result = Helper.FindEndQuote(data, pos, verbatim);

            Assert.AreEqual(expected, result);
        }
    }
}

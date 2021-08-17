/* Copyright (c) rubicon IT GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;
using NUnit.Framework;

namespace LicenseHeaderManager.Core.Tests
{
  [TestFixture]
  public class CommentParserTest
  {
    [Test]
    public void Parse_CommentBeforeRegionWithText_ReturnsHeaderWithoutNonCommentTextWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "",
                      "//This is a comment.",
                      "#region copyright",
                      "//This is a comment.",
                      "This is not a comment.",
                      "#endregion"
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "",
                         "//This is a comment.",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_EmptyFile_ReturnsEmptyString ()
    {
      const string input = "";
      const string expected = "";

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_EmptyLinesBeforeComment_ReturnsHeaderWithoutNonCommentTextWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "",
                      "",
                      "//This is a comment.",
                      "This is not a comment."
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "",
                         "",
                         "//This is a comment.",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_EndRegionWithSpace_ReturnsHeaderWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "#region ",
                      "//This is a comment.",
                      "#endregion",
                      ""
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "#region ",
                         "//This is a comment.",
                         "#endregion",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_EveryCommentPossible_ReturnsHeaderWithoutNonCommentText ()
    {
      var input = new[]
                  {
                      "",
                      " ",
                      "#region copyright",
                      "  ",
                      "//This is a comment.",
                      "",
                      "/*This is also part",
                      "",
                      "of the header*/",
                      "#region something else",
                      "",
                      "//So is this.",
                      "#endregion",
                      " ",
                      "#endregion",
                      " ",
                      "This is not a comment"
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "",
                         " ",
                         "#region copyright",
                         "  ",
                         "//This is a comment.",
                         "",
                         "/*This is also part",
                         "",
                         "of the header*/",
                         "#region something else",
                         "",
                         "//So is this.",
                         "#endregion",
                         " ",
                         "#endregion",
                         " ",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_HeaderOnly_ReturnsHeader ()
    {
      var input = new[]
                  { "//This is a comment." }.JoinWithNewLine();

      var expected = new[]
                     {
                         "//This is a comment."
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_HeaderOnlyWithEmptyLines_ReturnsHeader ()
    {
      var input = new[]
                  {
                      "",
                      "",
                      "//This is a comment.",
                      "",
                      ""
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "",
                         "",
                         "//This is a comment.",
                         "",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_MissingEndRegion_ReturnsEmptyString ()
    {
      var input = new[]
                  {
                      "#region copyright",
                      "//This is a comment.",
                      "This is not a comment."
                  }.JoinWithNewLine();

      const string expected = "";

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_MixedComments_ReturnsHeaderWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "/* This is a comment that",
                      "   spans multiple lines.  */",
                      "// This is also part of the header.",
                      "",
                      "//This is another comment."
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "/* This is a comment that",
                         "   spans multiple lines.  */",
                         "// This is also part of the header.",
                         "",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_MultiLineBeginEndComment_ReturnsHeaderWithoutNonCommentTextWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "/* This is a comment that ",
                      "   spans multiple lines.  */",
                      "This is not a comment."
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "/* This is a comment that ",
                         "   spans multiple lines.  */",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_MultiLineHeaderOnly_ReturnsHeaderWithoutEndingNewLine ()
    {
      var input = new[]
                  {
                      "/*",
                      "This is a comment that ",
                      "spans multiple lines.",
                      "*/"
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "/*",
                         "This is a comment that ",
                         "spans multiple lines.",
                         "*/"
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_MultipleBeginEndComments_ReturnsHeaderWithoutNonCommentTextWithEndingNewLine ()
    {
      var header = new[]
                   {
                       "/* This is a comment that */",
                       "/* spans multiple lines.  */",
                       "This is not a comment"
                   }.JoinWithNewLine();

      var expected = new[]
                     {
                         "/* This is a comment that */",
                         "/* spans multiple lines.  */",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (header, expected);
    }

    [Test]
    public void Parse_MultipleLineBeginEndCommentsWithEmptyLine_ReturnsHeaderWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "/* This is a comment that ",
                      "   spans multiple lines.  */",
                      "",
                      "/* This is another comment. */"
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "/* This is a comment that ",
                         "   spans multiple lines.  */",
                         "",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_MultipleLineComments_ReturnsHeaderWithoutNonCommentText ()
    {
      var input = new[]
                  {
                      "//This is a comment that",
                      "//spans multiple lines.",
                      "This is not a comment"
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "//This is a comment that",
                         "//spans multiple lines.",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_MultipleLineCommentsWithEmptyLine_ReturnsHeaderWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "//This is a comment that",
                      "//spans multiple lines.",
                      "",
                      "//This is another comment."
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "//This is a comment that",
                         "//spans multiple lines.",
                         "",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_MultipleRegions_ReturnsHeaderWithoutNonCommentText ()
    {
      var input = new[]
                  {
                      "#region copyright",
                      "//This is a comment.",
                      "",
                      "#endregion",
                      "#region something else",
                      "",
                      "#endregion",
                      "This is not a comment."
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "#region copyright",
                         "//This is a comment.",
                         "",
                         "#endregion",
                         "#region something else",
                         "",
                         "#endregion",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_NestedMissingEndRegion_ReturnsEmptyString ()
    {
      var input = new[]
                  {
                      "#region copyright",
                      "//This is a comment.",
                      "#region something else",
                      "",
                      "#endregion",
                      "This is not a comment."
                  }.JoinWithNewLine();

      const string expected = "";

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_NestedRegions_ReturnsHeaderWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "#region copyright",
                      "//This is a comment.",
                      "",
                      "#region something else",
                      "",
                      "#endregion",
                      "#endregion",
                      "This is not a comment."
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "#region copyright",
                         "//This is a comment.",
                         "",
                         "#region something else",
                         "",
                         "#endregion",
                         "#endregion",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_NoHeader_ReturnsEmptyString ()
    {
      var input = new[]
                  {
                      "This is not a comment.",
                      "//This is not part of the header."
                  }.JoinWithNewLine();

      const string expected = "";

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_NoHeaderButNormalRegion_ReturnsEmptyString ()
    {
      var input = new[]
                  {
                      "#region normal region",
                      "This is not a comment.",
                      "#endregion"
                  }.JoinWithNewLine();

      const string expected = "";

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_NoHeaderWithEmptyLines_ReturnsEmptyString ()
    {
      var input = new[]
                  {
                      "",
                      "",
                      "This is not a comment.",
                      "//This is not part of the header."
                  }.JoinWithNewLine();

      const string expected = "";

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_Region_ReturnsHeaderWithoutNonCommentTextWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "#region copyright",
                      "//This is a comment.",
                      "#endregion",
                      "This is not a comment."
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "#region copyright",
                         "//This is a comment.",
                         "#endregion",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_RegionWithEmptyLines_ReturnsHeaderWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "#region copyright",
                      "//This is a comment.",
                      "",
                      "//This is also part of the header.",
                      "#endregion",
                      "",
                      "//This is another comment."
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "#region copyright",
                         "//This is a comment.",
                         "",
                         "//This is also part of the header.",
                         "#endregion",
                         "",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_TextInRegion_ReturnsEmptyString ()
    {
      var input = new[]
                  {
                      "#region copyright",
                      "//This is a comment.",
                      "This is not a comment.",
                      "#endregion"
                  }.JoinWithNewLine();

      const string expected = "";

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_RegionsInText_ReturnsHeaderWithEndingNewLine ()
    {
      var input = new[]
                  {
                      "#region copyright",
                      "#region something",
                      "",
                      "#endregion",
                      "#endregion",
                      "#region something",
                      "#region something"
                  }.JoinWithNewLine();

      var expected = new[]
                     {
                         "#region copyright",
                         "#region something",
                         "",
                         "#endregion",
                         "#endregion",
                         ""
                     }.JoinWithNewLine();

      AssertCorrectlyParsed (input, expected);
    }

    [Test]
    public void Parse_MissingEndComment_ThrowsParseException ()
    {
      var input = new[]
                  {
                      "/* This is a comment.",
                      "This is not a comment."
                  }.JoinWithNewLine();

      AssertThrowsException (input);
    }

    [Test]
    public void Parse_EndRegionWithoutRegion_ThrowsParseException ()
    {
      var header = new[]
                   {
                       "#endregion"
                   }.JoinWithNewLine();

      AssertThrowsException (header);
    }

    [Test]
    public void Parse_EndRegionWithoutRegionAndSplitEndregion_ThrowsParseException ()
    {
      var header = new[]
                   {
                       "#end",
                       "region"
                   }.JoinWithNewLine();

      AssertThrowsException (header, regionEnd: "#end region");
    }

    private void AssertCorrectlyParsed (string input, string expectedHeader)
    {
      var parser = new CommentParser ("//", "/*", "*/", "#region", "#endregion");
      var extractedHeader = parser.Parse (input);
      Assert.That (extractedHeader, Is.EqualTo (expectedHeader));
    }

    private void AssertThrowsException (
        string text,
        string commentStart = "//",
        string blockCommentStart = "/*",
        string blockCommentEnd = "*/",
        string regionStart = "#region",
        string regionEnd = "#endregion")
    {
      var textString = string.Join (Environment.NewLine, text);
      var parser = new CommentParser (commentStart, blockCommentStart, blockCommentEnd, regionStart, regionEnd);
      Assert.That (() => parser.Parse (textString), Throws.InstanceOf<ParseException>());
    }
  }
}

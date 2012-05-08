#region copyright
// Copyright (c) 2011 rubicon informationstechnologie gmbh

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion
using System;
using LicenseHeaderManager.Headers;
using NUnit.Framework;

namespace LicenseHeaderManager.Test
{
  [TestFixture]
  public class CommentParserTests
  {

    private void Test(string[] header, string[] text)
    {
      string headerString = string.Join (Environment.NewLine, header);
      string textString = string.Join(Environment.NewLine, text);
      
      if (header.Length > 0 && text.Length > 0)
        headerString += Environment.NewLine;

      var parser = new CommentParser ("//", "/*", "*/", "#region", "#endregion");
      Assert.AreEqual (headerString, parser.Parse (headerString + textString));
    }

    private void TestError (string[] text)
    {
      string textString = string.Join (Environment.NewLine, text);
      var parser = new CommentParser ("//", "/*", "*/", "#region", "#endregion");
      Assert.Throws<ParseException>(() => parser.Parse (textString));
    }

    [Test]
    public void TestMultipleLineComments ()
    {
      var header = new[]
      {
        "//This is a comment that",
        "//spans multiple lines."
      };

      var text = new[]
      {
        "This is not a comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestMultipleBeginEndComments ()
    {
      var header = new[]
      {
        "/* This is a comment that */",
        "/* spans multiple lines.  */"
      };

      var text = new[]
      {
        "This is not a comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestMultiLineBeginEndComment ()
    {
      var header = new[]
      {
        "/* This is a comment that ",
        "   spans multiple lines.  */"
      };

      var text = new[]
      {
        "This is not a comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestMultipleLineBeginEndCommentsWithEmptyLine ()
    {
      var header = new[]
      {
        "/* This is a comment that ",
        "   spans multiple lines.  */",
        ""
      };

      var text = new[]
      {
        "/* This is another comment. */"
      };

      Test (header, text);
    }

    [Test]
    public void TestMultipleLineCommentsWithEmptyLine ()
    {
      var header = new[]
      {
        "//This is a comment that",
        "//spans multiple lines.",
        ""
      };

      var text = new[]
      {
        "//This is another comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestMixedComments ()
    {
      var header = new[]
      {
        "/* This is a comment that",
        "   spans multiple lines.  */",
        "// This is also part of the header.",
        ""
      };

      var text = new[]
      {
        "//This is another comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestRegion ()
    {
      var header = new[]
      {
        "#region copyright",
        "//This is a comment.",
        "#endregion"
      };

      var text = new[]
      {
        "This is not a comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestRegionWithEmptyLines ()
    {
      var header = new[]
      {
        "#region copyright",
        "//This is a comment.",
        "",
        "//This is also part of the header.",
        "#endregion",
        ""
      };

      var text = new[]
      {
        "//This is another comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestMultipleRegions ()
    {
      var header = new[]
      {
        "#region copyright",
        "//This is a comment.",
        "",
        "#endregion",
        "#region something else",
        "",
        "#endregion"
      };

      var text = new[]
      {
        "This is not a comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestNestedRegions ()
    {
      var header = new[]
      {
        "#region copyright",
        "//This is a comment.",
        "",
        "#region something else",
        "",
        "#endregion",
        "#endregion"
      };

      var text = new[]
      {
        "This is not a comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestEmptyLinesBeforeComment ()
    {
      var header = new[]
      {
        "",
        "",
        "//This is a comment.",
      };

      var text = new[]
      {
        "This is not a comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestEverything ()
    {
      var header = new[]
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
        " "
      };

      var text = new[]
      {
        "This is not a comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestHeaderOnly ()
    {
      var header = new[]
      {
        "//This is a comment.",
      };

      var text = new string[0];

      Test (header, text);
    }

    [Test]
    public void TestMultiLineHeaderOnly ()
    {
      var header = new[]
      {
        "/*",
        "This is a comment that ",
        "spans multiple lines.",
        "*/"
      };

      var text = new string[0];

      Test (header, text);
    }

    [Test]
    public void TestHeaderOnlyWithEmptyLines ()
    {
      var header = new[]
      {
        "",
        "",
        "//This is a comment.",
        "",
        ""
      };

      var text = new string[0];

      Test (header, text);
    }

    [Test]
    public void TestNoHeader ()
    {
      var header = new string[0];

      var text = new[]
      {
        "This is not a comment.",
        "//This is not part of the header.",
      };

      Test (header, text);
    }

    [Test]
    public void TestNoHeaderWithEmptyLines ()
    {
      var header = new string[0];

      var text = new[]
      {
        "",
        "",
        "This is not a comment.",
        "//This is not part of the header.",
      };

      Test (header, text);
    }

    [Test]
    public void TestNoHeaderButNormalRegion ()
    {
      var header = new string[0];

      var text = new[]
      {
        "#region normal region",
        "This is not a comment.",
        "#endregion"
      };

      Test (header, text);
    }

    [Test]
    public void TestEmptyFile ()
    {
      var header = new string[0];
      var text = new string[0];

      Test (header, text);
    }

    [Test]
    public void TestMissingEndComment ()
    {
      var text = new[]
      {
        "/* This is a comment.",
        "This is not a comment."
      };

      TestError (text);
    }

    [Test]
    public void TestMissingEndRegion ()
    {
      var header = new string[0];

      var text = new[]
      {
        "#region copyright",
        "//This is a comment.",
        "This is not a comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestNestedMissingEndRegion ()
    {
      var header = new string[0];

      var text = new[]
      {
        "#region copyright",
        "//This is a comment.",
        "#region something else",
        "",
        "#endregion",
        "This is not a comment."
      };

      Test (header, text);
    }

    [Test]
    public void TestTextInRegion ()
    {
      var header = new string[0];

      var text = new[]
      {
        "#region copyright",
        "//This is a comment.",
        "This is not a comment.",
        "#endregion"
      };

      Test (header, text);
    }

    [Test]
    public void TestCommentBeforeRegionWithText ()
    {
      var header = new []
      {
        "",
        "//This is a comment."
      };

      var text = new[]
      {
        "#region copyright",
        "//This is a comment.",
        "This is not a comment.",
        "#endregion"
      };

      Test (header, text);
    }
  }
}

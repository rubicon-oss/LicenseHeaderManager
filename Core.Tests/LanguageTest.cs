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
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Core.Tests
{
  [TestFixture]
  public class LanguageTest
  {
    private Language _language;

    [SetUp]
    public void Setup ()
    {
      _language = new Language();
    }

    [TearDown]
    public void TearDown ()
    {
      _language = new Language();
    }

    [Test]
    public void IsValid_ExtensionsIsNull_ReturnsFalse ()
    {
      _language.Extensions = null;
      Assert.That (_language.IsValid(), Is.False);
    }

    [Test]
    public void IsValid_ExtensionsIsEmpty_ReturnsFalse ()
    {
      _language.Extensions = new List<string>();
      Assert.That (_language.IsValid(), Is.False);
    }

    [Test]
    public void IsValid_BeginRegionIsNullEndRegionIsValid_ReturnsFalse ()
    {
      _language.Extensions = new List<string> { ".cs" };
      _language.BeginRegion = null;
      _language.EndRegion = "#endregion";
      Assert.That (_language.IsValid(), Is.False);
    }

    [Test]
    public void IsValid_BeginRegionIsValidEndRegionIsNull_ReturnsFalse ()
    {
      _language.Extensions = new List<string> { ".cs" };
      _language.BeginRegion = "#region";
      _language.EndRegion = null;
      Assert.That (_language.IsValid(), Is.False);
    }

    [Test]
    public void IsValid_LineCommendIsNullBeginCommendIsNullEndCommendIsValid_ReturnsFalse ()
    {
      _language.Extensions = new List<string> { ".cs" };
      _language.BeginRegion = "#region";
      _language.EndRegion = "#endregion";
      _language.LineComment = null;
      _language.BeginComment = null;
      _language.EndComment = "*/";
      Assert.That (_language.IsValid(), Is.False);
    }

    [Test]
    public void IsValid_LineCommendIsNullBeginCommendIsValidEndCommendIsNull_ReturnsFalse ()
    {
      _language.Extensions = new List<string> { ".cs" };
      _language.BeginRegion = "#region";
      _language.EndRegion = "#endregion";
      _language.LineComment = null;
      _language.BeginComment = "/*";
      _language.EndComment = null;
      Assert.That (_language.IsValid(), Is.False);
    }

    [Test]
    public void IsValid_BeginCommendIsNullEndCommendIsValid_ReturnsFalse ()
    {
      _language.Extensions = new List<string> { ".cs" };
      _language.BeginRegion = "#region";
      _language.EndRegion = "#endregion";
      _language.LineComment = "//";
      _language.BeginComment = null;
      _language.EndComment = "*/";
      Assert.That (_language.IsValid(), Is.False);
    }

    [Test]
    public void IsValid_BeginCommendIsValidEndCommendIsNull_ReturnsFalse ()
    {
      _language.Extensions = new List<string> { ".cs" };
      _language.BeginRegion = "#region";
      _language.EndRegion = "#endregion";
      _language.LineComment = "//";
      _language.BeginComment = "/*";
      _language.EndComment = null;
      Assert.That (_language.IsValid(), Is.False);
    }

    [Test]
    public void IsValid_BeginCommendIsValidEndCommendIsValid_ReturnsTrue ()
    {
      _language.Extensions = new List<string> { ".cs" };
      _language.BeginRegion = "#region";
      _language.EndRegion = "#endregion";
      _language.LineComment = "//";
      _language.BeginComment = "/*";
      _language.EndComment = "*/";
      Assert.That (_language.IsValid(), Is.True);
    }

    [Test]
    public void NormalizeExtensions_NonNormalizedExtensions_ReturnsNormalizedExtensions ()
    {
      _language.Extensions = new List<string> { ".Cs", ".cPp", ".TS", "js" };
      _language.NormalizeExtensions();
      var startsWithDot = _language.Extensions.All (x => x.StartsWith ("."));
      var isLowerCase = _language.Extensions.All (x => x.IsNormalized());
      Assert.That (startsWithDot, Is.True);
      Assert.That (isLowerCase, Is.True);
    }
  }
}

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
using NUnit.Framework;

namespace Core.Tests
{
  [TestFixture]
  public class DocumentHeaderTest
  {
    private string _documentFilePath;
    private string _text;
    private IEnumerable<DocumentHeaderProperty> _properties;

    [Test]
    public void DocumentHeader_DocumentFilePathIsNull_ThrowsArgumentNullException ()
    {
      _documentFilePath = null;
      _text = "";
      _properties = new DocumentHeaderProperty[] { };

      Assert.That (() => new DocumentHeader (_documentFilePath, _text, _properties), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void DocumentHeader_PropertiesIsNull_ThrowsArgumentNullException ()
    {
      _documentFilePath = "";
      _text = "";
      _properties = null;

      Assert.That (() => new DocumentHeader (_documentFilePath, _text, _properties), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void DocumentHeader_ValidInput_DoesNotThrowException ()
    {
      _documentFilePath = "";
      _text = "";
      _properties = new[]
                    {
                        new DocumentHeaderProperty (
                            "%Test%",
                            documentHeader => true,
                            documentHeader => "test")
                    };

      Assert.That (() => new DocumentHeader (_documentFilePath, _text, _properties), Throws.Nothing);
    }

    [Test]
    public void DocumentHeader_TextIsNull_DoesNotThrowException ()
    {
      _documentFilePath = "";
      _text = null;
      _properties = new DocumentHeaderProperty[] { };

      Assert.That (() => new DocumentHeader (_documentFilePath, _text, _properties), Throws.Nothing);
    }
  }
}

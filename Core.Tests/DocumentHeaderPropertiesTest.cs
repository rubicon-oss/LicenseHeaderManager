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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace Core.Tests
{
  [TestFixture]
  public class DocumentHeaderPropertiesTest
  {
    [Test]
    public void GetProperFilePathCapitalization_DocumentHeaderNull_ThrowsArgumentNullException ()
    {
      var documentHeaderProperties = new DocumentHeaderProperties();

      var property = documentHeaderProperties.ToArray()[0];
      Assert.That (property.Token, Is.EqualTo ("%FullFileName%"));

      var documentHeaderStub = MockRepository.GenerateStub<IDocumentHeader>();

      Assert.That (() => property.CreateValue (documentHeaderStub), Throws.ArgumentNullException);
    }

    [Test]
    public void GetProperFileNameCapitalization_DocumentHeaderNull_ThrowsArgumentNullException ()
    {
      var documentHeaderProperties = new DocumentHeaderProperties();

      var property = documentHeaderProperties.ToArray()[1];
      Assert.That (property.Token, Is.EqualTo ("%FileName%"));

      var documentHeaderStub = MockRepository.GenerateStub<IDocumentHeader>();

      Assert.That (() => property.CreateValue (documentHeaderStub), Throws.ArgumentNullException);
    }

    [Test]
    public void DocumentHeaderProperties_AdditionalProperties_AdditionalPropertiesAreAddedToCollection ()
    {
      var additionalProperties = new List<AdditionalProperty>
                                 {
                                     new AdditionalProperty ("%AdditionalProperty1%", "property 1"),
                                     new AdditionalProperty ("%AdditionalProperty2%", "property 2")
                                 };

      var documentHeaderProperties = new DocumentHeaderProperties (additionalProperties);
      foreach (var additionalProperty in additionalProperties)
        Assert.That (additionalProperty, Is.Not.Null);

      Assert.That (documentHeaderProperties, Has.Exactly (1).Matches<DocumentHeaderProperty> (p => p.Token == "%AdditionalProperty1%"));
      Assert.That (documentHeaderProperties, Has.Exactly (1).Matches<DocumentHeaderProperty> (p => p.Token == "%AdditionalProperty2%"));
    }

    [Test]
    public void GetEnumerator_ObjectOfClassExists_ReturnsEnumerator ()
    {
      var documentHeaderProperties = new DocumentHeaderProperties();
      var actual = ((IEnumerable) documentHeaderProperties).GetEnumerator();

      Assert.That (actual, Is.Not.Null);
      Assert.That (actual.MoveNext(), Is.True);
    }
  }
}

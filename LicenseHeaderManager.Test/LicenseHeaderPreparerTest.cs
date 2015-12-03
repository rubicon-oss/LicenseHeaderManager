using System;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace LicenseHeaderManager.Test
{
  [TestFixture]
  public class LicenseHeaderPreparerTest
  {
    [Test]
    public void TestLicenseHeaderWithoutEndingNewLine()
    {
      var currentHeaderText = "nothingRelevant";
      var toBeInsertedHeaderText = "//Testtext";

      var commentParserMock = MockRepository.GenerateMock<ICommentParser>();
      commentParserMock.Expect(x => x.Parse(currentHeaderText)).Return(String.Empty);
      
      string preparedHeader = LicenseHeaderPreparer.Prepare(toBeInsertedHeaderText, currentHeaderText, commentParserMock);
      
      Assert.AreEqual (toBeInsertedHeaderText + Environment.NewLine, preparedHeader);
    }

    [Test]
    public void TestLicenseHeaderWithEndingNewLine()
    {
      var currentHeaderText = "nothingRelevant";
      var toBeInsertedHeaderText = "//Testtext" + Environment.NewLine;

      var commentParserMock = MockRepository.GenerateMock<ICommentParser>();
      commentParserMock.Expect(x => x.Parse(currentHeaderText)).Return(String.Empty);
      
      string preparedHeader = LicenseHeaderPreparer.Prepare(toBeInsertedHeaderText, currentHeaderText, commentParserMock);
      
      Assert.AreEqual (toBeInsertedHeaderText, preparedHeader);
    }

    [Test]
    public void TestLicenseHeaderWhereCurrentHeaderHasComment()
    {
      var currentHeaderText = "//HighlyRelevantComment";
      var toBeInsertedHeaderText = "//Testtext" + Environment.NewLine;

      var commentParserMock = MockRepository.GenerateMock<ICommentParser>();
      commentParserMock.Expect(x => x.Parse(currentHeaderText)).Return("OhOurCurrentHeaderTextHasAComment");
      
      string preparedHeader = LicenseHeaderPreparer.Prepare(toBeInsertedHeaderText, currentHeaderText, commentParserMock);
      
      Assert.AreEqual (toBeInsertedHeaderText + Environment.NewLine, preparedHeader);
    }
  }
}

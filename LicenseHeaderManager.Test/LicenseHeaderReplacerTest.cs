using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using EnvDTE;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Options;
using NUnit.Framework;
using Rhino.Mocks;
using Document = LicenseHeaderManager.Headers.Document;
using Language = LicenseHeaderManager.Options.Language;

namespace LicenseHeaderManager.Test
{
  [TestFixture]
  public class LicenseHeaderReplacerTest
  {
    public class TryCreateDocument
    {
      private ILicenseHeaderExtension _extensionMock;
      private LicenseHeaderReplacer _replacer;
      private ProjectItem _projectItem;
      private ILanguagesPage _languagesPage;
      private IOptionsPage _optionsPage;

      private Document _document;


      [SetUp]
      public void SetUp ()
      {
        _extensionMock = MockRepository.GenerateMock<ILicenseHeaderExtension> ();
        _optionsPage = MockRepository.GenerateMock<IOptionsPage>();
        _replacer = new LicenseHeaderReplacer (_extensionMock);
        _projectItem = MockRepository.GenerateMock<ProjectItem> ();
        _languagesPage = MockRepository.GenerateMock<ILanguagesPage>();
        _extensionMock.Expect (x => x.LanguagesPage).Return (_languagesPage);
        _extensionMock.Expect (x => x.OptionsPage).Return (_optionsPage);
        _optionsPage.Expect (x => x.UseRequiredKeywords).Return (true);
        _optionsPage.Expect (x => x.RequiredKeywords).Return ("");
      }

      [Test]
      public void NoPhysicalFile_Kind ()
      {
        _projectItem.Expect (x => x.Kind).Return (Constants.vsProjectItemKindSubProject);

        var result = _replacer.TryCreateDocument (_projectItem, out _document);

        Assert.That (result, Is.EqualTo (CreateDocumentResult.NoPhyiscalFile));
      }

      [Test]
      public void NoPhysicalFile_Document ()
      {
        _projectItem.Expect (x => x.Kind).Return (Constants.vsProjectItemKindPhysicalFile);
        _projectItem.Expect (x => x.Document).Return(null);

        var result = _replacer.TryCreateDocument (_projectItem, out _document);

        Assert.That (result, Is.EqualTo (CreateDocumentResult.NoPhyiscalFile));
      }

      [Test]
      public void IgnoreLicenseHeaderFile ()
      {
        var textDocument = MockRepository.GenerateMock<TextDocument> ();

        var documentMock = MockRepository.GenerateMock<EnvDTE.Document> ();
        documentMock.Expect (x => x.Object ("TextDocument")).Return (textDocument);
        _projectItem.Expect (x => x.Kind).Return (Constants.vsProjectItemKindPhysicalFile);
        _projectItem.Expect (x => x.Document).Return (documentMock);
        _projectItem.Expect (x => x.Name).Return ("test.licenseheader");

        var result = _replacer.TryCreateDocument (_projectItem, out _document);

        Assert.That (result, Is.EqualTo (CreateDocumentResult.LicenseHeaderDocument));
      }

      [Test]
      public void NoTextDocument ()
      {
        var textDocument = MockRepository.GenerateMock<TextDocument> ();
        var documentMock = MockRepository.GenerateMock<EnvDTE.Document> ();
        documentMock.Expect (x => x.Object ("TextDocument")).Return (textDocument);
        _projectItem.Expect (x => x.Kind).Return (Constants.vsProjectItemKindPhysicalFile);
        _projectItem.Expect (x => x.Document).Return (documentMock);
        _projectItem.Expect (x => x.Name).Return ("test.resx");
        _projectItem.Expect (x => x.Open (Constants.vsViewKindTextView)).Throw (new COMException ());

        var result = _replacer.TryCreateDocument (_projectItem, out _document);

        Assert.That (result, Is.EqualTo (CreateDocumentResult.NoTextDocument));
      }

      [Test]
      public void LanguageNotFound ()
      {
        var textDocument = MockRepository.GenerateMock<TextDocument> ();
        var documentMock = MockRepository.GenerateMock<EnvDTE.Document> ();
        documentMock.Expect (x => x.Object ("TextDocument")).Return (textDocument);
        _projectItem.Expect (x => x.Kind).Return (Constants.vsProjectItemKindPhysicalFile);
        _projectItem.Expect (x => x.Document).Return (documentMock);
        _projectItem.Expect (x => x.Name).Return ("test.cs");
        _languagesPage.Expect (x => x.Languages).Return (
            new List<Language>
            {
                new Language { Extensions = new[] { ".txt" } }
            });

        var result = _replacer.TryCreateDocument (_projectItem, out _document);

        Assert.That (result, Is.EqualTo (CreateDocumentResult.LanguageNotFound));
      }

      [Test]
      public void NoHeaderFound ()
      {
        var textDocument = MockRepository.GenerateMock<TextDocument> ();
        var documentMock = MockRepository.GenerateMock<EnvDTE.Document> ();
        documentMock.Expect (x => x.Object ("TextDocument")).Return (textDocument);
        _projectItem.Expect (x => x.Kind).Return (Constants.vsProjectItemKindPhysicalFile);
        _projectItem.Expect (x => x.Document).Return (documentMock);
        _projectItem.Expect (x => x.Name).Return ("test.cs");
        _languagesPage.Expect (x => x.Languages).Return (
            new List<Language>
            {
                new Language { Extensions = new[] { ".cs" } }
            });
        var headers = new Dictionary<string, string[]>
                      {
                        { ".cs", new string[0] }
                      };

        var result = _replacer.TryCreateDocument (_projectItem, out _document, headers);

        Assert.That (result, Is.EqualTo (CreateDocumentResult.NoHeaderFound));
      }

      [Test]
      public void DocumentCreated ()
      {
        var parent = MockRepository.GenerateMock<EnvDTE.Document> ();
        parent.Expect (x => x.FullName).Return ("");
        var editPoint = MockRepository.GenerateMock<EditPoint> ();
        editPoint.Expect (x => x.GetText (null)).IgnoreArguments ().Return ("");
        var textDocument = MockRepository.GenerateMock<TextDocument> ();
        textDocument.Expect (x => x.CreateEditPoint ()).IgnoreArguments ().Return (editPoint);
        textDocument.Expect (x => x.Parent).Return (parent);
        var documentMock = MockRepository.GenerateMock<EnvDTE.Document> ();
        documentMock.Expect (x => x.Object ("TextDocument")).Return (textDocument);
        _projectItem.Expect (x => x.Kind).Return (Constants.vsProjectItemKindPhysicalFile);
        _projectItem.Expect (x => x.Document).Return (documentMock);
        _projectItem.Expect (x => x.Name).Return ("test.cs");
        _languagesPage.Expect (x => x.Languages).Return (
            new List<Language>
            {
                new Language { Extensions = new[] { ".cs" } }
            });
        var headers = new Dictionary<string, string[]>
                      {
                          { ".cs", new[] { "//" } }
                      };

        var result = _replacer.TryCreateDocument (_projectItem, out _document, headers);

        Assert.That (result, Is.EqualTo (CreateDocumentResult.DocumentCreated));
        Assert.That (_document, Is.Not.Null);
      }

      [Test]
      public void UseMostSignificantExtension ()
      {
        var parent = MockRepository.GenerateMock<EnvDTE.Document> ();
        parent.Expect (x => x.FullName).Return ("");
        var editPoint = MockRepository.GenerateMock<EditPoint> ();
        editPoint.Expect (x => x.GetText (null)).IgnoreArguments ().Return ("");
        var textDocument = MockRepository.GenerateMock<TextDocument> ();
        textDocument.Expect (x => x.CreateEditPoint ()).IgnoreArguments ().Return (editPoint);
        textDocument.Expect (x => x.Parent).Return (parent);
        var documentMock = MockRepository.GenerateMock<EnvDTE.Document> ();
        documentMock.Expect (x => x.Object ("TextDocument")).Return (textDocument);
        _projectItem.Expect (x => x.Kind).Return (Constants.vsProjectItemKindPhysicalFile);
        _projectItem.Expect (x => x.Document).Return (documentMock);
        _projectItem.Expect (x => x.Name).Return ("test.generated.cs");
        _languagesPage.Expect (x => x.Languages).Return (
            new List<Language>
            {
                new Language { Extensions = new[] { ".cs" } }
            });
        var headers = new Dictionary<string, string[]>
                      {
                          { ".cs", new[] { "cs" } },
                          { "generated.cs", new[] { "generated" } } 
                      };

        _replacer.TryCreateDocument (_projectItem, out _document, headers);

        Assert.That (_document._header.Text, Is.EqualTo ("generated\r\n"));
      }
    }
  }
}

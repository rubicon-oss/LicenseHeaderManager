using LicenseHeaderManager.Headers;
using NUnit.Framework;

namespace LicenseHeaderManager.Test
{
  [TestFixture]
  public class LineManagerTest
  {
    [Test]
    public void DetectLineEnd_CRLF ()
    {
      string text = "test\r\ntest";
      Assert.AreEqual ("\r\n", NewLineManager.DetectMostFrequentLineEnd (text));
    }

    [Test]
    public void DetectLineEnd_OnlyLF ()
    {
      string text = "test\ntest";
      Assert.AreEqual ("\n", NewLineManager.DetectMostFrequentLineEnd (text));
    }

    [Test]
    public void DetectLineEnd_OnlyCR ()
    {
      string text = "test\rtest";
      Assert.AreEqual ("\r", NewLineManager.DetectMostFrequentLineEnd (text));
    }

    [Test]
    public void ReplaceAllLineEnds_ReplaceFullLineEnding ()
    {
      string text1 = "test1";
      string text2 = "test2";
      string text3 = "test3";
      string text4 = "test4";
      string fulltext = text1 + "\r\n" + text2 + "\n" + text3 + "\r" + text4;
      Assert.AreEqual (text1 + text2 + text3 + text4, NewLineManager.ReplaceAllLineEnds (fulltext, string.Empty));
    }

    [Test]
    public void DetectMostFrequentLineEnd_DetectFullLineEnding ()
    {
      string text = "test\n\r\n\r\n test\r\n te\r restasd";
      var result = NewLineManager.DetectMostFrequentLineEnd (text);
      Assert.AreEqual ("\r\n", result);
    }
  }
}
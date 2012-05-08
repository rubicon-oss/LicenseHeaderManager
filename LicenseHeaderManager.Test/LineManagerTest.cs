using System;
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
  }
}

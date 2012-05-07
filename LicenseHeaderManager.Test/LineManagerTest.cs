using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LicenseHeaderManager.Headers;
using NUnit.Framework;

namespace LicenseHeaderManager.Test
{
  [TestFixture]
  public class LineManagerTest
  {
    [Test]
    public void CheckRandN ()
    {
      string text = "test\r\ntest";
      Assert.AreEqual ("\r\n", NewLineManager.DetectLineEnd (text));
    }

    [Test]
    public void CheckOnlyN ()
    {
      string text = "test\ntest";
      Assert.AreEqual ("\n", NewLineManager.DetectLineEnd (text));
    }

    [Test]
    public void CheckOnlyR ()
    {
      string text = "test\rtest";
      Assert.AreEqual ("\r", NewLineManager.DetectLineEnd (text));
    }
  }
}

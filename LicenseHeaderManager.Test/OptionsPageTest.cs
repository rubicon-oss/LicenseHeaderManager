using System;
using LicenseHeaderManager.Options;
using NUnit.Framework;

namespace LicenseHeaderManager.Test
{
  [TestFixture]
  public class OptionsPageTest
  {
    [Test]
    public void TestLinkedCommandsChangedCalledWhenNewLinkedCommandSavedWithExistingLinkedCommands ()
    {
      var optionsPage = new OptionsPage();
      var wasCalled = false;
      optionsPage.LinkedCommandsChanged += (sender, args) => wasCalled = true;
      const string emptySerializedLinkedCommands = "1*System.String*<LinkedCommands/>";
      optionsPage.LinkedCommandsSerialized = emptySerializedLinkedCommands;

      optionsPage.LinkedCommands.Add (new LinkedCommand());

      Assert.That (wasCalled, Is.True);
    }

    [Test]
    public void TestLinkedCommandsChangedCalledWhenNewLinkedCommandSavedWithDefaultLinkedCommands ()
    {
      var optionsPage = new OptionsPage();
      var wasCalled = false;
      optionsPage.LinkedCommandsChanged += (sender, args) => wasCalled = true;

      optionsPage.LinkedCommands.Add (new LinkedCommand());

      Assert.That (wasCalled, Is.True);
    }
  }
}
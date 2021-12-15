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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using LicenseHeaderManager.Options;
using LicenseHeaderManager.Options.Converters;
using LicenseHeaderManager.Options.Model;
using NUnit.Framework;

namespace LicenseHeaderManager.Tests
{
  [TestFixture]
  public class GeneralOptionsPageModelTest
  {
    [Test]
    public async Task GeneralOptionsPage_AddLinkedCommand_LinkedCommandsChangedEventTriggered ()
    {
      await VisualStudioTestContext.SwitchToMainThread();

      var optionsPage = new GeneralOptionsPageModel();
      var wasCalled = false;
      optionsPage.LinkedCommandsChanged += (sender, args) => wasCalled = true;
      optionsPage.LinkedCommands.Add (new LinkedCommand());

      Assert.That (wasCalled, Is.True);
    }

    [Test]
    public async Task GeneralOptionsPage_CreateNewLinkedCommandCollection_LinkedCommandsChangedEventTriggered ()
    {
      await VisualStudioTestContext.SwitchToMainThread();

      var optionsPage = new GeneralOptionsPageModel();
      var wasCalled = false;
      optionsPage.LinkedCommandsChanged += (sender, args) => wasCalled = true;
      const string emptySerializedLinkedCommands = "1*System.String*<LinkedCommands/>";
      var converter = new LinkedCommandConverter();
      optionsPage.LinkedCommands = new ObservableCollection<LinkedCommand> (converter.FromXml (emptySerializedLinkedCommands)) { new LinkedCommand() };

      Assert.That (wasCalled, Is.True);
    }
  }
}

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
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Sdk.TestFramework;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;

namespace LicenseHeaderManager.Tests
{
  /// <summary>
  ///   Provides an abstract base test fixture that allows for tests that invoke methods with assertions
  ///   ensuring execution from the UI thread (<c>ThreadHelper.ThrowIfNotOnUIThread()</c>) to succeed.
  /// </summary>
  [SetUpFixture]
  public sealed class VisualStudioTestContext
  {
    public static LicenseHeadersPackage LicenseHeaderPackage;
    private static GlobalServiceProvider s_mockServiceProvider;

    [OneTimeSetUp]
    public void FixtureSetup ()
    {
      s_mockServiceProvider = new GlobalServiceProvider();

      // The static Instance is set in the LicenseHeadersPackage constructor. Therefore, set it in the fixture setup
      // such that it is accessible during tests.
      LicenseHeaderPackage = new LicenseHeadersPackage();
    }

    [OneTimeTearDown]
    public void FixtureTearDown ()
    {
      s_mockServiceProvider.Dispose();
    }

    /// <summary>
    ///   Sets the value of a private-set property of the <see cref="LicenseHeadersPackage" /> instance.
    /// </summary>
    /// <param name="propertyName">The name of the private-set property whose value should be set.</param>
    /// <param name="propertyValue">The value the property determined by <paramref name="propertyName" /> should be set to.</param>
    public static void SetPrivateSetPackageProperty (string propertyName, object propertyValue)
    {
      typeof (LicenseHeadersPackage).GetProperty (propertyName)?.SetValue (LicenseHeaderPackage, propertyValue);
    }

    public static JoinableTaskFactory.MainThreadAwaitable SwitchToMainThread ()
    {
#pragma warning disable VSTHRD004
      return ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
#pragma warning restore VSTHRD004
    }
  }
}

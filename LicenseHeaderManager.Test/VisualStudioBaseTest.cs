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
using System.Windows;
using NUnit.Framework;

namespace LicenseHeaderManager.Test
{
  /// <summary>
  ///   Provides an abstract base test fixture that allows for tests that invoke methods with assertions
  ///   ensuring execution from the UI thread (<c>ThreadHelper.ThrowIfNotOnUIThread()</c>) to succeed.
  /// </summary>
  public abstract class VisualStudioBaseTest
  {
    private Application _application;
    private LicenseHeadersPackage _licenseHeaderPackage;

    [TestFixtureSetUp]
    public void FixtureSetup ()
    {
      // ThreadHelper.ThrowIfNotOnUIThread() accesses Application.Current.Dispatcher to determine whether
      // a call is made from the context of the UI thread. When running unit tests, Application.Current is null.
      // This property returns a static private variable of type Application that is set when the Application constructor
      // is executed. Hence, it is enough to simply create an Application object to satisfy the assertion that is present
      // in the production code.
      // Source: https://referencesource.microsoft.com/#PresentationFramework/src/Framework/System/Windows/Application.cs,143
      if (Application.Current == null)
        _application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

      // The static Instance is set in the LicenseHeadersPackage constructor. Therefore, set it in the fixture setup
      // such that it is accessible during tests.
      _licenseHeaderPackage = new LicenseHeadersPackage();
    }

    [TestFixtureTearDown]
    public void FixtureTearDown ()
    {
      _application?.Shutdown();
    }

    /// <summary>
    ///   Sets the value of a private-set property of the <see cref="LicenseHeadersPackage" /> instance.
    /// </summary>
    /// <param name="propertyName">The name of the private-set property whose value should be set.</param>
    /// <param name="propertyValue">The value the property determined by <paramref name="propertyName" /> should be set to.</param>
    protected void SetPrivateSetPackageProperty (string propertyName, object propertyValue)
    {
      typeof (LicenseHeadersPackage).GetProperty (propertyName)?.SetValue (_licenseHeaderPackage, propertyValue);
    }
  }
}

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
using LicenseHeaderManager.MenuItemButtonHandler;
using Microsoft.VisualStudio.Shell;

namespace LicenseHeaderManager.Interfaces
{
  /// <summary>
  ///   Provides a common type for classes which encapsulate logic handling the execution of a menu item command
  ///   that is more sophisticated than in the most common use cases.
  /// </summary>
  public interface IMenuItemButtonHandler
  {
    /// <summary>
    ///   Gets the scope of menu item command this <see cref="IMenuItemButtonHandler" /> corresponds to.
    /// </summary>
    MenuItemButtonLevel Level { get; }

    /// <summary>
    ///   Gets the type of operation an invocation of the command this <see cref="IMenuItemButtonHandler" /> corresponds to
    ///   entails.
    /// </summary>
    MenuItemButtonOperation Mode { get; }

    /// <summary>
    ///   Handles a click on a menu item.
    /// </summary>
    /// <param name="sender">The clicked menu item.</param>
    /// <param name="e">
    ///   A corresponding <see cref="EventArgs" /> instance. In most cases, an instance of the
    ///   <see cref="OleMenuCmdEventArgs" /> class.
    /// </param>
    void HandleButton (object sender, EventArgs e);
  }
}

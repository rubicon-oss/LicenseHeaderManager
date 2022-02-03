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

namespace LicenseHeaderManager.Utils
{
  /// <summary>
  ///   Contains utility methods to simplify MessageBox invocations and user interaction using them.
  /// </summary>
  public static class MessageBoxHelper
  {
    /// <summary>
    ///   Shows a simple MessageBox without icon.
    /// </summary>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="title">
    ///   The title of the MessageBox will be '<see cref="Resources.LicenseHeaderManagerName" />: <paramref name="title" />'.
    ///   If
    ///   this parameter is <see langword="null" />, the title will be '<see cref="Resources.LicenseHeaderManagerName" />'
    /// </param>
    public static void Show (string message, string title = null)
    {
      MessageBox.Show (message, $"{Resources.LicenseHeaderManagerName}{(title == null ? string.Empty : ": " + title)}");
    }

    /// <summary>
    ///   Shows a MessageBox with a blue information or a yellow warning icon.
    /// </summary>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="title">
    ///   The title of the MessageBox will be '<see cref="Resources.LicenseHeaderManagerName" />: <paramref name="title" />'.
    ///   If
    ///   this parameter is <see langword="null" />, the title will be '<see cref="Resources.LicenseHeaderManagerName" />:
    ///   <see cref="Resources.Message" />'
    /// </param>
    /// <param name="warning">
    ///   If <see langword="true" />, the MessageBox will have a yellow warning icon, otherwise a blue
    ///   information icon.
    /// </param>
    public static void ShowMessage (string message, string title = null, bool warning = false)
    {
      MessageBox.Show (
          message,
          $"{Resources.LicenseHeaderManagerName}: {title ?? Resources.Message}",
          MessageBoxButton.OK,
          warning ? MessageBoxImage.Warning : MessageBoxImage.Information);
    }

    /// <summary>
    ///   Shows a MessageBox with a red error icon.
    /// </summary>
    /// <param name="message">The message to be displayed.</param>
    /// <param name="title">
    ///   The title of the MessageBox will be '<see cref="Resources.LicenseHeaderManagerName" />: <paramref name="title" />'.
    ///   If
    ///   this parameter is <see langword="null" />, the title will be '<see cref="Resources.LicenseHeaderManagerName" />:
    ///   <see cref="Resources.Error" />'
    /// </param>
    public static void ShowError (string message, string title = null)
    {
      MessageBox.Show (message, $"{Resources.LicenseHeaderManagerName}: {title ?? Resources.Error}", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    ///   Shows a MessageBox posing a Yes-Or-No-Question with a blue information or a yellow warning icon.
    /// </summary>
    /// <param name="message">The question to be asked.</param>
    /// <param name="title">
    ///   The title of the MessageBox will be '<see cref="Resources.LicenseHeaderManagerName" />: <paramref name="title" />'.
    ///   If
    ///   this parameter is <see langword="null" />, the title will be '<see cref="Resources.LicenseHeaderManagerName" />:
    ///   <see cref="Resources.Question" />'
    /// </param>
    /// <param name="warning">
    ///   If <see langword="true" />, the MessageBox will have a yellow warning icon, otherwise a blue
    ///   information icon.
    /// </param>
    /// <returns>
    ///   Returns <see langword="true" /> if the user clicked the "Yes" button, otherwise <see langword="false" />
    /// </returns>
    public static bool AskYesNo (string message, string title = null, bool warning = false)
    {
      return MessageBox.Show (
          message,
          $"{Resources.LicenseHeaderManagerName}: {title ?? Resources.Question}",
          MessageBoxButton.YesNo,
          warning ? MessageBoxImage.Warning : MessageBoxImage.Question,
          MessageBoxResult.No) == MessageBoxResult.Yes;
    }

    /// <summary>
    ///   Shows a MessageBox posing a Yes-Or-No-Question with a blue information or a yellow warning icon. Unlike with
    ///   <see cref="AskYesNo(string, string, bool)" />, this
    ///   method internally enforces a switch to the main thread and explicitly uses the Window specified by
    ///   <paramref name="owner" /> as the owner of the MessageBox.
    /// </summary>
    /// <param name="owner">The window on top of which the MessageBox should pop up. Must not be null.</param>
    /// <param name="message">The question to be asked.</param>
    /// <param name="title">
    ///   The title of the MessageBox will be '<see cref="Resources.LicenseHeaderManagerName" />: <paramref name="title" />'.
    ///   If
    ///   this parameter is <see langword="null" />, the title will be '<see cref="Resources.LicenseHeaderManagerName" />:
    ///   <see cref="Resources.Question" />'
    /// </param>
    /// <param name="warning"></param>
    /// <returns>
    ///   Returns <see langword="true" /> if the user clicked the "Yes" button, otherwise <see langword="false" />
    /// </returns>
    public static async Task<bool> AskYesNoAsync (Window owner, string message, string title = null, bool warning = false)
    {
      await LicenseHeadersPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();

      return MessageBox.Show (
          owner,
          message,
          $"{Resources.LicenseHeaderManagerName}: {title ?? Resources.Question}",
          MessageBoxButton.YesNo,
          warning ? MessageBoxImage.Warning : MessageBoxImage.Question,
          MessageBoxResult.No) == MessageBoxResult.Yes;
    }
  }
}

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
using System.ComponentModel.Design;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace LicenseHeaderManager.MenuItemCommands.EditorMenu
{
  /// <summary>
  ///   Command handler
  /// </summary>
  internal sealed class AddLicenseHeaderEditorAdvancedMenuCommand
  {
    /// <summary>
    ///   Command ID.
    /// </summary>
    private const int c_commandId = 4144;

    /// <summary>
    ///   Command menu group (command set GUID).
    /// </summary>
    private static readonly Guid s_commandSet = new Guid ("1a75d6da-3b30-4ec9-81ae-72b8b7eba1a0");

    private readonly OleMenuCommand _menuItem;

    /// <summary>
    ///   Initializes a new instance of the <see cref="AddLicenseHeaderEditorAdvancedMenuCommand" /> class.
    ///   Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private AddLicenseHeaderEditorAdvancedMenuCommand (AsyncPackage package, OleMenuCommandService commandService)
    {
      ServiceProvider = (ILicenseHeaderExtension) package ?? throw new ArgumentNullException (nameof(package));
      commandService = commandService ?? throw new ArgumentNullException (nameof(commandService));

      var menuCommandID = new CommandID (s_commandSet, c_commandId);
      _menuItem = new OleMenuCommand (Execute, menuCommandID);
      _menuItem.BeforeQueryStatus += OnQueryEditCommandStatus;
      commandService.AddCommand (_menuItem);
    }

    /// <summary>
    ///   Gets the instance of the command.
    /// </summary>
    public static AddLicenseHeaderEditorAdvancedMenuCommand Instance { get; private set; }

    /// <summary>
    ///   Gets the service provider from the owner package.
    /// </summary>
    private ILicenseHeaderExtension ServiceProvider { get; }

    private void OnQueryEditCommandStatus (object sender, EventArgs e)
    {
      var visible = false;

      var item = ServiceProvider.GetActiveProjectItem();
      if (item != null)
        visible = ServiceProvider.ShouldBeVisible (item);

      _menuItem.Visible = visible;
    }

    /// <summary>
    ///   Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync (AsyncPackage package)
    {
      // Switch to the main thread - the call to AddCommand in AddLicenseHeaderEditorAdvancedMenuCommand's constructor requires
      // the UI thread.
      await LicenseHeadersPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync (package.DisposalToken);

      var commandService = await package.GetServiceAsync (typeof (IMenuCommandService)) as OleMenuCommandService;
      Instance = new AddLicenseHeaderEditorAdvancedMenuCommand (package, commandService);
    }

    public void Invoke ()
    {
      _menuItem.Invoke();
    }

    /// <summary>
    ///   This function is the callback used to execute the command when the menu item is clicked.
    ///   See the constructor to see how the menu item is associated with this function using
    ///   OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void Execute (object sender, EventArgs e)
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      ExecuteInternalAsync().FireAndForget();
    }

    private async Task ExecuteInternalAsync ()
    {
      var item = ServiceProvider.GetActiveProjectItem();
      await item.AddLicenseHeaderToItemAsync (ServiceProvider, !ServiceProvider.IsCalledByLinkedCommand);
    }
  }
}

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
using System.IO;
using System.Threading;
using Core;
using EnvDTE;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace LicenseHeaderManager.MenuItemCommands.ProjectItemMenu
{
  /// <summary>
  ///   Command handler
  /// </summary>
  internal sealed class RemoveHeaderFromProjectItemCommand
  {
    /// <summary>
    ///   Command ID.
    /// </summary>
    private const int c_commandId = 4129;

    /// <summary>
    ///   Command menu group (command set GUID).
    /// </summary>
    private static readonly Guid s_commandSet = new Guid ("1a75d6da-3b30-4ec9-81ae-72b8b7eba1a0");

    private readonly OleMenuCommand _menuItem;

    /// <summary>
    ///   Initializes a new instance of the <see cref="RemoveHeaderFromProjectItemCommand" /> class.
    ///   Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private RemoveHeaderFromProjectItemCommand (AsyncPackage package, OleMenuCommandService commandService)
    {
      ServiceProvider = (ILicenseHeaderExtension) package ?? throw new ArgumentNullException (nameof(package));
      commandService = commandService ?? throw new ArgumentNullException (nameof(commandService));

      var menuCommandID = new CommandID (s_commandSet, c_commandId);
      _menuItem = new OleMenuCommand (Execute, menuCommandID);
      _menuItem.BeforeQueryStatus += OnQueryProjectItemCommandStatus;
      commandService.AddCommand (_menuItem);
    }

    /// <summary>
    ///   Gets the instance of the command.
    /// </summary>
    public static RemoveHeaderFromProjectItemCommand Instance { get; private set; }

    /// <summary>
    ///   Gets the service provider from the owner package.
    /// </summary>
    private ILicenseHeaderExtension ServiceProvider { get; }

    private void OnQueryProjectItemCommandStatus (object sender, EventArgs e)
    {
      var visible = false;
      ThreadHelper.ThrowIfNotOnUIThread();
      if (ServiceProvider.GetSolutionExplorerItem() is ProjectItem item)
        visible = ServiceProvider.ShouldBeVisible (item);

      _menuItem.Visible = visible;
    }

    /// <summary>
    ///   Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync (AsyncPackage package)
    {
      // Switch to the main thread - the call to AddCommand in RemoveHeaderFromProjectItemCommand's constructor requires
      // the UI thread.
      await LicenseHeadersPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync (package.DisposalToken);

      var commandService = await package.GetServiceAsync (typeof (IMenuCommandService)) as OleMenuCommandService;
      Instance = new RemoveHeaderFromProjectItemCommand (package, commandService);
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

      if (!(e is OleMenuCmdEventArgs args))
        return;

      var item = args.InValue as ProjectItem ?? ServiceProvider.GetSolutionExplorerItem() as ProjectItem;
      if (item != null && Path.GetExtension (item.Name) != LicenseHeaderExtractor.HeaderDefinitionExtension)
        ExecuteInternalAsync (item).FireAndForget();
    }

    private async Task ExecuteInternalAsync (ProjectItem item)
    {
      await ServiceProvider.JoinableTaskFactory.SwitchToMainThreadAsync();
      var cancellationToken = new CancellationToken();
      var replacerInput = CoreHelpers.GetFilesToProcess (item, null, out _, out var fileOpenedStatus, false);
      replacerInput.IgnoreNonCommentText();

      var result = await ServiceProvider.LicenseHeaderReplacer.RemoveOrReplaceHeader (
          replacerInput,
          CoreHelpers.CreateProgress (default, default, fileOpenedStatus, cancellationToken),
          cancellationToken);
      await CoreHelpers.HandleResultAsync (result, ServiceProvider, default, default, fileOpenedStatus, new CancellationToken());
    }
  }
}

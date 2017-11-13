#region copyright
// Copyright (c) rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.ButtonHandler;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.Options;
using LicenseHeaderManager.PackageCommands;
using LicenseHeaderManager.ReturnObjects;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Constants = EnvDTE.Constants;
using Document = LicenseHeaderManager.Headers.Document;

namespace LicenseHeaderManager
{

  #region package infrastructure

  /// <summary>
  /// This is the class that implements the package exposed by this assembly.
  ///
  /// The minimum requirement for a class to be considered a valid package for Visual Studio
  /// is to implement the IVsPackage interface and register itself with the shell.
  /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
  /// to do it: it derives from the Package class that provides the implementation of the 
  /// IVsPackage interface and uses the registration attributes defined in the framework to 
  /// register itself and its components with the shell.
  /// </summary>
  // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
  // a package.
  [PackageRegistration (UseManagedResourcesOnly = true)]
  // This attribute is used to register the informations needed to show the this package
  // in the Help/About dialog of Visual Studio.
  [InstalledProductRegistration ("#110", "#112", Version, IconResourceID = 400)]
  // This attribute is needed to let the shell know that this package exposes some menus.
  [ProvideMenuResource ("Menus.ctmenu", 1)]
  [ProvideOptionPage (typeof (OptionsPage), c_licenseHeaders, c_general, 0, 0, true)]
  [ProvideOptionPage (typeof (LanguagesPage), c_licenseHeaders, c_languages, 0, 0, true)]
  [ProvideOptionPage (typeof (DefaultLicenseHeaderPage), c_licenseHeaders, c_defaultLicenseHeader, 0, 0, true)]
  [ProvideProfile (typeof (OptionsPage), c_licenseHeaders, c_general, 0, 0, true)]
  [ProvideProfile (typeof (LanguagesPage), c_licenseHeaders, c_languages, 0, 0, true)]
  [ProvideProfile (typeof (DefaultLicenseHeaderPage), c_licenseHeaders, c_defaultLicenseHeader, 0, 0, true)]
  [ProvideAutoLoad (VSConstants.UICONTEXT.SolutionOpening_string)]
  [Guid (GuidList.guidLicenseHeadersPkgString)]
  public sealed class LicenseHeadersPackage : Package, ILicenseHeaderExtension
  {
    /// <summary>
    /// Default constructor of the package.
    /// Inside this method you can place any initialization code that does not require 
    /// any Visual Studio service because at this point the package object is created but 
    /// not sited yet inside Visual Studio environment. The place to do all the other 
    /// initialization is the Initialize method.
    /// </summary>
    public LicenseHeadersPackage ()
    {
    }

    public const string Version = "2.0.2";

    private const string c_licenseHeaders = "License Header Manager";
    private const string c_general = "General";
    private const string c_languages = "Languages";
    private const string c_defaultLicenseHeader = "Default Header";

    private DTE2 _dte;

    private ProjectItemsEvents _projectItemEvents;
    private ProjectItemsEvents _websiteItemEvents;
    private CommandEvents _commandEvents;

    private OleMenuCommand _addHeaderCommand;
    private OleMenuCommand _removeHeaderCommand;

    private OleMenuCommand _addHeaderToProjectItemCommand;
    private OleMenuCommand _removeHeaderFromProjectItemCommand;

    private OleMenuCommand _addHeadersToAllFilesInProjectCommand;
    private OleMenuCommand _removeHeadersFromAllFilesInProjectCommand;

    private OleMenuCommand _addNewSolutionHeaderDefinitionFileCommand;
    private OleMenuCommand _openSolutionHeaderDefinitionFileCommand;
    private OleMenuCommand _removeSolutionHeaderDefinitionFileCommand;


    private LicenseHeaderReplacer _licenseReplacer;
    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the 
    /// place where you can put all the initilaization code that rely on services provided by VisualStudio.
    /// </summary>
    protected override void Initialize ()
    {
      base.Initialize ();
      OutputWindowHandler.Initialize(GetGlobalService( typeof( SVsOutputWindow ) ) as IVsOutputWindow);
      _licenseReplacer = new LicenseHeaderReplacer (this);
      _dte = GetService (typeof (DTE)) as DTE2;
      _addedItems = new Stack<ProjectItem>();
      var buttonHandlerFactory = new ButtonHandlerFactory(this, _licenseReplacer);

      //register commands
      OleMenuCommandService mcs = GetService (typeof (IMenuCommandService)) as OleMenuCommandService;
      if (mcs != null)
      {
        AddNewSolutionLicenseHeaderDefinitionFileCommand.Initialize(() =>
        {
          var page2 = (DefaultLicenseHeaderPage)GetDialogPage(typeof(DefaultLicenseHeaderPage));
          return page2.LicenseHeaderFileText;
        });
        OpenSolutionLicenseHeaderDefinitionFileCommand.Initialize();
        RemoveSolutionLicenseHeaderDefinitionFileCommand.Initialize();

        _addHeaderCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdAddLicenseHeader, AddLicenseHeaderCallback);
        _removeHeaderCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdRemoveLicenseHeader, RemoveLicenseHeaderCallback);
        _addHeaderCommand.BeforeQueryStatus += QueryEditCommandStatus;

        _addHeaderToProjectItemCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdAddLicenseHeaderToProjectItem, AddLicenseHeaderToProjectItemCallback);
        _removeHeaderFromProjectItemCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdLicenseRemoveHeaderFromProjectItem, RemoveLicenseHeaderFromProjectItemCallback);
        _addHeaderToProjectItemCommand.BeforeQueryStatus += QueryProjectItemCommandStatus;

        _addHeadersToAllFilesInProjectCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdAddLicenseHeadersToAllFilesInProject, AddLicenseHeadersToAllFilesInProjectCallback);
        _removeHeadersFromAllFilesInProjectCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdRemoveLicenseHeadersFromAllFilesInProject, RemoveLicenseHeadersFromAllFilesInProjectCallback);
        _addHeadersToAllFilesInProjectCommand.BeforeQueryStatus += QueryAllFilesCommandStatus;

        _addNewSolutionHeaderDefinitionFileCommand = RegisterCommand(mcs, PkgCmdIDList.cmdIdAddNewSolutionLicenseHeaderDefinitionFile, AddNewSolutionLicenseHeaderDefinitionFileCallback);
        _openSolutionHeaderDefinitionFileCommand = RegisterCommand(mcs, PkgCmdIDList.cmdIdOpenSolutionLicenseHeaderDefinitionFile, OpenSolutionLicenseHeaderDefinitionFileCallback);
        _removeSolutionHeaderDefinitionFileCommand = RegisterCommand(mcs, PkgCmdIDList.cmdIdRemoveSolutionLicenseHeaderDefinitionFile, RemoveSolutionLicenseHeaderDefinitionFileCallback);
        _addNewSolutionHeaderDefinitionFileCommand.BeforeQueryStatus += QuerySolutionCommandStatus;

        RegisterCommand(mcs, PkgCmdIDList.cmdIdAddNewLicenseHeaderDefinitionFileToProject, AddNewLicenseHeaderDefinitionFileToProjectCallback);
        RegisterCommand(mcs, PkgCmdIDList.cmdIdAddExistingLicenseHeaderDefinitionFileToProject, AddExistingLicenseHeaderDefinitionFileToProjectCallback);
        RegisterCommand(mcs, PkgCmdIDList.cmdIdLicenseHeaderOptions, LicenseHeaderOptionsCallback);
        RegisterCommand(mcs, PkgCmdIDList.cmdIdAddLicenseHeaderToAllFilesInSolution, buttonHandlerFactory.CreateAddLicenseHeaderToAllProjectsButtonHandler().HandleButton);
        RegisterCommand(mcs, PkgCmdIDList.cmdIdRemoveLicenseHeaderFromAllFilesInSolution, RemoveLicenseHeaderFromAllFilesInSolutionCallback);
      }

      

      //register ItemAdded event handler
      var events = _dte.Events as Events2;
      if (events != null)
      {
        _projectItemEvents = events.ProjectItemsEvents; //we need to keep a reference, otherwise the object is garbage collected and the event won't be fired
        _projectItemEvents.ItemAdded += ItemAdded;
       
        //Register to WebsiteItemEvents for Website Projects to work
        //Reference: https://social.msdn.microsoft.com/Forums/en-US/dde7d858-2440-43f9-bbdc-3e1b815d4d1e/itemadded-itemremoved-and-itemrenamed-events-not-firing-in-web-projects?forum=vsx
        //Concerns, that the ItemAdded Event gets called on unrelated events, like closing the solution or opening folder, could not be reproduced
        _websiteItemEvents = events.GetObject ("WebSiteItemsEvents") as ProjectItemsEvents;
        if (_websiteItemEvents != null)
        {
          _websiteItemEvents.ItemAdded += ItemAdded;
        }
      }

      //register event handlers for linked commands
      var page = (OptionsPage) GetDialogPage (typeof (OptionsPage));
      if (page != null)
      {
        foreach (var command in page.LinkedCommands)
        {
          command.Events = _dte.Events.CommandEvents[command.Guid, command.Id];

          switch (command.ExecutionTime)
          {
            case ExecutionTime.Before:
              command.Events.BeforeExecute += BeforeLinkedCommandExecuted;
              break;
            case ExecutionTime.After:
              command.Events.AfterExecute += AfterLinkedCommandExecuted;
              break;
          }
        }

        page.LinkedCommandsChanged += CommandsChanged;

        //register global event handler for ItemAdded
        _commandEvents = _dte.Events.CommandEvents;
        _commandEvents.BeforeExecute += BeforeAnyCommandExecuted;
      }
    }


    private OleMenuCommand RegisterCommand (OleMenuCommandService service, uint id, EventHandler handler)
    {
      var commandId = new CommandID (GuidList.guidLicenseHeadersCmdSet, (int) id);
      var command = new OleMenuCommand (handler, commandId);
      service.AddCommand (command);
      return command;
    }

    /// <summary>
    /// Called by Visual Studio. Hides the commands in the edit menu when the active document doesn't support license headers.
    /// </summary>
    private void QueryEditCommandStatus (object sender, EventArgs e)
    {
      bool visible = false;

      var item = GetActiveProjectItem ();
      if (item != null)
      {
        visible = ShouldBeVisible(item);
      }

      this._addHeaderCommand.Visible = visible;
      this._removeHeaderCommand.Visible = visible;
    }

    /// <summary>
    /// Called by Visual Studio. Hides the commands in the project item context menu.
    /// </summary>
    private void QueryProjectItemCommandStatus (object sender, EventArgs e)
    {
      bool visible = false;

      ProjectItem item = GetSolutionExplorerItem () as ProjectItem;

      if (item != null)
      {
        visible = ShouldBeVisible(item);
      }

      this._addHeaderToProjectItemCommand.Visible = visible;
      this._removeHeaderFromProjectItemCommand.Visible = visible;
    }

    /// <summary>
    /// Called by Visual Studio. Hides the commands in the project and folder context menu.
    /// </summary>
    private void QueryAllFilesCommandStatus (object sender, EventArgs e)
    {
      bool visible = false;

      object obj = GetSolutionExplorerItem ();
      ProjectItem item = obj as ProjectItem;
      if (item != null)
      {
        visible = ShouldBeVisible(item);
      }
      else
      {
        Project project = obj as Project;
        visible = project != null;
      }

      _addHeadersToAllFilesInProjectCommand.Visible = visible;
      _removeHeadersFromAllFilesInProjectCommand.Visible = visible;
    }

    private void QuerySolutionCommandStatus(object sender, EventArgs eventArgs)
    {
      bool solutionHeaderDefinitionExists = SolutionHeaderDefinitionExists();

      _addNewSolutionHeaderDefinitionFileCommand.Enabled = !solutionHeaderDefinitionExists;
      _openSolutionHeaderDefinitionFileCommand.Enabled = solutionHeaderDefinitionExists;
      _removeSolutionHeaderDefinitionFileCommand.Enabled = solutionHeaderDefinitionExists;
    }

    private bool SolutionHeaderDefinitionExists()
    {
      string solutionHeaderDefinitionFilePath = LicenseHeader.GetHeaderDefinitionFilePathForSolution(_dte.Solution);
      return File.Exists(solutionHeaderDefinitionFilePath);
    }

    private bool ShouldBeVisible(ProjectItem item)
    {
      bool visible = false;

      if (ProjectItemInspection.IsPhysicalFile(item))
      {
        Document document;
        bool wasOpen;

        visible = _licenseReplacer.TryCreateDocument(item, out document, out wasOpen) ==
                  CreateDocumentResult.DocumentCreated;
      }
      return visible;
    }

    private ProjectItem GetActiveProjectItem ()
    {
      try
      {
        var activeDocument = _dte.ActiveDocument;
        if (activeDocument == null)
          return null;
        else
          return activeDocument.ProjectItem;
      }
      catch (ArgumentException)
      {
        return null;
      }
    }

    private bool _isCalledByLinkedCommand = false;
    private object GetSolutionExplorerItem ()
    {
      IntPtr hierarchyPtr, selectionContainerPtr;
      uint projectItemId;

      IVsMultiItemSelect mis;
      IVsMonitorSelection monitorSelection = (IVsMonitorSelection) GetGlobalService (typeof (SVsShellMonitorSelection));

      monitorSelection.GetCurrentSelection (out hierarchyPtr, out projectItemId, out mis, out selectionContainerPtr);
      IVsHierarchy hierarchy = Marshal.GetTypedObjectForIUnknown (hierarchyPtr, typeof (IVsHierarchy)) as IVsHierarchy;

      if (hierarchy != null)
      {
        object item;
        hierarchy.GetProperty (projectItemId, (int) __VSHPROPID.VSHPROPID_ExtObject, out item);
        return item;
      }

      return null;
    }

    /// <summary>
    /// Executes a command asynchronously.
    /// </summary>
    private void PostExecCommand (Guid guid, uint id, object argument)
    {
      IVsUIShell shell = (IVsUIShell) GetService (typeof (SVsUIShell));
      shell.PostExecCommand (ref guid,
                            id,
                            (uint) vsCommandExecOption.vsCommandExecOptionDoDefault,
                            ref argument);
    }

  #endregion

    #region event handlers

    private void BeforeLinkedCommandExecuted (string guid, int id, object customIn, object customOut, ref bool cancelDefault)
    {
      InvokeAddLicenseHeaderCommandFromLinkedCmd ();
    }

    private void AfterLinkedCommandExecuted (string guid, int id, object customIn, object customOut)
    {
      InvokeAddLicenseHeaderCommandFromLinkedCmd();
    }

    private void InvokeAddLicenseHeaderCommandFromLinkedCmd()
    {
      _isCalledByLinkedCommand = true;
      this._addHeaderCommand.Invoke (false);
      _isCalledByLinkedCommand = false;
    }

    private void CommandsChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Move)
        return;

      if (e.OldItems != null)
      {
        foreach (LinkedCommand command in e.OldItems)
        {
          switch (command.ExecutionTime)
          {
            case ExecutionTime.Before:
              command.Events.BeforeExecute -= BeforeLinkedCommandExecuted;
              break;
            case ExecutionTime.After:
              command.Events.AfterExecute -= AfterLinkedCommandExecuted;
              break;
          }
        }
      }

      if (e.NewItems != null)
      {
        foreach (LinkedCommand command in e.NewItems)
        {
          command.Events = _dte.Events.CommandEvents[command.Guid, command.Id];

          switch (command.ExecutionTime)
          {
            case ExecutionTime.Before:
              command.Events.BeforeExecute += BeforeLinkedCommandExecuted;
              break;
            case ExecutionTime.After:
              command.Events.AfterExecute += AfterLinkedCommandExecuted;
              break;
          }
        }
      }
    }

    #region insert headers in new files

    private string _currentCommandGuid;
    private int _currentCommandId;
    private CommandEvents _currentCommandEvents;
    private Stack<ProjectItem> _addedItems;

    private void BeforeAnyCommandExecuted (string guid, int id, object customIn, object customOut, ref bool cancelDefault)
    {
      //Save the current command in case it adds a new item to the project.
      _currentCommandGuid = guid;
      _currentCommandId = id;
    }

    private void ItemAdded (ProjectItem item)
    {
      //An item was added. Check if we should insert a header automatically.
      var page = (OptionsPage) GetDialogPage (typeof (OptionsPage));
      if (page != null && page.InsertInNewFiles && item != null)
      {
        //Normally the header should be inserted here, but that might interfere with the command
        //currently being executed, so we wait until it is finished.
        _currentCommandEvents = _dte.Events.CommandEvents[_currentCommandGuid, _currentCommandId];
        _currentCommandEvents.AfterExecute += FinishedAddingItem;
        _addedItems.Push (item);
      }
    }

    private void FinishedAddingItem (string guid, int id, object customIn, object customOut)
    {
      //Now we can finally insert the header into the new item.

      while (_addedItems.Count > 0)
      {
        var item = _addedItems.Pop ();
        var headers = LicenseHeaderFinder.GetHeaderDefinitionForItem(item);
        if (headers != null)
          _licenseReplacer.RemoveOrReplaceHeader (item, headers, false);
      }
      _currentCommandEvents.AfterExecute -= FinishedAddingItem;
    }

    #endregion

    #endregion

    #region command handlers

    private void AddLicenseHeaderCallback (object sender, EventArgs e)
    {
      var item = GetActiveProjectItem ();
      AddLicenseHeaderToItem (item, !_isCalledByLinkedCommand);
    }

    private void AddLicenseHeaderToItem (ProjectItem item, bool calledByUser)
    {
      if (item == null || ProjectItemInspection.IsLicenseHeader(item)) return;


      var headers = LicenseHeaderFinder.GetHeaderDefinitionForItem(item);
      if (headers != null)
      {
        _licenseReplacer.RemoveOrReplaceHeader (item, headers, calledByUser);
      }
      else
      {
        var page = (DefaultLicenseHeaderPage) GetDialogPage (typeof (DefaultLicenseHeaderPage));
        if (calledByUser && LicenseHeader.ShowQuestionForAddingLicenseHeaderFile (item.ContainingProject, page))
          AddLicenseHeaderToItem (item, true);
      }
    }

    private void AddLicenseHeaderToProjectItemCallback (object sender, EventArgs e)
    {
      var args = e as OleMenuCmdEventArgs;
      if (args == null) return;
      var item = args.InValue as ProjectItem;
      if(item == null)
        item = GetSolutionExplorerItem() as ProjectItem;

      if (item != null && ProjectItemInspection.IsPhysicalFile(item) && !ProjectItemInspection.IsLicenseHeader(item))
      {
        AddLicenseHeaderToItem (item, !_isCalledByLinkedCommand);
      }
    }

    private void RemoveLicenseHeaderCallback (object sender, EventArgs e)
    {
      var item = GetActiveProjectItem ();

      if (item != null)
      {
        IDictionary<string, string[]> headers = null;
        _licenseReplacer.RemoveOrReplaceHeader (item, headers, true);
      }
    }

    private void RemoveLicenseHeaderFromProjectItemCallback (object sender, EventArgs e)
    {
      OleMenuCmdEventArgs args = e as OleMenuCmdEventArgs;
      if (args != null)
      {
        ProjectItem item = args.InValue as ProjectItem ?? GetSolutionExplorerItem () as ProjectItem;
        if (item != null && Path.GetExtension (item.Name) != LicenseHeader.Extension)
          _licenseReplacer.RemoveOrReplaceHeaderRecursive (item, null, false);
      }
    }

    private void AddLicenseHeadersToAllFilesInProjectCallback (object sender, EventArgs e)
    {
      var obj = GetSolutionExplorerItem ();
      var addLicenseHeaderToAllFilesCommand = new AddLicenseHeaderToAllFilesInProjectCommand(_licenseReplacer);
      
      var statusBar = (IVsStatusbar) GetService (typeof (SVsStatusbar));
      statusBar.SetText (Resources.UpdatingFiles);

      var addLicenseHeaderToAllFilesReturn = addLicenseHeaderToAllFilesCommand.Execute (obj);

      statusBar.SetText (String.Empty);

      HandleLinkedFilesAndShowMessageBox (addLicenseHeaderToAllFilesReturn.LinkedItems);

      this.HandleAddLicenseHeaderToAllFilesInProjectReturn(obj, addLicenseHeaderToAllFilesReturn);
    }

    private void HandleAddLicenseHeaderToAllFilesInProjectReturn(object obj,
      AddLicenseHeaderToAllFilesReturn addLicenseHeaderToAllFilesReturn)
    {
      var project = obj as Project;
      var projectItem = obj as ProjectItem;
      if (project == null && projectItem == null) return;

      if (addLicenseHeaderToAllFilesReturn.NoHeaderFound)
      {
        // No license header found...
        if (MessageBoxHelper.DoYouWant(Resources.Question_AddNewLicenseHeaderDefinitionForSolution))
        {
          this.AddNewSolutionLicenseHeaderDefinitionFileCallback(this, new EventArgs());
        }
      }
    }

    private void HandleLinkedFilesAndShowMessageBox(List<ProjectItem> linkedItems)
    {
      LinkedFileFilter linkedFileFilter = new LinkedFileFilter(_dte.Solution);
      linkedFileFilter.Filter(linkedItems);

      LinkedFileHandler linkedFileHandler = new LinkedFileHandler();
      linkedFileHandler.Handle(_licenseReplacer, linkedFileFilter);

      if (linkedFileHandler.Message != string.Empty)
      {
        MessageBox.Show(linkedFileHandler.Message, Resources.NameOfThisExtension, MessageBoxButton.OK,
          MessageBoxImage.Information);
      }
    }   

    private void RemoveLicenseHeadersFromAllFilesInProjectCallback (object sender, EventArgs e)
    {
      var obj = GetSolutionExplorerItem ();
      RemoveLicenseHeadersFromAllFiles(obj);
    }

    private void RemoveLicenseHeadersFromAllFiles(object obj)
    {
      var removeAllLicenseHeadersCommand = new RemoveLicenseHeaderFromAllFilesInProjectCommand(_licenseReplacer);

      IVsStatusbar statusBar = (IVsStatusbar) GetService (typeof (SVsStatusbar));
      statusBar.SetText (Resources.UpdatingFiles);

      removeAllLicenseHeadersCommand.Execute(obj);

      statusBar.SetText(String.Empty);
    }

    private void AddNewLicenseHeaderDefinitionFileToProjectCallback (object sender, EventArgs e)
    {
      var page = (DefaultLicenseHeaderPage) GetDialogPage (typeof (DefaultLicenseHeaderPage));
      var solutionItem = GetSolutionExplorerItem();
      var project = solutionItem as Project;
      if (project == null)
      {
        var projectItem = solutionItem as ProjectItem;
        if (projectItem != null)
          LicenseHeader.AddLicenseHeaderDefinitionFile (projectItem, page);
      }

      if (project != null)
      {
        var licenseHeaderDefinitionFile = LicenseHeader.AddHeaderDefinitionFile (project, page);
        licenseHeaderDefinitionFile.Open(Constants.vsViewKindCode).Activate();
      }
    }

    private void AddExistingLicenseHeaderDefinitionFileToProjectCallback (object sender, EventArgs e)
    {
      var project = GetSolutionExplorerItem () as Project;
      var projectItem = GetSolutionExplorerItem () as ProjectItem;

      string fileName = "";

      if (project != null)
      {
        fileName = project.FileName;
      }
      else if (projectItem != null)
      {
        fileName = projectItem.Name;
      }
      else
      {
        return;
      }

      ProjectItems projectItems = null;

      if (project != null)
      {
        projectItems = project.ProjectItems;
      }
      else if (projectItem != null)
      {
        projectItems = projectItem.ProjectItems;
      }

      new AddExistingLicenseHeaderDefinitionFileToProjectCommand().AddDefinitionFileToOneProject(fileName, projectItems);
    }

    private void AddNewSolutionLicenseHeaderDefinitionFileCallback(object sender, EventArgs e)
    {
      AddNewSolutionLicenseHeaderDefinitionFileCommand.Instance.Execute(_dte.Solution);
    }

    private void OpenSolutionLicenseHeaderDefinitionFileCallback(object sender, EventArgs e)
    {
      OpenSolutionLicenseHeaderDefinitionFileCommand.Instance.Execute(_dte.Solution);
    }

    private void RemoveSolutionLicenseHeaderDefinitionFileCallback(object sender, EventArgs e)
    {
      RemoveSolutionLicenseHeaderDefinitionFileCommand.Instance.Execute(_dte.Solution);
    }

    private void LicenseHeaderOptionsCallback (object sender, EventArgs e)
    {
      ShowOptionPage (typeof (OptionsPage));
    }

    private void RemoveLicenseHeaderFromAllFilesInSolutionCallback (object sender, EventArgs e)
    {
      Solution solution = _dte.Solution;
      IVsStatusbar statusBar = (IVsStatusbar) GetService (typeof (SVsStatusbar));
      var removeLicenseHeaderFromAllProjects = new RemoveLicenseHeaderFromAllFilesInSolutionCommand(statusBar, _licenseReplacer);
      bool resharperSuspended = CommandUtility.ExecuteCommandIfExists("ReSharper_Suspend", _dte);

      try
      {
        removeLicenseHeaderFromAllProjects.Execute(solution);
      }
      catch (Exception exception)
      {
        MessageBoxHelper.Information(string.Format("The command '{0}' failed with the exception '{1}'. See Visual Studio Output Window for Details.", removeLicenseHeaderFromAllProjects.GetCommandName(), exception.Message));
        OutputWindowHandler.WriteMessage(exception.ToString());
      }

      if (resharperSuspended)
      {
        CommandUtility.ExecuteCommand("ReSharper_Resume", _dte);  
      }
    }

    #endregion


    public void ShowLanguagesPage ()
    {
      ShowOptionPage (typeof (LanguagesPage));
    }

    public IDefaultLicenseHeaderPage DefaultLicenseHeaderPage
    {
      get { return (DefaultLicenseHeaderPage) GetDialogPage (typeof (DefaultLicenseHeaderPage)); }
    }

    public ILanguagesPage LanguagesPage
    {
      get { return (LanguagesPage) GetDialogPage (typeof (LanguagesPage)); }
    }

    public IOptionsPage OptionsPage
    {
      get { return (OptionsPage) GetDialogPage (typeof (OptionsPage)); }
    }

    public DTE2 Dte2
    {
      get { return GetService(typeof (DTE)) as DTE2; }
    }
  }
}
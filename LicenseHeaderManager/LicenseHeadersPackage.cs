#region copyright
// Copyright (c) 2011 rubicon IT GmbH

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
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.Headers;
using LicenseHeaderManager.Options;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Constants = EnvDTE.Constants;
using Document = LicenseHeaderManager.Headers.Document;
using Language = LicenseHeaderManager.Options.Language;

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

    public const string Version = "1.3.1";

    private const string c_licenseHeaders = "License Header Manager";
    private const string c_general = "General";
    private const string c_languages = "Languages";
    private const string c_defaultLicenseHeader = "Default Header";

    private DTE2 _dte;

    private ProjectItemsEvents _projectItemEvents;
    private CommandEvents _commandEvents;

    private OleMenuCommand _addLicenseHeaderCommand;
    private OleMenuCommand _removeLicenseHeaderCommand;

    private OleMenuCommand _addLicenseHeaderToProjectItemCommand;
    private OleMenuCommand _removeLicenseHeaderFromProjectItemCommand;

    private OleMenuCommand _addLicenseHeadersToAllFilesCommand;
    private OleMenuCommand _removeLicenseHeadersFromAllFilesCommand;


    private LicenseHeaderReplacer _licenseReplacer;
    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the 
    /// place where you can put all the initilaization code that rely on services provided by VisualStudio.
    /// </summary>
    protected override void Initialize ()
    {
      base.Initialize ();
      _licenseReplacer = new LicenseHeaderReplacer (this);
      _dte = GetService (typeof (DTE)) as DTE2;

      //register commands
      OleMenuCommandService mcs = GetService (typeof (IMenuCommandService)) as OleMenuCommandService;
      if (mcs != null)
      {
        _addLicenseHeaderCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdAddLicenseHeader, AddLicenseHeaderCallback);
        _removeLicenseHeaderCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdRemoveLicenseHeader, RemoveLicenseHeaderCallback);
        _addLicenseHeaderCommand.BeforeQueryStatus += QueryEditCommandStatus;

        _addLicenseHeaderToProjectItemCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdAddLicenseHeaderToProjectItem, AddLicenseHeaderToProjectItemCallback);
        _removeLicenseHeaderFromProjectItemCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdRemoveLicenseHeaderFromProjectItem, RemoveLicenseHeaderFromProjectItemCallback);
        _addLicenseHeaderToProjectItemCommand.BeforeQueryStatus += QueryProjectItemCommandStatus;

        _addLicenseHeadersToAllFilesCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdAddLicenseHeadersToAllFiles, AddLicenseHeadersToAllFilesCallback);
        _removeLicenseHeadersFromAllFilesCommand = RegisterCommand (mcs, PkgCmdIDList.cmdIdRemoveLicenseHeadersFromAllFiles, RemoveLicenseHeadersFromAllFilesCallback);
        _addLicenseHeadersToAllFilesCommand.BeforeQueryStatus += QueryAllFilesCommandStatus;

        RegisterCommand (mcs, PkgCmdIDList.cmdIdAddLicenseHeaderDefinitionFile, AddLicenseHeaderDefinitionFileCallback);
        RegisterCommand (mcs, PkgCmdIDList.cmdIdAddExistingLicenseHeaderDefinitionFile, AddExistingLicenseHeaderDefinitionFileCallback);
        RegisterCommand (mcs, PkgCmdIDList.cmdIdLicenseHeaderOptions, LicenseHeaderOptionsCallback);
      }

      //register ItemAdded event handler
      var events = _dte.Events as Events2;
      if (events != null)
      {
        _projectItemEvents = events.ProjectItemsEvents; //we need to keep a reference, otherwise the object is garbage collected and the event won't be fired
        _projectItemEvents.ItemAdded += ItemAdded;
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
        if (item.Kind == Constants.vsProjectItemKindPhysicalFile)
        {
          Document document;
          visible = _licenseReplacer.TryCreateDocument (item, out document) == CreateDocumentResult.DocumentCreated;
        }
        else
          visible = true;
      }

      _addLicenseHeaderCommand.Visible = visible;
      _removeLicenseHeaderCommand.Visible = visible;
    }

    /// <summary>
    /// Called by Visual Studio. Hides the commands in the project item context menu.
    /// </summary>
    private void QueryProjectItemCommandStatus (object sender, EventArgs e)
    {
      bool visible = false;

      ProjectItem item = GetSolutionExplorerItem () as ProjectItem;

      if (item != null && item.Kind == Constants.vsProjectItemKindPhysicalFile)
      {
        Document document;
        visible = _licenseReplacer.TryCreateDocument (item, out document) == CreateDocumentResult.DocumentCreated;
      }

      _addLicenseHeaderToProjectItemCommand.Visible = visible;
      _removeLicenseHeaderFromProjectItemCommand.Visible = visible;
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
        if (item.Kind == Constants.vsProjectItemKindPhysicalFile)
        {
          Document document;
          visible = _licenseReplacer.TryCreateDocument (item, out document) == CreateDocumentResult.DocumentCreated;
        }
      }
      else
      {
        Project project = obj as Project;
        visible = project != null;
      }

      _addLicenseHeadersToAllFilesCommand.Visible = visible;
      _removeLicenseHeadersFromAllFilesCommand.Visible = visible;
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
      _addLicenseHeaderCommand.Invoke (false);
    }

    private void AfterLinkedCommandExecuted (string guid, int id, object customIn, object customOut)
    {
      _addLicenseHeaderCommand.Invoke (false);
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
    private ProjectItem _addedItem;

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
        _addedItem = item;
      }
      else
        _addedItem = null;
    }

    private void FinishedAddingItem (string guid, int id, object customIn, object customOut)
    {
      //Now we can finally insert the header into the new item.
      if (_addedItem != null)
      {
        PostExecCommand (GuidList.guidLicenseHeadersCmdSet, PkgCmdIDList.cmdIdAddLicenseHeaderToProjectItem, _addedItem);
        _addedItem = null;
      }
      _currentCommandEvents.AfterExecute -= FinishedAddingItem;
    }

    #endregion

    #endregion

    #region command handlers

    private void AddLicenseHeaderCallback (object sender, EventArgs e)
    {
      var item = GetActiveProjectItem ();
      AddLicenseHeaderToItem (item, true);
    }

    private void AddLicenseHeaderToItem (ProjectItem item, bool calledByUser)
    {
      if (item != null)
      {
        var headers = LicenseHeaderFinder.GetHeaderRecursive (item);
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
    }


    private void AddLicenseHeaderToProjectItemCallback (object sender, EventArgs e)
    {
      var args = e as OleMenuCmdEventArgs;
      if (args != null)
      {
        var item = args.InValue as ProjectItem;
        bool calledByUser = item == null;
        if (calledByUser)
          item = GetSolutionExplorerItem () as ProjectItem;

        if (item != null && item.Kind == Constants.vsProjectItemKindPhysicalFile && Path.GetExtension (item.Name) != LicenseHeader.Extension)
        {
          AddLicenseHeaderToItem (item, calledByUser);
        }
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

    private void AddLicenseHeadersToAllFilesCallback (object sender, EventArgs e)
    {
      var obj = GetSolutionExplorerItem ();
      AddLicenseHeaderToAllFiles (obj);
    }

    private void AddLicenseHeaderToAllFiles(object obj)
    {
      var project = obj as Project;
      var item = obj as ProjectItem;
      int countSubLicenseHeadersFound = 0;
      if (project != null || item != null)
      {
        var statusBar = (IVsStatusbar) GetService (typeof (SVsStatusbar));
        statusBar.SetText (Resources.UpdatingFiles);

        _licenseReplacer.ResetExtensionsWithInvalidHeaders ();
        IDictionary<string, string[]> headers = null;
        if (project != null)
        {
          headers = LicenseHeaderFinder.GetHeader (project);
          foreach (ProjectItem i in project.ProjectItems)
            countSubLicenseHeadersFound = _licenseReplacer.RemoveOrReplaceHeaderRecursive (i, headers);
        }
        else
        {
          headers = LicenseHeaderFinder.GetHeaderRecursive (item);
          foreach (ProjectItem i in item.ProjectItems)
            countSubLicenseHeadersFound = _licenseReplacer.RemoveOrReplaceHeaderRecursive (i, headers);
        }

        statusBar.SetText (String.Empty);
        if (countSubLicenseHeadersFound == 0 && headers == null)
        {
          //No license header found...
          var page = (DefaultLicenseHeaderPage) GetDialogPage (typeof (DefaultLicenseHeaderPage));
          if (LicenseHeader.ShowQuestionForAddingLicenseHeaderFile (project ?? item.ContainingProject, page))
            AddLicenseHeaderToAllFiles (obj);
        }
      }
    }


    private void RemoveLicenseHeadersFromAllFilesCallback (object sender, EventArgs e)
    {
      var obj = GetSolutionExplorerItem ();
      var project = obj as Project;
      var item = obj as ProjectItem;

      if (project != null || item != null)
      {
        IVsStatusbar statusBar = (IVsStatusbar) GetService (typeof (SVsStatusbar));
        statusBar.SetText (Resources.UpdatingFiles);
        
        _licenseReplacer.ResetExtensionsWithInvalidHeaders ();
        if (project != null)
        {
          foreach (ProjectItem i in project.ProjectItems)
            _licenseReplacer.RemoveOrReplaceHeaderRecursive (i, null, false);
        }
        else
        {
          foreach (ProjectItem i in item.ProjectItems)
            _licenseReplacer.RemoveOrReplaceHeaderRecursive (i, null, false);
        }

        statusBar.SetText (String.Empty);
      }
    }

    private void AddLicenseHeaderDefinitionFileCallback (object sender, EventArgs e)
    {
      var page = (DefaultLicenseHeaderPage) GetDialogPage (typeof (DefaultLicenseHeaderPage));
      var solutionItem = GetSolutionExplorerItem();
      var project = solutionItem as Project;
      if (project == null)
      {
        var projectItem = solutionItem as ProjectItem;
        if (projectItem != null)
          project = projectItem.ContainingProject;
      }

      if(project != null)
        LicenseHeader.ShowQuestionForAddingLicenseHeaderFile(project, page);
    }

    private void AddExistingLicenseHeaderDefinitionFileCallback (object sender, EventArgs e)
    {
      var project = GetSolutionExplorerItem () as Project;
      if (project != null)
      {
        FileDialog dialog = new OpenFileDialog ();
        dialog.CheckFileExists = true;
        dialog.CheckPathExists = true;
        dialog.DefaultExt = LicenseHeader.Extension;
        dialog.DereferenceLinks = true;
        dialog.Filter = "License Header Definitions|*" + LicenseHeader.Extension;
        dialog.InitialDirectory = Path.GetDirectoryName (project.FileName);

        bool? result = dialog.ShowDialog ();
        if (result.HasValue && result.Value)
          project.ProjectItems.AddFromFile (dialog.FileName);
      }
    }

    private void LicenseHeaderOptionsCallback (object sender, EventArgs e)
    {
      ShowOptionPage (typeof (OptionsPage));
    }

    #endregion


    public void ShowLanguagesPage ()
    {
      ShowOptionPage (typeof (LanguagesPage));
    }

    public DefaultLicenseHeaderPage GetDefaultLicenseHeaderPage ()
    {
      return (DefaultLicenseHeaderPage) GetDialogPage (typeof (DefaultLicenseHeaderPage));
    }

    public LanguagesPage GetLanguagesPage ()
    {
      return (LanguagesPage) GetDialogPage (typeof (LanguagesPage));
    }

    public OptionsPage GetOptionsPage ()
    {
      return (OptionsPage) GetDialogPage (typeof (OptionsPage));
    }
  }
}
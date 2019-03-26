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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.Options.Converters;

namespace LicenseHeaderManager.Options
{
  [ClassInterface (ClassInterfaceType.AutoDual)]
  [Guid ("EB6F9B18-D203-43E3-8033-35AD9BEFC70D")]
  public class OptionsPage : VersionedDialogPage, IOptionsPage
  {

    private const bool c_defaultInsertInNewFiles = false;
    private const bool c_defaultUseRequiredKeywords = true;
    private const string c_defaultRequiredKeywords = "license, copyright, (c), ©";
    private readonly ObservableCollection<LinkedCommand> _defaultLinkedCommands = new ObservableCollection<LinkedCommand>();

    private readonly LinkedCommandConverter _linkedCommandConverter = new LinkedCommandConverter();

    public event NotifyCollectionChangedEventHandler LinkedCommandsChanged;

    private DTE2 Dte
    {
      get { return GetService (typeof(DTE)) as DTE2; }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Commands Commands
    {
      get { return Dte.Commands; }
    }

    //serialized properties
    public bool InsertInNewFiles { get; set; }
    public bool UseRequiredKeywords { get; set; }
    public string RequiredKeywords { get; set; }

    private ObservableCollection<LinkedCommand> _linkedCommands;

    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    public ObservableCollection<LinkedCommand> LinkedCommands
    {
      get { return _linkedCommands; }
      set
      {
        if (_linkedCommands != null)
        {
          _linkedCommands.CollectionChanged -= OnLinkedCommandsChanged;
          LinkedCommandsChanged?.Invoke (
              _linkedCommands,
              new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, _linkedCommands));
        }
        _linkedCommands = value;
        if (_linkedCommands != null)
        {
          _linkedCommands.CollectionChanged += OnLinkedCommandsChanged;
          LinkedCommandsChanged?.Invoke (value, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, _linkedCommands));
        }
      }
    }

    [DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
    // ReSharper disable once UnusedMember.Global
    public string LinkedCommandsSerialized
    {
      get { return _linkedCommandConverter.ToXml (_linkedCommands); }
      set { _linkedCommands = new ObservableCollection<LinkedCommand> (_linkedCommandConverter.FromXml (value)); }
    }

    private void OnLinkedCommandsChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
      LinkedCommandsChanged?.Invoke (sender, e);
    }

    public OptionsPage ()
    {
      ResetSettings();
    }

    public override sealed void ResetSettings ()
    {
      InsertInNewFiles = c_defaultInsertInNewFiles;
      UseRequiredKeywords = c_defaultUseRequiredKeywords;
      RequiredKeywords = c_defaultRequiredKeywords;
      LinkedCommands = _defaultLinkedCommands;
      base.ResetSettings();
    }

    [Browsable (false)]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    protected override IWin32Window Window
    {
      get
      {
        var host = new WpfHost (new WpfOptions (this));
        return host;
      }
    }

    #region version updates

    protected override IEnumerable<UpdateStep> GetVersionUpdateSteps ()
    {
      yield return new UpdateStep (new Version (3, 0, 1), MigrateStorageLocation_3_0_1);
    }

    private void MigrateStorageLocation_3_0_1 ()
    {
      if (!System.Version.TryParse (Version, out var version) || version < new Version (3, 0, 0))
      {
        LoadRegistryValuesBefore_3_0_0();
      }
      else
      {
        var migratedOptionsPage = new OptionsPage();
        LoadRegistryValuesBefore_3_0_0 (migratedOptionsPage);

        InsertInNewFiles = ThreeWaySelectionForMigration (
            InsertInNewFiles,
            migratedOptionsPage.InsertInNewFiles,
            c_defaultInsertInNewFiles);
        UseRequiredKeywords = ThreeWaySelectionForMigration (
            UseRequiredKeywords,
            migratedOptionsPage.UseRequiredKeywords,
            c_defaultUseRequiredKeywords);
        RequiredKeywords = ThreeWaySelectionForMigration (
            RequiredKeywords,
            migratedOptionsPage.RequiredKeywords,
            c_defaultRequiredKeywords);
        LinkedCommands = migratedOptionsPage.LinkedCommands;
      }
    }

    #endregion
  }
}
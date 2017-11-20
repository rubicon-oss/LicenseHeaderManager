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
    public event NotifyCollectionChangedEventHandler LinkedCommandsChanged;

    private DTE2 Dte
    {
      get { return GetService (typeof(DTE)) as DTE2; }
    }

    public Commands Commands
    {
      get { return Dte.Commands; }
    }

    //serialized properties
    public bool InsertInNewFiles { get; set; }
    public bool UseRequiredKeywords { get; set; }
    public string RequiredKeywords { get; set; }

    private ObservableCollection<LinkedCommand> _linkedCommands;

    [TypeConverter (typeof(LinkedCommandConverter))]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
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
      InsertInNewFiles = false;
      UseRequiredKeywords = true;
      RequiredKeywords = "license, copyright, (c), ©";
      LinkedCommands = new ObservableCollection<LinkedCommand>();
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
  }
}
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.Options.Converters;
using Microsoft.VisualStudio.Shell;
using System.Collections.ObjectModel;
using System.Collections;

namespace LicenseHeaderManager.Options
{
  [ClassInterface (ClassInterfaceType.AutoDual)]
  [Guid ("EB6F9B18-D203-43E3-8033-35AD9BEFC70D")]
  public class OptionsPage : DialogPage
  {
    public event NotifyCollectionChangedEventHandler ChainedCommandsChanged;

    private DTE2 Dte { get { return GetService (typeof (DTE)) as DTE2; } }
    
    public Commands Commands { get { return Dte.Commands; } }

    //serialized properties
    public bool UseRequiredKeywords { get; set; }
    public string RequiredKeywords { get; set; }

    private ObservableCollection<ChainedCommand> _chainedCommands;
    [TypeConverter (typeof (ChainedCommandConverter))]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
    public ObservableCollection<ChainedCommand> ChainedCommands {
      get { return _chainedCommands; }
      set {
        if (_chainedCommands != null)
        {
          _chainedCommands.CollectionChanged -= OnChainedCommandsChanged;
          if (ChainedCommandsChanged != null)
            ChainedCommandsChanged (_chainedCommands, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, _chainedCommands));
        }
        _chainedCommands = value;
        if (_chainedCommands != null)
        {
          _chainedCommands.CollectionChanged += OnChainedCommandsChanged;
          if (ChainedCommandsChanged != null)
            ChainedCommandsChanged (value, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, _chainedCommands));
        }
      }
    }

    private void OnChainedCommandsChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
      if (ChainedCommandsChanged != null)
        ChainedCommandsChanged (sender, e);
    }

    public OptionsPage ()
    {
      ResetSettings ();
    }

    public override void ResetSettings ()
    {
      UseRequiredKeywords = true;
      RequiredKeywords = "license, copyright, (c)";
      ChainedCommands = new ObservableCollection<ChainedCommand> ();
      base.ResetSettings ();
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
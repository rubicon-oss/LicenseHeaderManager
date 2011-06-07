using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using LicenseHeaderManager.Options.Converters;
using Microsoft.VisualStudio.Shell;
using System.Collections.ObjectModel;

namespace LicenseHeaderManager.Options
{
  [ClassInterface (ClassInterfaceType.AutoDual)]
  [Guid ("EB6F9B18-D203-43E3-8033-35AD9BEFC70D")]
  public class OptionsPage : DialogPage
  {
    public event NotifyCollectionChangedEventHandler LinkedCommandsChanged;

    private DTE2 Dte { get { return GetService (typeof (DTE)) as DTE2; } }
    
    public Commands Commands { get { return Dte.Commands; } }

    //serialized properties
    public bool InsertInNewFiles { get; set; }
    public bool UseRequiredKeywords { get; set; }
    public string RequiredKeywords { get; set; }

    private ObservableCollection<LinkedCommand> _linkedCommands;
    [TypeConverter (typeof (LinkedCommandConverter))]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
    public ObservableCollection<LinkedCommand> LinkedCommands {
      get { return _linkedCommands; }
      set {
        if (_linkedCommands != null)
        {
          _linkedCommands.CollectionChanged -= OnLinkedCommandsChanged;
          if (LinkedCommandsChanged != null)
            LinkedCommandsChanged (_linkedCommands, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, _linkedCommands));
        }
        _linkedCommands = value;
        if (_linkedCommands != null)
        {
          _linkedCommands.CollectionChanged += OnLinkedCommandsChanged;
          if (LinkedCommandsChanged != null)
            LinkedCommandsChanged (value, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, _linkedCommands));
        }
      }
    }

    private void OnLinkedCommandsChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
      if (LinkedCommandsChanged != null)
        LinkedCommandsChanged (sender, e);
    }

    public OptionsPage ()
    {
      ResetSettings ();
    }

    public override void ResetSettings ()
    {
      InsertInNewFiles = false;
      UseRequiredKeywords = true;
      RequiredKeywords = "license, copyright, (c)";
      LinkedCommands = new ObservableCollection<LinkedCommand> ();
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
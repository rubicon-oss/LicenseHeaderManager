using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using EnvDTE;

namespace LicenseHeaderManager.Options
{
  public partial class WpfOptions : System.Windows.Controls.UserControl
  {
    public OptionsPage Page { get; set; }

    public WpfOptions (OptionsPage page)
    {
      InitializeComponent();

      DataContext = Page = page;

      Loaded += (s, e) =>
      {
        grid.ItemsSource = page.ChainedCommands;
        EnableButtons ();
      };
    }

    private void Add (object sender, RoutedEventArgs e)
    {
      var dialog = new WpfCommandDialog (new ChainedCommand(), Page.Commands);
      bool? result = dialog.ShowDialog ();
      if (result.HasValue && result.Value)
        Page.ChainedCommands.Add (dialog.Command);
    }

    private void Remove (object sender, RoutedEventArgs e)
    {
      var command = grid.SelectedItem as ChainedCommand;
      if (command != null)
        Page.ChainedCommands.Remove (command);
    }

    private void Edit (object sender, RoutedEventArgs e)
    {
      Edit (grid.SelectedItem as ChainedCommand);
    }

    private void OnClick (object sender, MouseButtonEventArgs e)
    {
      if (e.ClickCount == 2)
        Edit (((FrameworkElement) sender).DataContext as ChainedCommand);
    }

    private void Edit (ChainedCommand command)
    {
      if (command == null)
        return;

      var dialog = new WpfCommandDialog (command, Page.Commands);
      dialog.ShowDialog ();
    }

    private void EnableButtons ()
    {
      edit.IsEnabled = remove.IsEnabled = grid.SelectedItem != null;
    }

    private void OnSelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      EnableButtons ();
    }
  }
}

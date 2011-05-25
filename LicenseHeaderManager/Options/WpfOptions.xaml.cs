using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using EnvDTE;

namespace LicenseHeaderManager.Options
{
  public partial class WpfOptions : System.Windows.Controls.UserControl
  {
    private Timer _timer;

    private ICollectionView View
    {
      get { return commands.ItemsSource as ICollectionView; }
      set {
        commands.ItemsSource = value;
        value.Filter = Filter;
      }
    }

    public IEnumerable Commands {
      get { return View.SourceCollection; }
      set { View = CollectionViewSource.GetDefaultView (value); }
    }

    public OptionsPage Page { get; set; }

    public WpfOptions (OptionsPage page)
    {
      InitializeComponent();

      _timer = new Timer();
      _timer.Interval = 200;
      _timer.Tick += OnTick;

      DataContext = Page = page;
      Commands = from Command c in page.Commands
                 where !String.IsNullOrEmpty (c.Name)
                 orderby c.Name
                 select c;

      Loaded += (s, e) =>
      {
        //reset the filter (without updating the view because the selected
        //item would scroll out of view again once the timer expires)
        search.TextChanged -= OnTextChanged;
        search.Text = string.Empty;
        search.TextChanged += OnTextChanged;  
        
        //refresh the view as it is still only displaying the last filtered result
        if (View != null)
          View.Refresh ();

        //select the currently attached command
        var command = page.GetAttachedCommand ();
        if (command != null)
        {
          commands.SelectedItem = command;
          commands.ScrollIntoView (command);
        }
      };
    }

    private void OnTick (object sender, EventArgs e)
    {
      _timer.Stop ();
      if (View != null)
        View.Refresh ();
    }

    private void OnTextChanged (object sender, TextChangedEventArgs e)
    {
      _timer.Stop ();
      _timer.Start ();
    }

    private bool Filter (object item)
    {
      var command = item as Command;
      if (command != null)
      {
        char[] chars = new[] { ' ', '.' };
        string[] parts = command.Name.Split (chars, StringSplitOptions.RemoveEmptyEntries);
        string[] queries = search.Text.Split (chars, StringSplitOptions.RemoveEmptyEntries);
        return queries.All (q => parts.Any (p => p.ToLower ().Contains (q.ToLower ())));
      }
      return false;
    }

    private void OnSelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      var command = commands.SelectedItem as Command;
      if (command == null)
      {
        Page.AttachedCommandGuid = null;
        Page.AttachedCommandId = -1;
      }
      else
      {
        Page.AttachedCommandGuid = command.Guid;
        Page.AttachedCommandId = command.ID;
      }
    }
  }
}

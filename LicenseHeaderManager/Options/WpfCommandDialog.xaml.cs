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
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using EnvDTE;

namespace LicenseHeaderManager.Options
{
  public partial class WpfCommandDialog : System.Windows.Window
  {
    private Timer _timer;

    private ICollectionView View
    {
      get { return commands.ItemsSource as ICollectionView; }
      set
      {
        commands.ItemsSource = value;
        value.Filter = Filter;
      }
    }

    public IEnumerable Commands
    {
      get { return View.SourceCollection; }
      set { View = CollectionViewSource.GetDefaultView (value); }
    }

    public LinkedCommand Command
    {
      get { return DataContext as LinkedCommand; }
      set { DataContext = value; }
    }

    public WpfCommandDialog (LinkedCommand command, Commands allCommands)
    {
      InitializeComponent();

      _timer = new Timer();
      _timer.Interval = 200;
      _timer.Tick += OnTick;

      Command = command;
      Commands = from Command c in allCommands
          where !String.IsNullOrEmpty (c.Name)
          orderby c.Name
          select c;

      before.IsChecked = command.ExecutionTime == ExecutionTime.Before;
      after.IsChecked = command.ExecutionTime == ExecutionTime.After;

      Loaded += (s, e) =>
      {
        //reset the filter (without updating the view because the selected
        //item would scroll out of view again once the timer expires)
        search.TextChanged -= OnTextChanged;
        search.Text = string.Empty;
        search.TextChanged += OnTextChanged;

        //refresh the view as it is still only displaying the last filtered result
        if (View != null)
          View.Refresh();

        //select the currently attached command
        try
        {
          var selected = allCommands.Item (command.Guid, command.Id);
          if (selected != null)
          {
            commands.SelectedItem = selected;
            commands.ScrollIntoView (selected);
          }
        }
        catch (ArgumentException)
        {
        }
      };
    }

    private void OkButton_Click (object sender, RoutedEventArgs e)
    {
      var command = commands.SelectedItem as Command;
      if (command != null)
      {
        Command.Name = command.Name;
        Command.ExecutionTime = before.IsChecked.Value ? ExecutionTime.Before : ExecutionTime.After;
        Command.Guid = command.Guid;
        Command.Id = command.ID;

        DialogResult = true;
        Close();
      }
    }

    private void OnTick (object sender, EventArgs e)
    {
      _timer.Stop();
      if (View != null)
        View.Refresh();
    }

    private void OnTextChanged (object sender, TextChangedEventArgs e)
    {
      _timer.Stop();
      _timer.Start();
    }

    private bool Filter (object item)
    {
      var command = item as Command;
      if (command != null)
      {
        char[] chars = new[] { ' ', '.' };
        string[] parts = command.Name.Split (chars, StringSplitOptions.RemoveEmptyEntries);
        string[] queries = search.Text.Split (chars, StringSplitOptions.RemoveEmptyEntries);
        return queries.All (q => parts.Any (p => p.ToLower().Contains (q.ToLower())));
      }
      return false;
    }

    private void OnSelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      ok.IsEnabled = commands.SelectedItem != null;
    }
  }
}
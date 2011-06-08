#region copyright
// Copyright (c) 2011 rubicon informationstechnologie gmbh

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LicenseHeaderManager.Options
{
  public partial class WpfLanguages : UserControl
  {
    private LanguagesPage _page;

    public WpfLanguages (LanguagesPage page)
        : this()
    {
      _page = page;

      Loaded += (s, e) =>
      {
        grid.ItemsSource = _page.Languages;
        EnableButtons ();
      };
    }

    public WpfLanguages ()
    {
      InitializeComponent();
    }

    private void Add (object sender, RoutedEventArgs e)
    {
      var dialog = new WpfLanguageDialog ();
      dialog.Language = new Language ();
      bool? result = dialog.ShowDialog ();
      if (result.HasValue && result.Value)
        _page.Languages.Add (dialog.Language);
    }

    private void Remove (object sender, RoutedEventArgs e)
    {
      var language = grid.SelectedItem as Language;
      if (language != null)
        _page.Languages.Remove (language);
    }

    private void Edit (object sender, RoutedEventArgs e)
    {
      Edit (grid.SelectedItem as Language);
    }

    private void OnClick (object sender, MouseButtonEventArgs e)
    {
      if (e.ClickCount == 2)
        Edit (((FrameworkElement) sender).DataContext as Language);
    }

    private void Edit (Language language)
    {
      if (language == null)
        return;

      var copy = language.Clone() as Language;

      var dialog = new WpfLanguageDialog();
      dialog.Language = copy;
      bool? result = dialog.ShowDialog();
      if (result.HasValue && result.Value)
      {
        int index = _page.Languages.IndexOf (language);
        _page.Languages.RemoveAt (index);
        _page.Languages.Insert (index, copy);
      }
    }

    private void EnableButtons ()
    {
      edit.IsEnabled = remove.IsEnabled = grid.SelectedItem != null;
    }

    private void OnSelectionChanged (object sender, SelectionChangedEventArgs e)
    {
      EnableButtons();
    }
  }
}
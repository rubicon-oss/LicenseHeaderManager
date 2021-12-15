/* Copyright (c) rubicon IT GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LicenseHeaderManager.UpdateViewModels;
using LicenseHeaderManager.Utils;
using Microsoft.VisualStudio.PlatformUI;

namespace LicenseHeaderManager.UpdateViews
{
  /// <summary>
  ///   Interaction logic for FolderProjectUpdateDialog.xaml
  /// </summary>
  public partial class FolderProjectUpdateDialog : DialogWindow
  {
    public FolderProjectUpdateDialog (FolderProjectUpdateViewModel folderProjectUpdateViewModel)
    {
      InitializeComponent();
      DataContext = folderProjectUpdateViewModel;
      ((FolderProjectUpdateViewModel) DataContext).PropertyChanged += OnViewModelUpdated;
    }

    private void OnViewModelUpdated (object sender, PropertyChangedEventArgs e)
    {
      UpdateControlsAsync (e).FireAndForget();
    }

    private async Task UpdateControlsAsync (PropertyChangedEventArgs args)
    {
      await LicenseHeadersPackage.Instance.JoinableTaskFactory.SwitchToMainThreadAsync();

      // for unknown reasons, changes in below DependencyProperties' data source are sometimes not reflected in the UI => trigger update manually
      var context = (FolderProjectUpdateViewModel) DataContext;
      switch (args.PropertyName)
      {
        case nameof(context.ProcessedFilesCountCurrentProject):
          BindingOperations.GetMultiBindingExpression (FilesDoneTextBlock, TextBlock.TextProperty)?.UpdateTarget();
          BindingOperations.GetBindingExpression (FilesDoneProgressBar, ProgressBar.ValueProperty)?.UpdateTarget();
          break;
        case nameof(context.FileCountCurrentProject):
          BindingOperations.GetBindingExpression (FilesDoneTextBlock, TextBlock.TextProperty)?.UpdateTarget();
          PreparingTextBlock.Visibility = Visibility.Hidden;
          break;
      }
    }
  }
}

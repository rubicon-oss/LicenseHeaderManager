using System;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.VisualStudio.PlatformUI;

namespace LicenseHeaderManager.SolutionUpdateViewModels
{
  public class SolutionUpdateViewModel : INotifyPropertyChanged
  {
    private string _progressText = "Preparing update...";

    public string ProgressText
    {
      get
      {
        return _progressText;
      }
      set
      {
        _progressText = value;
        NotifyPropertyChanged("ProgressText");
      }
    }

    public ICommand CloseCommand
    {
      get { return new RelayCommand(o => ((DialogWindow) o).Close()); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
  }
}

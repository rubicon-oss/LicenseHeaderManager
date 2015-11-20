#region copyright
// Copyright (c) 2011 rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using EnvDTE;
using System.ComponentModel;

namespace LicenseHeaderManager.Options
{
  public enum ExecutionTime
  {
    Before,
    After
  }

  public class LinkedCommand : INotifyPropertyChanged
  {
    public string Guid { get; set; }
    public int Id { get; set; }
    public CommandEvents Events { get; set; }

    private string _name;
    public string Name
    {
      get { return _name; }
      set {
        if (value != _name)
        {
          _name = value;
          OnPropertyChanged ("Name");
        }
      }
    }

    private ExecutionTime _executionTime;
    public ExecutionTime ExecutionTime
    {
      get { return _executionTime; }
      set
      {
        if (value != _executionTime)
        {
          _executionTime = value;
          OnPropertyChanged ("ExecutionTime");
        }
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged (string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
    }
  }
}

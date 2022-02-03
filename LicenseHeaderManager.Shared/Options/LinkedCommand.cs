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
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using EnvDTE;

namespace LicenseHeaderManager.Options
{
  /// <summary>
  ///   Provides members that define the point in time a <see cref="LinkedCommand" /> is executed.
  /// </summary>
  public enum ExecutionTime
  {
    /// <summary>
    ///   The <see cref="LinkedCommand" /> is executed before the Visual Studio Command it refers to is executed.
    /// </summary>
    Before = 0,

    /// <summary>
    ///   The <see cref="LinkedCommand" /> is executed after the Visual Studio Command it refers to is executed.
    /// </summary>
    After = 1
  }

  /// <summary>
  ///   Represents a
  /// </summary>
  public class LinkedCommand : INotifyPropertyChanged
  {
    private ExecutionTime _executionTime;

    private string _name;

    /// <summary>
    ///   Gets the internal GUID of the Visual Studio Command this <see cref="LinkedCommand" /> instance refers to.
    /// </summary>
    public string Guid { get; set; }

    /// <summary>
    ///   Gets the internal ID of the Visual Studio Command this <see cref="LinkedCommand" /> instance refers to.
    /// </summary>
    public int Id { get; set; }

    [JsonIgnore]
    public CommandEvents Events { get; set; }

    /// <summary>
    ///   Gets the name this <see cref="LinkedCommand" /> instance refers to.
    /// </summary>
    public string Name
    {
      get => _name;
      set
      {
        if (value == _name)
          return;

        _name = value;
        OnPropertyChanged();
      }
    }

    /// <summary>
    ///   Denotes the point in time this <see cref="LinkedCommand" /> instance is executed.
    /// </summary>
    public ExecutionTime ExecutionTime
    {
      get => _executionTime;
      set
      {
        if (value == _executionTime)
          return;

        _executionTime = value;
        OnPropertyChanged();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged ([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
    }

    /// <summary>
    ///   Performs a copy of the current <see cref="LinkedCommand" /> instance.
    /// </summary>
    /// <returns>Returns a copy of the current <see cref="LinkedCommand" /> instance that is equal to it.</returns>
    public LinkedCommand Clone ()
    {
      return new LinkedCommand
             {
                 Name = Name,
                 ExecutionTime = ExecutionTime,
                 Events = Events,
                 Guid = Guid,
                 Id = Id
             };
    }
  }
}

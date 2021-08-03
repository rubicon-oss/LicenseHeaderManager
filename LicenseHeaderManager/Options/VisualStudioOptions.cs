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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Core.Options;

namespace LicenseHeaderManager.Options
{
  /// <summary>
  ///   Encapsulates members that represent the configuration affecting the behaviour of the License Header Manager Visual
  ///   Studio HeaderDefinitionExtension.
  /// </summary>
  [LicenseHeaderManagerOptions]
  internal class VisualStudioOptions
  {
    public const bool DefaultInsertInNewFiles = false;

    public static readonly ObservableCollection<LinkedCommand> s_defaultLinkedCommands = new ObservableCollection<LinkedCommand>();
    private ObservableCollection<LinkedCommand> _linkedCommands;

    public VisualStudioOptions ()
    {
      SetDefaultValues();
    }

    public VisualStudioOptions (bool initializeWithDefaultValues)
    {
      if (initializeWithDefaultValues)
        SetDefaultValues();
      else
        InitializeValues();
    }

    /// <summary>
    ///   Gets or sets whether license headers are automatically inserted into new files.
    /// </summary>
    public virtual bool InsertInNewFiles { get; set; }

    /// <summary>
    ///   Gets or sets the version of the License Header Manager Visual Studio HeaderDefinitionExtension.
    /// </summary>
    public virtual string Version { get; set; }

    /// <summary>
    ///   Gets or sets commands provided by Visual Studio before or after which the "Add License Header" command should be
    ///   automatically executed.
    /// </summary>
    public virtual ObservableCollection<LinkedCommand> LinkedCommands
    {
      get => _linkedCommands;
      set
      {
        if (_linkedCommands != null)
        {
          _linkedCommands.CollectionChanged -= InvokeLinkedCommandsChanged;
          InvokeLinkedCommandsChanged (_linkedCommands, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, _linkedCommands));
        }

        _linkedCommands = value;
        if (_linkedCommands == null)
          return;

        _linkedCommands.CollectionChanged += InvokeLinkedCommandsChanged;
        InvokeLinkedCommandsChanged (_linkedCommands, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, _linkedCommands));
      }
    }

    /// <summary>
    ///   Creates a deep copy of the current <see cref="VisualStudioOptions" /> instance.
    /// </summary>
    /// <returns>A deep copy of the this <see cref="VisualStudioOptions" /> instance.</returns>
    public virtual VisualStudioOptions Clone ()
    {
      var clonedObject = new VisualStudioOptions
                         {
                             InsertInNewFiles = InsertInNewFiles,
                             LinkedCommands = new ObservableCollection<LinkedCommand> (LinkedCommands.Select (x => x.Clone()))
                         };
      return clonedObject;
    }

    /// <summary>
    ///   Is triggered when the contents of the collection held by <see cref="LinkedCommands" /> has changed.
    /// </summary>
    public virtual event EventHandler<NotifyCollectionChangedEventArgs> LinkedCommandsChanged;

    /// <summary>
    ///   Serializes an <see cref="VisualStudioOptions" /> instance to a file in the file system.
    /// </summary>
    /// <param name="visualStudioOptions">The <see cref="VisualStudioOptions" /> instance to serialize.</param>
    /// <param name="filePath">The path to which an options file should be persisted.</param>
    public static async Task SaveAsync (VisualStudioOptions visualStudioOptions, string filePath)
    {
      await JsonOptionsManager.SerializeAsync (visualStudioOptions, filePath);
    }

    /// <summary>
    ///   Deserializes an <see cref="VisualStudioOptions" /> instance from a file in the file system.
    /// </summary>
    /// <param name="filePath">
    ///   The path to an options file from which a corresponding <see cref="VisualStudioOptions" /> instance
    ///   should be constructed.
    /// </param>
    /// <returns>
    ///   An <see cref="VisualStudioOptions" /> instance that represents to configuration contained in the file specified by
    ///   <paramref name="filePath" />.
    ///   If there were errors upon deserialization, <see langword="null" /> is returned.
    /// </returns>
    public static async Task<VisualStudioOptions> LoadAsync (string filePath)
    {
      return await JsonOptionsManager.DeserializeAsync<VisualStudioOptions> (filePath);
    }

    /// <summary>
    ///   Sets all public members of this <see cref="VisualStudioOptions" /> instance to pre-defined default values.
    /// </summary>
    /// <remarks>The default values are implementation-dependent.</remarks>
    private void SetDefaultValues ()
    {
      InsertInNewFiles = DefaultInsertInNewFiles;
      LinkedCommands = new ObservableCollection<LinkedCommand> (s_defaultLinkedCommands);
    }

    /// <summary>
    ///   Initializes all public members of this <see cref="VisualStudioOptions" /> instance.
    /// </summary>
    /// <remarks>The default values are implementation-dependent.</remarks>
    private void InitializeValues ()
    {
      LinkedCommands = new ObservableCollection<LinkedCommand> (s_defaultLinkedCommands);
    }

    protected virtual void InvokeLinkedCommandsChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
      LinkedCommandsChanged?.Invoke (sender, e);
    }
  }
}

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Options;

namespace LicenseHeaderManager.Options
{
  /// <summary>
  ///   Provides a facade around an <see cref="CoreOptions" /> and a <see cref="VisualStudioOptions" /> instance for unified
  ///   access.
  /// </summary>
  /// <seealso cref="CoreOptions" />
  /// <seealso cref="VisualStudioOptions" />
  public class OptionsFacade
  {
    private readonly CoreOptions _coreOptions;
    private readonly VisualStudioOptions _visualStudioOptions;

    static OptionsFacade ()
    {
      CurrentOptions = new OptionsFacade();
    }

    private OptionsFacade ()
    {
      _coreOptions = new CoreOptions();
      _visualStudioOptions = new VisualStudioOptions();
      _visualStudioOptions.LinkedCommandsChanged += InvokeLinkedCommandsChanged;
    }

    private OptionsFacade (CoreOptions coreOptions, VisualStudioOptions visualStudioOptions)
    {
      _coreOptions = coreOptions;
      _visualStudioOptions = visualStudioOptions;
      _visualStudioOptions.LinkedCommandsChanged += InvokeLinkedCommandsChanged;
    }

    public static string DefaultCoreOptionsPath =>
        Environment.ExpandEnvironmentVariables ($@"%APPDATA%\rubicon\LicenseHeaderManager\{LicenseHeadersPackage.Instance.Dte2.Version}\CoreOptions.json");

    public static string DefaultVisualStudioOptionsPath =>
        Environment.ExpandEnvironmentVariables ($@"%APPDATA%\rubicon\LicenseHeaderManager\{LicenseHeadersPackage.Instance.Dte2.Version}\VisualStudioOptions.json");

    public static string DefaultLogPath =>
        Environment.ExpandEnvironmentVariables ($@"%APPDATA%\rubicon\LicenseHeaderManager\{LicenseHeadersPackage.Instance.Dte2.Version}\logs_lhm");

    /// <summary>
    ///   Gets or sets the currently up-to-date configuration of the License Header Manager HeaderDefinitionExtension, along
    ///   with the corresponding options for the Core.
    /// </summary>
    public static OptionsFacade CurrentOptions { get; set; }

    /// <summary>
    ///   Gets or sets whether license header comments should be removed only if they contain at least one of the keywords
    ///   specified by <see cref="RequiredKeywords" />.
    /// </summary>
    public virtual bool UseRequiredKeywords
    {
      get => _coreOptions.UseRequiredKeywords;
      set => _coreOptions.UseRequiredKeywords = value;
    }

    /// <summary>
    ///   If <see cref="UseRequiredKeywords" /> is <see langword="true" />, only license header comments that contain at
    ///   least one of the words specified by this property (separated by "," and possibly whitespaces) are removed.
    /// </summary>
    public virtual string RequiredKeywords
    {
      get => _coreOptions.RequiredKeywords;
      set => _coreOptions.RequiredKeywords = value;
    }

    /// <summary>
    ///   Gets or sets the text for new license header definition files.
    /// </summary>
    public virtual string LicenseHeaderFileText
    {
      get => _coreOptions.LicenseHeaderFileText;
      set => _coreOptions.LicenseHeaderFileText = value;
    }

    /// <summary>
    ///   Gets or sets a list of <see cref="Core.Language" /> objects that represents the
    ///   languages for which the <see cref="Core.LicenseHeaderReplacer" /> is configured to use.
    /// </summary>
    public virtual ObservableCollection<Language> Languages
    {
      get => _coreOptions.Languages;
      set => _coreOptions.Languages = value;
    }

    /// <summary>
    ///   Gets or sets whether license headers are automatically inserted into new files.
    /// </summary>
    public virtual bool InsertInNewFiles
    {
      get => _visualStudioOptions.InsertInNewFiles;
      set => _visualStudioOptions.InsertInNewFiles = value;
    }

    /// <summary>
    ///   Gets or sets commands provided by Visual Studio before or after which the "Add License Header" command should be
    ///   automatically executed.
    /// </summary>
    /// <remarks>
    ///   Note that upon setter invocation, a copy of the supplied <see cref="ICollection{T}" /> is created. Hence, future
    ///   updates to this
    ///   initial collection are not reflected in this property.
    /// </remarks>
    public virtual ObservableCollection<LinkedCommand> LinkedCommands
    {
      get => _visualStudioOptions.LinkedCommands;
      set => _visualStudioOptions.LinkedCommands = _visualStudioOptions.LinkedCommands != null ? new ObservableCollection<LinkedCommand> (value) : null;
    }

    /// <summary>
    ///   Gets or sets the version of the License Header Manager Visual Studio HeaderDefinitionExtension.
    /// </summary>
    public virtual string Version
    {
      get => _visualStudioOptions.Version;
      set => _visualStudioOptions.Version = value;
    }

    /// <summary>
    ///   Serializes an <see cref="OptionsFacade" /> instance along with its underlying
    ///   <see cref="CoreOptions" /> and <see cref="VisualStudioOptions" /> instances into separate files
    ///   in the file system.
    /// </summary>
    /// <param name="options">The <see cref="OptionsFacade" /> instance to serialize.</param>
    /// <param name="coreOptionsFilePath">The path to which the <see cref="CoreOptions" /> should be serialized.</param>
    /// <param name="visualStudioOptionsFilePath">
    ///   The path to which the <see cref="VisualStudioOptions" /> should be
    ///   serialized.
    /// </param>
    public static async Task SaveAsync (OptionsFacade options, string coreOptionsFilePath = null, string visualStudioOptionsFilePath = null)
    {
      await JsonOptionsManager.SerializeAsync (options._coreOptions, coreOptionsFilePath ?? DefaultCoreOptionsPath);
      await JsonOptionsManager.SerializeAsync (options._visualStudioOptions, visualStudioOptionsFilePath ?? DefaultVisualStudioOptionsPath);
    }

    /// <summary>
    ///   Deserializes an <see cref="OptionsFacade" /> instance from files representing
    ///   <see cref="CoreOptions" /> and <see cref="VisualStudioOptions" /> instances in the file system.
    /// </summary>
    /// <param name="coreOptionsFilePath">
    ///   The path to an options file from which a corresponding <see cref="CoreOptions" /> instance
    ///   should be constructed.
    /// </param>
    /// <param name="visualStudioOptionsFilePath">
    ///   The path to an options file from which a corresponding <see cref="VisualStudioOptions" /> instance
    ///   should be constructed.
    /// </param>
    /// <returns>
    ///   An <see cref="OptionsFacade" /> instance that represents to configuration contained in the file specified by
    ///   <paramref name="coreOptionsFilePath" />.
    ///   If there were errors upon deserialization, <see langword="null" /> is returned.
    /// </returns>
    public static async Task<OptionsFacade> LoadAsync (string coreOptionsFilePath = null, string visualStudioOptionsFilePath = null)
    {
      var corePath = coreOptionsFilePath ?? DefaultCoreOptionsPath;
      var visualStudioPath = visualStudioOptionsFilePath ?? DefaultVisualStudioOptionsPath;

      // if either of the option files is not found, create it with default options and save it before loading
      if (!File.Exists (corePath))
        await CoreOptionsRepository.SaveAsync (new CoreOptions(), corePath);

      if (!File.Exists (visualStudioPath))
        await VisualStudioOptions.SaveAsync (new VisualStudioOptions(), visualStudioPath);

      var coreOptions = await CoreOptionsRepository.LoadAsync (corePath);
      var visualStudioOptions = await VisualStudioOptions.LoadAsync (visualStudioPath);

      return new OptionsFacade (coreOptions, visualStudioOptions);
    }

    /// <summary>
    ///   Is triggered when the contents of the collection held by <see cref="LinkedCommands" /> has changed.
    /// </summary>
    public virtual event EventHandler<NotifyCollectionChangedEventArgs> LinkedCommandsChanged;

    /// <summary>
    ///   Creates a deep copy of the current <see cref="OptionsFacade" /> instance.
    /// </summary>
    /// <returns>A deep copy of the this <see cref="OptionsFacade" /> instance.</returns>
    public virtual OptionsFacade Clone ()
    {
      var clonedObject = new OptionsFacade
                         {
                             UseRequiredKeywords = UseRequiredKeywords,
                             RequiredKeywords = RequiredKeywords,
                             LicenseHeaderFileText = LicenseHeaderFileText,
                             Languages = new ObservableCollection<Language> (Languages.Select (x => x.Clone())),
                             InsertInNewFiles = InsertInNewFiles,
                             LinkedCommands = new ObservableCollection<LinkedCommand> (LinkedCommands.Select (x => x.Clone()))
                         };
      return clonedObject;
    }

    protected virtual void InvokeLinkedCommandsChanged (object sender, NotifyCollectionChangedEventArgs e)
    {
      LinkedCommandsChanged?.Invoke (sender, e);
    }
  }
}

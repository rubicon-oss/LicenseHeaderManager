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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace LicenseHeaderManager.Options.Model
{
  /// <summary>
  ///   A base class for specifying options
  /// </summary>
  public abstract class BaseOptionModel<T>
      where T : BaseOptionModel<T>, new()
  {
    private static readonly AsyncLazy<T> s_liveModel = new AsyncLazy<T> (CreateAsync, LicenseHeadersPackage.Instance.JoinableTaskFactory);
    private static readonly ILog s_log = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    protected BaseOptionModel ()
    {
    }

    /// <summary>
    ///   A singleton instance of the options. MUST be called from UI thread only
    /// </summary>
    public static T Instance
    {
      get
      {
        ThreadHelper.ThrowIfNotOnUIThread();

#pragma warning disable VSTHRD104 // Offer async methods
        return LicenseHeadersPackage.Instance.JoinableTaskFactory.Run (GetLiveInstanceAsync);
#pragma warning restore VSTHRD104 // Offer async methods
      }
    }

    /// <summary>
    ///   Get the singleton instance of the options. Thread safe.
    /// </summary>
    public static Task<T> GetLiveInstanceAsync ()
    {
      return s_liveModel.GetValueAsync();
    }

    /// <summary>
    ///   Creates a new instance of the options class and loads the values from the store. For internal use only
    /// </summary>
    /// <returns></returns>
    public static async Task<T> CreateAsync ()
    {
      var instance = new T();
      await instance.LoadAsync();
      return instance;
    }

    /// <summary>
    ///   Hydrates the properties from the registry.
    /// </summary>
    public virtual void Load ()
    {
      LicenseHeadersPackage.Instance.JoinableTaskFactory.Run (LoadAsync);
    }

    /// <summary>
    ///   Hydrates the properties from the registry asynchronously.
    /// </summary>
    public virtual async Task LoadAsync ()
    {
      s_log.Info ("Load options from config file");
      OptionsFacade.CurrentOptions = await OptionsFacade.LoadAsync();

      foreach (var property in GetOptionProperties())
      {
        if (typeof (OptionsFacade).GetProperty (property.Name)?.PropertyType != property.PropertyType)
          continue;

        var facadeProperty = typeof (OptionsFacade).GetProperty (property.Name);
        if (facadeProperty != null)
          property.SetValue (this, facadeProperty.GetValue (OptionsFacade.CurrentOptions));
      }
    }

    /// <summary>
    ///   Saves the properties to the registry.
    /// </summary>
    public virtual void Save ()
    {
      LicenseHeadersPackage.Instance.JoinableTaskFactory.Run (SaveAsync);
    }

    /// <summary>
    ///   Saves the properties to the registry asynchronously.
    /// </summary>
    public virtual async Task SaveAsync ()
    {
      s_log.Info ("Save options to config file");
      foreach (var property in GetOptionProperties())
      {
        if (typeof (OptionsFacade).GetProperty (property.Name)?.PropertyType != property.PropertyType)
          continue;

        var facadeProperty = typeof (OptionsFacade).GetProperty (property.Name);
        facadeProperty?.SetValue (OptionsFacade.CurrentOptions, property.GetValue (this));
      }

      await OptionsFacade.SaveAsync (OptionsFacade.CurrentOptions);

      var liveModel = await GetLiveInstanceAsync();

      if (this != liveModel)
        await liveModel.LoadAsync();
    }

    private IEnumerable<PropertyInfo> GetOptionProperties ()
    {
      return GetType().GetProperties().Where (p => p.PropertyType.IsSerializable && p.PropertyType.IsPublic);
    }
  }
}

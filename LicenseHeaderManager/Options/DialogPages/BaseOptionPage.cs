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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using LicenseHeaderManager.Options.Converters;
using LicenseHeaderManager.Options.Model;
using LicenseHeaderManager.Utils;
using log4net;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;

namespace LicenseHeaderManager.Options.DialogPages
{
  /// <summary>
  ///   A base class for a DialogPage to show in Tools -> Options.
  /// </summary>
  public class BaseOptionPage<T> : DialogPage
      where T : BaseOptionModel<T>, new()
  {
    private static bool s_firstDialogPageLoaded = true;
    private static readonly ILog s_log = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    private static OptionsStoreMode s_optionsStoreMode = OptionsStoreMode.RegistryStore_3_0_3;
    protected readonly BaseOptionModel<T> Model;

    public BaseOptionPage ()
    {
#pragma warning disable VSTHRD104 // Offer async methods
      Model = LicenseHeadersPackage.Instance.JoinableTaskFactory.Run (BaseOptionModel<T>.CreateAsync);
#pragma warning restore VSTHRD104 // Offer async methods
    }

    public string Version { get; private set; }

    public override object AutomationObject => Model;

    /// <summary>
    ///   Loads the current options from the configuration files.
    /// </summary>
    public override void LoadSettingsFromStorage ()
    {
      Model.Load();
    }

    /// <summary>
    ///   Migrates the options from the storage of older LHM versions to the storage of new versions.
    /// </summary>
    public void MigrateOptions ()
    {
      //Could happen if you install a LicenseHeaderManager (LHM) version which is older than the ever installed highest version
      //Should only happen to developers of LHM, but could theoretically also happen if someone downgrades LHM.

      var parsedVersion = GetParsedVersion();
      var currentlyInstalledVersion = GetCurrentlyInstalledVersion();
      s_log.Debug ($"Parsed version: {parsedVersion}, currently installed version: {currentlyInstalledVersion}");

      if (parsedVersion > currentlyInstalledVersion)
      {
        if (s_firstDialogPageLoaded)
        {
          MessageBoxHelper.ShowMessage (
              "We detected that you are downgrading LicenseHeaderManager from a higher version." + Environment.NewLine +
              "As we don't know what you did to get to that state, it is possible that you missed an update for the Language Settings."
              + Environment.NewLine +
              "If some of your license headers do not update, check if your Language Settings (Options -> LicenseHeaderManager -> Languages) "
              + Environment.NewLine +
              "contain all the extensions you require.");

          s_firstDialogPageLoaded = false;
        }

        Version = LicenseHeadersPackage.Version;
        Model.Save();
      }
      else
      {
        var saveRequired = false;

        foreach (var updateStep in GetVersionUpdateSteps())
          saveRequired |= Update (updateStep);

        if (Version != LicenseHeadersPackage.Version)
          saveRequired |= Update (new UpdateStep (currentlyInstalledVersion));

        if (saveRequired)
          Model.Save();
      }
    }

    /// <summary>
    ///   Saves the current options to the configuration files.
    /// </summary>
    public override void SaveSettingsToStorage ()
    {
      Model.Save();
    }

    /// <summary>
    ///   Gets a new collection of type <see cref="Enumerable" /> for update steps.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<UpdateStep> GetVersionUpdateSteps ()
    {
      return Enumerable.Empty<UpdateStep>();
    }

    private bool Update (UpdateStep updateStep)
    {
      var version = GetParsedVersion();
      s_log.Info ($"Executing update step. Version: {version}, target version: {updateStep.TargetVersion}");
      if (version >= updateStep.TargetVersion)
        return false;

      updateStep.ExecuteActions();

      Version = updateStep.TargetVersion.ToString();
      return true;
    }

    private Version GetParsedVersion ()
    {
      s_log.Info ($"Retrieving version for page {GetType().Name}");

      switch (s_optionsStoreMode)
      {
        case OptionsStoreMode.RegistryStore_3_0_3:
          var key = GetRegistryKey (@$"ApplicationPrivateSettings\LicenseHeaderManager\Options\{GetType().Name}");
          if (key != null)
          {
            s_log.Debug ($"Retrieved registry version key: {key.Name}");
            var version = Registry.GetValue (key.Name, "Version", "failure").ToString();
            var converter = TypeDescriptor
                .GetProperties (this).Cast<PropertyDescriptor>().First (x => x.Name == "Version").Converter;
            Version = DeserializeValue (converter, version).ToString();
          }

          break;

        case OptionsStoreMode.JsonStore:
          Version = OptionsFacade.CurrentOptions.Version;
          break;

        default:
          throw new ArgumentOutOfRangeException();
      }

      s_log.Info ($"Retrieved version: {Version}");
      System.Version.TryParse (Version, out var result);
      return result;
    }

    private Version GetCurrentlyInstalledVersion ()
    {
      return System.Version.Parse (LicenseHeadersPackage.Version);
    }

    #region migration to 4.0.0

    /// <summary>
    ///   Loads the option values from the registry and deserializes them so that they can be stored in configuration files.
    /// </summary>
    /// <param name="dialogPage">Specifies the dialog page to which the deserialized values belong.</param>
    protected void LoadCurrentRegistryValues_3_0_3 (BaseOptionModel<T> dialogPage = null)
    {
      using var currentRegistryKey = GetRegistryKey (@$"ApplicationPrivateSettings\LicenseHeaderManager\Options\{GetType().Name}");

      s_log.Info ($"Loading values from registry key: {currentRegistryKey.Name}");

      foreach (var property in GetVisibleProperties())
      {
        if (property.Name == "Commands")
          continue;

        var propertyName = property.Name switch
        {
            "LinkedCommands" => "LinkedCommandsSerialized",
            "Languages" => "LanguagesSerialized",
            _ => property.Name
        };

        var converter = GetPropertyConverterOrDefault (property);
        var registryValue = GetRegistryValue (currentRegistryKey, propertyName);
        s_log.Debug ($"Property {propertyName} with value '{registryValue}' read from registry");

        if (registryValue == null)
          continue;

        try
        {
          var deserializedValue = DeserializeValue (converter, registryValue);
          property.SetValue (dialogPage ?? AutomationObject, deserializedValue);
          s_log.Debug ($"Deserialized value: '{deserializedValue}'");
        }
        catch (Exception ex)
        {
          s_log.Error ($"Could not restore registry value for {propertyName}", ex);
        }
      }

      s_optionsStoreMode = OptionsStoreMode.JsonStore;
    }

    #endregion

    private RegistryKey GetRegistryKey (string path)
    {
      s_log.Debug ($"Get registry key from path '{path}'");
      RegistryKey subKey = null;
      try
      {
        var service = (AsyncPackage) GetService (typeof (AsyncPackage));
        subKey = service?.UserRegistryRoot.OpenSubKey (path);
      }
      catch (Exception ex)
      {
        s_log.Error ($"Could not retrieve registry key from path '{path}'", ex);
      }

      s_log.Debug ($"Returned registry key: {subKey}");
      return subKey;
    }

    private IEnumerable<PropertyDescriptor> GetVisibleProperties ()
    {
      return TypeDescriptor.GetProperties (AutomationObject).Cast<PropertyDescriptor>();
    }

    private TypeConverter GetPropertyConverterOrDefault (PropertyDescriptor propertyDescriptor)
    {
      return propertyDescriptor.Name switch
      {
          nameof(LanguagesPageModel.Languages) => new LanguageConverter(),
          nameof(GeneralOptionsPageModel.LinkedCommands) => new LinkedCommandConverter(),
          _ => propertyDescriptor.Converter
      };
    }

    private string GetRegistryValue (RegistryKey key, string subKeyName)
    {
      return key?.GetValue (subKeyName)?.ToString();
    }

    private object DeserializeValue (TypeConverter converter, string value)
    {
      if (value == null)
        return null;

      var actualValue = string.Join ("*", value.Split ('*').Skip (2));
      return converter.ConvertFromInvariantString (actualValue);
    }

    /// <summary>
    ///   Compares three values and returns the migrated value if current and default value are equal. Otherwise the current value is returned.
    /// </summary>
    /// <typeparam name="U">The type of the values to compare.</typeparam>
    /// <param name="currentValue">The current value.</param>
    /// <param name="migratedValue">The migrated value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns></returns>
    protected U ThreeWaySelectionForMigration<U> (U currentValue, U migratedValue, U defaultValue)
    {
      if (defaultValue is ICollection)
        throw new InvalidOperationException ("ThreeWaySelectionForMigration does currently not support ICollections.");

      return currentValue.Equals (defaultValue) ? migratedValue : currentValue;
    }
  }
}

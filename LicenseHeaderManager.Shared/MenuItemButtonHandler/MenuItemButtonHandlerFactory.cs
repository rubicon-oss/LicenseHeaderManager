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
using LicenseHeaderManager.Interfaces;
using LicenseHeaderManager.MenuItemButtonHandler.Implementations;

namespace LicenseHeaderManager.MenuItemButtonHandler
{
  /// <summary>
  ///   Represents static factory class providing static methods facilitating the instantiation of types implementing the
  ///   <see cref="IMenuItemButtonHandler" /> interface.
  /// </summary>
  internal static class MenuItemButtonHandlerFactory
  {
    /// <summary>
    ///   Creates an instance of the type implementing the <see cref="IMenuItemButtonHandler" /> interface that fits the given
    ///   parameters.
    /// </summary>
    /// <param name="level">The level the <see cref="IMenuItemButtonHandler" /> instance to be created should operate on.</param>
    /// <param name="mode">
    ///   The license header insertion mode to be used by the <see cref="IMenuItemButtonHandler" /> instance
    ///   to be created.
    /// </param>
    /// <param name="licenseHeadersPackage">
    ///   The <see cref="ILicenseHeaderExtension" /> instance the
    ///   <see cref="IMenuItemButtonHandler" /> instance to be created may use for its operations.
    /// </param>
    /// <returns>
    ///   Returns a´n <see cref="IMenuItemButtonHandler" /> instance operating on the level specified by
    ///   <paramref name="level" /> and executing operations of the mode specified by <paramref name="mode" />.
    /// </returns>
    public static IMenuItemButtonHandler CreateHandler (MenuItemButtonLevel level, MenuItemButtonOperation mode, ILicenseHeaderExtension licenseHeadersPackage)
    {
      return level switch
      {
          MenuItemButtonLevel.Solution => CreateSolutionHandler (licenseHeadersPackage, mode),
          MenuItemButtonLevel.Folder => CreateFolderHandler (licenseHeadersPackage, mode),
          MenuItemButtonLevel.Project => CreateProjectHandler (licenseHeadersPackage, mode),
          _ => throw new ArgumentOutOfRangeException (nameof(level), level, null)
      };
    }

    private static SolutionMenuItemButtonHandler CreateSolutionHandler (ILicenseHeaderExtension licenseHeadersPackage, MenuItemButtonOperation mode)
    {
      MenuItemButtonHandlerImplementation implementation = mode switch
      {
          MenuItemButtonOperation.Add => new AddLicenseHeaderToAllFilesInSolutionImplementation (licenseHeadersPackage),
          MenuItemButtonOperation.Remove => new RemoveLicenseHeaderFromAllFilesInSolutionImplementation (licenseHeadersPackage),
          _ => throw new ArgumentOutOfRangeException (nameof(mode), mode, null)
      };

      return new SolutionMenuItemButtonHandler (licenseHeadersPackage.Dte2, mode, implementation);
    }

    private static FolderProjectMenuItemButtonHandler CreateFolderHandler (ILicenseHeaderExtension licenseHeadersPackage, MenuItemButtonOperation mode)
    {
      MenuItemButtonHandlerImplementation implementation = mode switch
      {
          MenuItemButtonOperation.Add => new AddLicenseHeaderToAllFilesInFolderProjectImplementation (licenseHeadersPackage),
          MenuItemButtonOperation.Remove => new RemoveLicenseHeaderToAllFilesInFolderProjectImplementation (licenseHeadersPackage),
          _ => throw new ArgumentOutOfRangeException (nameof(mode), mode, null)
      };

      return new FolderProjectMenuItemButtonHandler (mode, MenuItemButtonLevel.Folder, implementation);
    }

    private static FolderProjectMenuItemButtonHandler CreateProjectHandler (ILicenseHeaderExtension licenseHeadersPackage, MenuItemButtonOperation mode)
    {
      MenuItemButtonHandlerImplementation implementation = mode switch
      {
          MenuItemButtonOperation.Add => new AddLicenseHeaderToAllFilesInFolderProjectImplementation (licenseHeadersPackage),
          MenuItemButtonOperation.Remove => new RemoveLicenseHeaderToAllFilesInFolderProjectImplementation (licenseHeadersPackage),
          _ => throw new ArgumentOutOfRangeException (nameof(mode), mode, null)
      };

      return new FolderProjectMenuItemButtonHandler (mode, MenuItemButtonLevel.Project, implementation);
    }
  }
}

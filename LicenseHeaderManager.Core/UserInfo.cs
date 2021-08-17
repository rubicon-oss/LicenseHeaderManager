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
using System.DirectoryServices.AccountManagement;
using LicenseHeaderManager.Core.Properties;

namespace LicenseHeaderManager.Core
{
  /// <summary>
  ///   Represents general information about the current user, e.g. login or display name.
  ///   Information about the current user is retrieved in static constructor.
  /// </summary>
  internal static class UserInfo
  {
    private static readonly object s_nameLock = new object();
    private static readonly object s_displayNameLock = new object();
    private static string s_name;

    private static string s_displayName = "";
    private static DateTime s_lastPropertyCall = DateTime.MinValue;
    private static bool s_lookupSuccessful;

    /// <summary>
    ///   Gets name (login) of the current user.
    /// </summary>
    public static string Name
    {
      get
      {
        lock (s_nameLock)
        {
          if (string.IsNullOrEmpty (s_name))
            s_name = Environment.UserName;
          return s_name;
        }
      }
    }

    /// <summary>
    ///   Gets display name of the current user, e.g. "John Smith".
    /// </summary>
    public static string DisplayName
    {
      get
      {
        lock (s_displayNameLock)
        {
          if (!s_lookupSuccessful && LastLookupAttemptTooOld())
            TryLookupNow();

          s_lastPropertyCall = DateTime.Now;
          return s_displayName;
        }
      }
    }

    private static bool LastLookupAttemptTooOld ()
    {
      // Use _lastPropertyCall to stop lookups in case of BatchOperations as well.
      return DateTime.Now.Subtract (s_lastPropertyCall).TotalSeconds > Resources.Constant_DisplayNameLookup_TimeDifferenceInSecondsBeforeTooOld;
    }

    private static void TryLookupNow ()
    {
      try
      {
        s_displayName = UserPrincipal.Current.DisplayName;
        s_lookupSuccessful = true;
      }
      catch (Exception)
      {
        s_displayName = Resources.UserInfo_UnknownDisplayNameString;
        s_lookupSuccessful = false;
      }
    }
  }
}

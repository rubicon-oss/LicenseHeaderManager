#region copyright
// Copyright (c) rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using System.IO;
using System.DirectoryServices.AccountManagement;

namespace LicenseHeaderManager.Headers
{
    /// <summary>
    /// Represents general information about the current user, e.g. login or display name.
    /// Information about the current user is retrieved in static constructor.
    /// </summary>
    public static class UserInfo
    {
        #region Properties
        private static readonly object NameLock = new object();
        private static readonly object DisplayNameLock = new object();

        private static string _name;
        /// <summary>
        /// Gets name (login) of the current user.
        /// </summary>
        public static string Name 
        {
          get
          {
            lock (NameLock)
            {
              if (string.IsNullOrEmpty(_name))
              {
                _name = Environment.UserName;
              }
              return _name;
            }
          }
        }

        private static string _displayName = "";
        private static DateTime _lastPropertyCall = DateTime.MinValue;
        private static bool _lookupSuccessful = false;

        /// <summary>
        /// Gets display name of the current user, e.g. "John Smith".
        /// </summary>
        public static string DisplayName 
        {
          get
          {
            lock (DisplayNameLock)
            {
              if (!_lookupSuccessful && LastLookupAttemptTooOld())
                TryLookupNow();

              _lastPropertyCall = DateTime.Now;
              return _displayName;
            }
          }
        }

      private static bool LastLookupAttemptTooOld()
      {
        // Use _lastPropertyCall to stop lookups in case of BatchOperations as well.
        return DateTime.Now.Subtract(_lastPropertyCall).TotalSeconds > Resources.Constant_DisplayNameLookup_TimeDifferenceInSecondsBeforeTooOld;
      }

      private static void TryLookupNow()
        {
          try
          {
            _displayName = UserPrincipal.Current.DisplayName;
            _lookupSuccessful = true;
            
          }
          catch (Exception e)
          {
            string OutputMessage = string.Format(Resources.UserInfo_LookupFailure_Information, e).Replace (@"\n", "\n");
 
            if (e is FileNotFoundException)
            {
              OutputMessage = string.Format(Resources.UserInfo_LookupFailure_FileNotFoundException_Information).Replace (@"\n", "\n");
            }
            
            OutputWindowHandler.WriteMessage(OutputMessage);
            _displayName = Resources.UserInfo_UnknownDisplayNameString;
            _lookupSuccessful = false;
          }
        }

        #endregion
    }
}

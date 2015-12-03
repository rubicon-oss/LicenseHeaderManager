#region copyright
// Copyright (c) 2013 rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;
using System.DirectoryServices.AccountManagement;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.Headers
{
    /// <summary>
    /// Represents general information about the current user, e.g. login or display name.
    /// Information about the current user is retrieved in static constructor.
    /// </summary>
    public static class UserInfo
    {
        #region Properties
        private static Object _nameLock = new Object();
        private static Object _displayNameLock = new Object();

        private static string _name;
        /// <summary>
        /// Gets name (login) of the current user.
        /// </summary>
        public static string Name 
        {
          get
          {
            lock (_nameLock)
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
        private static DateTime? _lastLookup = null;
        private static bool _knowDisplayName = false;

        /// <summary>
        /// Gets display name of the current user, e.g. "John Smith".
        /// </summary>
        public static string DisplayName 
        {
          get
          {
            lock (_displayName)
            {
              if (!_knowDisplayName)
              {
                if (_lastLookup == null)
                {
                  TryLookup();
                }
                else if (DateTime.Now.Subtract((DateTime) _lastLookup).TotalSeconds > 5)
                {
                  TryLookup();
                }
                else
                {
                  //Set _lastLookup to stop Lookups in case of BatchOperations
                  _lastLookup = DateTime.Now;
                }
              }
              return _displayName;
            
            }
          }
        }

        private static void TryLookup()
        {
          try
          {
            _displayName = UserPrincipal.Current.DisplayName;
            _knowDisplayName = true;
          }
          catch (PrincipalServerDownException)
          {
            _knowDisplayName = false;
            _lastLookup = DateTime.Now;
            OutputWindowHandler.WriteMessage("Active Directory Lookup failed");
            _displayName = "<Unknown>";
          }
        }

        #endregion
    }
}

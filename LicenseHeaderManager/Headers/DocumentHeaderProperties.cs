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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using LicenseHeaderManager.Utils;

namespace LicenseHeaderManager.Headers
{
  class DocumentHeaderProperties : IEnumerable<DocumentHeaderProperty>
  {
    private readonly IEnumerable<DocumentHeaderProperty> _properties;

    public DocumentHeaderProperties (ProjectItem projectItem)
    {
      _properties = CreateProperties (projectItem);
    }

    public IEnumerator<DocumentHeaderProperty> GetEnumerator ()
    {
      return _properties.GetEnumerator ();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
    {
      return GetEnumerator ();
    }

    private IEnumerable<DocumentHeaderProperty> CreateProperties (ProjectItem projectItem)
    {
      if (projectItem == null)
        throw new ArgumentNullException ("projectItem");

      List<DocumentHeaderProperty> properties = new List<DocumentHeaderProperty> ()
      {
        new DocumentHeaderProperty(
          "%FullFileName%",
          documentHeader => documentHeader.FileInfo != null,
          documentHeader => GetProperFilePathCapitalization (documentHeader.FileInfo)),
        new DocumentHeaderProperty(
          "%FileName%",
          documentHeader => documentHeader.FileInfo != null, 
          documentHeader => GetProperFileNameCapitalization (documentHeader.FileInfo)),
        new DocumentHeaderProperty(
          "%CreationYear%", 
          documentHeader => documentHeader.FileInfo != null, 
          documentHeader => documentHeader.FileInfo.CreationTime.Year.ToString()),
        new DocumentHeaderProperty(
          "%CreationMonth%", 
          documentHeader => documentHeader.FileInfo != null, 
          documentHeader => documentHeader.FileInfo.CreationTime.Month.ToString()),
        new DocumentHeaderProperty(
          "%CreationDay%", 
          documentHeader => documentHeader.FileInfo != null, 
          documentHeader => documentHeader.FileInfo.CreationTime.Day.ToString()),
        new DocumentHeaderProperty(
          "%CreationTime%", 
          documentHeader => documentHeader.FileInfo != null, 
          documentHeader => documentHeader.FileInfo.CreationTime.ToShortTimeString()),
        new DocumentHeaderProperty(
          "%CurrentYear%", 
          documentHeader => true, 
          documentHeader => DateTime.Now.Year.ToString()),
        new DocumentHeaderProperty(
          "%CurrentMonth%", 
          documentHeader => true, 
          documentHeader => DateTime.Now.Month.ToString()),
        new DocumentHeaderProperty(
          "%CurrentDay%", 
          documentHeader => true, 
          documentHeader => DateTime.Now.Day.ToString()),
        new DocumentHeaderProperty(
          "%CurrentTime%", 
          documentHeader => true, 
          documentHeader => DateTime.Now.ToShortTimeString()),
        new DocumentHeaderProperty(
          "%UserName%", 
          documentHeader => UserInfo.Name != null, 
          documentHeader => UserInfo.Name),
        new DocumentHeaderProperty(
          "%UserDisplayName%", 
          documentHeader => UserInfo.DisplayName != null, 
          documentHeader => UserInfo.DisplayName),
        new DocumentHeaderProperty(
          "%Project%", 
          documentHeader => projectItem.ContainingProject != null, 
          documentHeader => projectItem.ContainingProject.Name),
        new DocumentHeaderProperty(
          "%Namespace%", 
          documentHeader => 
            projectItem.FileCodeModel != null &&
            projectItem.FileCodeModel.CodeElements.Cast<CodeElement>().Any (ce => ce.Kind == vsCMElement.vsCMElementNamespace), 
          documentHeader => projectItem.FileCodeModel.CodeElements.Cast<CodeElement>().First (ce => ce.Kind == vsCMElement.vsCMElementNamespace).Name)
      };
      return properties;
    }

    private string GetProperFilePathCapitalization (FileInfo fileInfo)
    {
      try
      {
        return PathUtility.GetProperFilePathCapitalization (fileInfo);
      }
      catch (Exception e)
      {
        OutputWindowHandler.WriteMessage ("Could not get proper file path capitalization.");
        OutputWindowHandler.WriteMessage ("Falling back to path as we receive it from 'FileInfo'.");
        OutputWindowHandler.WriteMessage (e.ToString());

        //Use the FilePath in the same capitalization as we got it
        return fileInfo.FullName;
      }
    }

    private string GetProperFileNameCapitalization (FileInfo fileInfo)
    {
      try
      {
        return Path.GetFileName (PathUtility.GetProperFilePathCapitalization (fileInfo));
      }
      catch (Exception e)
      {
        OutputWindowHandler.WriteMessage ("Could not get proper file name capitalization.");
        OutputWindowHandler.WriteMessage ("Falling back to name as we receive it from 'FileInfo'.");
        OutputWindowHandler.WriteMessage (e.ToString());

        //Use the FileName in the same capitalization as we got it
        return fileInfo.Name;
      }
    }
  }
}

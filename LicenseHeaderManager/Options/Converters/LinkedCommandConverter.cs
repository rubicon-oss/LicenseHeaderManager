#region copyright
// Copyright (c) 2011 rubicon informationstechnologie gmbh

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
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace LicenseHeaderManager.Options.Converters
{
  class LinkedCommandConverter : XmlTypeConverter<IEnumerable<LinkedCommand>>
  {
    private const string c_linkedCommands = "LinkedCommands";
    private const string c_command = "Command";
    private const string c_guid = "Languages";
    private const string c_id = "Extension";
    private const string c_name = "Name";
    private const string c_executionTime = "Extensions";

    public override string ToXml (IEnumerable<LinkedCommand> commands)
    {
      var xml = from c in commands
                select new XElement (c_command,
                  new XAttribute(c_name, c.Name ?? string.Empty),
                  new XAttribute (c_guid, c.Guid ?? string.Empty),
                  new XAttribute (c_id, c.Id),
                  new XAttribute (c_executionTime, c.ExecutionTime));

      return new XElement (c_linkedCommands, xml).ToString ();
    }

    public override IEnumerable<LinkedCommand> FromXml (string xml)
    {
      try
      {
        var commands = from c in XElement.Parse (xml).Descendants (c_command)
                        select new LinkedCommand()
                        {
                          Name = GetAttributeValue(c, c_name),
                          Guid = GetAttributeValue (c, c_guid),
                          Id = int.Parse(GetAttributeValue (c, c_id)),
                          ExecutionTime = (ExecutionTime)Enum.Parse(typeof(ExecutionTime), GetAttributeValue (c, c_executionTime))
                        };
        return new ObservableCollection<LinkedCommand> (commands);
      }
      catch(Exception)
      {
        return new ObservableCollection<LinkedCommand> ();
      }
    }
  }
}

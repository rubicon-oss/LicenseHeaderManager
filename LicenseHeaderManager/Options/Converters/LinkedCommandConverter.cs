using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Collections.Generic;

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

using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Collections.Generic;

namespace LicenseHeaderManager.Options.Converters
{
  class ChainedCommandConverter : XmlTypeConverter<IEnumerable<ChainedCommand>>
  {
    private const string c_chainedCommands = "ChainedCommands";
    private const string c_command = "Command";
    private const string c_guid = "Languages";
    private const string c_id = "Extension";
    private const string c_executionTime = "Extensions";

    public override string ToXml (IEnumerable<ChainedCommand> commands)
    {
      var xml = from c in commands
                select new XElement (c_command,
                  new XAttribute (c_guid, c.Guid ?? string.Empty),
                  new XAttribute (c_id, c.Id),
                  new XAttribute (c_executionTime, c.ExecutionTime));

      return new XElement (c_chainedCommands, xml).ToString ();
    }

    public override IEnumerable<ChainedCommand> FromXml (string xml)
    {
      try
      {
        var commands = from l in XElement.Parse (xml).Descendants (c_command)
                        select new ChainedCommand()
                        {
                          Guid = GetAttributeValue (l, c_guid),
                          Id = int.Parse(GetAttributeValue (l, c_id)),
                          ExecutionTime = (ExecutionTime)Enum.Parse(typeof(ExecutionTime), GetAttributeValue (l, c_executionTime))
                        };
        return new ObservableCollection<ChainedCommand> (commands);
      }
      catch(Exception)
      {
        return new ObservableCollection<ChainedCommand> ();
      }
    }
  }
}

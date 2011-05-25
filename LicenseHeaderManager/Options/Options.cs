using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace LicenseHeaderManager.Options
{
  public class Options
  {
    private const string c_licenseHeaders = "LicenseHeaders";
    private const string c_attachedCommand = "AttachedCommand";
    private const string c_attach = "attach";
    private const string c_guid = "Guid";
    private const string c_id = "Id";

    public bool AttachToCommand { get; set; }
    public string AttachedCommandGuid { get; set; }
    public int? AttachedCommandId { get; set; }

    public Options (string xml = null)
    {
      if (xml == null)
      {
        AttachToCommand = false;
        AttachedCommandGuid = null;
        AttachedCommandId = null;
      }
      else
      {
        var root = XElement.Load (XmlReader.Create (new StringReader (xml)));
        var attachedCommand = root.Descendants (c_attachedCommand).FirstOrDefault();
        if (attachedCommand != null)
        {
          var attachAttribute = attachedCommand.Attribute (c_attach);
          var guidAttribute = attachedCommand.Attribute (c_guid);
          var idAttribute = attachedCommand.Attribute (c_id);

          if (attachAttribute != null)
          {
            bool attach;
            if (bool.TryParse (attachAttribute.Value, out attach))
              AttachToCommand = attach;
          }

          if (guidAttribute != null && idAttribute != null)
          {
            int id;
            if (int.TryParse (idAttribute.Value, out id))
            {
              AttachedCommandGuid = guidAttribute.Value;
              AttachedCommandId = id;
            }
          }
        }
      }
    }

    public string ToXml ()
    {
      return new XElement (
          c_licenseHeaders,
          new XElement (
              c_attachedCommand,
              new XAttribute (c_attach, AttachToCommand.ToString()),
              (AttachedCommandGuid != null ? new XAttribute (c_guid, AttachedCommandGuid) : null),
              (AttachedCommandId.HasValue ? new XAttribute (c_id, AttachedCommandId.Value.ToString()) : null))).ToString();
    }
  }
}
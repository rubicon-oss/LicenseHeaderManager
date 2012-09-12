//Sample license text.
using System.Collections.ObjectModel;
using System.ComponentModel;
using LicenseHeaderManager.Options.Converters;

namespace LicenseHeaderManager.Options
{
  public interface IOptionsPage
  {
    bool UseRequiredKeywords { get; set; }
    string RequiredKeywords { get; set; }

    [TypeConverter (typeof (LinkedCommandConverter))]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
    ObservableCollection<LinkedCommand> LinkedCommands { get; set; }
  }
}
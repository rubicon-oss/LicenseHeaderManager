using System.Security.RightsManagement;

namespace LicenseHeaderManager.Interfaces
{
  public interface ICommentParser
  {
    string Parse(string text);
  }
}

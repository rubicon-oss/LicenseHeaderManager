using EnvDTE;

namespace LicenseHeaderManager.Interfaces
{
  public interface ISolutionLevelCommand
  {
    void Execute(Solution solutionObject);
  }
}

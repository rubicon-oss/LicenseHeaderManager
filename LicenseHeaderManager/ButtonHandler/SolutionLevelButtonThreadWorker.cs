using System;
using EnvDTE;
using LicenseHeaderManager.Interfaces;

namespace LicenseHeaderManager.ButtonHandler
{
  class SolutionLevelButtonThreadWorker
  {
    private readonly ISolutionLevelCommand _solutionLevelCommand;

    public SolutionLevelButtonThreadWorker(ISolutionLevelCommand solutionLevelCommand)
    {
      _solutionLevelCommand = solutionLevelCommand;
    }

    public event EventHandler ThreadDone;

    public void Run(object solutionObject)
    {
      Solution solution = solutionObject as Solution;
      if (solution == null) return;

      _solutionLevelCommand.Execute(solution);

      if (ThreadDone != null)
          ThreadDone(this, EventArgs.Empty);
    }
  }
}

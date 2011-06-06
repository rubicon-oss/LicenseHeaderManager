using System;
using EnvDTE;

namespace LicenseHeaderManager.Options
{
  public enum ExecutionTime
  {
    Before,
    After
  }

  public class ChainedCommand
  {
    public string Name { get; set; }
    public string Guid { get; set; }
    public int Id { get; set; }
    public ExecutionTime ExecutionTime { get; set; }

    public CommandEvents Events { get; set; }
  }
}

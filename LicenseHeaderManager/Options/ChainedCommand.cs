using System;

namespace LicenseHeaderManager.Options
{
  public enum ExecutionTime
  {
    Before,
    After
  }

  public class ChainedCommand
  {
    public string Guid { get; set; }
    public int Id { get; set; }
    public ExecutionTime ExecutionTime { get; set; }
  }
}

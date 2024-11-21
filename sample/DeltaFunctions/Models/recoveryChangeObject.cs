using Microsoft.OData.ModelBuilder;

namespace microsoft.graph;

/// <summary>
/// RecoveryChangeObject as entity
/// </summary>
public class recoveryChangeObject
{
    public string id { get; set; }
    [AutoExpand]
    public object currentState { get; set; }
    [AutoExpand]
    public object deltaFromCurrent { get; set; }
}


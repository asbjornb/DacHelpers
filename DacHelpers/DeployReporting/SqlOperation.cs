namespace DacHelpers.DeployReporting;

/// <summary>
/// Represents a single operation in a deployment report
/// </summary>
/// <param name="OperationType">The type of operation</param>
/// <param name="ObjectType">The type of object affected </param>
/// <param name="ObjectName">The name of the object affected</param>
public record SqlOperation(SqlOperationType OperationType, string ObjectType, string ObjectName);

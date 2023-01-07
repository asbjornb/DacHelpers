namespace DacHelpers.DeployReporting;

/// <summary>
/// The type of an operation in a deployment report
/// </summary>
public enum SqlOperationType
{
    /// <summary>
    /// A sql CREATE operation
    /// </summary>
    Create,
    /// <summary>
    /// A sql ALTER operation
    /// </summary>
    Alter,
    /// <summary>
    /// A sql DROP operation
    /// </summary>
    Drop
}

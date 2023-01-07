namespace DacHelpers.DeployReporting;

/// <summary>
/// Represents a single alert in a deployment report
/// </summary>
/// <param name="AlertType">The type of alert</param>
/// <param name="Issue">The issue that caused the alert</param>
public record Alert(string AlertType, string Issue);

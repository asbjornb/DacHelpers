namespace DacHelpers.DockerHelpers;

/// <summary>
/// A status class used to return a bit more information than a pure bool
/// </summary>
public class Status
{
    /// <summary>
    /// An error message detailing any errors that happened in operation
    /// </summary>
    public string? Error { get; }
    /// <summary>
    /// Whether the opration ran successfully
    /// </summary>
    public bool IsSuccess => Error == null;

    /// <summary>
    /// Create a new status. If error is null, then IsSuccess will be true
    /// </summary>
    /// <param name="error"></param>
    protected internal Status(string? error)
    {
        Error = error;
    }

    internal static Status Success() => new(null);
    internal static Status Faillure(string error) => new(error);
}

namespace AsmResolver.PE.Exceptions;

/// <summary>
/// Represents unwind information associated to a <see cref="IRuntimeFunction"/> for handling an exception.
/// </summary>
public interface IUnwindInfo
{
    /// <summary>
    /// When available, gets the reference to the exception handler code that is associated with this function.
    /// </summary>
    ISegmentReference ExceptionHandler
    {
        get;
    }

    /// <summary>
    /// When available, gets the reference to the exception handler data that is associated with this function.
    /// </summary>
    ISegmentReference ExceptionHandlerData
    {
        get;
    }
}

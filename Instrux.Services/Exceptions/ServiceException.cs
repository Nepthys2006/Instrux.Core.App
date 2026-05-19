namespace Instrux.Services.Exceptions;

public sealed class ServiceException : Exception
{
    public string UserFacingMessage { get; }

    public ServiceException(string userFacingMessage, Exception? inner = null)
        : base(userFacingMessage, inner)
    {
        UserFacingMessage = userFacingMessage;
    }
}

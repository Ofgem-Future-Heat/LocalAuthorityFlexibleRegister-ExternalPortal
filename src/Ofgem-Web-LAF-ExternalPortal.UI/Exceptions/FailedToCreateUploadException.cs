namespace Ofgem_Web_LAF_ExternalPortal.Exceptions;

public class FailedToCreateUploadException : Exception
{
    private const string BuildMessage = "An upload was not created.";

    public FailedToCreateUploadException() { }

    public FailedToCreateUploadException(string message) : base(message) { }

    public FailedToCreateUploadException(Exception innerException) : base(BuildMessage, innerException) { }

}
namespace Ofgem_Web_LAF_ExternalPortal.Exceptions;

public class MissingDocumentInformationException : Exception
{
    private const string BuildMessage = "Document information is not present.";

    public MissingDocumentInformationException() { }

    public MissingDocumentInformationException(string message) : base(message) { }

    public MissingDocumentInformationException(Exception innerException) : base(BuildMessage, innerException) { }

}
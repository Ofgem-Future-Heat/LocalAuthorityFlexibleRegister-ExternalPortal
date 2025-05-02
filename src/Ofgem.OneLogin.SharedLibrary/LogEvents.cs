namespace Ofgem.API.LAF.LocalAuthorityManagement.Application;

public static class LogEvents
{
    public const int UploadFile = 1000;
    public const int DeleteFile = 1001;
    public const int CreateBlob = 1002;
    public const int GetBlob = 1003;

    public const int UploadDocument = 2000;
    public const int NewDocument = 2001;
    public const int GetDocument = 2002;
    public const int DeleteDocument = 2003;
    public const int UpdateDocument = 2004;
    public const int GetAllDocuments = 2005;

    public const int ValidateFile = 3500;
    public const int RedactionService = 3501;

    public const int Announcements = 4000;
    public const int EditAnnouncement = 4001;
    public const int Admin = 5000;

    public const int Profiles = 6000;
    public const int CreateProfile = 6001;
    public const int GetProfile = 6002;
    public const int DeleteProfile = 6003;
    public const int UpdateProfile = 6004;
    public const int GetProfileAssociatedLocalAuthorities = 6005;

    public const int StatementOfIntents = 7000;
    public const int CreateStatementOfIntent = 7001;
    public const int GetStatementOfIntent = 7002;
    public const int UpdateStatementOfIntent = 7003;

    public const int Health = 9000;
    public const int HealthFull = 9001;

    public const int Infrastructure = 9010;
    public const int ExternalUsers = 70000;


    public const int IndexGet = 10000;
    public const int GetDeclarations = 90000;
    public const int GetDeclaration = 90001;
    public const int GetSupersededDeclarations = 90002;
    public const int SignOut = 90003;
    public const int GetUserLocalAuthorities = 90004;
    public const int AuthenticationCallback = 90005;
    public const int JwtSecurityTokenService = 90006;
    public const int OidcService = 90007;
}
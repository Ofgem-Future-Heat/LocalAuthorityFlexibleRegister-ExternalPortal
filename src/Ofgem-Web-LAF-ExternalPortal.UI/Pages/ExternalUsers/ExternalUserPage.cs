using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ofgem_Web_LAF_ExternalPortal.Extensions;
using Ofgem.LAF.SharedLibrary.Enums;
using Ofgem_Web_LAF_ExternalPortal.Services;
using Ofgem.LAF.SharedLibrary.Constants;

namespace Ofgem_Web_LAF_ExternalPortal.Pages.ExternalUsers
{
    public class ExternalUserPage(IUserManagementServiceUI userService) : PageModel
    {

        [BindProperty(SupportsGet = true)]
        public Ofgem.LAF.SharedLibrary.Models.User ExternalUser { get; set; } = new()
        {
            FirstName = string.Empty,
            LastName = string.Empty,
            EmailAddress = string.Empty,
            UserType = ExternalUserType.LocalAuthorityOfficer
        };

        [BindProperty] public required string HomeBaseLocalAuthority { get; set; } = string.Empty;


        [BindProperty]
        public required List<Ofgem.LAF.SharedLibrary.Models.LocalAuthority>? LocalAuthorities { get; set; } = [];

        [BindProperty] public required List<SelectListItem> LaSelectList { get; set; } = [];
        [BindProperty] public required string SelectedLa { get; set; } = string.Empty;


        [BindProperty]
        public required List<SelectListItem> UserTypes { get; set; } = [];
        [BindProperty] public required string SelectedUserType { get; set; } = string.Empty;


        [BindProperty] public List<(string Id, string Message)> Errors { get; set; } = [];
        [BindProperty] public bool HasMessage => Errors.Count > 0;


        [BindProperty] public bool ErrorFirstname { get; set; }
        [BindProperty] public bool ErrorLastname { get; set; }
        [BindProperty] public bool ErrorEmail { get; set; }
        [BindProperty] public bool ErrorExternalUserType { get; set; }
        [BindProperty] public bool ErrorLocalAuthority { get; set; }


        public const string LocalAuthoritySelectInstruction = ErrorMessagesExternalSite.AddExternalUser.LocalAuthoritySelectInstruction;

        public const string ValidDomain = ErrorMessagesExternalSite.AddExternalUser.ValidDomain;

        public const string FirstnameIsMandatory = ErrorMessagesExternalSite.AddExternalUser.FirstnameIsMandatory;

        public const string FirstnameIsInvalid = ErrorMessagesExternalSite.AddExternalUser.FirstnameIsInvalid;

        public const string LastnameIsMandatory = ErrorMessagesExternalSite.AddExternalUser.LastnameIsMandatory;

        public const string LastnameIsInvalid = ErrorMessagesExternalSite.AddExternalUser.LastnameIsInvalid;

        public const string EmailIsMandatory = ErrorMessagesExternalSite.AddExternalUser.EmailIsMandatory;

        public const string InvalidEmail = ErrorMessagesExternalSite.AddExternalUser.InvalidEmail;

        public const string ExternalUserTypeIsMandatory = ErrorMessagesExternalSite.AddExternalUser.ExternalUserTypeIsMandatory;

        public const string ExternalUserTypeIsInvalid = ErrorMessagesExternalSite.AddExternalUser.ExternalUserTypeIsInvalid;

        public const string LocalAuthoritySelectionMissing = ErrorMessagesExternalSite.AddExternalUser.LocalAuthoritySelectionMissing;

        public const string LocalAuthoritySelectionMatchNotFound = ErrorMessagesExternalSite.AddExternalUser.LocalAuthoritySelectionMatchNotFound;

        public const string ExternalUserFirstNameId = ErrorMessagesExternalSite.AddExternalUser.ExternalUserFirstNameId;

        public const string ExternalUserLastNameId = ErrorMessagesExternalSite.AddExternalUser.ExternalUserLastNameId;

        public const string ExternalUserEmailId = ErrorMessagesExternalSite.AddExternalUser.ExternalUserEmailId;

        public const string ExternalUserLocalAuthorityId = ErrorMessagesExternalSite.AddExternalUser.ExternalUserLocalAuthorityId;

        public const string ExternalUserTypeId = ErrorMessagesExternalSite.AddExternalUser.ExternalUserTypeId;


        public bool ValidFirstname()
        {
            // firstname

            if (string.IsNullOrEmpty(ExternalUser.FirstName))
            {
                Errors.Add(("ExternalUser_FirstName", FirstnameIsMandatory));
                ErrorFirstname = true;
                return false;
            }

            if (TextValidation.IsValidName(ExternalUser.FirstName))
            {
                Errors.Add(("ExternalUser_FirstName", FirstnameIsInvalid));
                ErrorFirstname = true;
                return false;
            }

            return true;
        }

        public bool ValidLastname()
        {
            // lastname

            if (string.IsNullOrEmpty(ExternalUser.LastName))
            {
                Errors.Add(("ExternalUser_LastName", LastnameIsMandatory));
                ErrorLastname = true;
                return false;
            }

            if (TextValidation.IsValidName(ExternalUser.LastName))
            {
                Errors.Add(("ExternalUser_LastName", LastnameIsInvalid));
                ErrorLastname = true;
                return false;
            }

            return true;
        }

        public bool ValidEmail()
        {
            // email
            if (string.IsNullOrEmpty(ExternalUser.EmailAddress))
            {
                Errors.Add(("ExternalUser_EmailAddress", EmailIsMandatory));
                ErrorEmail = true;
                return false;
            }

            if (ExternalUser.EmailAddress.Contains(' '))
            {
                Errors.Add(("ExternalUser_EmailAddress", InvalidEmail));
                ErrorEmail = true;
                return false;
            }

            if (!ExternalUser.EmailAddress.Contains('@'))
            {
                Errors.Add(("ExternalUser_EmailAddress", InvalidEmail));
                ErrorEmail = true;
                return false;
            }

            return true;
        }

        public bool ValidLocalAuthority(out Ofgem.LAF.SharedLibrary.Models.LocalAuthority? baseLocalAuthority)
        {
            var claims = new Models.Claims(HttpContext);

            if (claims.BaseLocalAuthority is null)
            {
                Errors.Add(("External xxxxxxxx", LocalAuthoritySelectionMissing));
                ErrorLocalAuthority = true;
                baseLocalAuthority = null;
                return true;
            }

            SelectedLa = claims.BaseLocalAuthority.OnsCode ?? string.Empty;

            if (string.IsNullOrEmpty(SelectedLa))
            {
                Errors.Add(("External xxxxxxxx", LocalAuthoritySelectionMissing));
                ErrorLocalAuthority = true;
                baseLocalAuthority = null;
                return true;
            }

            if (SelectedLa == LocalAuthoritySelectInstruction)
            {
                Errors.Add(("External xxxxxxxx", LocalAuthoritySelectionMissing));
                ErrorLocalAuthority = true;
                baseLocalAuthority = null;
                return true;
            }

            var source = new Ofgem.LAF.SharedLibrary.Models.LocalAuthority
            {
                Name = claims.BaseLocalAuthority.Name,
                OnsCode = SelectedLa,
                Email = claims.EmailAddress,
            };

            baseLocalAuthority = source;
            return true;
        }

        public async Task<bool> ValidUserTypeAsync()
        {
            if (string.IsNullOrEmpty(SelectedUserType))
            {
                Errors.Add(("SelectedUserType", ExternalUserTypeIsMandatory));
                ErrorExternalUserType = true;
                return false;
            }

            if (SelectedUserType == "0")
            {
                Errors.Add(("SelectedUserType", ExternalUserTypeIsMandatory));
                ErrorExternalUserType = true;
                return false;
            }

            if (TextValidation.IsValidName(SelectedUserType))
            {
                Errors.Add(("SelectedUserType", ExternalUserTypeIsInvalid));
                ErrorExternalUserType = true;
                return false;
            }


            if (Enum.TryParse(SelectedUserType, out ExternalUserType selectedType))
            {
                ExternalUser.UserType = selectedType;
            }
            else
            {
                Errors.Add(("SelectedUserType", ExternalUserTypeIsInvalid));
                ErrorExternalUserType = true;
                return false;
            }

            if (selectedType != ExternalUserType.DesignatedAuthorisedSignatory) return true;

            var (dasUser, found, errorMessage) = await userService.AuthorisedSignatoryExistsAsync(HomeBaseLocalAuthority, SelectedLa);

            if (!found) return true;

            if (dasUser is not null)
            {
                if (dasUser.UserId == ExternalUser.UserId)
                {
                    return true;
                }
                else
                {
                    errorMessage = ErrorMessagesExternalSite.AddExternalUser.YouCannotHaveTwoDedicatedAuthorisedSignatoryUsersForOneLa;

                    Errors.Add(("SelectedUserType", errorMessage));
                    return false;
                }
            }

            Errors.Add(("SelectedUserType", errorMessage));
            return false;
        }

        public void PageDataInitialise()
        {
            CreateUserTypesList();
        }

        public void CreateUserTypesList()
        {
            var list = new List<SelectListItem>();

            foreach (ExternalUserType type in Enum.GetValues(typeof(ExternalUserType)))
            {
                list.Add(new SelectListItem(type.GetDescription(), ((int)type).ToString()));
            }
            if (((int)ExternalUser.UserType).ToString() != "0")
            {
                SelectedUserType = ((int)ExternalUser.UserType).ToString();
                list.Insert(0, new SelectListItem(ExternalUserTypeIsMandatory, "0", false));
                var item = list.Single(x => x.Value == ((int)ExternalUser.UserType).ToString());
                item.Selected = true;
            }

            UserTypes = list;
        }
    }
}

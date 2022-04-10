using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.CognitoIdentityProvider.Model;

namespace ThrivePlanningAPI.Features.UserManagement
{
    public interface ICognitoUserManagement
    {
        Task AdminCreateUserAsync(
            string username,
            string password,
            List<AttributeType> attributeTypes);
        Task AdminAddUserToGroupAsync(
            string username,
            string groupName);
        Task<AdminInitiateAuthResponse> AdminAuthenticateUserAsync(
            string username,
            string password);
        Task AdminRemoveUserFromGroupAsync(
            string username,
            string groupName);
        Task AdminDisableUserAsync(
            string username);
        Task AdminDeleteUserAsync(
            string username);
    }
}
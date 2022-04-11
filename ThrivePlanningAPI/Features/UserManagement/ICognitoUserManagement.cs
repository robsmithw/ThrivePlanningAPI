using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Amazon.CognitoIdentityProvider.Model;

namespace ThrivePlanningAPI.Features.UserManagement
{
    public interface ICognitoUserManagement
    {
        Task AdminCreateUserAsync(
            string username,
            string password,
            List<AttributeType> userAttributes,
            CancellationToken cancellationToken = default);
        Task AdminUpdateUserAttributesAsync(
            string username,
            List<AttributeType> userAttributes,
            CancellationToken cancellationToken = default
        );
        Task AdminAddUserToGroupAsync(
            string username,
            string groupName,
            CancellationToken cancellationToken = default);
        Task<AdminInitiateAuthResponse> AdminAuthenticateUserAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default);
        Task<List<string>> AdminGetUserGroupsAsync(
            string username,
            CancellationToken cancellationToken = default
        );
        Task AdminRemoveUserFromGroupAsync(
            string username,
            string groupName,
            CancellationToken cancellationToken = default);
        Task AdminDisableUserAsync(
            string username,
            CancellationToken cancellationToken = default);
        Task AdminDeleteUserAsync(
            string username,
            CancellationToken cancellationToken = default);
    }
}
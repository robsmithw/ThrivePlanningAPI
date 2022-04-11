using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
using ThrivePlanningAPI.Common.Constants;

namespace ThrivePlanningAPI.Features.UserManagement
{
    public class CognitoUserManagement : ICognitoUserManagement
    {
        private readonly AWSCredentials awsCredentials;
        private readonly AmazonCognitoIdentityProviderClient adminAmazonCognitoIdentityProviderClient;
        private readonly AmazonCognitoIdentityProviderClient anonymousAmazonCognitoIdentityProviderClient;
        private readonly string _userPoolId;
        private readonly string _appClientId;
        private readonly bool _shouldUseCognito;

        public CognitoUserManagement(IConfiguration configuration, string profileName = "default")
        {
            _shouldUseCognito = configuration.GetValue<bool>(AppSettings.FeatureFlags.ShouldUseCognito, false);
            RegionEndpoint regionEndpoint = RegionEndpoint.USWest1;
            CredentialProfileStoreChain credentialProfileStoreChain = new CredentialProfileStoreChain();

            if (_shouldUseCognito)
            {
                _userPoolId = configuration.GetValue<string>(AppSettings.AWS.UserPoolId);
                _appClientId = configuration.GetValue<string>(AppSettings.AWS.AppClientId);
                if (credentialProfileStoreChain.TryGetAWSCredentials(profileName, out AWSCredentials internalAwsCredentials))
                {
                    awsCredentials = internalAwsCredentials;
                    adminAmazonCognitoIdentityProviderClient = new AmazonCognitoIdentityProviderClient(
                        awsCredentials,
                        regionEndpoint);
                    anonymousAmazonCognitoIdentityProviderClient = new AmazonCognitoIdentityProviderClient(
                        new AnonymousAWSCredentials(),
                        regionEndpoint);
                }
                else
                {
                    throw new ArgumentNullException(nameof(AWSCredentials));
                }

                if (_userPoolId is null)
                    throw new ArgumentNullException(nameof(_userPoolId));

                if (_appClientId is null)
                    throw new ArgumentNullException(nameof(_appClientId));
            }
        }

        public async Task AdminCreateUserAsync(
            string username,
            string password,
            List<AttributeType> userAttributes,
            CancellationToken cancellationToken = default)
        {

            if (!_shouldUseCognito)
            {
                return;
            }

            AdminCreateUserRequest adminCreateUserRequest = new AdminCreateUserRequest
            {
                Username = username,
                TemporaryPassword = password,
                UserPoolId = _userPoolId,
                UserAttributes = userAttributes
            };
            AdminCreateUserResponse adminCreateUserResponse = await adminAmazonCognitoIdentityProviderClient
                .AdminCreateUserAsync(adminCreateUserRequest, cancellationToken);

            AdminInitiateAuthRequest adminInitiateAuthRequest = new AdminInitiateAuthRequest
            {
                UserPoolId = _userPoolId,
                ClientId = _appClientId,
                AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", username},
                    { "PASSWORD", password}
                }
            };

            AdminInitiateAuthResponse adminInitiateAuthResponse = await adminAmazonCognitoIdentityProviderClient
                .AdminInitiateAuthAsync(adminInitiateAuthRequest, cancellationToken);

            AdminRespondToAuthChallengeRequest adminRespondToAuthChallengeRequest = new AdminRespondToAuthChallengeRequest
            {
                ChallengeName = ChallengeNameType.NEW_PASSWORD_REQUIRED,
                ClientId = _appClientId,
                UserPoolId = _userPoolId,
                ChallengeResponses = new Dictionary<string, string>
                {
                    { "USERNAME", username },
                    { "NEW_PASSWORD", password }
                },
                Session = adminInitiateAuthResponse.Session
            };

            AdminRespondToAuthChallengeResponse adminRespondToAuthChallengeResponse = await adminAmazonCognitoIdentityProviderClient
                .AdminRespondToAuthChallengeAsync(adminRespondToAuthChallengeRequest, cancellationToken);
        }

        public async Task AdminUpdateUserAttributesAsync(
            string username,
            List<AttributeType> userAttributes,
            CancellationToken cancellationToken = default)
        {
            // This is the user attribute required to mark user as email verified
            // {
            //     new AttributeType()
            //     {
            //         Name = "email_verified",
            //         Value = "true"
            //     }
            // }
            AdminUpdateUserAttributesRequest adminUpdateUserAttributesRequest = new AdminUpdateUserAttributesRequest
            {
                Username = username,
                UserPoolId = _userPoolId,
                UserAttributes = userAttributes
            };

            AdminUpdateUserAttributesResponse adminUpdateUserAttributesResponse = await adminAmazonCognitoIdentityProviderClient
               .AdminUpdateUserAttributesAsync(adminUpdateUserAttributesRequest);
        }

        public async Task AdminAddUserToGroupAsync(
            string username,
            string groupName,
            CancellationToken cancellationToken = default)
        {
            AdminAddUserToGroupRequest adminAddUserToGroupRequest = new AdminAddUserToGroupRequest
            {
                Username = username,
                UserPoolId = _userPoolId,
                GroupName = groupName
            };

            AdminAddUserToGroupResponse adminAddUserToGroupResponse = await adminAmazonCognitoIdentityProviderClient
                .AdminAddUserToGroupAsync(adminAddUserToGroupRequest, cancellationToken);
        }

        public async Task<AdminInitiateAuthResponse> AdminAuthenticateUserAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            AdminInitiateAuthRequest adminInitiateAuthRequest = new AdminInitiateAuthRequest
            {
                UserPoolId = _userPoolId,
                ClientId = _appClientId,
                AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", username},
                    { "PASSWORD", password}
                }
            };
            return await adminAmazonCognitoIdentityProviderClient
                .AdminInitiateAuthAsync(adminInitiateAuthRequest, cancellationToken);
        }

        public async Task<List<string>> AdminGetUserGroupsAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            var request = new AdminListGroupsForUserRequest()
            {
                Username = username,
                UserPoolId = _userPoolId
            };

            var response = await adminAmazonCognitoIdentityProviderClient
                .AdminListGroupsForUserAsync(request, cancellationToken);

            return response.Groups.Select(x => x.GroupName).ToList();
        }

        public async Task AdminRemoveUserFromGroupAsync(
            string username,
            string groupName,
            CancellationToken cancellationToken = default)
        {
            AdminRemoveUserFromGroupRequest adminRemoveUserFromGroupRequest = new AdminRemoveUserFromGroupRequest
            {
                Username = username,
                UserPoolId = _userPoolId,
                GroupName = groupName
            };

            await adminAmazonCognitoIdentityProviderClient
                .AdminRemoveUserFromGroupAsync(adminRemoveUserFromGroupRequest, cancellationToken);
        }

        public async Task AdminDisableUserAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            AdminDisableUserRequest adminDisableUserRequest = new AdminDisableUserRequest
            {
                Username = username,
                UserPoolId = _userPoolId
            };

            await adminAmazonCognitoIdentityProviderClient
                .AdminDisableUserAsync(adminDisableUserRequest, cancellationToken);
        }

        public async Task AdminDeleteUserAsync(
            string username,
            CancellationToken cancellationToken = default)
        {
            AdminDeleteUserRequest deleteUserRequest = new AdminDeleteUserRequest
            {
                Username = username,
                UserPoolId = _userPoolId
            };

            await adminAmazonCognitoIdentityProviderClient
                .AdminDeleteUserAsync(deleteUserRequest, cancellationToken);
        }
    }
}
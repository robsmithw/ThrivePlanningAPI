using System;
using System.Collections.Generic;
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
            List<AttributeType> attributeTypes)
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
                UserAttributes = attributeTypes
            };
            AdminCreateUserResponse adminCreateUserResponse = await adminAmazonCognitoIdentityProviderClient
                .AdminCreateUserAsync(adminCreateUserRequest);

            //AdminUpdateUserAttributesRequest adminUpdateUserAttributesRequest = new AdminUpdateUserAttributesRequest
            //{
            //    Username = username,
            //    UserPoolId = userPoolId,
            //    UserAttributes = new List<AttributeType>
            //            {
            //                new AttributeType()
            //                {
            //                    Name = "email_verified",
            //                    Value = "true"
            //                }
            //            }
            //};

            //AdminUpdateUserAttributesResponse adminUpdateUserAttributesResponse = adminAmazonCognitoIdentityProviderClient
            //    .AdminUpdateUserAttributesAsync(adminUpdateUserAttributesRequest)
            //    .Result;


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
                .AdminInitiateAuthAsync(adminInitiateAuthRequest);

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
                .AdminRespondToAuthChallengeAsync(adminRespondToAuthChallengeRequest);
        }

        public async Task AdminAddUserToGroupAsync(
            string username,
            string groupName)
        {
            AdminAddUserToGroupRequest adminAddUserToGroupRequest = new AdminAddUserToGroupRequest
            {
                Username = username,
                UserPoolId = _userPoolId,
                GroupName = groupName
            };

            AdminAddUserToGroupResponse adminAddUserToGroupResponse = await adminAmazonCognitoIdentityProviderClient
                .AdminAddUserToGroupAsync(adminAddUserToGroupRequest);
        }

        public async Task<AdminInitiateAuthResponse> AdminAuthenticateUserAsync(
            string username,
            string password)
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
                .AdminInitiateAuthAsync(adminInitiateAuthRequest);
        }

        public async Task AdminRemoveUserFromGroupAsync(
            string username,
            string groupName)
        {
            AdminRemoveUserFromGroupRequest adminRemoveUserFromGroupRequest = new AdminRemoveUserFromGroupRequest
            {
                Username = username,
                UserPoolId = _userPoolId,
                GroupName = groupName
            };

            await adminAmazonCognitoIdentityProviderClient
                .AdminRemoveUserFromGroupAsync(adminRemoveUserFromGroupRequest);
        }

        public async Task AdminDisableUserAsync(
            string username)
        {
            AdminDisableUserRequest adminDisableUserRequest = new AdminDisableUserRequest
            {
                Username = username,
                UserPoolId = _userPoolId
            };

            await adminAmazonCognitoIdentityProviderClient
                .AdminDisableUserAsync(adminDisableUserRequest);
        }

        public async Task AdminDeleteUserAsync(
            string username)
        {
            AdminDeleteUserRequest deleteUserRequest = new AdminDeleteUserRequest
            {
                Username = username,
                UserPoolId = _userPoolId
            };

            await adminAmazonCognitoIdentityProviderClient
                .AdminDeleteUserAsync(deleteUserRequest);
        }
    }
}
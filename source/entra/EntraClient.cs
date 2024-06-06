﻿using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Maestro
{
    // EntraID Microsoft Graph client
    public class EntraClient
    {
        private readonly IAuthClient _authClient;
        private readonly IHttpHandler _httpHandler;
        public string BearerToken;

        public EntraClient()
        {
            _httpHandler = new HttpHandler();
            _authClient = new AuthClient(_httpHandler);
        }

        // Check the database for a stored access token before fetching from Intune
        public static async Task<EntraClient> CreateAndGetToken(IDatabaseHandler database = null, string bearerToken = "", bool reauth = false)
        {
            var entraClient = new EntraClient();

            // Use the provided bearer token if available
            if (!string.IsNullOrEmpty(bearerToken))
            {
                entraClient.BearerToken = bearerToken;
                return entraClient;
            }

            // Check the database for a stored access token before fetching from Intune
            if (database != null && !reauth)
            {
                //entraClient.FindStoredAccessToken(database);
            }

            // Get a new access token if none found
            if (string.IsNullOrEmpty(entraClient.BearerToken))
            {
                //await entraClient.SignInToIntuneAndGetAccessToken(database);
            }
            return entraClient;
        }

        public async Task<string> GetAccessToken(string tenantId, string portalAuthorization)
        {
            Logger.Info("Requesting EntraID access token");
            string entraIdAccessToken = await _authClient.GetAccessToken(tenantId, portalAuthorization,
                "https://intune.microsoft.com/api/DelegationToken",
                "Microsoft_AAD_IAM", "microsoft.graph");
            if (entraIdAccessToken is null) return null;

            _httpHandler.SetAuthorizationHeader(entraIdAccessToken);

            BearerToken = entraIdAccessToken;
            return entraIdAccessToken;
        }

        public async Task<HttpResponseMessage> GetGroups()
        {
            string url = "https://graph.microsoft.com/v1.0/$batch";
            var jsonObject = new
            {
                requests = new[]
                {
                    new
                    {
                        id = "SecurityEnabledGroups",
                        method = "GET",
                        url = "groups?$select=displayName,mail,id,onPremisesSyncEnabled,onPremisesLastSyncDateTime,groupTypes,mailEnabled,securityEnabled,resourceProvisioningOptions,isAssignableToRole&$top=100&$filter=securityEnabled eq true",
                        headers = new Dictionary<string, object>()
                    }
                }
            };
            var serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(jsonObject);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpHandler.PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> GetGroupMembers(string groupId)
        {
            string url = $"https://graph.microsoft.com/beta/groups/{groupId}/members?$select=id,displayName,userType,appId,mail,onPremisesSyncEnabled,deviceId&$orderby=displayName%20asc&$count=true";
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Set required header per https://aka.ms/graph-docs/advanced-queries
            request.Headers.Add("Consistencylevel", "eventual");

            return await _httpHandler.SendRequestAsync(request);
        }
    }
}

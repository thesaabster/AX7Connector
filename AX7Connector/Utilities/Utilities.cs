using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Net.Http.Formatting;
using System.Configuration;

namespace AX7Connector.Utilities
{
    public class AXUtilities
    {
        string authenticationHeader;
        public string AuthenticationHeader
        {
            get
            {
                return authenticationHeader;
            }
        }

        public AXUtilities()
        {
            //authentication string details
            AuthenticationContext authenticationContext = new AuthenticationContext(ClientConfiguration.Default.ActiveDirectoryTenant);

            UserCredential userCred = new UserCredential(ClientConfiguration.Default.UserName, ClientConfiguration.Default.Password);

            AuthenticationResult authenticationResult = authenticationContext.AcquireToken(ClientConfiguration.Default.ActiveDirectoryResource, ClientConfiguration.Default.ActiveDirectoryClientAppId, userCred);

            authenticationHeader = authenticationResult.CreateAuthorizationHeader();
        }

        /// <summary>
        /// Post request
        /// </summary>
        /// <param name="uri">Enqueue endpoint URI</param>
        /// <param name="authenticationHeader">Authentication header</param>
        /// <param name="bodyStream">Body stream</param>
        /// <param name="body">Body string content</param>
        /// <param name="message">ActivityMessage context</param>
        /// <returns></returns>
        public HttpResponseMessage SendPostRequest(Uri uri, string authenticationHeader, Stream bodyStream, string body, ActivityMessage message, string externalCorrelationHeaderValue = null)
        {

            using (HttpClientHandler handler = new HttpClientHandler() { UseCookies = false })
            {
                using (HttpClient webClient = new HttpClient(handler))
                {
                    webClient.DefaultRequestHeaders.Authorization = ParseAuthenticationHeader(authenticationHeader);

                    HttpResponseMessage response;

                    if (bodyStream != null)
                    {
                        using (StreamContent content = new StreamContent(bodyStream))
                        {
                            response = webClient.PostAsync(uri, content).Result;
                        }

                    }
                    else if (body != null)
                    {
                        //convert string into memorystream to be passed in the post request
                        byte[] bytes = Encoding.ASCII.GetBytes(body);
                        MemoryStream memStream = new MemoryStream(bytes);

                        using (StreamContent content = new StreamContent(memStream))
                        {
                            response = webClient.PostAsync(uri, content).Result;
                        }
                    }
                    else
                    {
                        response = webClient.PostAsJsonAsync<ActivityMessage>(uri.ToString(), message).Result;
                    }

                    return response;
                }
            }
        }

        public async Task<HttpResponseMessage> GetRequestAsync(Uri uri, string authenticationHeader)
        {
            HttpResponseMessage responseMessage;
            using (HttpClientHandler handler = new HttpClientHandler() { UseCookies = false })
            {
                using (HttpClient webClient = new HttpClient(handler))
                {
                    webClient.DefaultRequestHeaders.Authorization = ParseAuthenticationHeader(authenticationHeader);

                    responseMessage = await webClient.GetAsync(uri).ConfigureAwait(false);
                }
            }
            return responseMessage;
        }


        public HttpResponseMessage SendRequest(HttpRequestMessage request)
        {
            using (HttpClientHandler handler = new HttpClientHandler() { UseCookies = false })
            {
                using (HttpClient webClient = new HttpClient(handler))
                {
                    HttpResponseMessage response = webClient.SendAsync(request).Result;

                    return response;
                }
            }
        }

        private static AuthenticationHeaderValue ParseAuthenticationHeader(string authenticationHeader)
        {
            string[] split = authenticationHeader.Split(' ');
            string scheme = split[0];
            string parameter = split[1];
            return new AuthenticationHeaderValue(scheme, parameter);
        }


    }
}
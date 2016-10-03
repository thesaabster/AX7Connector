using Microsoft.Dynamics.AX.Framework.Tools.DataManagement.Serialization;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using AX7Connector.Models;
using AX7Connector.Utilities;
using System.IO;
using AX7Connector.Microsoft.Dynamics.DataEntities;
using Microsoft.OData.Client;
namespace AX7Connector.Controllers
{
    public class AxController : ApiController
    {


        [HttpPost]
        [Route("dixf/import")]
        public async Task<string> Import([FromBody] DIXFEntity entity)
        {
            HttpResponseMessage responseMsg = Request.CreateResponse(HttpStatusCode.OK);

            //string relativeUri = string.Format(@"{0}", executionId);
            //responseMsg.Headers.Location = new Uri(Request.RequestUri, relativeUri);

            if (entity == null)
                return string.Empty;

            /*uri = new Uri("https://dev-matt-3devaos.cloudax.dynamics.com/data");

            var context = new Resources(uri);
            context.SendingRequest2 += new EventHandler<SendingRequest2EventArgs>(delegate (object sender, SendingRequest2EventArgs e)
            {
                e.RequestMessage.SetHeader("Authorization", AXUtilities.AuthorizationHeader);
            });

            var myendpoint = from EndPoint in context.IntegrationDataProjectEndpoints
                             where EndPoint.LegacySourceEndpointId == entity.SourceEndpointId
                             select EndPoint;
            */

            string entityName = "Customer groups";// myendpoint.First().EntityName;

            AXUtilities axUtil = new AXUtilities();

            UriBuilder enqueueUri = new UriBuilder(ClientConfiguration.Default.ActiveDirectoryResource);
            string activityId = "bd2578ac-9690-4b93-908e-ccfb3b786f42";//myendpoint.First().ActivityId.ToString();


            enqueueUri.Path = string.Format(@"api/connector/enqueue/{0}", activityId);

            string enqueueQuery = string.Format("entity={0}", entityName);

            string company = "wcf";//myendpoint.First().Company

            if (!string.IsNullOrEmpty(company))
            {
                enqueueQuery += "&company=" + company;
            }


            enqueueUri.Query = enqueueQuery;

            HttpResponseMessage response = null;

            using (Stream bodyStream = GenerateStreamFromString(entity.FileContent))
            {
                response = axUtil.SendPostRequest(enqueueUri.Uri, AXUtilities.AuthorizationHeader, bodyStream, null, null);
            }
            var messageId = await response.Content.ReadAsStringAsync();


            return messageId;
        }

        [HttpGet]
        [Route("dixf/GetJobStatus/{messageId}")]
        public async Task<DataJobStatusDetail> GetJobStatus(string messageId)
        {
            DataJobStatusDetail jobStatusDetail = null;

            /// get status
            UriBuilder statusUri = new UriBuilder(ClientConfiguration.Default.ActiveDirectoryResource);
            string activityId = ClientConfiguration.Default.CustomerImportActivityId;


            statusUri.Path = string.Format(@"api/connector/jobstatus/{0}", activityId);

            string statusQuery = string.Format("jobid={0}", messageId.Replace(@"""", ""));

            statusUri.Query = statusQuery;


            //send a request to get the message status
            AXUtilities axUtil = new AXUtilities();


            var response = await axUtil.GetRequestAsync(statusUri.Uri);
            if (response.IsSuccessStatusCode)
            {
                // Deserialize response to the DataJobStatusDetail object
                jobStatusDetail = JsonConvert.DeserializeObject<DataJobStatusDetail>(response.Content.ReadAsStringAsync().Result);
            }
            else
            {

            }

            return jobStatusDetail;
        }

        [HttpGet]
        [Route("dixf/WaitForJob/{messageId}")]
        public async Task<HttpResponseMessage> WaitForJob(string messageId)
        {
            DataJobStatusDetail jobStatusDetail = null;
            HttpResponseMessage responseMsg = Request.CreateResponse(HttpStatusCode.Accepted);
            string relativeUri = string.Empty;


            /// get status
            UriBuilder statusUri = new UriBuilder(ClientConfiguration.Default.ActiveDirectoryResource);
            string activityId = ClientConfiguration.Default.CustomerImportActivityId;


            statusUri.Path = string.Format(@"api/connector/jobstatus/{0}", activityId);

            string statusQuery = string.Format("jobid={0}", messageId.Replace(@"""", ""));

            statusUri.Query = statusQuery;


            //send a request to get the message status
            AXUtilities axUtil = new AXUtilities();


            var response = await axUtil.GetRequestAsync(statusUri.Uri);
            if (response.IsSuccessStatusCode)
            {
                // Deserialize response to the DataJobStatusDetail object
                jobStatusDetail = JsonConvert.DeserializeObject<DataJobStatusDetail>(response.Content.ReadAsStringAsync().Result);

                if (jobStatusDetail != null)
                {
                    if (jobStatusDetail.DataJobStatus.DataJobState == DataJobState.Processed || jobStatusDetail.DataJobStatus.DataJobState == DataJobState.ProcessedWithErrors)
                    {
                        responseMsg.StatusCode = HttpStatusCode.OK;
                    }
                    else
                    {
                        responseMsg.StatusCode = HttpStatusCode.Accepted;
                        relativeUri = string.Format("{0}", messageId);
                        responseMsg.Headers.Location = new Uri(Request.RequestUri, relativeUri);
                    }

                }
            }
            else
            {

            }


            return responseMsg;
        }

        private static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


    }
}

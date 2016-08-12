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

namespace AX7Connector.Controllers
{
    public class DIXFController : ApiController
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

            string[] folderSegments = entity.Name.Split(new string[]{ "/"}, StringSplitOptions.RemoveEmptyEntries);

            if (folderSegments.Length < 1)
                return string.Empty;

            string entityName = folderSegments[folderSegments.Length - 2];

            AXUtilities axUtil = new AXUtilities();

            UriBuilder enqueueUri = new UriBuilder(ClientConfiguration.Default.ActiveDirectoryResource);
            string activityId = string.Empty;

            switch (entityName)
            {
                case "Fleet Management Customers":
                    activityId = ClientConfiguration.Default.CustomerImportActivityId;
                    break;

                case "Fleet Management Rentals":
                    activityId = ClientConfiguration.Default.RentalImportActivityId;
                    break;
            }

            enqueueUri.Path = string.Format(@"api/connector/enqueue/{0}", activityId);
            
            string enqueueQuery = string.Format("entity={0}", entityName);
           
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
                        relativeUri = string.Format("{0}",  messageId);
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

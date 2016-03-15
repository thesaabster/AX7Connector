using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public HttpResponseMessage Import([FromBody] DIXFEntity entity)
        {
            HttpResponseMessage responseMsg = Request.CreateResponse(HttpStatusCode.OK);

            //string relativeUri = string.Format(@"{0}", executionId);
            //responseMsg.Headers.Location = new Uri(Request.RequestUri, relativeUri);

            if (entity == null)
                return responseMsg;

            string[] folderSegments = entity.Name.Split(new string[]{ "\\"}, StringSplitOptions.RemoveEmptyEntries);

            if (folderSegments.Length < 1)
                return responseMsg;

            string entityName = folderSegments[folderSegments.Length - 1];

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
                response = axUtil.SendPostRequest(enqueueUri.Uri, axUtil.AuthenticationHeader, bodyStream, null, null);

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

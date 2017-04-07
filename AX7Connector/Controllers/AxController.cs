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
using System.Diagnostics;

namespace AX7Connector.Controllers
{
    public class AxController : ApiController
    {
        static Dictionary<Guid, int> StatusTable = new Dictionary<Guid, int>();
        static Dictionary<Guid, string> RequestXmlTable = new Dictionary<Guid, string>();
        static Dictionary<Guid, string> ResponseXmlTable = new Dictionary<Guid, string>();


        [HttpGet]
        [Route("connectship/getkey")]
        public async Task<HttpResponseMessage> GetKey()
        {
            HttpResponseMessage responseMsg = Request.CreateResponse(HttpStatusCode.OK);

            Guid key = StatusTable.Where(x => x.Value == 0).FirstOrDefault().Key;

            if (key != Guid.Empty)
            {
                StatusTable[key] = 1;
                responseMsg.Content = new StringContent(RequestXmlTable[key]);
            }

            return responseMsg;
        }

        [HttpPost]        
        [Route("connectship/request")]
        public async Task<HttpResponseMessage> SendRequest()
        {
            HttpResponseMessage responseMsg = Request.CreateResponse(HttpStatusCode.OK);


            string response = string.Empty;

            try
            {
                var body = await Request.Content.ReadAsStringAsync();

                Guid trackerId = Guid.NewGuid();

                StatusTable.Add(trackerId, 0);
                RequestXmlTable.Add(trackerId, body);

                Stopwatch watch = new Stopwatch();
                watch.Start();
                while (watch.Elapsed.Seconds <= 30)
                {
                    ResponseXmlTable.TryGetValue(trackerId, out response);

                    if (!string.IsNullOrEmpty(response))
                        break;


                }

                if (string.IsNullOrEmpty(response))
                {
                    responseMsg.StatusCode = HttpStatusCode.RequestTimeout;

                }

                responseMsg.Content = new StringContent(response, System.Text.Encoding.UTF8, "application/soap+xml");

            }
            catch (Exception ex)
            {
                response = ex.ToString();
                responseMsg.Content = new StringContent(response);
            }




            return responseMsg;
        }




        [HttpPost]
        [Route("connectship/setresponse")]
        public async Task<HttpResponseMessage> SetResponse()
        {
            HttpResponseMessage responseMsg = Request.CreateResponse(HttpStatusCode.OK);
            string response = string.Empty;

            try
            {
                var body = await Request.Content.ReadAsStringAsync();

                Guid key = StatusTable.Where(x => x.Value == 1).FirstOrDefault().Key;

                if (key != Guid.Empty)
                {
                    StatusTable[key] = 2;


                    if (!ResponseXmlTable.ContainsKey(key))
                    {
                        ResponseXmlTable.Add(key, body);
                    }

                    responseMsg.Content = new StringContent(Convert.ToString(key));
                }


            }
            catch (Exception ex)
            {
                responseMsg.Content = new StringContent(ex.ToString());

            }

            return responseMsg;
        }

    }
}

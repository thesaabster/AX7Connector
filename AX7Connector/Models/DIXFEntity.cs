using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace AX7Connector.Models
{
    public class DIXFEntity
    {
        [DataMember]
        public string MessageId { get; set; }

        [DataMember]
        public string SourceEndpointId { get; set; }


        [DataMember]
        public string DestinationEndpointId { get; set; }


        [DataMember]
        public string ActionId { get; set; }

        [DataMember]
        public string FileContent { get; set; }
    }
}
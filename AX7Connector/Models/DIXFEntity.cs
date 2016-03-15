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
        public string Name { get; set; }

        [DataMember]
        public string FileContent { get; set; }
    }
}
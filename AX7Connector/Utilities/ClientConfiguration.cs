using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AX7Connector.Utilities
{
    public partial class ClientConfiguration
    {
        public static ClientConfiguration Default { get { return ClientConfiguration.OneBox; } }

        public static ClientConfiguration OneBox = new ClientConfiguration()
        {
            UriString = "https://paulwuax7-ctp8aos.cloudax.dynamics.com/",

            #region
            UserName = "qipengwu@pwax7.onmicrosoft.com",
            Password = "Pass1Word",
            #endregion

            ActiveDirectoryResource = "https://paulwuax7-ctp8aos.cloudax.dynamics.com",
            ActiveDirectoryTenant = "https://login.windows.net/pwax7.onmicrosoft.com",
            ActiveDirectoryClientAppId = "f00ced34-2591-41f4-b22d-5005000f05be",
            CustomerImportActivityId = "D595048A-65D8-4BDC-B4D7-5B3AAA82EC69",
            RentalImportActivityId = "2BD483F3-BB15-4562-9E5B-F1875DECEA97"
        };

    public string UriString { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string ActiveDirectoryResource { get; set; }
    public String ActiveDirectoryTenant { get; set; }
    public String ActiveDirectoryClientAppId { get; set; }
    public string CustomerImportActivityId { get; set; }
    public string RentalImportActivityId { get; set; }
}
}

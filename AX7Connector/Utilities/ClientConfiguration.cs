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
           UriString = "https://paulwuupdate1aos.cloudax.dynamics.com/",

            #region
            UserName = "",
            Password = "",
            #endregion

            ActiveDirectoryResource = "https://paulwuupdate1aos.cloudax.dynamics.com",
            ActiveDirectoryTenant = "https://login.windows.net/pwax7.onmicrosoft.com",
            ActiveDirectoryClientAppId = "",
            CustomerImportActivityId = "",
            RentalImportActivityId = ""
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

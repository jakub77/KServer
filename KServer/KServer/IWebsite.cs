using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace KServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IWebsite" in both code and config file together.
    [ServiceContract]
    public interface IWebsite
    {
        [OperationContract]
        Response DJValidateUsername(string username);

        [OperationContract]
        Response MobileValidateUsername(string username);

        [OperationContract]
        Response DJSignUp(string userName, string password, Venue venue, string email);

        [OperationContract]
        Response MobileSignUp(string username, string password, string email);

        [OperationContract]
        Response DJRequestPasswordChange(string username, string password);

        [OperationContract]
        Response MobileRequestPasswordChange(string username, string password);

        [OperationContract]
        Response CompletePasswordChange(string hashKey);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace KServer
{
    [ServiceContract]
    public interface IWebsite
    {
        [OperationContract]
        Response SendEmailWithUsername(string email);

        [OperationContract]
        Response StartPasswordReset(string email, string username, bool isDJ, string websiteAddress);

        [OperationContract]
        Response ValidatePasswordResetKey(string key, bool isDJ, out int ID);

        [OperationContract]
        Response Login(string username, string password, string role, out int ID);

        [OperationContract]
        Response ChangePassword(int ID, string role, string newPassword);

        [OperationContract]
        Response ChangeEmail(int ID, string role, string newEmail);

        [OperationContract]
        Response DJSignUp(string username, string password, Venue venue, string email);

        [OperationContract]
        Response MobileSignUp(string username, string password, string email);

        [OperationContract]
        Response EnableDisableRegistration(bool enableRegistration);

        [OperationContract]
        Response IsRegistrationAllowed(out bool registrationAllowed);
    }
}

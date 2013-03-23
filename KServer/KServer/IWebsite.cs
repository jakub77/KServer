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
        Response SendEmailWithUsername(string email, string role);

        [OperationContract]
        Response StartPasswordReset(string email, out string key, string role);

        [OperationContract]
        Response CompletePasswordReset(string key, string newPassword);

        [OperationContract]
        Response ChangePassword(string username, string oldPassword, string newPassword, string role);

        [OperationContract]
        Response ChangeEmail(string username, string password, string newEmail, string role);

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

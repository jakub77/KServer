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
        void DoWork();
    }
}

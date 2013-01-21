using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace KServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IMobile" in both code and config file together.
    [ServiceContract]
    public interface IMobile
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/{strSessionString}/activityStatus")]
        MResponse test(string strSessionString);
    }

    public class MResponse
    {
        public bool successful { get; set; }
        public string comment { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace KServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Mobile" in code, svc and config file together.
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class Mobile : IMobile
    {
        public string test(string param1)
        {
            return param1 + "XYZ";
        }
    }
}

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace GreetingService
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        string Greet(string name);
    }
}

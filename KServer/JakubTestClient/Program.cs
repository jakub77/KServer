using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JakubTestClient.ServiceReference1;
using System.ServiceModel;

namespace JakubTestClient
{
    public class CallBacks : IService1Callback
    {
        Response IService1Callback.DJQueueChanged(queueSinger[] queue)
        {
            Console.WriteLine("Got a callback");
            return null;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            InstanceContext site = new InstanceContext(new CallBacks());
            Service1Client client = new Service1Client(site, "WSDualHttpBinding_IService1");

            for (; ; )
            {
                string a = Console.ReadLine();
                string b = Console.ReadLine();
                Response r = client.DJSignUp(a, b);
                Console.WriteLine(r.error);
                Console.ReadKey();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace GreetingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service1 : IService1
    {
        public string Greet(string name)
        {
            return "Hello - " + name;
        }
    }
}

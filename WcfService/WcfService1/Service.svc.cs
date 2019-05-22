using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Web;

namespace WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.

    //[ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class Service : IService
    {
        public string GetCurrentUser()
        {
            foreach(var key in HttpContext.Current.Request.Headers.Cast<string>())
            {
                Console.Error.WriteLine($"{key}: {HttpContext.Current.Request.Headers[key]}");
            }

            Console.Error.WriteLine($"Is Authenticated: {Thread.CurrentPrincipal.Identity.IsAuthenticated}");
            Console.Error.WriteLine($"Thread.CurrentPrincipal.Identity.Name.User: {Thread.CurrentPrincipal.Identity.Name}");
            Console.Error.WriteLine($"HttpContext.Current.User.Identity.Name: {HttpContext.Current.User.Identity.Name}");

            return $"Current user: {Thread.CurrentPrincipal.Identity.Name}";
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
    }
}

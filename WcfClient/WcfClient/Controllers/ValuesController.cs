using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using WcfClient.MyService;

namespace WcfClient.Controllers
{
    public class ValuesController : ApiController
    {
        [HttpGet]
        [Route("api/values/{id}/{protocol=http}")]
        public string Get(int id, string protocol)
        {
            var client = new ServiceClient(protocol == "tcp" ? "NetTcpBinding_IService" : "BasicHttpBinding_IService");

            try
            {
                return $"{client.GetData(id)}, returned by My Wcf Service";
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: " + e);
                return e.ToString();
            }
        }

        [HttpGet]
        [Route("api/values/serviceuser/{protocol=http}")]
        public string GetServiceUser(string protocol)
        {
            var client = new ServiceClient(protocol == "tcp" ? "NetTcpBinding_IService" : "BasicHttpBinding_IService");

            try
            {
                return $"WcfService: {client.GetCurrentUser()}";
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: " + e);
                return e.ToString();
            }
        }

        [HttpGet]
        [Route("api/values/myuser/{protocol=http}")]
        public string GetUser(string protocol)
        {
            var client = new ServiceClient(protocol == "tcp" ? "NetTcpBinding_IService" : "BasicHttpBinding_IService");

            try
            {
                return $"Current user: {Thread.CurrentPrincipal.Identity.Name}";
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: " + e);
                return e.ToString();
            }
        }

        [HttpGet]
        [Route("api/values")]
        public string Get()
        {
            return $"You invoked Get";
        }

        [HttpGet]
        [Route("api/values/svc/{protocol=http}")]
        public string GetSvcResponse(string protocol)
        {
            try
            {
                var svcClient = new ServiceClient(protocol == "tcp" ? "NetTcpBinding_IService" : "BasicHttpBinding_IService");
                var client = new HttpClient();
                var response = client.GetAsync(svcClient.Endpoint.Address.Uri).Result;
                return response.ToString();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: " + e);
                return e.ToString();
            }
        }

        [HttpGet]
        [Route("api/values/wsdl/{protocol=http}")]
        public string GetWsdl(string protocol)
        {
            try
            {
                var svcClient = new ServiceClient(protocol == "tcp" ? "NetTcpBinding_IService" : "BasicHttpBinding_IService");
                var client = new HttpClient();
                var response = client.GetAsync(svcClient.Endpoint.Address.Uri).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: " + e);
                return e.ToString();
            }
        }
    }
}

using Microsoft.AspNetCore.Authentication.GssKerberos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text.RegularExpressions;
using System.Web;

namespace WcfClient
{
    public class MyHttpRequestIwaInterceptor : IClientMessageInspector
    {
        private const string AUTHORIZATION_HEADER = "Authorization";

        public MyHttpRequestIwaInterceptor()
        {
        }

        #region IClientMessageInspector Members
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {

        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var spn = ConfigurationManager.AppSettings["BasicHttpBinding_IService_SPN"];//Need to pull it from the channel/request

            HttpRequestMessageProperty httpRequestMessage;
            object httpRequestMessageObject;
            if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
            {
                httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
                if (string.IsNullOrEmpty(httpRequestMessage.Headers[AUTHORIZATION_HEADER]))
                {
                    httpRequestMessage.Headers[AUTHORIZATION_HEADER] = GetKerberosTicket(spn);
                }
            }
            else
            {
                httpRequestMessage = new HttpRequestMessageProperty();
                httpRequestMessage.Headers.Add(AUTHORIZATION_HEADER, GetKerberosTicket(spn));
                request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
            }
            return null;
        }
        private string GetKerberosTicket(string spn)
        {
            var clientUpn = ConfigurationManager.AppSettings["Client_UPN"];

            Console.WriteLine($"Client_UPN: {clientUpn}");
            Console.WriteLine($"SPN: {spn}");

            EnsureTgt(clientUpn);

            using (var clientCredentials = GssCredentials.FromKeytab(clientUpn, CredentialUsage.Initiate))
            {
                using (var initiator = new GssInitiator(credential: clientCredentials, spn: spn))
                {
                    try
                    {
                        var kerberosTicket = Convert.ToBase64String(initiator.Initiate(null));
                        Console.WriteLine($"Ticket: {kerberosTicket}");
                        return $"Negotiate {kerberosTicket}";
                    }
                    catch (GssException exception)
                    {
                        Console.Error.WriteLine(exception.Message);
                        return string.Empty;
                    }
                }
            }
        }
        #endregion

        private static void EnsureTgt(string principal)
        {
            var expiry = GetTgtExpiry();
            if (expiry < DateTime.Now)
            {
                Console.WriteLine($"Obtaining TGT for principal {principal}...");
                ObtainTgt(principal);
            }
        }

        private static DateTime GetTgtExpiry()
        {
            //todo: klist exists in c:\windows\system32 which is totally different and will be defaulted to if not in bin folder
            try
            {
                Console.WriteLine($"Running command to obtain existing TGT...");
                var klistResult = RunCmd(@"C:\Users\vcap\app\bin\klist.exe", null);
                var tgtExpiryMatch = Regex.Match(klistResult, ".{17}(?=  krbtgt)");
                if (tgtExpiryMatch.Success && DateTime.TryParse(tgtExpiryMatch.Value, out var expiry))
                    return expiry;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return DateTime.MinValue;
        }
        private static void ObtainTgt(string principal)
        {
            RunCmd(@"C:\Users\vcap\app\bin\kinit.exe", $"-k -i {principal}");
        }


        private static string RunCmd(string cmd, string args)
        {
            //            if (!ExistsOnPath(cmd)) throw new FileNotFoundException("Cannot initialize TGT - kinit.exe is not found", "kinit.exe");
            var cmdsi = new ProcessStartInfo(cmd);
            cmdsi.Arguments = args;
            cmdsi.RedirectStandardOutput = true;
            cmdsi.RedirectStandardError = true;
            cmdsi.UseShellExecute = false;
            var proc = Process.Start(cmdsi);
            var result = proc.StandardOutput.ReadToEnd();
            var err = proc.StandardError.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(err)) throw new Exception(err);
            proc.WaitForExit();
            return result;
        }
    }
}
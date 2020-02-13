using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace NMS_Sunstate.Healpers
{
    public class ConnectionHelper
    {
        public IOrganizationService CRMConnect()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ClientCredentials credentials = new ClientCredentials();
                credentials.UserName.UserName = System.Configuration.ConfigurationManager.AppSettings["UserName"];
                credentials.UserName.Password = System.Configuration.ConfigurationManager.AppSettings["Password"];
                Uri serviceUri = new Uri(System.Configuration.ConfigurationManager.AppSettings["OrgServiceUri"]);
                
                OrganizationServiceProxy proxy = new OrganizationServiceProxy(serviceUri, null, credentials, null);
                proxy.EnableProxyTypes();
                var service = (IOrganizationService)proxy;
                var resp = ((WhoAmIResponse)service.Execute(new WhoAmIRequest()));
                if (resp.UserId != Guid.Empty)
                {
                    Console.WriteLine("Connection Established Successfully");
                    return service;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while connecting to CRM " + ex.Message);
                throw;
            }
        }
    }
}

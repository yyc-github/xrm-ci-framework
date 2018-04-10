using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System.Configuration;
using System.Threading;
using System;
using System.Net;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk.Client;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public abstract class XrmCommandBase : Cmdlet
    {
        protected IOrganizationService OrganizationService;
        protected CrmServiceClient ServiceClient;
        private int DefaultTime = 120;
        private TimeSpan ConnectPolingInterval = TimeSpan.FromSeconds(15);
        private int ConnectRetryCount = 3;

        [Parameter(Mandatory = true)]
        public string Username { get; set; }
        [Parameter(Mandatory = true)]
        public string Password { get; set; }
        [Parameter(Mandatory = true)]
        public string EndPoint { get; set; }

        /// <summary>
        /// <para type="description">Timeout in seconds</para>
        /// </summary>
        [Parameter(Mandatory = false)]
        public int Timeout { get; set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            try
            {
                var crmEndPoint = new Uri(EndPoint);
                var manager = ServiceConfigurationFactory.CreateManagement<IOrganizationService>(crmEndPoint);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var credentials = new ClientCredentials();
                credentials.Windows.ClientCredential = new NetworkCredential(Username, Password);

                var proxy = new OrganizationServiceProxy(manager.CurrentServiceEndpoint.ListenUri, null, credentials,
                    null);
                proxy.Timeout = new TimeSpan(0, 0, Timeout == 0 ? DefaultTime : Timeout);
                proxy.EnableProxyTypes();
                OrganizationService = proxy;
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to connect to CRM on endpoint: ${EndPoint}");
            }

        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (ServiceClient != null)
                ServiceClient.Dispose();
        }
    }
}
namespace NMS_Sunstate
{
    using Microsoft.Xrm.Sdk;
    using NMS_Sunstate.Healpers;
    using NMS_Sunstate.Model;
    using NMS_Sunstate.NewFolder;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class Program
    {
        public static readonly ILogger log = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();
        static void Main(string[] args)
        {
            BidderBL objbidderbl = new BidderBL(log);
            try
            {
                ConnectionHelper crmConnection = new ConnectionHelper();
                IOrganizationService organizationService = crmConnection.CRMConnect();
                Console.WriteLine("Connected to Organization URI - "+System.Configuration.ConfigurationManager.AppSettings["OrgServiceUri"]);
                Console.WriteLine("Enter 'y' to continue");
                string confirm = Console.ReadLine();
                if (confirm.Equals("y", StringComparison.CurrentCultureIgnoreCase))
                {
                    EntityCollection ec_bidders = objbidderbl.GetActiveRecords(organizationService);
                    log.Information("Total Active Bidder Records = " + ec_bidders.Entities.Count() + "\n");
                    Bidder bidder = objbidderbl.ProcessDuplicates(organizationService, ec_bidders);
                    Console.WriteLine("Process completed");
                }
                //else
                //{
                //    Console.WriteLine("");
                //}
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

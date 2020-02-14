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

                    var totalCount = bidder.bidderInformation.Sum(i => i.TotalDuplicates);
                    var totalActiveRecords = bidder.bidderInformation.Sum(i => i.ActiveBidderCount);
                    var totalInactiveRecord = bidder.bidderInformation.Sum(i => i.DeactivatedBidderCount);
                    log.Information("Total Count : " + totalCount);
                    log.Information("Total Active Records : " + totalActiveRecords);
                    log.Information("Total Inactive Record : " + totalInactiveRecord + "\n\n");

                    if (totalCount != totalActiveRecords + totalInactiveRecord)
                    {
                        foreach (var item in bidder.FailedRecord)
                        {
                            log.Information(item);
                        }
                    }
                    Console.WriteLine("Process completed");
                }
                Console.ReadLine();
            }
            catch (Exception ex)
            
            {
                throw ;
            }
        }
    }
}

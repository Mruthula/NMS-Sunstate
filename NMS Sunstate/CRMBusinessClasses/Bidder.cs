using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NMS_Sunstate.Healpers;
using NMS_Sunstate.Model;
using NMS_Sunstate.Resources;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NMS_Sunstate.NewFolder
{
    public class BidderBL
    {
        public static ILogger log { get; set; }
        public Bidder bidder { get; set; }
        public BidderBL(ILogger logger)
        {
            log = logger;
            bidder = new Bidder();
        }
        internal EntityCollection GetActiveRecords(IOrganizationService organizationService)
        {
            string query = GetData.GetAllActiveRecords;
            EntityCollection ec_bidders = organizationService.RetrieveMultiple(new FetchExpression(query));
            return ec_bidders;
        }
        private static EntityCollection RetrieveAllRecordsUsingFetchXML(IOrganizationService service, string fetchXML)
        {
            var moreRecords = false;
            int page = 1;
            var cookie = string.Empty;
            var entityCollection = new EntityCollection();
            do
            {
                var xml = string.Format(fetchXML, cookie);
                var collection = service.RetrieveMultiple(new FetchExpression(xml));
                if (collection.Entities.Count > 0) entityCollection.Entities.AddRange(collection.Entities);
                moreRecords = collection.MoreRecords;
                if (moreRecords)
                {
                    page++;
                    cookie = string.Format("paging-cookie='{0}' page='{1}'", System.Security.SecurityElement.Escape(collection.PagingCookie), page);
                }
            } while (moreRecords);

            return entityCollection;
        }
        internal Bidder ProcessDuplicates(IOrganizationService organizationService, EntityCollection ec_bidders)
        {
            List<Entity> list_Bidder = ec_bidders.Entities.ToList();
            int bidderNumber = 0;
            foreach (Entity e_bidder in list_Bidder)
            {
                var IsRecordInToBeDeactivated = bidder.TotalRecordsToBeDeactivated.Find(i => i.Id == e_bidder.Id);
                var IsRecordsInToBeActivated = bidder.TotalRecordsToBeActivated.Find(i => i.Id == e_bidder.Id);
                if (IsRecordInToBeDeactivated == null && IsRecordsInToBeActivated == null)
                {
                    bidderNumber++;
                    List<Entity> duplicateRecords = GetDuplicateRecords(organizationService, list_Bidder, e_bidder);
                    log.Information("Bidder " + bidderNumber/*e_bidder.Attributes["sse_factorkey"]*/ + " : " + duplicateRecords.Count());
                    var oldestrecord = duplicateRecords.OrderBy(i => i.Attributes["createdon"]).FirstOrDefault();
                    duplicateRecords.Remove(oldestrecord);
                    bidder.TotalRecordsToBeActivated.Add(oldestrecord);
                    int activecount = oldestrecord != null ? 1 : 0;
                    BidderInformation bidderInformation = new BidderInformation() { ActiveBidderCount = activecount, ActiveBidder = oldestrecord, CurentBidder = e_bidder, ToBeDeactivated = duplicateRecords, TotalDuplicates = duplicateRecords.Count + activecount };
                    bidder.bidderInformation.Add(bidderInformation);
                    bidder.TotalRecordsToBeDeactivated.AddRange(bidderInformation.ToBeDeactivated);

                    Deactivate(organizationService, bidder, bidderInformation, bidderInformation.ToBeDeactivated, bidderNumber);
                    log.Information("Bidder " + bidderNumber/*bidderInformation.CurentBidder.Attributes["sse_factorkey"]*/ + " Active  : " + bidderInformation.ActiveBidderCount);
                    log.Information("Bidder " + bidderNumber/*bidderInformation.CurentBidder.Attributes["sse_factorkey"]*/ + " Inactive : " + bidderInformation.DeactivatedBidderCount + "\n");
                }
            }
            var totalCount = bidder.bidderInformation.Sum(i => i.TotalDuplicates);
            log.Information("Total Count : " + totalCount);
            var totalActiveRecords = bidder.bidderInformation.Sum(i => i.ActiveBidderCount);
            var totalInactiveRecord = bidder.bidderInformation.Sum(i => i.DeactivatedBidderCount);
            log.Information("Total Active Records : " + totalActiveRecords);
            log.Information("Total Inactive Record : " + totalInactiveRecord + "\n\n");
            if (totalCount != totalActiveRecords + totalInactiveRecord)
            {
                foreach (var item in bidder.FailedRecord)
                {
                    log.Information(item);
                }
            }
            return bidder;
        }
        private void Deactivate(IOrganizationService organizationService, Bidder bidder, BidderInformation bidderinfo, List<Entity> ToBeDeactivated,int bidderNumber)
        {
            foreach (var record in ToBeDeactivated)
            {
                try
                {
                    Entity entity = new Entity("sse_bidder");
                    entity.Id = record.Id;
                    entity.Attributes["statecode"] = new OptionSetValue(1);
                    organizationService.Update(entity);
                    log.Information("Deactivating Bidder : " + entity.Id);
                    bidderinfo.DeactivatedBidderCount++;
                    bidderinfo.Deactivated.Add(record);
                }
                catch (Exception ex)
                {
                    // For Record Level
                    bidderinfo.NoOfRecordsFailed++;
                    bidderinfo.FailedRecord.Add("For Bidder " + bidderNumber/*bidderinfo.CurentBidder.Attributes["sse_factorkey"]*/ + " Deactivating DuplicateBidder -: " + record.Id);

                    // Aggregate
                    bidder.NoOfRecordsFailed++;
                    bidder.FailedRecord.Add("For Bidder " + bidderNumber/*bidderinfo.CurentBidder.Attributes["sse_factorkey"]*/ + " Deactivating DuplicateBidder -: " + record.Id);
                }
            }
        }
        private List<Entity> GetDuplicateRecords(IOrganizationService organizationService, List<Entity> records, Entity bidder)
        {
            var address = string.Empty;
            var company = string.Empty;
            var phoneNumber = string.Empty;
            var factorKey = string.Empty;
            //var factorKey = Convert.ToString(bidder.Attributes["sse_factorkey"]);
            if (bidder.Attributes.ContainsKey("sse_factorkey"))
            {
                factorKey = Convert.ToString(bidder.Attributes["sse_factorkey"]);
            }
            if (bidder.Attributes.ContainsKey("sse_dodge_companyname"))
            {
                company = Convert.ToString(bidder.Attributes["sse_dodge_companyname"]);
                company = HttpUtility.HtmlEncode(company);
            }
            if (bidder.Attributes.ContainsKey("sse_dodge_streetaddress"))
            {
                address = Convert.ToString(bidder.Attributes["sse_dodge_streetaddress"]);
                address = HttpUtility.HtmlEncode(address);
            }
            if(bidder.Attributes.ContainsKey("sse_dodge_contactphone"))
            {
                phoneNumber = Convert.ToString(bidder.Attributes["sse_dodge_contactphone"]);
            }
            string query = String.Format(GetData.GetDuplicateRecords, factorKey, company, address,phoneNumber);

            EntityCollection ec = RetrieveAllRecordsUsingFetchXML(organizationService, query);
            List<Entity> duplicateRecords = ec.Entities.ToList();
            return duplicateRecords;
        }
    }
}



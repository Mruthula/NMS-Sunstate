using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NMS_Sunstate.Healpers;
using NMS_Sunstate.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS_Sunstate.NewFolder
{
    public class Bidder
    {
        public EntityCollection GetActiveRecords(IOrganizationService organizationService)
        {
            //string factorkey = "textboxinput";
            string query = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='sse_bidder'>
                        <attribute name='sse_name' />
                        <attribute name='createdon' />
                        <attribute name='sse_bidderid' />
                        <attribute name='sse_dodge_contactphone' />
                        <attribute name='sse_factorkey' />
                        <attribute name='sse_dodge_companyname' />
                        <attribute name='sse_dodge_streetaddress' />
	                    <attribute name='sse_streetaddress' />
                        <attribute name='sse_stateorprovince' />
                        <attribute name='sse_projectcontact' />
                        <attribute name='sse_city' />
                        <attribute name='sse_businessphone' />
                        <order attribute='createdon' descending='true' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                        </filter>
                      </entity>
                    </fetch>";
            //string.Format(query, factorkey);

            //EntityCollection ec = RetrieveAllRecordsUsingFetchXML(organizationService, query);
            EntityCollection ec = organizationService.RetrieveMultiple(new FetchExpression(query));
            return ec;
        }


        public static EntityCollection RetrieveAllRecordsUsingFetchXML(IOrganizationService service, string fetchXML)
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



        public void Duplicate(IOrganizationService organizationService, EntityCollection records)
        {
            List<Entity> listOfBidder = records.Entities.ToList();
            foreach (Entity bidder in listOfBidder)
            {

                List<Entity> duplicateRecords = GetDuplicateRecords(organizationService, listOfBidder, bidder);
            }
        }
        int bidderNumber = 1;
        private List<Entity> GetDuplicateRecords(IOrganizationService organizationService, List<Entity> records, Entity bidder)
        {
            var factorKey = bidder.Attributes["sse_factorkey"];
            var company = bidder.Attributes["sse_dodge_companyname"];
            var address = bidder.Attributes["sse_dodge_streetaddress"];
            string query = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='sse_bidder'>
    <attribute name='sse_name' />
    <attribute name='createdon' />
    <attribute name='sse_streetaddress' />
    <attribute name='sse_stateorprovince' />
    <attribute name='sse_projectcontact' />
    <attribute name='sse_city' />
    <attribute name='sse_businessphone' />
    <attribute name='sse_action' />
    <attribute name='statuscode' />
    <attribute name='ownerid' />
    <attribute name='sse_bidderid' />
    <attribute name='sse_factorkey' />
    <attribute name='sse_dodge_companyname' />
    <attribute name='sse_dodge_streetaddress' />
    <order attribute='createdon' descending='true' />
    <filter type='and'>
      <filter type='or'>
        <condition attribute='sse_factorkey' operator='eq' value='"+factorKey+@"' />
        <filter type='and'>
          <condition attribute='sse_dodge_companyname' operator='eq' value='"+ company+ @"' />
          <condition attribute='sse_dodge_streetaddress' operator='eq' value='"+address+@"' />
        </filter>
      </filter>
    </filter>
  </entity>
</fetch>";

            EntityCollection ec = RetrieveAllRecordsUsingFetchXML(organizationService, query);
            Logger log = new Logger();
            log.Log("Bidder " + bidderNumber + " : " + ec.Entities.Count);
            ///call FindOldest()
            /// call method for delete all other from list and inactivate them
            List<Entity> duplicateRecords = ec.Entities.ToList();

            return duplicateRecords;
        }
    }
}

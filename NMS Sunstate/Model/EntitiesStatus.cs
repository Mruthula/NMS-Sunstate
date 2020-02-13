using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS_Sunstate.Model
{
    public class Bidder
    {
        public List<BidderInformation> bidderInformation { get; set; } = new List<BidderInformation>();
        public int NoOfRecordsFailed { get; set; }
        public List<string> FailedRecord { get; set; } = new List<string>();
        public List<Entity> TotalRecordsToBeDeactivated { get; set; } = new List<Entity>();

        public List<Entity> TotalRecordsToBeActivated { get; set; } = new List<Entity>();
    }

    public class BidderInformation
    {
        public Entity CurentBidder { get; set; }
        public int ActiveBidderCount { get; set; }
        public Entity ActiveBidder { get; set; }
        public List<Entity> ToBeDeactivated { get; set; }
        public List<Entity> Deactivated { get; set; } = new List<Entity>();
        public int DeactivatedBidderCount { get; set; }
        public int TotalDuplicates { get; set; }
        public int NoOfRecordsFailed { get; set; }
        public List<string> FailedRecord { get; set; }
    }
}

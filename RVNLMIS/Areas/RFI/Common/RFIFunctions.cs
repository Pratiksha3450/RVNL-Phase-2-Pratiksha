using RVNLMIS.DAC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Areas.RFI.Common
{
    public class RFIFunctions
    {
        public static void AddTimelineActivity(int rfiId, string actText, string statusClass, string attachType, string enclAutoId)
        {
            using (dbRVNLMISEntities dbContext = new dbRVNLMISEntities())
            {
                tblRFITimelineActivity objAdd = new tblRFITimelineActivity();
                objAdd.RFIId = rfiId;
                objAdd.TimelineActText = actText;
                objAdd.StatusClass = statusClass;
                objAdd.TimelineActDate = DateTime.Now;
                objAdd.AttachAutoId = enclAutoId.ToString();
                objAdd.AttachmentType = attachType;

                dbContext.tblRFITimelineActivities.Add(objAdd);
                dbContext.SaveChanges();
            }
        }
    }
}
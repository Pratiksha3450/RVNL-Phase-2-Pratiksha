using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RVNLMIS.DAC;

namespace RVNLMIS.API
{
    public class DataCleanUpApiController : ApiController
    {

        /// <summary>
        /// Removes the expired data.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string RemoveExpiredData()
        {
            try
            {
                DateTime oldDate = DateTime.Now.AddDays(30);
                
                using (var db= new dbRVNLMISEntities()  )
                {
                    #region --- Remove 30 day old soft deleted records from tblstripProgress ----
                    
                    var objstripProgress = db.tblStripPkgProgresses.Where(o => o.isDeleted == true && o.ModifiedOn < oldDate).ToList();
                    if (objstripProgress.Count>0)
                    {                        
                        db.tblStripPkgProgresses.RemoveRange(objstripProgress);

                        tblDeletedRecordLog obj = new tblDeletedRecordLog();
                        obj.TableName = "tblStripPkgProgress";
                        obj.NoOfRecords = objstripProgress.Count;
                        obj.DeletedOn = DateTime.Now;
                        obj.DeletedIds = string.Join(",", objstripProgress.Select(x => x.ActProgressId));
                        
                        db.tblDeletedRecordLogs.Add(obj);
                        db.SaveChanges();
                    }

                    #endregion --- Remove 30 day old soft deleted records from tblstripProgress ----
                }
                return "Done";
            }
            catch (Exception ex)
            {
                Logger.LogInfo(ex);
                return "Failed";
            }
        }

    }
}
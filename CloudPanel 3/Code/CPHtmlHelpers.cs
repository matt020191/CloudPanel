using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel
{
    public class CPHtmlHelpers
    {
        public static IHtmlString GetCompanyPlans(int selectedId)
        {
            var stringBuilder = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var companyPlans = (from d in db.Plans_Organization
                                    orderby d.OrgPlanName
                                    select d).ToList();

                companyPlans.ForEach(x =>
                {
                    stringBuilder.AppendFormat("<option value=\"{0}\" {1}>{2}</option>", 
                            x.OrgPlanID,
                            x.OrgPlanID == selectedId ? "selected" : "",
                            x.OrgPlanName
                        );
                });

                string returnValue = string.Format("<select id=\"{0}\" name=\"{0}\" class=\"form-control\">{1}</select>", "OrgPlanId", stringBuilder.ToString());
                return new NonEncodedHtmlString(returnValue);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}
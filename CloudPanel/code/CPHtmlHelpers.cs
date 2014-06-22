//
// Copyright (c) 2014, Jacob Dixon
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by KnowMoreIT and Compsys.
// 4. Neither the name of KnowMoreIT and Compsys nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY Jacob Dixon ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL Jacob Dixon BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.ViewEngines.Razor;
using System.Text;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Base.Config;
using log4net;

namespace CloudPanel.code
{
    public class CPHtmlHelpers
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(CPHtmlHelpers));

        public static IHtmlString GetCompanyPlans(string companyCode, int orgID)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var plans = from p in db.Plans_Organization orderby p.OrgPlanName select p;
                foreach (var p in plans)
                {
                    sb.AppendFormat("<option value='{0}' {1}>{2}</option>",
                        p.OrgPlanID,
                        p.OrgPlanID == orgID ? "selected" : "",
                        p.OrgPlanName);
                }
            }
            catch (Exception ex)
            {
                sb.AppendFormat("<option value='0' selected>{1}: {2}</option>",
                    "ERROR",
                    ex.Message);

                log.ErrorFormat("Error getting company plans for {0}. Error: {1}", companyCode, ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }

            var htmlSelectBox = string.Format("<select id='{0}' name='{1}' class='form-control'>{2}{3}</select>",
                "OrgPlanID",
                "OrgPlanID",
                "<option value='0'> --- Select Plan --- </option>",
                sb.ToString());

            return new NonEncodedHtmlString(htmlSelectBox);
        }
    }
}
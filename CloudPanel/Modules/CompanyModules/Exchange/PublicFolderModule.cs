using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class PublicFolderModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PublicFolderModule() : base("/company/{CompanyCode}/exchange/publicfolders")
        {
            Get["/"] = _ =>
                {
                    #region Returns the public folder v iew
                    string companyCode = _.CompanyCode;
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var pfPlan = db.Companies
                                        .Where(x => x.CompanyCode == companyCode)
                                        .Single()
                                        .ExchPFPlan;

                        if (Settings.ExchangeVersion == 2010)
                            return Negotiate.WithModel(new { plan = pfPlan })
                                            .WithView("Company/Exchange/publicfolder_2010.cshtml");
                        else
                            return Negotiate.WithModel(new { plan = pfPlan })
                                            .WithView("Company/Exchange/publicfolder_2013.cshtml");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting public folder view for {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Post["/", c => (Settings.ExchangeVersion == 2010)] = _ =>
                {
                    string companyCode = _.CompanyCode;
                    CloudPanelContext db = null;
                    ADGroups groups = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var company = db.Companies
                                        .Where(x => x.CompanyCode == companyCode)
                                        .Single();
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error modifying public folders for {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (groups != null)
                            groups.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                };
        }
    }
}
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.IO;

namespace CloudPanel.Modules
{
    public class BrandingModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BrandingModule));

        public static List<Branding> AllBrandings;

        public BrandingModule() : base("/admin/brandings")
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin" });

            Get["/new"] = _ =>
            {
                #region Gets the page to create a new branding
                try
                {
                    return Negotiate.WithModel(new { brandings = AllBrandings, branding = new Branding() })
                                    .WithView("Admin/brandings.cshtml")
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error loading company brandings: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                #endregion
            };

            Post["/new"] = _ =>
            {
                #region Creates a new branding
                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Opening database connection to create new branding");
                    db = new CloudPanelContext(Settings.ConnectionString);

                    logger.DebugFormat("Binding to form");
                    var branding = this.Bind<Branding>(new[] { "LoginLogo", "HeaderLogo" } );

                    logger.DebugFormat("Saving images...");
                    var files = Request.Files;
                    foreach (var f in files) 
                    {
                        string ext = f.Name.Split('.').Last().ToString();

                        var newFileName = string.Format("{0}.{1}", Guid.NewGuid(), ext);
                        var rootPath = string.Format(@"{0}Content\img\branding\{1}", HostingEnvironment.MapPath("~/"), branding.HostName);
                        var fullPath = Path.Combine(rootPath, newFileName);

                        logger.DebugFormat("Cheacking that path {0} exists", rootPath);
                        if (!Directory.Exists(rootPath))
                            Directory.CreateDirectory(rootPath);

                        logger.DebugFormat("Writing file {0}", fullPath);
                        using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
                        {
                            f.Value.CopyTo(fs);
                        }

                        logger.DebugFormat("Settings values to save to database");
                        if (f.Key.Equals("LoginLogo", StringComparison.InvariantCultureIgnoreCase))
                            branding.LoginLogo = string.Format("~/Content/img/branding/{0}/{1}", branding.HostName, newFileName);
                        else
                            branding.HeaderLogo = string.Format("~/Content/img/branding/{0}/{1}", branding.HostName, newFileName);
                    }

                    if (branding.HeaderLogo == null)
                        branding.HeaderLogo = "";

                    if (branding.LoginLogo == null)
                        branding.LoginLogo = "";

                    logger.DebugFormat("Saving to the database");
                    db.Brandings.Add(branding);
                    db.SaveChanges();

                    logger.DebugFormat("Reloading brandings");
                    LoadAllBrandings();

                    return Negotiate.WithModel(new { brandings = AllBrandings, branding = branding, success = "Successfully created new branding" })
                                    .WithView("Admin/brandings.cshtml")
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating new branding: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                #endregion
            };

            Get["/{BrandingID:int}"] = _ =>
            {
                #region Gets a specific branding
                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Retrieving branding {0} from the database", _.BrandingID);
                    db = new CloudPanelContext(Settings.ConnectionString);

                    int id = _.BrandingID;
                    var branding = (from d in db.Brandings
                                    where d.BrandingID == id
                                    select d).FirstOrDefault();

                    if (branding == null)
                        throw new Exception("Unable to find branding in database.");
                    else
                    {
                        return Negotiate.WithModel(new { brandings = AllBrandings, branding = branding })
                                        .WithView("Admin/brandings.cshtml")
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error loading company brandings: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                #endregion
            };

            Post["/{BrandingID:int}"] = _ =>
            {
                #region Updates an existing branding
                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Opening database connection to update branding");
                    db = new CloudPanelContext(Settings.ConnectionString);

                    logger.DebugFormat("Loading branding from database");
                    int id = _.BrandingID;
                    var oldBranding = (from d in db.Brandings
                                    where d.BrandingID == id
                                    select d).FirstOrDefault();

                    logger.DebugFormat("Binding to form");
                    var newBranding = this.Bind<Branding>(new[] { "LoginLogo", "HeaderLogo" });
                    oldBranding.Name = newBranding.Name;
                    oldBranding.Phone = newBranding.Phone;
                    oldBranding.Email = newBranding.Email;
                    oldBranding.Theme = newBranding.Theme;
                    oldBranding.MenuType = newBranding.MenuType;

                    logger.DebugFormat("Checking if images were changed");
                    var files = Request.Files;
                    if (files != null)
                    {
                        logger.DebugFormat("There were some images that changed");
                        foreach (var f in files)
                        {
                            string ext = f.Name.Split('.').Last().ToString();

                            var newFileName = string.Format("{0}.{1}", Guid.NewGuid(), ext);
                            var rootPath = string.Format(@"{0}Content\img\branding\{1}", HostingEnvironment.MapPath("~/"), oldBranding.HostName);
                            var fullPath = Path.Combine(rootPath, newFileName);

                            logger.DebugFormat("Cheacking that path {0} exists", rootPath);
                            if (!Directory.Exists(rootPath))
                                Directory.CreateDirectory(rootPath);

                            logger.DebugFormat("Writing file {0}", fullPath);
                            using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
                            {
                                f.Value.CopyTo(fs);
                            }

                            logger.DebugFormat("Settings values to save to database");
                            if (f.Key.Equals("LoginLogo", StringComparison.InvariantCultureIgnoreCase))
                                oldBranding.LoginLogo = string.Format("~/Content/img/branding/{0}/{1}", oldBranding.HostName, newFileName);
                            else
                                oldBranding.HeaderLogo = string.Format("~/Content/img/branding/{0}/{1}", oldBranding.HostName, newFileName);
                        }
                    }

                    if (newBranding.HeaderLogo == null)
                        oldBranding.HeaderLogo = "";

                    if (newBranding.LoginLogo == null)
                        oldBranding.LoginLogo = "";

                    logger.DebugFormat("Saving to the database");
                    db.SaveChanges();

                    logger.DebugFormat("Reloading brandings");
                    LoadAllBrandings();

                    return Negotiate.WithModel(new { brandings = AllBrandings, branding = oldBranding, success = "Successfully updated branding" })
                                    .WithView("Admin/brandings.cshtml")
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating new branding: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                #endregion
            };

            Delete["/{BrandingID:int}"] = _ =>
            {
                #region Deletes a branding
                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Opening database connection to delete branding");
                    db = new CloudPanelContext(Settings.ConnectionString);

                    int id = _.BrandingID;
                    var branding = (from d in db.Brandings
                                    where d.BrandingID == id
                                    select d).FirstOrDefault();

                    logger.DebugFormat("Deleting branding {0}", id);
                    db.Brandings.Remove(branding);
                    db.SaveChanges();

                    logger.DebugFormat("Reloading brandings");
                    LoadAllBrandings();

                    return Negotiate.WithModel(new { brandings = AllBrandings, branding = branding, success = "Successfully deleted branding" })
                                    .WithView("Admin/brandings.cshtml")
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error deleting branding: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                #endregion
            };
        }

        /// <summary>
        /// Loads all brandings
        /// </summary>
        public static List<Branding> LoadAllBrandings()
        {
            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Retrieving brandings from database with connection string {0}...", Settings.ConnectionString);
                db = new CloudPanelContext(Settings.ConnectionString);

                var brandings =  (from d in db.Brandings
                                 orderby d.HostName
                                 select d).ToList();

                if (brandings != null)
                {
                    logger.DebugFormat("Found a total of {0} brandings", brandings.Count());
                    AllBrandings = brandings.ToList();
                }
                else
                {
                    logger.DebugFormat("There were no brandings found");
                    AllBrandings = new List<Branding>();
                }

                return AllBrandings;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error loading company brandings: {0}", ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        /// <summary>
        /// Gets a specific brandings from what is in memory
        /// </summary>
        /// <param name="httpHeader"></param>
        /// <returns></returns>
        public static Branding GetBranding(string httpHeader)
        {
            logger.DebugFormat("Processing branding request for {0}", httpHeader);

            var returnBranding = DefaultBranding();
            if (AllBrandings != null)
            {
                var brandingExist = (from b in AllBrandings
                                     where b.HostName.Equals(httpHeader, StringComparison.InvariantCultureIgnoreCase)
                                     select b).FirstOrDefault();

                if (brandingExist != null)
                    return brandingExist;
                else
                    logger.DebugFormat("Branding request for {0} was not found. Returning default.", httpHeader);
            }
            else
                logger.DebugFormat("No brandings have been defined");

            return returnBranding;
        }

        /// <summary>
        /// Static branding for CloudPanel
        /// </summary>
        /// <returns></returns>
        private static Branding DefaultBranding()
        {
            return new Branding()
            {
                Name = "CloudPanel",
                Phone = "1 (501) 758-6818",
                Email = "jdixon@knowmoreit.com",
                HeaderLogo = "",
                LoginLogo = "~/Content/img/logo-big.png",
                Theme = "theme-default.css"
            };
        }
    }
}
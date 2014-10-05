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

        public BrandingModule() : base("/settings")
        {
            //this.RequiresAuthentication();
            //this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin" });

            Get["/brandings/new"] = _ =>
            {
                try
                {
                    return Negotiate.WithModel(new { brandings = AllBrandings, branding = new Branding() })
                                    .WithView("ADmin/brandings.cshtml")
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error loading company brandings: {0}", ex.ToString());

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };

            Post["/brandings/new"] = _ =>
            {
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

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };

            Get["/brandings/{BrandingID:int}"] = _ =>
            {
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

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };

            Post["/brandings/{BrandingID:int}"] = _ =>
            {
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

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };

            Delete["/brandings/{BrandingID:int}"] = _ =>
            {
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

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
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
                logger.DebugFormat("Retrieving brandings from datbase...");
                db = new CloudPanelContext(Settings.ConnectionString);
                var brandings =  from d in db.Brandings
                                 orderby d.HostName
                                 select d;

                if (brandings != null)
                    AllBrandings = brandings.ToList();

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
                Email = "support@compsys.com",
                HeaderLogo = "~/Content/img/logo.png",
                LoginLogo = "~/Content/img/logo-big.png",
                Theme = "theme-default.css"
            };
        }
    }
}
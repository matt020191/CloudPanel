using CloudPanel.Base.Config;
using CloudPanel.Citrix;
using CloudPanel.Code;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CloudPanel.Modules.Citrix
{
    public class CitrixModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Citrix");

        public CitrixModule() : base("/citrix")
        {
            Get["/group/{UUID:Guid}/sessions"] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });

                Guid uuid = _.UUID;
                #region Gets the sessions for a specific desktop group
                XenDesktop7 xd7 = null;
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var desktopGroup = db.CitrixDesktopGroup.Where(x => x.UUID == uuid).First();

                    xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);
                    var sessions = xd7.GetSessionsByDesktopGroup(desktopGroup.Uid);

                    int draw = 0, start = 0, length = 0, recordsTotal = sessions.Count, recordsFiltered = sessions.Count, orderColumn = 0;
                    string searchValue = "", orderColumnName = "";
                    bool isAscendingOrder = true;

                    if (Request.Query.draw.HasValue)
                    {
                        draw = Request.Query.draw;
                        start = Request.Query.start;
                        length = Request.Query.length;
                        orderColumn = Request.Query["order[0][column]"];
                        searchValue = Request.Query["search[value]"].HasValue ? Request.Query["search[value]"] : string.Empty;
                        isAscendingOrder = Request.Query["order[0][dir]"] == "asc" ? true : false;
                        orderColumnName = Request.Query["columns[" + orderColumn + "][data]"];

                        // See if we are using dataTables to search
                        if (!string.IsNullOrEmpty(searchValue))
                        {
                            sessions = (from d in sessions
                                        where d.UserName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                        select d).ToList();
                            recordsFiltered = sessions.Count;
                        }

                        if (isAscendingOrder)
                            sessions = sessions.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take((length > 0 ? length : sessions.Count))
                                                    .ToList();
                        else
                            sessions = sessions.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take((length > 0 ? length : sessions.Count))
                                                    .ToList();
                    }

                    logger.DebugFormat("Completed getting citrix session data");
                    return Negotiate.WithModel(new
                    {
                        draw = draw,
                        recordsTotal = recordsTotal,
                        recordsFiltered = recordsFiltered,
                        data = sessions
                    });
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting citrix session data: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (xd7 != null)
                        xd7.Dispose();
                }
                #endregion
            };

            Get["/desktop/{DESKTOP:int}/sessions"] = _ =>
            {
                this.RequiresClaims(new[] { "SuperAdmin" });

                int desktopid = _.DESKTOP;
                #region Gets the sessions for a specific desktop
                XenDesktop7 xd7 = null;
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var desktop = db.CitrixDesktop
                                    .Where(x => x.Uid == desktopid)
                                    .First();

                    xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);
                    var sessions = xd7.GetSessionsByDesktop(desktop.Uid);

                    int draw = 0, start = 0, length = 0, recordsTotal = sessions.Count, recordsFiltered = sessions.Count, orderColumn = 0;
                    string searchValue = "", orderColumnName = "";
                    bool isAscendingOrder = true;

                    if (Request.Query.draw.HasValue)
                    {
                        draw = Request.Query.draw;
                        start = Request.Query.start;
                        length = Request.Query.length;
                        orderColumn = Request.Query["order[0][column]"];
                        searchValue = Request.Query["search[value]"].HasValue ? Request.Query["search[value]"] : string.Empty;
                        isAscendingOrder = Request.Query["order[0][dir]"] == "asc" ? true : false;
                        orderColumnName = Request.Query["columns[" + orderColumn + "][data]"];

                        // See if we are using dataTables to search
                        if (!string.IsNullOrEmpty(searchValue))
                        {
                            sessions = (from d in sessions
                                        where d.UserName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                        select d).ToList();
                            recordsFiltered = sessions.Count;
                        }

                        if (isAscendingOrder)
                            sessions = sessions.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take((length > 0 ? length : sessions.Count))
                                                    .ToList();
                        else
                            sessions = sessions.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take((length > 0 ? length : sessions.Count))
                                                    .ToList();
                    }

                    logger.DebugFormat("Completed getting citrix session data");
                    return Negotiate.WithModel(new
                    {
                        draw = draw,
                        recordsTotal = recordsTotal,
                        recordsFiltered = recordsFiltered,
                        data = sessions
                    });
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting citrix session data: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (xd7 != null)
                        xd7.Dispose();
                }
                #endregion
            };

            Post["/logoff"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eCitrix"));

                #region Logs off users
                XenDesktop7 xd7 = null;
                try
                {
                    if (!Request.Form["values[]"].HasValue)
                        throw new MissingFieldException("", "values[]");

                    xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);

                    string[] values = Request.Form["values[]"].Value.Split(',');
                    xd7.LogOffSessionsBySessionKeys(values);

                    return Negotiate.WithModel(new { success = "Successfully sent command to log off users" })
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error logging off users: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (xd7 != null)
                        xd7.Dispose();
                }
                #endregion
            };

            Post["/sendmessage"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eCitrix"));

                #region Send message to users
                XenDesktop7 xd7 = null;
                try
                {
                    if (!Request.Form.Title.HasValue)
                        throw new MissingFieldException("", "Title");

                    if (!Request.Form.MessageStyle.HasValue)
                        throw new MissingFieldException("", "MessageStyle");

                    if (!Request.Form.Message.HasValue)
                        throw new MissingFieldException("", "Message");

                    if (!Request.Form["SessionKeys"].HasValue)
                        throw new MissingFieldException("", "SessionKeys");

                    xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);

                    string[] sessionKeys = Request.Form["SessionKeys"].Value.Split(',');
                    xd7.SendMessageBySessionKeys(sessionKeys, 
                                                 Request.Form.MessageStyle.Value, 
                                                 Request.Form.Title.Value, 
                                                 Request.Form.Message.Value);

                    return Negotiate.WithModel(new { success = "Successfully sent message to users" })
                                    .WithStatusCode(HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error sending message to users: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (xd7 != null)
                        xd7.Dispose();
                }
                #endregion
            };
        }
    }
}
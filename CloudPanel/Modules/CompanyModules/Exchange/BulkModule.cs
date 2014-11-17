﻿using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class BulkModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(BulkModule));

        private enum ActionToTake
        {
            Enable = 0,
            Change = 2,
            Disable = 1
        }

        public enum EmailFormat
        {
            Username = 0,
            Firstname = 1,
            Lastname = 2,
            FirstNameDotLastname = 3,
            FirstnameLastname =4,
            LastnameDotFirstname = 5,
            LastnameFirstname = 6,
            FirstInitialLastname = 7,
            LastnameFirstInitial = 8
        }

        public BulkModule() : base("/company/{CompanyCode}/exchange/bulk")
        {
            this.RequiresAuthentication();

            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                return View["Company/Exchange/Bulk.cshtml"];
            };

            Post["/"] = _ =>
            {
                try
                {
                    string companyCode = _.CompanyCode;

                    if (!Request.Form.CheckedUsers.HasValue)
                        throw new Exception("No users were selected.");

                    if (!Request.Form.ActionToTake.HasValue)
                        throw new Exception("No action was selected.");

                    logger.DebugFormat("Unparsed users is {0}", Request.Form.CheckedUsers.Value);
                    string users = Request.Form.CheckedUsers.Value;
                    string[] userPrincipalName = users.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    ActionToTake action = Enum.Parse(typeof(ActionToTake), Request.Form.ActionToTake.Value);
                    switch (action)
                    {
                        case ActionToTake.Disable:
                            logger.DebugFormat("Disable action was taken");
                            break;
                        default:
                            throw new Exception("Unknown action was specified");
                    }


                    return Negotiate.WithModel(new { success = "" })
                                    .WithView("error.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error bulk updating mailboxes: {0}", ex.ToString());

                    ViewBag.error = ex.ToString();
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml");
                }
                finally
                {

                }
            };
        }

        private void DisableMailboxes(string[] userPrincipalNames)
        {

        }
    }
}
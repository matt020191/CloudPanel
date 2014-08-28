using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Exceptions;
using CloudPanel.code;
using log4net;
using Nancy;
using Nancy.ModelBinding;
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
using System.Dynamic;

namespace CloudPanel.modules
{
    public class PlansModule : NancyModule
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(PlansModule));

        public PlansModule() : base("/plans")
        {
            #region Organization / Company Plans

            Get["/company"] = _ =>
                {
                    return View["p_organization.cshtml"];
                };

            Post["/company"] = _ =>
                {
                    try
                    {
                        Plans_Organization p = this.Bind<Plans_Organization>();
                        
                        Plans plans = new Plans();
                        if (p.OrgPlanID > 0)
                        {
                            log.DebugFormat("Updating existing organization plan {0}", p.OrgPlanID);
                            plans.Update_Organization(p.OrgPlanID, p);
                        }
                        else
                        {
                            log.DebugFormat("Creating new organization plan {0}", p.OrgPlanName);
                            plans.Create_Organization(p);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error posting new organization plan. Error {0}", ex.ToString());
                        ViewBag.Error = ex.Message;
                    }

                    return View["p_organization.cshtml"];
                };

            Delete["/company"] = _ =>
                {
                    try
                    {
                        Plans_Organization p = this.Bind<Plans_Organization>();

                        Plans plans = new Plans();
                        plans.Delete_Organization(p.OrgPlanID);
                    }
                    catch (MatchFoundException ex)
                    {
                        ViewBag.Warning = "Multiple companies are using this plan. Unable to delete.";
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error posting new organization plan. Error {0}", ex.ToString());
                        ViewBag.Error = ex.Message;
                    }

                    return View["p_organization.cshtml"];
                };

            #endregion

            #region Mailbox Plans

            Get["/mailbox"] = _ =>
                {
                    return View["p_mailbox.cshtml"];
                };

            Get["/mailbox/{MailboxPlanID}/json"] = _ =>
                {
                    try
                    {
                        Plans plans = new Plans();
                        Plans_ExchangeMailbox p = plans.Get_MailboxPlan(_.MailboxPlanID);

                        return Response.AsJson(p, HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving mailbox plan {0}. Error: {1}", _.MailboxPlanID, ex.ToString());
                        return HttpStatusCode.InternalServerError;
                    }
                };

            #endregion

            #region Activesync Plans
            Get["/activesync"] = _ =>
                {
                    var thing = View["p_activesync.cshtml", new {
                        Plans = new[] { new { Name = "Chicken", ID = "12345" }.ToExpando() },
                        DisplayName = "Ploob",
                        Description = "Description",
                        RefreshIntervalInHours = "3",
                        AllowNonProvisionableDevices = true,
                        RequirePassword = true,
                        RequireAlphanumericPassword = false,
                        MinCharacterSets = "4",
                        EnablePasswordRecovery = true,
                        RequireEncryptionOnDevice = false,
                        RequireEncryptionOnStorageCard = true,
                        AllowSimplePassword = true,
                        NumberOfFailedAttempted = "50",
                        MinimumPasswordLength = "5",
                        InactivityTimeoutInMinutes = "15",
                        PasswordExpirationInDays = "30",
                        EnforcePasswordHistory = false,
                        IncludePastCalendarItems = "TwoWeeks",
                        IncludePastEmailItems = "All",
                        AllowDirectPushWhenRoaming = true,
                        LimitEmailSizeInKB = "10000",
                        AllowHTMLEmail = true,
                        AllowAttachmentsDownload = false,
                        MaximumAttachmentSizeInKB = "8000",
                        AllowRemovableStorage = true,
                        AllowCamera = true,
                        AllowWiFi = true,
                        AllowInfrared = true,
                        AllowInternetSharing = true,
                        AllowRemoteDesktop = true,
                        AllowDesktopSync = true,
                        AllowTextMessaging = true,
                        AllowBluetooth = "HandsfreeOnly",
                        AllowBrowser = true,
                        AllowConsumerMail = true,
                        AllowUnsignedApplications = true,
                        AllowUnsignedInstallationPackages = true,
                    }.ToExpando()];

                    return thing;
                };
            #endregion

        }
    }

    public static class AnonymousTypeMixins
    {
        public static ExpandoObject ToExpando(this object obj)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var info in obj.GetType().GetProperties())
                expando.Add(new KeyValuePair<string, object>(info.Name, info.GetValue(obj)));
            return (ExpandoObject)expando;
        }
    }
}
using CloudPanel.Base.Branding;
using log4net;
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
using System.Web;
using System.Xml.Linq;

namespace CloudPanel
{
    public class BrandingsRequest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BrandingsRequest));

        internal static readonly string path = HttpContext.Current.Server.MapPath("~/config/branding.xml");

        /// <summary>
        /// Loads the brandings from the config file
        /// </summary>
        public static void RetrieveBrandings()
        {
            try
            {
                XDocument xDoc = XDocument.Load(path);

                /* Load Brandings */
                log.Debug("Retrieving brandings from the config file");
                var brandings = from s in xDoc.Descendants("branding").Elements()
                                select s;

                // Clear our current static brandings
                Brandings._staticBrandings = new List<BrandingSettings>();

                // Everything here should be a base URL or default
                foreach (var b in brandings)
                {
                    log.DebugFormat("Found host {0} in brandings", b.Name.LocalName);

                    BrandingSettings newBranding = new BrandingSettings();
                    newBranding.DisplayName = b.Element("DisplayName").Value;
                    newBranding.CopyrightNotice = b.Element("Copyright").Value;
                    newBranding.LoginTitle = b.Element("LoginTitle").Value;
                    newBranding.LoginMessage = b.Element("LoginMessage").Value;
                    newBranding.SupportNumber = b.Element("SupportNumber").Value;
                    newBranding.BaseURL = b.Name.LocalName;

                    if (b.Name.LocalName == "default")
                        newBranding.IsDefault = true;

                    Brandings._staticBrandings.Add(newBranding);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error retrieving brandings from config file. Error: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Retrieves the branding based on the url
        /// </summary>
        /// <param name="baseurl"></param>
        /// <returns></returns>
        public static BrandingSettings RetrieveBranding(dynamic urlHelper)
        {
            string baseurl = urlHelper.RenderContext.Context.Request.Url.HostName;
            log.DebugFormat("Base URL for retrieving branding is {0}", baseurl);

            // Find a branding with matching base url
            var foundBranding = (from b in Brandings._staticBrandings
                                 where b.BaseURL == baseurl
                                 select b).FirstOrDefault();

            if (foundBranding == null)
                return RetrieveDefaultBranding();
            else
                return foundBranding;
        }

        /// <summary>
        /// Returns the default branding if it couldn't find a matching URL
        /// </summary>
        /// <returns></returns>
        private static BrandingSettings RetrieveDefaultBranding()
        {
            var defaultBranding = (from b in Brandings._staticBrandings
                                   where b.IsDefault
                                   select b).FirstOrDefault();

            if (defaultBranding != null)
                return defaultBranding;
            else
            {
                // Seems we can't find the default branding so we will compile a new one at runtime
                BrandingSettings branding = new BrandingSettings();
                branding.DisplayName = "CloudPanel";
                branding.CopyrightNotice = "";
                branding.LoginTitle = "Welcome to CloudPanel";
                branding.LoginMessage = "Please login";

                // Add to the list for next time
                Brandings._staticBrandings.Add(branding);

                return branding;
            }
        }
    }
}
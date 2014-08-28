using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
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

namespace CloudPanel
{
    public class GetData
    {
        /// <summary>
        /// Gets a list of all resellers
        /// </summary>
        /// <returns>Returns a collection of Company objects that contain reseller data or null</returns>
        public static List<Company> GetResellers()
        {
            List<Company> returnObject = null;
            using (var db = new CloudPanelContext(Settings.ConnectionString))
            {
                db.Database.Connection.Open();

                returnObject = (from c in db.Companies
                                where c.IsReseller
                                orderby c.CompanyName
                                select c).ToList();

                foreach (var reseller in returnObject)
                {
                    reseller.TotalCompanies = (from t in db.Companies
                                               where t.ResellerCode == reseller.CompanyCode
                                               select t.CompanyId).Count();
                }
            }
            return returnObject;
        }

        /// <summary>
        /// Gets a specific reseller
        /// </summary>
        /// <param name="resellerCode">The reseller code to search for</param>
        /// <returns>Returns a Company object that contain reseller data or null</returns>
        public static Company GetReseller(string resellerCode)
        {
            Company returnObject = null;
            using (var db = new CloudPanelContext(Settings.ConnectionString))
            {
                returnObject = (from c in db.Companies
                                where c.IsReseller
                                orderby c.CompanyName
                                select c).FirstOrDefault();
            }
            return returnObject;
        }

        /// <summary>
        /// Gets a list of all companies
        /// </summary>
        /// <returns>Returns a collection of Company objects that contain company data or null</returns>
        public static List<Company> GetCompanies()
        {
            List<Company> returnObject = null;
            using (var db = new CloudPanelContext(Settings.ConnectionString))
            {
                returnObject = (from c in db.Companies
                                where !c.IsReseller
                                orderby c.CompanyName
                                select c).ToList();
            }
            return returnObject;
        }

        /// <summary>
        /// Gets a list a specific reseller
        /// </summary>
        /// <param name="resellerCode">The reseller code that the company belongs to</param>
        /// <returns>Returns a collection of Company objects that contain company data or null</returns>
        public static List<Company> GetCompanies(string resellerCode)
        {
            List<Company> returnObject = null;
            using (var db = new CloudPanelContext(Settings.ConnectionString))
            {
                returnObject = (from c in db.Companies
                                where !c.IsReseller
                                where c.ResellerCode == resellerCode
                                orderby c.CompanyName
                                select c).ToList();
            }
            return returnObject;
        }

        /// <summary>
        /// Gets a specific company
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns>Returns a Company object that contains company data or null</returns>
        public static Company GetCompany(string companyCode)
        {
            Company returnObject = null;
            using (var db = new CloudPanelContext(Settings.ConnectionString))
            {
                returnObject = (from c in db.Companies
                                where !c.IsReseller
                                where c.CompanyCode == companyCode
                                orderby c.CompanyName
                                select c).FirstOrDefault();
            }
            return returnObject;
        }
    }
}
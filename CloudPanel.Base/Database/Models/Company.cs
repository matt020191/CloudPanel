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
using System.ComponentModel.DataAnnotations;

namespace CloudPanel.Base.Database.Models
{
    public partial class Company
    {
        public int CompanyId { get; set; }

        public string ResellerCode { get; set; }
        
        [Required]
        public bool IsReseller { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string CompanyCode { get; set; }

        private string _street;
        public string Street
        {
            get { return string.IsNullOrEmpty(_street) ? "" : _street; }
            set { _street = value;  }
        }

        private string _city;
        public string City
        {
            get { return string.IsNullOrEmpty(_city) ? "" : _city; }
            set { _city = value; }
        }

        private string _state;
        public string State
        {
            get { return string.IsNullOrEmpty(_state) ? "" : _state; }
            set { _state = value; }
        }

        private string _zipcode;
        public string ZipCode
        {
            get { return string.IsNullOrEmpty(_zipcode) ? "" : _zipcode; }
            set { _zipcode = value; }
        }

        private string _phonenumber;
        public string PhoneNumber
        {
            get { return string.IsNullOrEmpty(_phonenumber) ? "" : _phonenumber; }
            set { _phonenumber = value; }
        }

        private string _website;
        public string Website
        {
            get { return string.IsNullOrEmpty(_website) ? "" : _website; }
            set { _website = value; }
        }

        private string _description;
        public string Description
        {
            get { return string.IsNullOrEmpty(_description) ? "" : _description; }
            set { _description = value; }
        }

        private string _adminname;
        public string AdminName
        {
            get { return string.IsNullOrEmpty(_adminname) ? "" : _adminname; }
            set { _adminname = value; }
        }

        private string _adminemail;
        public string AdminEmail
        {
            get { return string.IsNullOrEmpty(_adminemail) ? "" : _adminemail; }
            set { _adminemail = value; }
        }

        private string _country;
        public string Country
        {
            get { return string.IsNullOrEmpty(_country) ? "" : _country; }
            set { _country = value; }
        }

        [Required]
        public string DistinguishedName { get; set; }

        [Required]
        public bool ExchEnabled { get; set; }

        public Nullable<int> OrgPlanID { get; set; }

        public System.DateTime Created { get; set; }

        public Nullable<bool> LyncEnabled { get; set; }

        public Nullable<bool> CitrixEnabled { get; set; }

        public Nullable<int> ExchPFPlan { get; set; }

        public Nullable<bool> ExchPermFixed { get; set; }

        // Not database related
        public string FullAddressFormatted
        {
            get
            {
                return string.Format("{0}<br/>{1} {2} {3}", Street, City, State, ZipCode);
            }
        }
    }
}

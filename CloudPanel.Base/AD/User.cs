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
using System.Text;

namespace CloudPanel.Base.AD
{
    public class User
    {
        /// <summary>
        /// Property: objectGUID
        /// </summary>
        public Guid ObjectGUID { get; set; }

        /// <summary>
        /// Property: distinguishedName
        /// </summary>
        public string DistinguishedName { get; set; }

        /// <summary>
        /// Property: name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Property: givenName
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Property: sn
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Property: displayName
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Property: userPrincipalName
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Property: sAMAccountName
        /// </summary>
        public string SamAccountName { get; set; }

        /// <summary>
        /// Property: streetAddress
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Property: l
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Property: st
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Property: postalCode
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Property: postOfficeBox
        /// </summary>
        public string POBox { get; set; }

        /// <summary>
        /// Property: co
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Property: c
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Property: department
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Property: company
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Property: description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Property: title
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// Property: mail
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Property: facsimileTelephoneNumber
        /// </summary>
        public string Fax { get; set; }

        /// <summary>
        /// Property: telephoneNumber
        /// </summary>
        public string TelephoneNumber { get; set; }

        /// <summary>
        /// Property: homePhone
        /// </summary>
        public string HomePhone { get; set; }

        /// <summary>
        /// Property: mobile
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Property: pager
        /// </summary>
        public string Pager { get; set; }

        /// <summary>
        /// Property: ipPhone
        /// </summary>
        public string IPPhone { get; set; }

        /// <summary>
        /// Property: physicalDeliveryOfficeName
        /// </summary>
        public string Office { get; set; }

        /// <summary>
        /// Property: info
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Property: profilePath
        /// </summary>
        public string ProfilePath { get; set; }

        /// <summary>
        /// Property: scriptPath
        /// </summary>
        public string ScriptPath { get; set; }

        /// <summary>
        /// Property: wWWHomePAge
        /// </summary>
        public string Webpage { get; set; }

        /// <summary>
        /// Property: badPwdCount
        /// </summary>
        public int BadPwdCount { get; set; }

        /// <summary>
        /// Property: userAccountControl
        /// </summary>
        public int UserAccountControl { get; set; }

        /// <summary>
        /// Property: sAMAccountType
        /// </summary>
        public int SamAccountType { get; set; }

        /// <summary>
        /// Property: badPasswordTime
        /// </summary>
        public long BadPasswordTime { get; set; }

        /// <summary>
        /// Property: pwdLastSet
        /// </summary>
        public long PwdLastSet { get; set; }

        /// <summary>
        /// Property: accountExpires
        /// </summary>
        public long AccountExpires { get; set; }

        /// <summary>
        /// Property: memberOf
        /// </summary>
        public string[] MemberOf { get; set; }

        /// <summary>
        /// Property: thumbnailPhoto
        /// </summary>
        public byte[] ImageFromAD { get; set; }
    }
}

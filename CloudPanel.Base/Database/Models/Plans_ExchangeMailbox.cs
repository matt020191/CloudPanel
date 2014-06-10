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

namespace CloudPanel.Base.Database.Models
{
    public partial class Plans_ExchangeMailbox
    {
        public int MailboxPlanID { get; set; }
        public string MailboxPlanName { get; set; }
        public Nullable<int> ProductID { get; set; }
        public string ResellerCode { get; set; }
        public string CompanyCode { get; set; }
        public int MailboxSizeMB { get; set; }
        public Nullable<int> MaxMailboxSizeMB { get; set; }
        public int MaxSendKB { get; set; }
        public int MaxReceiveKB { get; set; }
        public int MaxRecipients { get; set; }
        public bool EnablePOP3 { get; set; }
        public bool EnableIMAP { get; set; }
        public bool EnableOWA { get; set; }
        public bool EnableMAPI { get; set; }
        public bool EnableAS { get; set; }
        public bool EnableECP { get; set; }
        public int MaxKeepDeletedItems { get; set; }
        public string MailboxPlanDesc { get; set; }
        public string Price { get; set; }
        public string Cost { get; set; }
        public string AdditionalGBPrice { get; set; }
    }
}

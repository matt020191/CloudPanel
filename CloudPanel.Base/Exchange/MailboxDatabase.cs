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

namespace CloudPanel.Base.Exchange
{
    public class MailboxDatabase
    {
        public string Identity { get; set; }

        public Guid Guid { get; set; }

        public bool IsMailboxDatabase { get; set; }

        public string LogFilePrefix { get; set; }

        public string LogFolderPath { get; set; }

        public string DatabaseSize { get; set; }

        public string Server { get; set; }

        public DateTime Retrieved { get; set; }

        public long DatabaseSizeInBytes { get; set; }

        public decimal DatabaseSizeInKB
        {
            get
            {
                return DatabaseSizeInBytes > 0 ? Math.Round(decimal.Divide(DatabaseSizeInBytes, 1024), 2) : 0;
            }
        }

        public decimal DatabaseSizeInMB
        {
            get
            {
                return DatabaseSizeInKB > 0 ? Math.Round(decimal.Divide(DatabaseSizeInKB, 1024), 2) : 0;
            }
        }

        public decimal DatabaseSizeInGB
        {
            get
            {
                return DatabaseSizeInMB > 0 ? Math.Round(decimal.Divide(DatabaseSizeInMB, 1024), 2) : 0;
            }
        }
    }
}

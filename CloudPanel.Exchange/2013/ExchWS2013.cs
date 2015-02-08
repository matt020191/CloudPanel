using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Exchange._2013
{
    public class ExchWS2013 : ExchWebServices
    {
        public ExchWS2013(string uri, string username, string password, int version) : base(uri, username, password, version)
        {

        }
    }
}

using CloudPanel.Base.Exchange;
using CloudPanel.Exchange;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.Tests
{
    public class ExchangeTests : NancyModule
    {
        public ExchangeTests() :base("/test")
        {
        
        }
    }
}
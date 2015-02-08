using log4net;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Exchange
{
    public class ExchWebServices
    {
        private readonly ILog logger = LogManager.GetLogger("Exchange");

        internal ExchangeService _exchService;

        public ExchWebServices(string uri, string username, string password, int version)
        {
            logger.DebugFormat("Exchange web services called {0}, {1}, {2}", uri, username, version);

            switch (version)
            {
                case 2010:
                    _exchService = new ExchangeService(ExchangeVersion.Exchange2010_SP2);
                    break;
                case 2013:
                    _exchService = new ExchangeService(ExchangeVersion.Exchange2013);
                    break;
                case 20135:
                    _exchService = new ExchangeService(ExchangeVersion.Exchange2013_SP1);
                    break;
                default:
                    throw new InvalidOperationException("Version: " + version);
            }

            _exchService.Credentials = new WebCredentials(username, password);
            _exchService.Url = new Uri(uri);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Billing
{
    public class CustomPrice
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public decimal DefaultPrice { get; set; }

        public decimal Custom { get; set; }
    }
}

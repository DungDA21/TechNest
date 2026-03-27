using System;
using System.Collections.Generic;
using System.Text;

namespace WebsiteComputer.Models.Policy
{
    public class Guarantee
    {
        public record GuaranteeProduct()
        {
            public string id { get; set; } = "";
            public string productID { get; set; } = "";
            public DateTime dateStart { get; set; }
            public DateTime dateEnd { get; set; }
        }
    }
}

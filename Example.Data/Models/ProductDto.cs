using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Data.Models
{
    public class ProductDto
    {
        public string ProductCode { get; set; }
        public string ClientName { get; set; }
        public int ProductQuantity { get; set; }
        public decimal? ProductPrice { get; set; }
        public string? ProductName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemple.Domain.Models
{
    public record CalculatedClientProducts(ClientRegistrationName ClientRegistrationName, Product ProductPrice)
    {
        public string ProductCode { get; set; }
        public bool IsUpdated { get; set; } 
    }
}

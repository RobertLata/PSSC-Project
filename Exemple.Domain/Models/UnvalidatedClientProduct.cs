using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemple.Domain.Models
{
    public record UnvalidatedClientProduct(string ClientName, string ProductPrice, string ProductQuantity, string ProductCode, string ProductName);
}

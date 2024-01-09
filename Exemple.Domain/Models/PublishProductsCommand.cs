using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Exemple.Domain.Models.TotalPrice;

namespace Exemple.Domain.Models
{
    public record PublishProductsCommand
    {
        public PublishProductsCommand(IReadOnlyCollection<UnvalidatedClientProduct> inputProductPrice)
        {
            InputProductPrice = inputProductPrice;
        }

        public IReadOnlyCollection<UnvalidatedClientProduct> InputProductPrice { get; }
    }
}

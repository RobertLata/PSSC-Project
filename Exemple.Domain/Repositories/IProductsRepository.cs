using Exemple.Domain.Models;
using LanguageExt;
using System.Collections.Generic;
using static Exemple.Domain.Models.TotalPrice;

namespace Exemple.Domain.Repositories
{
    public interface IProductsRepository
    {
        TryAsync<List<CalculatedClientProducts>> TryGetExistingProducts();

        TryAsync<Unit> TrySaveProducts(TotalProductsPrice products);
    }
}

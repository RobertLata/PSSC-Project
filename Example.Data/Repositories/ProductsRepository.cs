using Exemple.Domain.Models;
using Exemple.Domain.Repositories;
using LanguageExt;
using Example.Data.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using static Exemple.Domain.Models.TotalPrice;
using static LanguageExt.Prelude;

namespace Example.Data.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly ProductsContext dbContext;

        public ProductsRepository(ProductsContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public TryAsync<List<CalculatedClientProducts>> TryGetExistingProducts() => async () => (await (
                          from p in dbContext.Product
                          join c in dbContext.Client on p.ProductName equals c.ClientName
                          select new { c.ClientName, p.ProductCode, p.ProductPrice, p.ProductQuantity, p.ProductName })
                          .AsNoTracking()
                          .ToListAsync())
                          .Select(result => new CalculatedClientProducts(
                                                    ClientRegistrationName: new(result.ClientName),
                                                    ProductPrice: new(result.ProductPrice ?? 0m, result.ProductQuantity))
                          { 
                            ProductCode = result.ProductCode
                          })
                          .ToList();

        public TryAsync<Unit> TrySaveProducts(TotalProductsPrice products) => async () =>
        {
            var clients = (await dbContext.Client.ToListAsync()).ToLookup(client=>client.ClientName);
            var newProducts = products.ProductList
                                    .Where(p => p.IsUpdated && p.ProductCode == "")
                                    .Select(p => new ProductDto()
                                    {
                                        ProductCode = "",
                                        ClientName = clients[p.ClientRegistrationName.Value].Single().ClientName,
                                        ProductPrice = p.ProductPrice.Price,
                                        ProductQuantity = p.ProductPrice.Quantity,
                                        ProductName = "a"

                                    });
            var updatedProducts = products.ProductList.Where(p => p.IsUpdated && p.ProductCode != "")
                                    .Select(p => new ProductDto()
                                    {
                                        ProductCode = p.ProductCode,
                                        ClientName = clients[p.ClientRegistrationName.Value].Single().ClientName,
                                        ProductPrice = p.ProductPrice.Price,
                                        ProductQuantity = p.ProductPrice.Quantity,
                                        ProductName = "a"
                                    });

            dbContext.AddRange(newProducts);
            foreach (var entity in updatedProducts)
            {
                dbContext.Entry(entity).State = EntityState.Modified;
            }

            await dbContext.SaveChangesAsync();

            return unit;
        };
    }
}

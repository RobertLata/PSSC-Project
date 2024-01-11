using Exemple.Domain.Models;
using static LanguageExt.Prelude;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Exemple.Domain.Models.TotalPrice;
using System.Threading.Tasks;

namespace Exemple.Domain
{
    public static class ProductPriceOperation
    {
        public static Task<IProductPrice> ValidateTotalPrice(Func<ClientRegistrationName, Option<ClientRegistrationName>> checkClientExists, UnvalidatedProductPrice totalPrice) =>
            totalPrice.ProductList
                      .Select(ValidateClientProducts(checkClientExists))
                      .Aggregate(CreateEmptyValatedProductsList().ToAsync(), ReduceValidProducts)
                      .MatchAsync(
                            Right: validatedPrice => new ValidatedTotalPrice(validatedPrice),
                            LeftAsync: errorMessage => Task.FromResult((IProductPrice)new InvalidTotalPrice(totalPrice.ProductList, errorMessage))
                      );

        private static Func<UnvalidatedClientProduct, EitherAsync<string, ValidatedClientProduct>> ValidateClientProducts(Func<ClientRegistrationName, Option<ClientRegistrationName>> checkClientExists) =>
            unvalidatedClientProduct => ValidateClientProduct(checkClientExists, unvalidatedClientProduct);

        private static EitherAsync<string, ValidatedClientProduct> ValidateClientProduct(Func<ClientRegistrationName, Option<ClientRegistrationName>> checkClientExists, UnvalidatedClientProduct unvalidatedProduct)=>
            from clientRegistrationName in ClientRegistrationName.TryParse(unvalidatedProduct.ClientName)
                                   .ToEitherAsync($"Invalid client registration name ({unvalidatedProduct.ClientName})")
            from clientExists in checkClientExists(clientRegistrationName)
                                   .ToEitherAsync($"Client {clientRegistrationName.Value} does not exist.")
            from totalPrice in Product.TryParseProduct(unvalidatedProduct.ProductPrice, unvalidatedProduct.ProductQuantity)
                                   .ToEitherAsync("Invalid product price or quantity")
            select new ValidatedClientProduct(clientRegistrationName, totalPrice);

        private static Either<string, List<ValidatedClientProduct>> CreateEmptyValatedProductsList() =>
            Right(new List<ValidatedClientProduct>());

        private static EitherAsync<string, List<ValidatedClientProduct>> ReduceValidProducts(EitherAsync<string, List<ValidatedClientProduct>> acc, EitherAsync<string, ValidatedClientProduct> next) =>
            from list in acc
            from nextProduct in next
            select list.AppendValidProduct(nextProduct);

        private static List<ValidatedClientProduct> AppendValidProduct(this List<ValidatedClientProduct> list, ValidatedClientProduct validProduct)
        {
            list.Add(validProduct);
            return list;
        }

        public static IProductPrice CalculateFinalProductPrice(IProductPrice totalPrice) => totalPrice.Match(
            whenUnvalidatedProductPrice: unvalidatedTotalPrice => unvalidatedTotalPrice,
            whenInvalidTotalPrice: invalidatedTotalPrice => invalidatedTotalPrice,
            whenFailedProductPrice: failedTotalPrice => failedTotalPrice,
            whenValidatedTotalPrice: CalculateFinalPrice,
            whenCalculatedTotalPrice: calculatedTotalPrice => calculatedTotalPrice,
            whenTotalProductsPrice: calculatedTotalPrice => calculatedTotalPrice
        );

        private static IProductPrice CalculateFinalPrice(ValidatedTotalPrice validTotalPrice) =>
            new CalculatedTotalPrice(validTotalPrice.ProductList
                                                    .Select(CalculateClientFinalPrice)
                                                    .ToList()
                                                    .AsReadOnly());

        private static CalculatedClientProducts CalculateClientFinalPrice(ValidatedClientProduct validProduct) => 
            new CalculatedClientProducts(validProduct.ClientRegistrationName,
                                      Product.Multiply(validProduct.ProductPrice));

        public static IProductPrice MergeProducts(IProductPrice totalPrice, IEnumerable<CalculatedClientProducts> existingProducts) => totalPrice.Match(
            whenUnvalidatedProductPrice: unvalidatedTotalPrice => unvalidatedTotalPrice,
            whenInvalidTotalPrice: invalidatedTotalPrice => invalidatedTotalPrice,
            whenFailedProductPrice: failedTotalPrice => failedTotalPrice,
            whenValidatedTotalPrice: validatedTotalPrice => validatedTotalPrice,
            whenCalculatedTotalPrice: calculatedTotalPrice => MergeProducts(calculatedTotalPrice.ProductList, existingProducts),
            whenTotalProductsPrice: calculatedTotalPrice => calculatedTotalPrice
        );   

        private static CalculatedTotalPrice MergeProducts(IEnumerable<CalculatedClientProducts> newList, IEnumerable<CalculatedClientProducts> existingList)
        {
            var updatedAndNewProducts = newList.Select(product => product with { ProductCode = existingList.FirstOrDefault(p => p.ClientRegistrationName == product.ClientRegistrationName)?.ProductCode ?? "", IsUpdated = true });
            var oldProducts = existingList.Where(grade => !newList.Any(p => p.ClientRegistrationName == grade.ClientRegistrationName));
            var allProducts = updatedAndNewProducts.Union(oldProducts)
                                               .ToList()
                                               .AsReadOnly();
            return new CalculatedTotalPrice(allProducts);
        }

        public static IProductPrice CalculateTotalPrice(IProductPrice totalPrice) => totalPrice.Match(
            whenUnvalidatedProductPrice: unvalidatedTotalPrice => unvalidatedTotalPrice,
            whenInvalidTotalPrice: invalidatedTotalPrice => invalidatedTotalPrice,
            whenFailedProductPrice: failedTotalPrice => failedTotalPrice,
            whenValidatedTotalPrice: validatedTotalPrice => validatedTotalPrice,
            whenCalculatedTotalPrice: GenerateExport,
            whenTotalProductsPrice: calculatedTotalPrice => calculatedTotalPrice
        );
   
        private static IProductPrice GenerateExport(CalculatedTotalPrice calculatedPrice) => 
            new TotalProductsPrice(calculatedPrice.ProductList, 
                                    calculatedPrice.ProductList.Aggregate(new StringBuilder(), CreateCsvLine).ToString(), 
                                    DateTime.Now);

        private static StringBuilder CreateCsvLine(StringBuilder export, CalculatedClientProducts product) =>
            export.AppendLine($"{product.ClientRegistrationName.Value}, {product.ProductPrice}");
    }
}

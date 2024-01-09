using CSharp.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exemple.Domain.Models
{
    [AsChoice]
    public static partial class TotalPrice
    {
        public interface IProductPrice { }

        public record UnvalidatedProductPrice: IProductPrice
        {
            public UnvalidatedProductPrice(IReadOnlyCollection<UnvalidatedClientProduct> productList)
            {
                ProductList = productList;
            }

            public IReadOnlyCollection<UnvalidatedClientProduct> ProductList { get; }
        }

        public record InvalidTotalPrice: IProductPrice
        {
            internal InvalidTotalPrice(IReadOnlyCollection<UnvalidatedClientProduct> productsList, string reason)
            {
                ProductList = productsList;
                Reason = reason;
            }

            public IReadOnlyCollection<UnvalidatedClientProduct> ProductList { get; }
            public string Reason { get; }
        }

        public record FailedProductPrice : IProductPrice
        {
            internal FailedProductPrice(IReadOnlyCollection<UnvalidatedClientProduct> productList, Exception exception)
            {
                ProductList = productList;
                Exception = exception;
            }

            public IReadOnlyCollection<UnvalidatedClientProduct> ProductList { get; }
            public Exception Exception { get; }
        }

        public record ValidatedTotalPrice: IProductPrice
        {
            internal ValidatedTotalPrice(IReadOnlyCollection<ValidatedClientProduct> productsList)
            {
                ProductList = productsList;
            }

            public IReadOnlyCollection<ValidatedClientProduct> ProductList { get; }
        }

        public record CalculatedTotalPrice : IProductPrice
        {
            internal CalculatedTotalPrice(IReadOnlyCollection<CalculatedClientProducts> productsList)
            {
                ProductList = productsList;
            }

            public IReadOnlyCollection<CalculatedClientProducts> ProductList { get; }
        }

        public record TotalProductsPrice : IProductPrice
        {
            internal TotalProductsPrice(IReadOnlyCollection<CalculatedClientProducts> productsList, string csv, DateTime publishedDate)
            {
                ProductList = productsList;
                PublishedDate = publishedDate;
                Csv = csv;
            }

            public IReadOnlyCollection<CalculatedClientProducts> ProductList { get; }
            public DateTime PublishedDate { get; }
            public string Csv { get; }
        }
    }
}

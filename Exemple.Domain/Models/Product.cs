using LanguageExt;
using System;
using static LanguageExt.Prelude;

namespace Exemple.Domain.Models
{
    public record Product
    {
        public decimal Price { get; }
        public int Quantity { get; }

        internal Product(decimal price, int quantity)
        {
            if (IsValid(price, quantity))
            {
                Price = price;
                Quantity = quantity;
            }
            else
            {
                throw new InvalidProductException($"Invalid product price.");
            }
        }

        public static Option<Product> TryParseProduct(string priceString, string quantityString)
        {
            if(decimal.TryParse(priceString, out decimal price) && int.TryParse(quantityString, out int quantity) && IsValid(price, quantity))
            {
                return Some<Product>(new(price, quantity));
            } else
            {
                return None;
            }
        }

        public static Product Multiply(Product a) => new Product(a.Price * a.Quantity, a.Quantity);

        private static bool IsValid(decimal productPrice, int productQuantity) => productPrice > 0 && productQuantity > 0;

    }
}

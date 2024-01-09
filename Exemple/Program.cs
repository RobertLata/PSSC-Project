using Exemple.Domain.Models;
using System;
using System.Collections.Generic;
using static Exemple.Domain.Models.TotalPrice;
using static Exemple.Domain.ProductPriceOperation;
using Exemple.Domain;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;
using System.Net.Http;
using Example.Data.Repositories;
using Example.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exemple
{
    class Program
    {
        private static bool isCommandCanceled = false;

        private static string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;";

        static async Task Main(string[] args)
        {
            using ILoggerFactory loggerFactory = ConfigureLoggerFactory();
            ILogger<CalculateTotalPriceWorkflow> logger = loggerFactory.CreateLogger<CalculateTotalPriceWorkflow>();
            var listOfProducts = ReadListOfProducts().ToArray();
            PublishProductsCommand command = new(listOfProducts);
            if (!isCommandCanceled)
            {
                var dbContextBuilder = new DbContextOptionsBuilder<ProductsContext>()
                                                    .UseSqlServer(ConnectionString)
                                                    .UseLoggerFactory(loggerFactory);
                ProductsContext productsContext = new ProductsContext(dbContextBuilder.Options);
                ClientsRepository clientsRepository = new(productsContext);
                ProductsRepository productsRepository = new(productsContext);
                CalculateTotalPriceWorkflow workflow = new(clientsRepository, productsRepository, logger);
                var result = await workflow.ExecuteAsync(command);

                result.Match(
                        whenTotalPriceCalculationSucceededEvent: @event =>
                        {
                            Console.WriteLine($"Publish succeeded.");
                            Console.WriteLine("Your order is placed: " + @event.PublishedDate);
                            Console.WriteLine("Billing details: " + productName + ", " + @event.Csv);
                            
                            return @event;
                        },
                        whenTotalPriceCalculationFaildEvent: @event =>
                        {
                            Console.WriteLine($"Publish failed: {@event.Reason}");
                            return @event;
                        }
                    );
            }
        }

        private static ILoggerFactory ConfigureLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
                                builder.AddSimpleConsole(options =>
                                {
                                    options.IncludeScopes = true;
                                    options.SingleLine = true;
                                    options.TimestampFormat = "hh:mm:ss ";
                                })
                                .AddProvider(new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()));
        }

        private static List<UnvalidatedClientProduct> ReadListOfProducts()
        {
            List<UnvalidatedClientProduct> listOfProducts = new();
            
            do
            {
                //read client name and product and create a list of products
                var clientName = ReadValue("Client Name: ");
                if (string.IsNullOrEmpty(clientName))
                {
                    break;
                }

                var clientAddress = ReadValue("Client Address: ");
                if (string.IsNullOrEmpty(clientAddress))
                {
                    break;
                }

                var productPrice = ReadValue("Product Price: ");
                if (string.IsNullOrEmpty(productPrice))
                {
                    break;
                }

                var productQuantity = ReadValue("Product Quantity: ");
                if (string.IsNullOrEmpty(productQuantity))
                {
                    break;
                }

                productName = ReadValue("Product Name: ");
                if (string.IsNullOrEmpty(productName))
                {
                    break;
                }

                var productCode = ReadValue("Product Code: ");
                if (string.IsNullOrEmpty(productCode))
                {
                    break;
                }

                var cancelCommand = ReadValue("Anulati Comanda (Y/N): ");
                if (string.IsNullOrEmpty(cancelCommand) || cancelCommand == "Y" || cancelCommand == "y")
                {
                    Console.WriteLine("Command is canceled.");
                    isCommandCanceled = true;
                    break;
                }

                listOfProducts.Add(new(clientName, productPrice, productQuantity, productCode, productName));
            } while (true);
            return listOfProducts;
        }

        private static string? ReadValue(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }
        private static string productName = "";
    }

}

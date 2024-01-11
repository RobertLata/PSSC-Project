using Exemple.Domain.Models;
using static Exemple.Domain.Models.TotalPriceCalculatedEvent;
using static Exemple.Domain.ProductPriceOperation;
using System;
using static Exemple.Domain.Models.TotalPrice;
using LanguageExt;
using System.Threading.Tasks;
using System.Collections.Generic;
using Exemple.Domain.Repositories;
using System.Linq;
using static LanguageExt.Prelude;
using Microsoft.Extensions.Logging;

namespace Exemple.Domain
{
    public class BillingWorkflow
    {
        private readonly IClientsRepository clientsRepository;
        private readonly IProductsRepository productsRepository;
        private readonly ILogger<BillingWorkflow> logger;

        public BillingWorkflow(IClientsRepository clientsRepository, IProductsRepository productsRepository, ILogger<BillingWorkflow> logger)
        {
            this.clientsRepository = clientsRepository;
            this.productsRepository = productsRepository;
            this.logger = logger;
        }

        public async Task<ITotalPriceCalculatedEvent> ExecuteAsync(PublishProductsCommand command)
        {
            UnvalidatedProductPrice unvalidatedProducts = new UnvalidatedProductPrice(command.InputProductPrice);

            var result = from clients in clientsRepository.TryGetExistingClients(unvalidatedProducts.ProductList.Select(product => product.ClientName))
                                          .ToEither(ex => new FailedProductPrice(unvalidatedProducts.ProductList, ex) as IProductPrice)
                         from existingProducts in productsRepository.TryGetExistingProducts()
                                          .ToEither(ex => new FailedProductPrice(unvalidatedProducts.ProductList, ex) as IProductPrice)
                         let checkClientExists = (Func<ClientRegistrationName, Option<ClientRegistrationName>>)(client => CheckClientExists(clients, client))
                         from totalPrice in ExecuteWorkflowAsync(unvalidatedProducts, existingProducts, checkClientExists).ToAsync()
                         from _ in productsRepository.TrySaveProducts(totalPrice)
                                          .ToEither(ex => new FailedProductPrice(unvalidatedProducts.ProductList, ex) as IProductPrice)
                         select totalPrice;

            return await result.Match(
                    Left: productPrice => GenerateFailedEvent(productPrice) as ITotalPriceCalculatedEvent,
                    Right: totalPrice => new TotalPriceCalculationSucceededEvent(totalPrice.Csv, totalPrice.PublishedDate)
                );
        }

        private async Task<Either<IProductPrice, TotalProductsPrice>> ExecuteWorkflowAsync(UnvalidatedProductPrice unvalidatedProducts, 
                                                                                          IEnumerable<CalculatedClientProducts> existingProducts, 
                                                                                          Func<ClientRegistrationName, Option<ClientRegistrationName>> checkClientExists)
        {
            
            IProductPrice products = await ValidateTotalPrice(checkClientExists, unvalidatedProducts);
            products = CalculateFinalProductPrice(products);
            products = MergeProducts(products, existingProducts);
            products = CalculateTotalPrice(products);

            return products.Match<Either<IProductPrice, TotalProductsPrice>>(
                whenUnvalidatedProductPrice: unvalidatedProducts => Left(unvalidatedProducts as IProductPrice),
                whenInvalidTotalPrice: invalidProducts => Left(invalidProducts as IProductPrice),
                whenFailedProductPrice: failedProducts => Left(failedProducts as IProductPrice),
                whenValidatedTotalPrice: validatedProducts => Left(validatedProducts as IProductPrice),
                whenCalculatedTotalPrice: calculatedProducts => Left(calculatedProducts as IProductPrice),
                whenTotalProductsPrice: sentProducts => Right(sentProducts)
            );
        }

        private Option<ClientRegistrationName> CheckClientExists(IEnumerable<ClientRegistrationName> clients, ClientRegistrationName clientRegistrationNumber)
        {
            if(clients.Any(c=>c == clientRegistrationNumber))
            {
                return Some(clientRegistrationNumber);
            }
            else
            {
                return None;
            }
        }

        private TotalPriceCalculationFaildEvent GenerateFailedEvent(IProductPrice totalPrice) =>
            totalPrice.Match<TotalPriceCalculationFaildEvent>(
                whenUnvalidatedProductPrice: unvalidatedTotalPrice => new($"Invalid state {nameof(UnvalidatedProductPrice)}"),
                whenInvalidTotalPrice: invalidTotalPrice => new(invalidTotalPrice.Reason),
                whenFailedProductPrice: failedTotalPrice =>
                {
                    logger.LogError(failedTotalPrice.Exception, failedTotalPrice.Exception.Message);
                    return new(failedTotalPrice.Exception.Message);
                },
                whenValidatedTotalPrice: validatedTotalPrice => new($"Invalid state {nameof(ValidatedTotalPrice)}"),
                whenCalculatedTotalPrice: calculatedTotalPrice => new($"Invalid state {nameof(CalculatedTotalPrice)}"),
                whenTotalProductsPrice: calculatedTotalPrice => new($"Invalid state {nameof(TotalProductsPrice)}"));
    }
}

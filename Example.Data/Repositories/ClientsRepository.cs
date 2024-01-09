using Exemple.Domain.Models;
using Exemple.Domain.Repositories;
using LanguageExt;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Example.Data.Repositories
{
    public class ClientsRepository: IClientsRepository
    {
        private readonly ProductsContext productsContext;

        public ClientsRepository(ProductsContext productsContext)
        {
            this.productsContext = productsContext;  
        }

        public TryAsync<List<ClientRegistrationName>> TryGetExistingClients(IEnumerable<string> clientsToCheck) => async () =>
        {
            var clients = await productsContext.Client
                                              .Where(client => clientsToCheck.Contains(client.ClientName))
                                              .AsNoTracking()
                                              .ToListAsync();
            return clients.Select(client => new ClientRegistrationName(client.ClientName))
                           .ToList();
        };
    }
}

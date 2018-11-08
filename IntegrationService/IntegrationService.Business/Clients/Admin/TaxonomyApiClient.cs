using System;
using System.Collections.Generic;
using Taxonomy.Client;

namespace IntegrationService.Business.Clients.Admin
{
    public class TaxonomyApiClient : ITaxonomyApiClient
    {
        private readonly ITaxonomyClient _client;

        public TaxonomyApiClient(ITaxonomyClient client)
        {
            _client = client;
        }

        public IEnumerable<KeyValuePair<int, string>> GetTaxonomyTree(Guid branchId, Guid treeId)
        {
            return _client.GetTaxonomyTree(branchId, treeId);
        }
    }
}
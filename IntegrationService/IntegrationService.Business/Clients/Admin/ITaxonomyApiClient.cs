using System;
using System.Collections.Generic;

namespace IntegrationService.Business.Clients.Admin
{
    public interface ITaxonomyApiClient
    {
        IEnumerable<KeyValuePair<int, string>> GetTaxonomyTree(Guid branchId, Guid treeId);
    }
}
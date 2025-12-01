using Entities.ViewModels.Funding;
using System.Collections.Generic;
using Entities.Models;
using System.Threading.Tasks;

using Entities.ViewModels;
using Entities.ViewModels.SupplierConfig;


namespace Repositories.IRepositories
{
    public interface ISupplierRepository
    {
       
        int GetByIDOrName(int suplier_id, string name);
        // Banking account
        IEnumerable<SupplierPaymentViewModel> GetSupplierPaymentList(int supplier_id);
        Task<List<Supplier>> GetSuggestionList(string name);
        SupplierViewModel GetById(int supplierId);
      

        // Contact


    }
}

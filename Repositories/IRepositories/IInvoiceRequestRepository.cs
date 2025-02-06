using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.InvoiceRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IInvoiceRequestRepository
    {
        Task<List<InvoiceRequestViewModel>> GetListInvoiceRequestByOrderId(int OrderId);
        Task<List<InvoiceRequestViewModel>> GetInvoiceRequestById(int OrderId);
        Task<GenericViewModel<InvoiceRequestViewModel>> GetListInvoiceRequest(InvoiceSearchViewModel request);
        Task<int> SetUpInvoiceRequest(InvoiceRequest request);
    }
}

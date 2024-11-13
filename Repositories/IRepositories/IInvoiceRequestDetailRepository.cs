using Entities.Models;
using Entities.ViewModels.InvoiceRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IInvoiceRequestDetailRepository
    {
        Task<List<InvoiceRequestDetail>> GetListInvoiceRequestDetailByInvoiceRequestId(int OrderId);
        Task<int> SetUpInvoiceRequestDetail(InvoiceRequestDetail request);
        Task<int> DeleteInvoiceRequestDetailById(int Id);
    }
}

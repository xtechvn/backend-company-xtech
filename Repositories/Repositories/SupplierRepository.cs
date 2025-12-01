using Aspose.Cells;
using DAL;
using DAL.Funding;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Entities.ViewModels.SupplierConfig;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

using Repositories.IRepositories;
using Repositories.Repositories.BaseRepos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class SupplierRepository : BaseRepository, ISupplierRepository
    {

        private readonly BankingAccountDAL bankingAccountDAL;
        private readonly SupplierDAL supplierDAL;
        private readonly AllCodeDAL allCodeDAL;
      

        private readonly string _UrlStaticImage;

        public SupplierRepository(IHttpContextAccessor context, IOptions<DataBaseConfig> dataBaseConfig,
            IOptions<DomainConfig> domainConfig, IUserRepository userRepository, IConfiguration configuration) : base(context, dataBaseConfig, configuration, userRepository)
        {
            supplierDAL = new SupplierDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            allCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            bankingAccountDAL = new BankingAccountDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
           
            _UrlStaticImage = domainConfig.Value.ImageStatic;
        }
        public IEnumerable<SupplierPaymentViewModel> GetSupplierPaymentList(int supplier_id)
        {
            try
            {
                var dataTable = bankingAccountDAL.GetBankAccountDataTableBySupplierId(supplier_id);
                return dataTable.ToList<SupplierPaymentViewModel>();
            }
            catch
            {
                throw;
            }
        }
        public SupplierViewModel GetById(int supplierId)
        {
            var detail = supplierDAL.GetById(supplierId);
            SupplierViewModel supplierViewModel = new SupplierViewModel();
            detail.CopyProperties(supplierViewModel);
            return supplierViewModel;
        }
        public async Task<List<Supplier>> GetSuggestionList(string name)
        {
            List<Supplier> data = new List<Supplier>();
            try
            {
                var suppliers = await supplierDAL.GetAllAsync();
                data = suppliers;
                if (!string.IsNullOrEmpty(name))
                {
                    data = suppliers.Where(s =>
                    s.FullName.Trim().ToLower().Contains(name.Trim().ToLower())
                    || (!string.IsNullOrEmpty(s.ShortName) && s.ShortName.Trim().ToLower().Contains(name.Trim().ToLower()))
                    || (!string.IsNullOrEmpty(s.Email) && s.Email.Trim().ToLower().Contains(name.Trim().ToLower()))
                    || (!string.IsNullOrEmpty(s.Phone) && s.Phone.Trim().ToLower().Contains(name.Trim().ToLower()))
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetSuggestionList - SupplierRepository: " + ex);
                data = new List<Supplier>();
            }
            return data;
        }



        public int GetByIDOrName(int suplier_id, string name)
        {
            try
            {
                var data = supplierDAL.CheckExistName(suplier_id, name);

                if (data != null && data.SupplierId > 0)
                {
                    return data.SupplierId;
                }
            }
            catch
            {
            }
            return -1;

        }

       
    }
}

using Aspose.Cells;
using DAL.StoreProcedure;
using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Contants;
using System.Drawing;
using Utilities;
using Entities.ViewModels.Funding;

namespace Repositories.Repositories
{
    public class ContractPayRepository : IContractPayRepository
    {
        private readonly ContractPayDAL _contractPayDAL;

        public ContractPayRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _contractPayDAL = new ContractPayDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<List<ContractPayDetaiByOrderIdlViewModel>> GetContractPayByOrderId(long OrderId)
        {
            try
            {

                DataTable data = await _contractPayDAL.GetContractPayByOrderId(OrderId);
                var listData = data.ToList<ContractPayDetaiByOrderIdlViewModel>();
                if (listData.Count > 0)
                {
                    return listData;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetContractPayByOrderId - ContractPayDAL. " + ex);
            }
            return null;
        }
        public double GetTotalAmountContractPayByServiceId(string ServiceId, long ServiceType, long ContractPayType)
        {
            try
            {

                return _contractPayDAL.GetTotalAmountContractPayByServiceId(ServiceId, ServiceType, ContractPayType);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetServiceDetail - ContractPayRepository: " + ex);
            }
            return 0;
        }
    }
}


using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels.Funding;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;
using Utilities.Contants;
using System.Linq;
using System.Threading.Tasks;
using Entities.ViewModels;
using Aspose.Cells;
using System.Drawing;
using DAL.StoreProcedure;
using System.Data;
using System.Globalization;
using DAL.Funding;

namespace Repositories.Repositories
{
    public class DepositHistoryRepository : IDepositHistoryRepository
    {
        private readonly DepositHistoryDAL depositHistoryDAL;
        private readonly AllCodeDAL allCodeDAL;
        private readonly ClientDAL clientDAL;
    
        private readonly UserDAL userDAL;
        private readonly ContractPayDAL contractPayDAL;
        private readonly BankingAccountDAL bankingAccountDAL;

       

        public List<DepositFunding> GetByClientId(long clientId, int payId = 0)
        {
            try
            {
                var listDepositFunding = new List<DepositFunding>();
                var listDepositFundingOutput = new List<DepositFunding>();
                var dt = depositHistoryDAL.GetListOrderByClientId(clientId, StoreProcedureConstant.SP_GetDepositHistoryByClientId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    listDepositFunding = (from row in dt.AsEnumerable()
                                          select new DepositFunding
                                          {
                                              Id = Convert.ToInt32(row["Id"].ToString()),
                                              TransNo = row["TransNo"].ToString(),
                                              ServiceName = row["ServiceName"].ToString(),
                                              StatusStr = row["Status"].ToString(),
                                              UserName = row["UserName"].ToString(),
                                              Price = !row["Amount"].Equals(DBNull.Value) ? Convert.ToDouble(row["Amount"].ToString()) : 0,
                                          }).ToList();
                    if (payId > 0)
                    {
                        var allCode_SERVICE_TYPE = allCodeDAL.GetListByType(AllCodeType.SERVICE_TYPE);
                        var allCode_DEPOSIT_STATUS = allCodeDAL.GetListByType(AllCodeType.DEPOSIT_STATUS);
                        var contractPayDetail = contractPayDAL.GetByContractPayIds(new List<int>() { payId });
                        foreach (var item in contractPayDetail)
                        {
                            var depositHisInfo = depositHistoryDAL.GetById((int)item.DataId.Value);
                            DepositFunding deposit = new DepositFunding();
                            depositHisInfo.CopyProperties(deposit);
                            deposit.ServiceName = allCode_SERVICE_TYPE.FirstOrDefault(n => n.CodeValue == deposit.ServiceType)?.Description;
                            deposit.StatusStr = allCode_DEPOSIT_STATUS.FirstOrDefault(n => n.CodeValue == deposit.Status)?.Description;
                            var accountClient = clientDAL.GetAccountClientByID(depositHisInfo.UserId.Value).Result;
                            deposit.UserName = accountClient?.UserName;
                            if (listDepositFunding.FirstOrDefault(n => n.Id == depositHisInfo.Id) == null)
                                listDepositFunding.Add(deposit);
                        }
                    }
                    var listContractPayDetail = contractPayDAL.GetByContractDataIds(listDepositFunding.Select(n => Convert.ToInt64(n.Id)).ToList());
                    foreach (var item in listDepositFunding)
                    {
                        DepositFunding depositFunding = new DepositFunding();
                        var details = listContractPayDetail.Where(n => n.DataId != null && n.DataId.Value == Convert.ToInt64(item.Id)).ToList();
                        item.TotalDisarmed = details.Sum(n => (double)n.Amount);
                        item.TotalNeedPayment = item.Price.Value - item.TotalDisarmed;
                        item.CopyProperties(depositFunding);
                        var detail = listContractPayDetail.Where(n => n.DataId != null
                             && n.DataId.Value == Convert.ToInt64(item.Id) && n.PayId == payId).FirstOrDefault();
                        if (detail != null)
                        {
                            depositFunding.PayDetailId = detail.Id;
                            depositFunding.IsChecked = true;
                            depositFunding.Payment = (double)detail?.Amount;
                        }
                        if (item.TotalNeedPayment > 0)
                            listDepositFundingOutput.Add(depositFunding);
                    }
                }
                return listDepositFundingOutput;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByClientId - OrderRepository" + ex);
            }
            return new List<DepositFunding>();
        }
    }
}


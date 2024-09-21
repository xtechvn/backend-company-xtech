using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class PaymentAccountRepository : IPaymentAccountRepository
    {
        private readonly PaymentAccountDAL _PaymentAccountDAL;
        private readonly UserAgentDAL _UserAgentDAL;
        private readonly BankingAccountDAL bankingAccountDAL;

        public PaymentAccountRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {

            _PaymentAccountDAL = new PaymentAccountDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _UserAgentDAL = new UserAgentDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            bankingAccountDAL = new BankingAccountDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
       
        public int Delete(int Id)
        {
            try
            {
                return _PaymentAccountDAL.Delete(Id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Delete - PaymentAccountRepository: " + ex);
                return 0;
            }
        }
   
        public int UpsertBankingAccount(BankingAccount model)
        {
            try
            {
                if (model.Id > 0)
                {
                    return bankingAccountDAL.UpdateBankingAccount(model);
                }
                else
                {
                    return bankingAccountDAL.InsertBankingAccount(model);
                }
            }
            catch
            {
                throw;
            }
        }
        public BankingAccount GetBankingAccountById(int Id)
        {
            try
            {
                return bankingAccountDAL.GetById(Id);
            }
            catch
            {
                throw;
            }
        }
        public int DeleteBankingAccountById(int id)
        {
            try
            {
                bankingAccountDAL.Delete(id);
                return id;
            }
            catch
            {
                throw;
            }
        }
    }
}

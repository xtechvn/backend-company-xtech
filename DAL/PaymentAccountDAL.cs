using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class PaymentAccountDAL
    {
        private static DbWorker _DbWorker;
        private readonly string _connection;
        public PaymentAccountDAL(string connection)
        {
            _connection = connection;
            _DbWorker = new DbWorker(connection);
        }

        public int CreatePaymentAccount(PaymentAccount model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[] 
                {
                    new SqlParameter("@AccountNumb", model.AccountNumb),
                    new SqlParameter("@AccountName", model.AccountName),
                    new SqlParameter("@BankName", model.BankName),
                    new SqlParameter("@Branch", model.Branch),
                    new SqlParameter("@ClientId", model.ClientId)
                };

                var rs = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertPaymentAccount, objParam);
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreatePaymentAccount - PaymentAccountDAL: " + ex);
                return 0;
            }
        }

        public int Delete(int Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    if (Id != 0)
                    {
                        var deleteModel = _DbContext.PaymentAccounts.FirstOrDefault(s => s.Id == Id);
                        _DbContext.PaymentAccounts.Remove(deleteModel);
                        _DbContext.SaveChanges();
                    }

                }
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Delete - PaymentAccountDAL: " + ex);
                return 0;
            }
        }
    }
}

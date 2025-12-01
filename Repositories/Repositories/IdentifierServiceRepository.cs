using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ultilities.Constants;
using Utilities;

namespace Repositories.Repositories
{
    public class IdentifierServiceRepository : IIdentifierServiceRepository
    {
        private readonly ClientDAL clientDAL;
        private readonly ContractPayDAL contractPayDAL;

        public IdentifierServiceRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            clientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            contractPayDAL = new ContractPayDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<string> buildContractPay()
        {
            string bill_no = string.Empty;
            try
            {
                var current_date = DateTime.Now;
                bill_no = "PT";

                //1. 2 số cuối của năm
                bill_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);


                //2. Số thứ tự phiếu thu trong năm.
                long bill_count = contractPayDAL.CountContractPayInYear();

                //format numb
                string s_bill_new = string.Format(String.Format("{0,5:00000}", bill_count + 1));

                //3.1 Check số phiếu thu này có chưa
                var check = await contractPayDAL.getContractPayByBillNo(bill_no + s_bill_new);

                if (!string.IsNullOrEmpty(check))
                {
                    //Nếu có rồi tăng lên 1
                    bill_no += string.Format(String.Format("{0,5:00000}", bill_count + 2));
                    LogHelper.InsertLogTelegram("buildContractPay - IdentifierServiceRepository" + bill_no + s_bill_new + " đã có. Check lại code");
                }
                else
                {
                    bill_no += s_bill_new;
                }

                return bill_no;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("buildContractPay - IdentifierServiceRepository" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var contract_pay_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                bill_no = "PT-" + contract_pay_default;
                return bill_no;
            }
        }
        public async Task<string> buildClientNo(int client_type)
        {
            string code = ClientTypeName.service[Convert.ToInt16(client_type)];

            try
            {
                var current_date = DateTime.Now;
                int count = clientDAL.countClientTypeUse(client_type);

                //so tu tang
                string s_format = string.Format(String.Format("{0,5:00000}", count + 1));

                //1. 2 số cuối của năm
                string two_year_last = current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                code = code + two_year_last + s_format;

                return code;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("buildClientNo - IdentifierServiceRepository" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var num_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                code = code + num_default;
                return code;
            }
        }
    }
}

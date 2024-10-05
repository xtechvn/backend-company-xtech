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

        public IdentifierServiceRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            clientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
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

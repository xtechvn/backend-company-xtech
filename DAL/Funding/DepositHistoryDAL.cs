

using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Utilities;

namespace DAL.Funding
{
    public class DepositHistoryDAL : GenericService<DepositHistory>
    {
        private static DbWorker _DbWorker;
        public DepositHistoryDAL(string connection) : base(connection)
        {
            _connection = connection;
            _DbWorker = new DbWorker(connection);
        }


        public List<DepositHistory> GetByIds(List<int> depositHistoryIds)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var details = _DbContext.DepositHistories.AsNoTracking().Where(x => depositHistoryIds.Contains(x.Id)).ToList();
                    if (details != null)
                    {
                        return details;
                    }
                }
                return new List<DepositHistory>();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByIds - DepositHisotyDAL: " + ex);
                return new List<DepositHistory>();
            }
        }
        public DepositHistory GetById(int depositHistoryId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.DepositHistories.AsNoTracking().FirstOrDefault(x => x.Id == depositHistoryId);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - DepositHisotyDAL: " + ex);
                return null;
            }
        }

      
       

       

        public DataTable GetListOrderByClientId(long clienId, string proc)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@ClientId", clienId);
                objParam[1] = new SqlParameter("@IsFinishPayment", DBNull.Value);
                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListOrderByClientId - DepositHistoryDAL: " + ex);
            }
            return null;
        }
    }
}

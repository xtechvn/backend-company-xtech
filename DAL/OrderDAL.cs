using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class OrderDAL
    {
        private static DbWorker _DbWorker;
        public OrderDAL(string connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public async Task<DataTable> GetPagingList(OrderViewSearchModel searchModel, string proc)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@status", searchModel.Status == null ? DBNull.Value : searchModel.Status),
                    new SqlParameter("@CreateTimeFrom",(CheckDate(searchModel.CreateDateFrom) == DateTime.MinValue) ? DBNull.Value : CheckDate(searchModel.CreateDateFrom)),
                    new SqlParameter("@CreateTimeTo",(CheckDate(searchModel.CreateDateTo) == DateTime.MinValue) ? DBNull.Value : CheckDate(searchModel.CreateDateTo).AddDays(1)),
                    new SqlParameter("@PaymentStatus",searchModel.PaymentStatus != null ? searchModel.PaymentStatus : DBNull.Value),
                    new SqlParameter("@ClientId", searchModel.ClientId != null ? searchModel.ClientId : DBNull.Value),
                    new SqlParameter("@OrderId",searchModel.OrderId != null ? searchModel.OrderId : DBNull.Value),
                    new SqlParameter("@PageIndex", searchModel.PageIndex),
                    new SqlParameter("@PageSize", searchModel.pageSize)
                };

                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - OrderDal: " + ex);
            }
            return null;
        }
        private DateTime CheckDate(string dateTime)
        {
            DateTime _date = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateTime))
            {
                _date = DateTime.ParseExact(dateTime, "d/M/yyyy", CultureInfo.InvariantCulture);
            }

            return _date != DateTime.MinValue ? _date : DateTime.MinValue;
        }
    }
}

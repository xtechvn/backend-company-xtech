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
using Ultilities.Constants;
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

        public async Task<OrderDetailViewModel> GetDetailOrderByOrderId(long OrderId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                DataTable dt = _DbWorker.GetDataTable(ProcedureConstants.SP_GetDetailOrderByOrderId, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<OrderDetailViewModel>();
                    return data[0];
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderByOrderId - OrderDal: " + ex);
            }
            return null;
        }

        public async Task<long> UpdateOrder(Order model)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[18];
                objParam[0] = new SqlParameter("@OrderId", model.OrderId);
                objParam[1] = new SqlParameter("@ClientId", model.ClientId == 0 ? DBNull.Value : model.ClientId);
                objParam[2] = new SqlParameter("@OrderNo", model.OrderNo == null ? DBNull.Value : model.OrderNo);
                objParam[3] = new SqlParameter("@Price", model.Price == null ? DBNull.Value : model.Price);
                objParam[4] = new SqlParameter("@Profit", model.Profit == null ? DBNull.Value : model.Profit);
                objParam[5] = new SqlParameter("@Discount", model.Discount == null ? DBNull.Value : model.Discount);
                objParam[6] = new SqlParameter("@Amount", model.Amount == null ? DBNull.Value : model.Amount);
                objParam[7] = new SqlParameter("@Status", model.OrderStatus == 0 ? DBNull.Value : model.OrderStatus);
                objParam[8] = new SqlParameter("@PaymentType", model.PaymentType == 0 ? DBNull.Value : model.PaymentType);
                objParam[9] = new SqlParameter("@PaymentStatus", model.PaymentStatus == 0 ? DBNull.Value : model.PaymentStatus);
                objParam[14] = new SqlParameter("@SalerId", model.SalerId == 0 ? DBNull.Value : model.SalerId);
                objParam[15] = new SqlParameter("@SalerGroupId", model.SalerGroupId == null ? DBNull.Value : model.SalerGroupId);
                objParam[10] = new SqlParameter("@UtmSource", model.UtmSource == null ? DBNull.Value : model.UtmSource);
                objParam[11] = new SqlParameter("@UtmMedium", model.UtmMedium == null ? DBNull.Value : model.UtmMedium);
                objParam[12] = new SqlParameter("@Note", model.Note == null ? DBNull.Value : model.Note);
                objParam[13] = new SqlParameter("@VoucherId", model.VoucherId == null ? DBNull.Value : model.VoucherId);
                objParam[16] = new SqlParameter("@UserUpdateId", model.UserUpdateId == null ? DBNull.Value : model.UserUpdateId);
                objParam[17] = new SqlParameter("@Label", model.Label == null ? DBNull.Value : model.Label);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.Sp_UpdateOrder, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderSaler - OrderDal: " + ex);
                return -2;
            }
        }

        public async Task<int> UpdateAmountOrder(long OrderId)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderID", OrderId);

                 return _DbWorker.ExecuteNonQuery(ProcedureConstants.SP_UpdateOrderAmount, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateAmountOrder - OrderDal: " + ex);
            }
            return -1;
        }

        public async Task<int> InsertOrder(Order model)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@ClientId",model.ClientId),
                    new SqlParameter("@ServiceType",model.ServiceType != null ? model.ServiceType : DBNull.Value),
                    new SqlParameter("@SalerId",model.SalerId),
                    new SqlParameter("@OrderNo",model.OrderNo),
                    new SqlParameter("@SalerGroupId",model.SalerGroupId != null ? model.SalerGroupId : DBNull.Value),
                    new SqlParameter("@CreateTime",DateTime.Now),
                    new SqlParameter("@PaymentStatus",PaymentStatus.UNPAID),
                    new SqlParameter("@BranchCode",model.BranchCode),
                    new SqlParameter("@Note",model.Note),
                    new SqlParameter("@CreatedBy",model.CreatedBy != null? model.CreatedBy : DBNull.Value),
                    new SqlParameter("@SmsContent",model.SmsContent != null? model.SmsContent : DBNull.Value),
                    new SqlParameter("@OrderStatus",OrderStatus.New),
                    new SqlParameter("@Description",model.Description != null?model.Description : DBNull.Value),
                    new SqlParameter("@Amount",model.Amount != null ? model.Amount : DBNull.Value),
                    new SqlParameter("@Label",model.Label != null ? model.Label : DBNull.Value),
                };
               

                return _DbWorker.ExecuteNonQuery(ProcedureConstants.SP_InsertOrder, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderSaler - OrderDal: " + ex);
                return -2;
            }
        }

        public async Task<DataTable> GetOrderNo() 
        {
            int Year = DateTime.Now.Year;
            try
            {

                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@Year",Year),

                };
                return _DbWorker.GetDataTable(ProcedureConstants.SP_GetOrderNo, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderSaler - OrderDal: " + ex);  
            }
            return null;
        } 

        public async Task<DataTable> GetPagingList(OrderViewSearchModel searchModel, string proc)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@status", searchModel.Status == null ? DBNull.Value : searchModel.Status),
                    new SqlParameter("@CreateTimeFrom",(searchModel.CreateDateFrom == DateTime.MinValue) ? DBNull.Value : searchModel.CreateDateFrom),
                    new SqlParameter("@CreateTimeTo",(searchModel.CreateDateTo == DateTime.MinValue) ? DBNull.Value : searchModel.CreateDateTo.AddDays(1)),
                    new SqlParameter("@StartDate",(searchModel.StartDate == DateTime.MinValue) ? DBNull.Value : searchModel.StartDate),
                    new SqlParameter("@EndDate",(searchModel.EndDate == DateTime.MinValue) ? DBNull.Value : searchModel.EndDate.AddDays(1)),
                    new SqlParameter("@PaymentStatus",searchModel.PaymentStatus != null ? searchModel.PaymentStatus : DBNull.Value),
                    new SqlParameter("@PaymentMethod",searchModel.PaymentMethod != null ? searchModel.PaymentMethod : DBNull.Value),
                    new SqlParameter("@ClientId", searchModel.ClientId != null ? searchModel.ClientId : DBNull.Value),
                    new SqlParameter("@OrderId",searchModel.OrderId != null ? searchModel.OrderId : DBNull.Value),
                    new SqlParameter("@SalerId",searchModel.SalerId != null ? searchModel.SalerId : DBNull.Value),
                    new SqlParameter("@ServiceType",searchModel.ServiceType != null ? searchModel.ServiceType : DBNull.Value),
                    new SqlParameter("@OrderNo",searchModel.OrderNo != null ? searchModel.OrderNo : DBNull.Value),
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

        public async Task<object> getSumAmount(int? paymentStatus,string proc) 
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[]
                {
                    new SqlParameter("@PaymentStatus",paymentStatus != null ? paymentStatus : DBNull.Value)
                };

                return _DbWorker.ExecuteScalar(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAmountOrder - OrderDal: " + ex);
            }
            return null;
        }
    }
}

using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
    public class OrderDAL : GenericService<Order>
    {
        private static DbWorker _DbWorker;
        public OrderDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
        public List<Order> GetByOrderIds(List<long> orderIds)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Order.AsNoTracking().Where(s => orderIds.Contains(s.OrderId)).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderIds - OrderDal: " + ex);
                return new List<Order>();
            }
        }
        public List<Order> GetByOrderNos(List<string> orderNos)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Order.AsNoTracking().Where(s => orderNos.Contains(s.OrderNo)).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderNos - OrderDal: " + ex);
                return new List<Order>();
            }
        }
        public DataTable GetListOrderByClientId(long clienId, string proc, int status = 0)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[3];
                objParam[0] = new SqlParameter("@ClientId", clienId);
                objParam[1] = new SqlParameter("@IsFinishPayment", DBNull.Value);
                if (status == 0)
                    objParam[2] = new SqlParameter("@OrderStatus", DBNull.Value);
                else
                    objParam[2] = new SqlParameter("@OrderStatus", status);

                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListOrderByClientId - OrderDal: " + ex);
            }
            return null;
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
                    new SqlParameter("@Note",model.Note != null ? model.Note : DBNull.Value),
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

       
        public List<Order> GetByClientId(long Client_Id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Order.AsNoTracking().Where(s => s.ClientId == Client_Id).OrderByDescending(s => s.CreateTime).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByClientId - OrderDal: " + ex);
                return null;
            }
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
        public Order GetByOrderId(long OrderId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    return _DbContext.Order.AsNoTracking().FirstOrDefault(s => s.OrderId == OrderId);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderId - OrderDal: " + ex);
                return null;
            }
        }
        public async Task<int> UpdateOrderStatus(long OrderId, long Status, long UpdatedBy, long UserVerify)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[4];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                objParam[1] = new SqlParameter("@Status", Status);
                objParam[2] = new SqlParameter("@UpdatedBy", UpdatedBy);
                objParam[3] = UserVerify == 0 ? new SqlParameter("@UserVerify", DBNull.Value) : new SqlParameter("@UserVerify", UserVerify);

                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderStatus, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderServiceByOrderId - OrderDal: " + ex);
            }
            return 0;
        }
        public async Task<DataTable> GetAllServiceByOrderId(long OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);
                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAllServiceByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAllServiceByOrderId - OrderDal: " + ex);
            }
            return null;
        }
        public async Task<double> UpdateOrderDetail(long OrderId, long user_id)
        {
            try
            {
                List<int> order_status_not_allowed = new List<int>() { (int)OrderStatus2.FINISHED, (int)OrderStatus2.WAITING_FOR_ACCOUNTANT, (int)OrderStatus2.CANCEL, (int)OrderStatus2.ACCOUNTANT_DECLINE, (int)OrderStatus2.CREATED_ORDER, (int)OrderStatus2.CONFIRMED_SALE };

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var data = _DbContext.Order.AsNoTracking().FirstOrDefault(s => s.OrderId == OrderId);
                    double amount = 0;
                    double price = 0;
                    double discount = 0;
                    double profit = 0;
                    double commission = 0;
                    double FundCustomerCare = 0;
                    List<int> product_service = new List<int>();
                    data.StartDate = null;
                    data.EndDate = null;
                    List<int> other_servicetype_main = new List<int>() { (int)ServiceOtherType.WaterSport };
                    if (data != null && data.OrderId > 0)
                    {
                        if (data.VoucherId != null && data.VoucherId > 0)
                        {
                            amount -= (data.Discount == null ? 0 : (double)data.Discount);
                        }
                        var order_status_old = data.OrderStatus;
                        var list_other_booking_all = await _DbContext.OtherBooking.AsNoTracking().Where(s => s.OrderId == OrderId).ToListAsync();
                        
                        var list_watersport = await _DbContext.OtherBooking.AsNoTracking().Where(s => s.OrderId == OrderId && s.ServiceType > 0 && s.ServiceType == (int)ServiceOtherType.WaterSport).ToListAsync();


                        
                        var list_other_booking = list_other_booking_all.Where(s => s.Status != (int)ServiceStatus.Cancel).ToList();
                        var list_other_id = list_other_booking.Select(x => x.Id);
                        var list_other_optional = await _DbContext.OtherBookingPackagesOptional.AsNoTracking().Where(s => list_other_id.Contains(s.BookingId) && s.Status != 1).ToListAsync();
                        if (list_other_booking != null && list_other_booking.Count > 0)
                        {
                            amount += list_other_booking.Sum(x => x.Amount);
                            if (list_other_optional != null && list_other_optional.Count > 0)
                            {
                                price += list_other_optional.Sum(x => x.Amount);
                            }
                            else
                            {
                                price += list_other_booking.Sum(x => x.Amount - x.Profit);// - (x.Commission != null ? (double)x.Commission : 0) - (x.OthersAmount != null ? (double)x.OthersAmount : 0));
                            }
                            // discount += list_other_booking.Sum(x => x.TotalDiscount != null ? (double)x.TotalDiscount : 0);
                            profit += list_other_booking.Sum(x => x.Profit);
                            product_service.Add((int)ServicesType.Other);
                            var min_date = list_other_booking.OrderBy(x => x.StartDate).FirstOrDefault();
                            var max_date = list_other_booking.OrderByDescending(x => x.EndDate).FirstOrDefault();
                            if (data.StartDate == null || data.StartDate > min_date.StartDate) data.StartDate = min_date.StartDate;
                            if (data.EndDate == null || data.EndDate < max_date.EndDate) data.EndDate = max_date.EndDate;
                            commission += list_other_booking.Sum(x => x.Commission != null ? (double)x.Commission : 0);
                            

                        }

                        var list_ws_booking = list_watersport.Where(s => s.Status != (int)ServiceStatus.Cancel).ToList();
                        if (list_ws_booking != null && list_ws_booking.Count > 0)
                        {
                            product_service.Add((int)ServicesType.WaterSport);
                        }
                        DataTable contract_pay_list = await GetContractPayByOrderId(OrderId);
                        var listData_contract_pay = contract_pay_list.ToList<ContractPayDetaiByOrderIdlViewModel>();
                        double contract_pay_total_amount = listData_contract_pay.Sum(x => x.AmountPay);
                        if (amount > contract_pay_total_amount && data.PaymentStatus == (int)PaymentStatus.PAID)
                        {
                            data.PaymentStatus = (int)PaymentStatus.PAID_NOT_ENOUGH;
                            data.IsFinishPayment = 0;
                        }
                        if (amount == contract_pay_total_amount)
                        {
                            data.PaymentStatus = (int)PaymentStatus.PAID;
                            data.IsFinishPayment = 1;
                        }
                        data.Amount = amount;
                        data.Price = price;
                        data.Profit = profit - (data.Discount == null ? 0 : data.Discount);
                        //data.TotalFundCustomerCare = FundCustomerCare;
                        //data.Discount = discount;
                        data.Commission = commission;
                        //data.ProductService = string.Join(",", product_service);
                        data.UpdateLast = DateTime.Now;
                        data.UserUpdateId = user_id;
                        if (data.StartDate == null) data.StartDate = DateTime.Now;
                        if (data.EndDate == null) data.EndDate = DateTime.Now.AddHours(2);
                        // Update Order Status:
                        bool status_confirm = false;
                        //-- Case CANCEL:
                        bool has_other_than_cancel = 
                            (list_other_booking_all != null && list_other_booking_all.Count > 0 && list_other_booking_all.Any(x => x.Status != (int)ServiceStatus.Cancel));
                        bool atleast_has_one = 
                            (list_other_booking_all != null && list_other_booking_all.Count > 0);

                        if (!has_other_than_cancel && atleast_has_one && order_status_old == (int)OrderStatus2.OPERATOR_DECLINE)
                        {
                            data.OrderStatus = (int)OrderStatus2.CANCEL;
                            status_confirm = true;
                        }
                        //-- Case Waiting Accountant:
                        if (!status_confirm)
                        {
                            bool has_other_than_payment = 
                           (list_other_booking_all != null && list_other_booking_all.Count > 0 && list_other_booking_all.Any(x => x.Status != (int)ServiceStatus.Payment && x.Status != (int)ServiceStatus.Cancel));
                            atleast_has_one = false;
                            atleast_has_one = 
                                (list_other_booking != null && list_other_booking.Count > 0);

                            if (!has_other_than_payment && atleast_has_one && order_status_old == (int)OrderStatus2.WAITING_FOR_OPERATOR)
                            {
                                data.OrderStatus = (int)OrderStatus2.WAITING_FOR_ACCOUNTANT;
                                status_confirm = true;
                            }
                        }

                        _DbContext.Order.Update(data);
                        await _DbContext.SaveChangesAsync();
                        UpdateOrderOperator(OrderId);
                        var ListOrderBookClosing = await GetListOrderBookClosingByOrderId(OrderId);
                        DataTable dt = ListOrderBookClosing;
                        //if (dt != null && dt.Rows.Count > 0)
                        //{

                        //    var dataOrderBookClosing = dt.ToList<OrderBookClosingRequestViewModel>();

                        //    var OrderBookClosingModel = new OrderBookClosingViewModel
                        //    {
                        //        FromDateStr = dataOrderBookClosing[0].FromDate.ToString("dd/MM/yyyy"),
                        //        ToDateStr = dataOrderBookClosing[0].ToDate.ToString("dd/MM/yyyy"),
                        //        UserFinalize = (long)data.UserUpdateId,
                        //    };
                        //    var date = DateUtil.StringToDate(OrderBookClosingModel.ToDateStr);
                        //    OrderBookClosingModel.ToDate = ((DateTime)date).AddHours(23).AddMinutes(59).AddSeconds(59);
                        //    await OrderBookClosing(OrderBookClosingModel);
                        //    await UpdateBookClosingByOrderId(OrderId, 0, (long)data.UserUpdateId);
                        //}


                        return amount;
                    }
                    else return -1;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderAmount - OrderDal: " + ex);
            }
            return -2;

        }
        public async Task<DataTable> GetListOrderBookClosingByOrderId(long OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListOrderBookClosingByOrderId, objParam);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckBookClosingByDate - OrderDal. " + ex);
            }
            return null;
        }
        public int UpdateOrderOperator(long order_id)
        {
            try
            {
                SqlParameter[] objParam_order = new SqlParameter[1];
                objParam_order[0] = new SqlParameter("@Orderid", order_id);
                var id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOperatorByOrderid, objParam_order);
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderOperator - OrderDal: " + ex);
                return -1;
            }
        }
        public async Task<DataTable> GetContractPayByOrderId(long OrderId)
        {
            try
            {

                SqlParameter[] objParam_contractPay = new SqlParameter[1];
                objParam_contractPay[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetContractPayByOrderId, objParam_contractPay);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetContractPayByOrderId - ContractPayDAL. " + ex);
                return null;
            }
        }
        public async Task<DataTable> GetDetailOrderServiceByOrderId(int OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailOrderServiceByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderServiceByOrderId - OrderDal: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetDetailOrderByOrderId2(int OrderId)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@OrderId", OrderId);

                return _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetDetailOrderByOrderId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SP_GetDetailOrderByOrderId - OrderDal: " + ex);
            }
            return null;
        }
    }
}

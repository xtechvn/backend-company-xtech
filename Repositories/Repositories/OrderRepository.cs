using DAL;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Entities.OrderDetail;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDAL _OrderDal;
        private readonly ClientDAL _clientDAL;
        private readonly ContractPayDAL contractPayDAL;
        private readonly AllCodeDAL allCodeDAL;
        private readonly UserDAL userDAL;
        
        private readonly ContractPayDAL _contractPayDAL;
        public OrderRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _OrderDal = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            contractPayDAL = new ContractPayDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            allCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            userDAL = new UserDAL(dataBaseConfig.Value.SqlServer.ConnectionString);

            _clientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _contractPayDAL = new ContractPayDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        

        }

        public async Task<OrderDetailViewModel> GetOrderDetailByOrderId(long OrderId)
        {
            try
            {
                return await _OrderDal.GetDetailOrderByOrderId(OrderId);


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderDetailByOrderId - OrderRepository: " + ex);
            }
            return null;
        }
        public List<OrderViewModel> GetByClientId(long clientId, int payId = 0, int status = 0)
        {
            try
            {
                var listOrder = new List<OrderViewModel>();
                var listOrderOutput = new List<OrderViewModel>();
                var dt = _OrderDal.GetListOrderByClientId(clientId, StoreProcedureConstant.SP_GetDetailOrderByClientId, status);
                if (dt != null && dt.Rows.Count > 0)
                {
                    listOrder = (from row in dt.AsEnumerable()
                                 select new OrderViewModel
                                 {
                                     OrderId = row["OrderId"].ToString(),
                                     OrderCode = row["OrderNo"].ToString(),
                                     //StartDate = !row["StartDate"].Equals(DBNull.Value) ? Convert.ToDateTime(row["StartDate"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
                                     //EndDate = !row["EndDate"].Equals(DBNull.Value) ? Convert.ToDateTime(row["EndDate"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
                                     Status = row["OrderStatus"].ToString(),
                                     PaymentStatus = row["PaymentStatus"].ToString(),
                                     SalerName = row["SalerName"].ToString(),
                                     CreateDate = row["CreateTime"].Equals(DBNull.Value) ? "" : Convert.ToDateTime(row["CreateTime"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                     Amount = !row["Amount"].Equals(DBNull.Value) ? Convert.ToDouble(row["Amount"].ToString()) : 0,
                                     IsFinishPayment = Convert.ToInt32(row["IsFinishPayment"].ToString()),
                                     //StatusCode = Convert.ToInt32(row["StatusCode"].ToString()),
                                     //IsLock = row["IsLock"].Equals(DBNull.Value) ? null : Convert.ToInt32(row["IsLock"].ToString()),
                                 }).ToList();
                    //listOrder = listOrder.Where(n => n.IsLock != 1).ToList();
                    var listContractPayDetail = contractPayDAL.GetByContractDataIds(listOrder.Select(n => Convert.ToInt64(n.OrderId)).ToList());
                    foreach (var item in listOrder)
                    {
                        OrderViewModel orderViewModel = new OrderViewModel();
                        var detail = listContractPayDetail.Where(n => n.DataId != null
                                && n.DataId.Value == Convert.ToInt64(item.OrderId) && n.PayId == payId).FirstOrDefault();
                        var TotalDisarmed = listContractPayDetail.Where(n => n.DataId != null
                                && n.DataId.Value == Convert.ToInt64(item.OrderId)).ToList().Sum(n => n.Amount);
                        item.TotalDisarmed = (double)TotalDisarmed;
                        item.TotalAmount = item.Amount;
                        item.TotalNeedPayment = item.Amount - item.TotalDisarmed;
                        item.CopyProperties(orderViewModel);
                        if (detail != null)
                        {
                            orderViewModel.PayDetailId = detail.Id;
                            orderViewModel.IsChecked = true;
                            orderViewModel.Amount = (double)detail?.Amount;
                            orderViewModel.Payment = (double)detail?.Amount;
                        }

                        if (item.TotalNeedPayment > 0 || (item.Amount == 0 && item.IsFinishPayment == 0))
                        {
                            if (payId <= 0)
                                orderViewModel.Amount = item.TotalNeedPayment;
                            listOrderOutput.Add(orderViewModel);
                        }

                    }
                    if (payId != 0)
                    {
                        var allCode_ORDER_STATUS = allCodeDAL.GetListByType(AllCodeType.ORDER_STATUS);
                        var listOrderId = listOrderOutput.Select(n => Convert.ToInt64(n.OrderId)).ToList();
                        listContractPayDetail = contractPayDAL.GetByContractPayIds(new List<int>() { payId });
                        var listOrderDisable = listContractPayDetail.Where(n => !listOrderId.Contains(n.DataId.Value)).ToList();
                        foreach (var item in listOrderDisable)
                        {
                            OrderViewModel orderViewModel = new OrderViewModel();
                            var order = listOrder.FirstOrDefault(n => Convert.ToInt64(n.OrderId) == item.DataId);
                            if (order != null)
                            {
                                order.CopyProperties(orderViewModel);
                                orderViewModel.Amount = (double)item?.Amount;
                                orderViewModel.Payment = (double)item?.Amount;
                                orderViewModel.TotalDisarmed = order.Amount;
                                orderViewModel.TotalAmount = order.Amount;
                            }
                            else
                            {
                                var orderInfo = _OrderDal.GetByOrderId(item.DataId.Value);
                                if (orderInfo != null)
                                {
                                    orderViewModel.OrderId = orderInfo.OrderId.ToString();
                                    orderViewModel.OrderCode = orderInfo.OrderNo;
                                    //orderViewModel.StartDate = orderInfo.StartDate != null ?
                                    //    orderInfo.StartDate.Value.ToString("dd:MM:yyyy") : string.Empty;
                                    //orderViewModel.EndDate = orderInfo.EndDate != null ?
                                    //    orderInfo.EndDate.Value.ToString("dd:MM:yyyy") : string.Empty;
                                    orderViewModel.Status = allCode_ORDER_STATUS.FirstOrDefault(n => n.CodeValue == orderInfo.OrderStatus)?.Description;
                                    orderViewModel.SalerName = userDAL.GetById(orderInfo.SalerId != null ? orderInfo.SalerId.Value : 0).Result?.FullName;
                                    orderViewModel.Amount = (double)item?.Amount;
                                    orderViewModel.Payment = (double)item?.Amount;
                                    orderViewModel.TotalDisarmed = orderInfo.Amount.Value;
                                    orderViewModel.TotalAmount = orderInfo.Amount.Value;
                                }
                            }
                            orderViewModel.PayDetailId = item.Id;
                            orderViewModel.IsChecked = true;

                            orderViewModel.IsDisabled = true;
                            listOrderOutput.Add(orderViewModel);
                        }
                    }
                }
                return listOrderOutput;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByClientId - OrderRepository: " + ex);
            }
            return new List<OrderViewModel>();
        }
        public async Task<int> CreateOrder(Entities.Models.Order order)
        {
            try 
            {
                order.OrderNo = await GetOrderNo();
                return await _OrderDal.InsertOrder(order);
            }
            catch (Exception ex) 
            {
                LogHelper.InsertLogTelegram("CreateOrder - OrderRepository: " + ex);
                return -1;
            }
        }

        public async Task<GenericViewModel<OrderViewModel>> GetList(OrderViewSearchModel searchModel)
        {
            var model = new GenericViewModel<OrderViewModel>();

            try
            {
                DataTable dt = await _OrderDal.GetPagingList(searchModel, ProcedureConstants.GETALLORDER_SEARCH);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<OrderViewModel>();
                    model.ListData = data;
                    model.CurrentPage = searchModel.PageIndex;
                    model.PageSize = searchModel.pageSize;
                    model.TotalRecord = Convert.ToInt32(dt.Rows[0]["TotalRow"]);
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList - OrderRepository: " + ex);
            }
            return model;
        }
        public async Task<Entities.Models.Order> GetOrderByID(long id)
        {
            try
            {

                return _OrderDal.GetByOrderId(id);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderByID - OrderRepository: " + ex);
            }
            return null;
        }

        public async Task<string> GetOrderNo()
        {
            try
            {
                DataTable dt = await _OrderDal.GetOrderNo();
                if (dt != null && dt.Rows.Count > 0) 
                {
                    string? Id = dt.Rows[0]["OrderNo"] as string;

                    if (Id != null)
                    {
                        long newId = long.Parse(Id) + 1;
                        return newId.ToString();
                    }
                    else 
                    {
                        var year = DateTime.Now.Year;
                        var month = DateTime.Now.Month;
                        var day = DateTime.Now.Day;
                        var Code = $"{year}{month}{day}00001";
                        return Code;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderNo - OrderRepository: " + ex);
            }
            return null;
        }

        public async Task<double> GetTotalAmountByPaymentStatus(int? paymentStatus)
        {
            try
            {
                double dt = (double)await _OrderDal.getSumAmount(paymentStatus, ProcedureConstants.GET_TotalAmountByPaymentStatus);
                return dt;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalCountSumOrder in OrderRepository: " + ex);
            }
            return 0;
        }

        public async Task<int> UpdateAmountOrder(int OrderId)
        {
            try
            {
                return await _OrderDal.UpdateAmountOrder(OrderId);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateAmountOrder - OrderRepository: " + ex);
            }
            return -1;
        }

        public async Task<long> UpdateOrder(Entities.Models.Order model)
        {
            try
            {
                return await _OrderDal.UpdateOrder(model);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrder - OrderRepository: " + ex);
            }
            return -1;
        }
        public async Task<List<ProductServiceName>> ProductServiceName(string OrderId)
        {
            var ListData = new List<ProductServiceName>();
            try
            {

                DataTable dt = await _OrderDal.GetDetailOrderServiceByOrderId(Convert.ToInt32(OrderId));
                if (dt != null && dt.Rows.Count > 0)
                {
                    ListData = (from row in dt.AsEnumerable()
                                select new ProductServiceName
                                {
                                    OrderId = row["OrderId"].ToString(),
                                    ServiceName = row["ServiceName"].ToString(),
                                    StatusName = row["StatusName"].ToString(),
                                    ServiceId = row["ServiceId"].ToString(),
                                    Type = row["Type"].ToString(),
                                    Status = Convert.ToInt32(row["Status"].ToString()),
                                }).ToList();
                    
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ProductServiceName- OrderRepository: " + ex);
            }
            return ListData;
        }
        public async Task<double> UpdateOrderDetail(long OrderId, long user_id)
        {
            try
            {
                var result = await _OrderDal.UpdateOrderDetail(OrderId, user_id);
                var order = _OrderDal.GetByOrderId(OrderId);
                if (order == null || order.OrderId <= 0)
                {
                    return result;
                }
                //await UndoContractPayByOrderId(OrderId, (int)user_id);

                return result;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderAmount - OrderRepository: " + ex);
            }
            return -2;
        }

        public async Task<int> UpdateOrderStatus(long OrderId, long Status, long UpdatedBy, long UserVerify)
        {
            try
            {
                return await _OrderDal.UpdateOrderStatus(OrderId, Status, UpdatedBy, UserVerify);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderAmount - OrderRepository: " + ex);
            }
            return 0;
        }
        //public List<OrderViewModel> GetByClientId(long clientId, int payId = 0, int status = 0)
        //{
        //    try
        //    {
        //        var listOrder = new List<OrderViewModel>();
        //        var listOrderOutput = new List<OrderViewModel>();
        //        var dt = _OrderDal.GetListOrderByClientId(clientId, StoreProcedureConstant.SP_GetDetailOrderByClientId, status);
        //        if (dt != null && dt.Rows.Count > 0)
        //        {
        //            listOrder = (from row in dt.AsEnumerable()
        //                         select new OrderViewModel
        //                         {
        //                             OrderId = row["OrderId"].ToString(),
        //                             OrderCode = row["OrderNo"].ToString(),
        //                             StartDate = !row["StartDate"].Equals(DBNull.Value) ? Convert.ToDateTime(row["StartDate"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
        //                             EndDate = !row["EndDate"].Equals(DBNull.Value) ? Convert.ToDateTime(row["EndDate"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : "",
        //                             Status = row["OrderStatus"].ToString(),
        //                             PaymentStatus = row["PaymentStatus"].ToString(),
        //                             SalerName = row["SalerName"].ToString(),
        //                             CreateDate = row["CreateTime"].Equals(DBNull.Value) ? "" : Convert.ToDateTime(row["CreateTime"]).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        //                             Amount = !row["Amount"].Equals(DBNull.Value) ? Convert.ToDouble(row["Amount"].ToString()) : 0,
        //                             IsFinishPayment = Convert.ToInt32(row["IsFinishPayment"].ToString()),
        //                             StatusCode = Convert.ToInt32(row["StatusCode"].ToString()),
        //                             IsLock = row["IsLock"].Equals(DBNull.Value) ? null : Convert.ToInt32(row["IsLock"].ToString()),
        //                         }).ToList();
        //            listOrder = listOrder.Where(n => n.IsLock != 1).ToList();
        //            var listContractPayDetail = contractPayDAL.GetByContractDataIds(listOrder.Select(n => Convert.ToInt64(n.OrderId)).ToList());
        //            foreach (var item in listOrder)
        //            {
        //                OrderViewModel orderViewModel = new OrderViewModel();
        //                var detail = listContractPayDetail.Where(n => n.DataId != null
        //                        && n.DataId.Value == Convert.ToInt64(item.OrderId) && n.PayId == payId).FirstOrDefault();
        //                var TotalDisarmed = listContractPayDetail.Where(n => n.DataId != null
        //                        && n.DataId.Value == Convert.ToInt64(item.OrderId)).ToList().Sum(n => n.Amount);
        //                item.TotalDisarmed = (double)TotalDisarmed;
        //                item.TotalAmount = item.Amount;
        //                item.TotalNeedPayment = item.Amount - item.TotalDisarmed;
        //                item.CopyProperties(orderViewModel);
        //                if (detail != null)
        //                {
        //                    orderViewModel.PayDetailId = detail.Id;
        //                    orderViewModel.IsChecked = true;
        //                    orderViewModel.Amount = (double)detail?.Amount;
        //                    orderViewModel.Payment = (double)detail?.Amount;
        //                }

        //                if (item.TotalNeedPayment > 0 || (item.Amount == 0 && item.IsFinishPayment == 0))
        //                {
        //                    if (payId <= 0)
        //                        orderViewModel.Amount = item.TotalNeedPayment;
        //                    listOrderOutput.Add(orderViewModel);
        //                }

        //            }
        //            if (payId != 0)
        //            {
        //                var allCode_ORDER_STATUS = allCodeDAL.GetListByType(AllCodeType.ORDER_STATUS);
        //                var listOrderId = listOrderOutput.Select(n => Convert.ToInt64(n.OrderId)).ToList();
        //                listContractPayDetail = contractPayDAL.GetByContractPayIds(new List<int>() { payId });
        //                var listOrderDisable = listContractPayDetail.Where(n => !listOrderId.Contains(n.DataId.Value)).ToList();
        //                foreach (var item in listOrderDisable)
        //                {
        //                    OrderViewModel orderViewModel = new OrderViewModel();
        //                    var order = listOrder.FirstOrDefault(n => Convert.ToInt64(n.OrderId) == item.DataId);
        //                    if (order != null)
        //                    {
        //                        order.CopyProperties(orderViewModel);
        //                        orderViewModel.Amount = (double)item?.Amount;
        //                        orderViewModel.Payment = (double)item?.Amount;
        //                        orderViewModel.TotalDisarmed = order.Amount;
        //                        orderViewModel.TotalAmount = order.Amount;
        //                    }
        //                    else
        //                    {
        //                        var orderInfo = _OrderDal.GetByOrderId(item.DataId.Value);
        //                        if (orderInfo != null)
        //                        {
        //                            orderViewModel.OrderId = orderInfo.OrderId.ToString();
        //                            orderViewModel.OrderCode = orderInfo.OrderNo;
        //                            orderViewModel.StartDate = orderInfo.StartDate != null ?
                                      
        //                            orderViewModel.EndDate = orderInfo.EndDate != null ?
                                
        //                            orderViewModel.Status = allCode_ORDER_STATUS.FirstOrDefault(n => n.CodeValue == orderInfo.OrderStatus)?.Description;
        //                            orderViewModel.SalerName = userDAL.GetById(orderInfo.SalerId != null ? orderInfo.SalerId.Value : 0).Result?.FullName;
        //                            orderViewModel.Amount = (double)item?.Amount;
        //                            orderViewModel.Payment = (double)item?.Amount;
        //                            orderViewModel.TotalDisarmed = orderInfo.Amount.Value;
        //                            orderViewModel.TotalAmount = orderInfo.Amount.Value;
        //                        }
        //                    }
        //                    orderViewModel.PayDetailId = item.Id;
        //                    orderViewModel.IsChecked = true;

        //                    orderViewModel.IsDisabled = true;
        //                    listOrderOutput.Add(orderViewModel);
        //                }
        //            }
        //        }
        //        return listOrderOutput;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertLogTelegram("GetByClientId - OrderRepository: " + ex);
        //    }
        //    return new List<OrderViewModel>();
        //}
        public async Task<List<OrderServiceViewModel>> GetAllServiceByOrderId(long OrderId)
        {
            //var data = new List<OrderServiceViewModel>();
            try
            {
                DataTable dt = await _OrderDal.GetAllServiceByOrderId(OrderId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var listData = dt.ToList<OrderServiceViewModel>();
                    return listData;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderAmount - OrderRepository: " + ex);
            }
            return null;
        }
    }
}

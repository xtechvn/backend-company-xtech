using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class OrderRepositor : IOrderRepositor
    {

        private readonly OrderDAL OrderDAL;
        private readonly UserDAL UserDAL;
        private readonly AllCodeDAL AllCodeDAL;
        private readonly VoucherDAL VoucherDAL;
        private readonly ClientDAL ClientDAL;
        private readonly IContractPayRepository _contractPayRepository;

        
        public OrderRepositor(IOptions<DataBaseConfig> dataBaseConfig,  IContractPayRepository contractPayRepository)
        {

            OrderDAL = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            UserDAL = new UserDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            AllCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            VoucherDAL = new VoucherDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            ClientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
         
            _contractPayRepository = contractPayRepository;
        }
        public Order GetByOrderId(long OrderId)
        {
            try
            {

                var data = OrderDAL.GetByOrderId(OrderId);
                if (data != null)
                    return data;
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByOrderId - OrderRepositor: " + ex);
                return null;
            }
        }

        public async Task<GenericViewModel<OrderViewModel>> GetByClientId(long ClientId, int currentPage, int pageSize)
        {
            var model = new GenericViewModel<OrderViewModel>();
            try
            {
                List<OrderViewModel> ListOrder = new List<OrderViewModel>();
                var data = OrderDAL.GetByClientId(ClientId);
                var Client =await ClientDAL.GetClientDetail(ClientId);
                var List_data = data.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var allcode = await AllCodeDAL.GetListSortByName("SERVICE_TYPE");
                var allcode2 = await AllCodeDAL.GetListSortByName("ORDER_STATUS");
                var allcode3 = await AllCodeDAL.GetListSortByName("PAYMENT_STATUS");
                var allcode4 = await AllCodeDAL.GetListSortByName("PERMISION_TYPE");
                foreach (var item in List_data)
                {
                    var Order = new OrderViewModel();
                    if (item.SalerId != 0 && item.SalerId != null)
                    {
                        var User = await UserDAL.GetById((long)item.SalerId);
                        if (User != null)
                            Order.SalerName = User.FullName;
                    }
                    if (item.CreatedBy != 0 && item.CreatedBy != null)
                    {
                        var User = await UserDAL.GetById((long)item.CreatedBy);
                        if (User != null)
                        {
                            Order.CreateName = User.FullName;
                        }
                    }
                    foreach (var ac in allcode2)
                    {
                        if (item.OrderStatus == ac.CodeValue)
                        {
                            Order.Status = ac.Description;
                        }
                    }
                    foreach (var ac in allcode3)
                    {
                        if (item.PaymentStatus == ac.CodeValue)
                        {
                            Order.PaymentStatusName = ac.Description;
                        }
                    }
                    foreach (var ac in allcode4)
                    {
                        if (Client.PermisionType == ac.CodeValue)
                        {
                            Order.PermisionTypeName = ac.Description;
                        }
                    }
                    Order.OrderId = item.OrderId.ToString();
                    Order.OrderCode = item.OrderNo;
                    //if (item.StartDate != null)
                    //    Order.StartDate = ((DateTime)item.StartDate).ToString("dd/MM/yyyy HH:mm:ss");
                    //if (item.EndDate != null)
                    //    Order.EndDate = ((DateTime)item.EndDate).ToString("dd/MM/yyyy HH:mm:ss");

                    Order.CreateDate = ((DateTime)item.CreateTime).ToString("dd/MM/yyyy HH:mm:ss");
                    Order.Note = item.Label;
                    Order.Amount = (double)(item.Amount == null ? 0 : item.Amount);
                    if (item.Profit == null)
                    {
                        Order.Profit = 0;
                    }
                    else
                    {
                        Order.Profit = (double)item.Profit;
                    }
                    Order.StatusCode = (int)item.OrderStatus;


                    var databk = await _contractPayRepository.GetContractPayByOrderId(Convert.ToInt32(Order.OrderId));
                    if (databk != null)
                    {
                        var sumdata = databk.Sum(s => s.AmountPay);
                        Order.TotalAmount += Convert.ToInt32(sumdata);

                    }

                    ListOrder.Add(Order);
                }
                model.ListData = ListOrder;
                model.PageSize = pageSize;
                model.CurrentPage = currentPage;
                model.TotalRecord = data.Count;
                model.TotalPage = (int)Math.Ceiling((double)data.Count / pageSize);
                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByClientId - OrderRepositor: " + ex);
                return null;
            }
        }
        public async Task<string> FindByVoucherid(int voucherId)
        {
            try
            {
                return await VoucherDAL.FindByVoucherid(voucherId);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("FindByVoucherCode - VoucherDAL: " + ex.ToString());
                return null;
            }
        }
        public async Task<int> UpdateOrderStatus(long OrderId, long Status, long UpdatedBy, long UserVerify)
        {
            try
            {
                return await OrderDAL.UpdateOrderStatus(OrderId, Status, UpdatedBy, UserVerify);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateOrderAmount - OrderRepository: " + ex);
            }
            return 0;
        }
        public async Task<List<ProductServiceName>> ProductServiceName(string OrderId)
        {
            var ListData = new List<ProductServiceName>();
            try
            {

                DataTable dt = await OrderDAL.GetDetailOrderServiceByOrderId(Convert.ToInt32(OrderId));
                if (dt != null && dt.Rows.Count > 0)
                {
                    ListData = (from row in dt.AsEnumerable()
                                select new ProductServiceName
                                {
                                    OrderId = row["OrderId"].ToString(),
                                    ServiceName = row["ServiceName"].ToString(),
                                    StatusName = row["StatusName"].ToString(),
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
        public async Task<List<OrderDetailViewModel>> GetDetailOrderByOrderId(int OrderId)
        {
            var ListData = new List<OrderDetailViewModel>();
            try
            {

                DataTable dt = await OrderDAL.GetDetailOrderByOrderId2(OrderId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    ListData = dt.ToList<OrderDetailViewModel>();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailOrderByOrderId- OrderRepository: " + ex);
            }
            return ListData;
        }
    }
}

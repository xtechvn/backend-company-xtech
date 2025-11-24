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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDAL _OrderDal;
        public OrderRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _OrderDal = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
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
    }
}

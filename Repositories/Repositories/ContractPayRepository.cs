using Aspose.Cells;
using DAL.StoreProcedure;
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
using Utilities.Contants;
using System.Drawing;
using Utilities;
using Entities.ViewModels.Funding;
using DAL.Funding;
using static Utilities.DepositHistoryConstant;

namespace Repositories.Repositories
{
    public class ContractPayRepository : IContractPayRepository
    {
        private readonly ContractPayDAL _contractPayDAL;
        private readonly AllCodeDAL allCodeDAL;
        private readonly OrderDAL orderDAL;
        private readonly DepositHistoryDAL depositHistoryDAL;
        private readonly BankingAccountDAL bankingAccountDAL;
        private readonly ClientDAL clientDAL;
        private readonly UserDAL userDAL;
        //private readonly SupplierDAL supplierDAL;

        public ContractPayRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            allCodeDAL = new AllCodeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            _contractPayDAL = new ContractPayDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            clientDAL = new ClientDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            orderDAL = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            depositHistoryDAL = new DepositHistoryDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            bankingAccountDAL = new BankingAccountDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            userDAL = new UserDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            //supplierDAL = new SupplierDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<List<ContractPayDetaiByOrderIdlViewModel>> GetContractPayByOrderId(long OrderId)
        {
            try
            {

                DataTable data = await _contractPayDAL.GetContractPayByOrderId(OrderId);
                var listData = data.ToList<ContractPayDetaiByOrderIdlViewModel>();
                if (listData.Count > 0)
                {
                    return listData;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetContractPayByOrderId - ContractPayDAL. " + ex);
            }
            return null;
        }
        public double GetTotalAmountContractPayByServiceId(string ServiceId, long ServiceType, long ContractPayType)
        {
            try
            {

                return _contractPayDAL.GetTotalAmountContractPayByServiceId(ServiceId, ServiceType, ContractPayType);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetServiceDetail - ContractPayRepository: " + ex);
            }
            return 0;
        }
        public List<ContractPayViewModel> GetListContractPay(ContractPaySearchModel searchModel, out long total, int currentPage = 1, int pageSize = 20)
        {
            total = 0;
            //var listContractPays = _contractPayDAL.GetContractPays(searchModel, out total, currentPage, pageSize);
            //var dt = _contractPayDAL.GetPagingList(searchModel, currentPage, pageSize, StoreProcedureConstant.SP_GetListContractPay);
            var listContractPays = _contractPayDAL.GetPagingList(searchModel, currentPage, pageSize,
                StoreProcedureConstant.SP_GetListContractPay).ToList<ContractPayViewModel>();
            if (listContractPays.Count > 0)
                total = listContractPays.FirstOrDefault().TotalRow;
            var listContractPayView = new List<ContractPayViewModel>();
            var listContractPayDetail = _contractPayDAL.GetByContractPayIds(listContractPays.Select(n => n.PayId).ToList());
            var orderList = orderDAL.GetByOrderIds(listContractPayDetail.Where(n => n.DataId != null).Select(n => n.DataId.Value).ToList());
            var depositHistoryList = depositHistoryDAL.GetByIds(listContractPayDetail.Where(n => n.DataId != null).Select(n => (int)n.DataId.Value).ToList());
            var userList = userDAL.GetByIds(listContractPays.Select(n => (long)n.CreatedBy).ToList()).Result;
            foreach (var item in listContractPays)
            {
                if (string.IsNullOrEmpty(item.UserName))
                {
                    var user = userList.FirstOrDefault(n => n.Id == item.CreatedBy);
                    item.UserName = user?.FullName;
                }
                ContractPayViewModel contractPayViewModel = new ContractPayViewModel();
                item.CopyProperties(contractPayViewModel);
                var contractPayDetail = listContractPayDetail.Where(n => n.PayId == item.PayId).ToList();
                var dataIds = contractPayDetail.Select(n => n.DataId).ToList();
                contractPayViewModel.TotalDeposit = item.TotalDeposit = (double)contractPayDetail.Where(n => n.Amount != null).Sum(n => n.Amount.Value);
                contractPayViewModel.DataContent = new List<CountStatus>();
                contractPayViewModel.DataIds = new List<long>();
                if (!string.IsNullOrEmpty(item.PayDetailId))
                {
                    item.DataIds = item.PayDetailId.Split(",").Select(n => Convert.ToInt64(n)).ToList();
                }
                if (!string.IsNullOrEmpty(item.PayDetail))
                {
                    item.DataNo = item.PayDetail.Split(",").Select(n => n).ToList();
                }

                if (item.Type == (int)CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG)
                {
                    var orders = orderList.Where(n => dataIds.Contains(n.OrderId)).ToList();
                    contractPayViewModel.DataIds = orders.Select(n => n.OrderId).ToList();
                    foreach (var order in orders)
                    {
                        CountStatus countStatus = new CountStatus();
                        countStatus.DataId = order.OrderId;
                        countStatus.DataNo = order.OrderNo;
                        contractPayViewModel.DataContent.Add(countStatus);
                    }
                }
                if (item.Type == (int)CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY)
                {
                    var depositHistories = depositHistoryList.Where(n => dataIds.Contains(n.Id)).ToList();
                    foreach (var depositHistory in depositHistories)
                    {
                        contractPayViewModel.DataIds = depositHistories.Select(n => (long)n.Id).ToList();
                        CountStatus countStatus = new CountStatus();
                        countStatus.DataId = depositHistory.Id;
                        countStatus.DataNo = depositHistory.TransNo;
                        contractPayViewModel.DataContent.Add(countStatus);
                    }
                }
                if (item.Type == (int)CONTRACT_PAY_TYPE.THU_TIEN_NCC_HOAN_TRA || item.Type == (int)CONTRACT_PAY_TYPE.THU_TIEN_HOA_HONG_NCC)
                {
                    if (item.DataNo != null && item.DataNo.Count > 0)
                    {
                        foreach (var serviceCode in item.DataNo)
                        {
                            var serviceInfo = GetServiceDetail(serviceCode);
                            if (serviceInfo != null)
                            {
                                CountStatus countStatus = new CountStatus();
                                countStatus.DataId = serviceInfo.ServiceId;
                                countStatus.DataIdFly = serviceInfo.GroupBookingId;
                                countStatus.DataNo = serviceCode;
                                countStatus.ServiceType = serviceInfo.ServiceType;
                                contractPayViewModel.DataContent.Add(countStatus);
                            }
                        }
                    }
                }
                listContractPayView.Add(contractPayViewModel);
            }
            return listContractPayView;
        }
        public PaymentRequestViewModel GetServiceDetail(string serviceCode)
        {
            try
            {
                var listServiceOutput = _contractPayDAL.GetServiceDetail(serviceCode,
                    StoreProcedureConstant.SP_GetAllServiceByServiceCode).ToList<PaymentRequestViewModel>();
                return listServiceOutput.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetServiceDetail - ContractPayRepository: " + ex);
            }
            return null;
        }
        public ContractPayViewModel GetByContractPayId(int contractPayId)
        {
            ContractPayViewModel contractPayViewModel = new ContractPayViewModel();
            var contractPay = _contractPayDAL.GetById(contractPayId);
            contractPay.CopyProperties(contractPayViewModel);
            var listContractPayView = new List<ContractPayViewModel>();
            var allCode_PAY_TYPE = allCodeDAL.GetListByType(AllCodeType.PAY_TYPE);
            var allCode_CONTRACT_PAY_TYPE = allCodeDAL.GetListByType(AllCodeType.CONTRACT_PAY_TYPE);
            var allCode_DEPOSITHISTORY_TYPE = allCodeDAL.GetListByType(AllCodeType.DEPOSITHISTORY_TYPE);
            var allCode_DEPOSIT_STATUS = allCodeDAL.GetListByType(AllCodeType.DEPOSIT_STATUS);
            var allCode_SERVICE_TYPE = allCodeDAL.GetListByType(AllCodeType.SERVICE_TYPE);
            var allCode_ORDER_STATUS = allCodeDAL.GetListByType(AllCodeType.ORDER_STATUS);
            var listAccountClient = new List<Client>();
            if (contractPay.CreatedBy != null)
                listAccountClient = clientDAL.GetClientInfo(new List<long>() { (long)contractPay.CreatedBy }).Result;
            //if (contractPay.SupplierId != null && contractPay.SupplierId > 0)
            //{
            //    var Supplier = supplierDAL.GetById((long)contractPay.SupplierId);
            //    contractPayViewModel.SupplierName = Supplier.FullName;
            //}
            var listClient = clientDAL.GetClientByIds(new List<long>() { (long)(contractPay.ClientId != null ? contractPay.ClientId.Value : 0) });
            var listContractPayDetail = _contractPayDAL.GetByContractPayIds(new List<int>() { contractPay.PayId });
            var orderList = orderDAL.GetByOrderIds(listContractPayDetail.Where(n => n.DataId != null).Select(n => n.DataId.Value).ToList());
            var depositHistoryList = depositHistoryDAL.GetByIds(listContractPayDetail.Where(n => n.DataId != null).Select(n => (int)n.DataId.Value).ToList());
            contractPayViewModel.TypeStr = allCode_CONTRACT_PAY_TYPE.FirstOrDefault(n => n.CodeValue == contractPay.Type)?.Description;
            contractPayViewModel.PayTypeStr = allCode_PAY_TYPE.FirstOrDefault(n => n.CodeValue == contractPay.PayType)?.Description;
            var accountClient = listAccountClient.FirstOrDefault(n => n.ClientMapId == contractPay.CreatedBy);
            contractPayViewModel.CreatedByName = accountClient?.ClientName;

            if (string.IsNullOrEmpty(contractPayViewModel.CreatedByName))
            {
                var user = userDAL.GetById((int)contractPay.CreatedBy).Result;
                contractPayViewModel.CreatedByName = user?.FullName;
            }
            var client = listClient.FirstOrDefault(n => n.Id == contractPay.ClientId);
            if (client != null)
                contractPayViewModel.ClientName = client?.ClientName + " - " + client?.Email + " - " + client?.Phone;
            var bankingAccount = bankingAccountDAL.GetById(contractPay.BankingAccountId != null ? contractPay.BankingAccountId.Value : 0);
            contractPayViewModel.BankAccount = bankingAccount?.AccountNumber;
            contractPayViewModel.BankName = bankingAccount?.BankId;
            var contractPayDetail = listContractPayDetail.Where(n => n.PayId == contractPay.PayId).ToList();
            contractPayViewModel.TotalDeposit = (double)contractPayDetail.Where(n => n.Amount != null).Sum(n => n.Amount.Value);
            contractPayViewModel.RelateData = new List<ContractPayDetailViewModel>();
            foreach (var item in contractPayDetail)
            {
                ContractPayDetailViewModel model = new ContractPayDetailViewModel();
                model.ContractPay = contractPay;
                model.ContractPayDetail = item;
                if (contractPay.Type == (int)CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG)
                {
                    //var contractPayDetails = _contractPayDAL.GetByContractDataIds(new List<long>() { (long)item.DataId });
                    //model.ContractPayDetail.Amount = contractPayDetails.Sum(n => n.Amount);
                    model.Order = new OrderViewModel();
                    var order = orderDAL.GetByOrderId(item.DataId != null ? item.DataId.Value : 0);
                    if (order != null)
                    {
                        OrderViewModel orderViewModel = new OrderViewModel();
                        orderViewModel.OrderId = order.OrderId.ToString();
                        orderViewModel.OrderCode = order.OrderNo;
                        //if (order.StartDate != null)
                        //    orderViewModel.StartDate = order.StartDate.Value.ToString("dd/MM/yyyy");
                        //if (order.EndDate != null)
                        //    orderViewModel.EndDate = order.EndDate.Value.ToString("dd/MM/yyyy");
                        orderViewModel.Status = allCode_ORDER_STATUS.FirstOrDefault(n => n.CodeValue == order.OrderStatus)?.Description;
                        orderViewModel.Amount = order.Amount != null ? order.Amount.Value : 0;
                        if (order.SalerId != null)
                        {
                            var accountClientOrder = userDAL.GetById((long)order.SalerId).Result;
                            orderViewModel.CreateName = accountClientOrder?.FullName;
                        }
                        model.Order = orderViewModel;
                    }
                }
                if (contractPay.Type == (int)CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY)
                {
                    var deposit = depositHistoryDAL.GetById(item.DataId != null ? (int)item.DataId.Value : 0);
                    if (deposit != null)
                    {
                        var listAccountClientDetail = clientDAL.GetClientInfo(new List<long>() { (long)deposit?.UserId }).Result;
                        var accountClientDetail = listAccountClientDetail.FirstOrDefault(n => n.ClientMapId == deposit.UserId);
                        DepositFunding depositFunding = new DepositFunding();
                        deposit.CopyProperties(depositFunding);
                        depositFunding.ServiceTypeStr = allCode_SERVICE_TYPE.FirstOrDefault(n => n.CodeValue == deposit?.ServiceType)?.Description;
                        depositFunding.StatusStr = allCode_DEPOSIT_STATUS.FirstOrDefault(n => n.CodeValue == deposit?.Status)?.Description;
                        depositFunding.CreatedBy = accountClientDetail?.ClientName;
                        depositFunding.Email = accountClientDetail?.Email;
                        depositFunding.TransNo = deposit?.TransNo;
                        model.DepositHistory = depositFunding;
                    }
                }
                contractPayViewModel.RelateData.Add(model);
            }
            return contractPayViewModel;
        }
        public ContractPayViewModel GetByPayId(int contractPayId)
        {
            var contractPayViewModel = _contractPayDAL.GetByPayId(contractPayId, StoreProcedureConstant.sp_GetDetailContractPay).ToList<ContractPayViewModel>().FirstOrDefault();
            if (contractPayViewModel.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG)
            {
                var listOrderByPayId = _contractPayDAL.GetDetailContractPayById(contractPayId,
                    StoreProcedureConstant.sp_GetListOrderByPayId).ToList<ContractPayViewModel>();
                contractPayViewModel.ContractPayDetail = listOrderByPayId;
            }

            if (contractPayViewModel.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY)
            {
                var listDepositHistoryByPayId = _contractPayDAL.GetDetailContractPayById(contractPayId,
                    StoreProcedureConstant.sp_GetListDepositHistoryByPayId).ToList<ContractPayViewModel>();
                contractPayViewModel.ContractPayDetail = listDepositHistoryByPayId;
            }

            if (contractPayViewModel.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_HOA_HONG_NCC ||
                contractPayViewModel.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_NCC_HOAN_TRA)
            {
                var listSubServiceByPayId = _contractPayDAL.GetDetailContractPayById(contractPayId,
                    StoreProcedureConstant.SP_GetListSubServiceByPayId).ToList<ContractPayViewModel>();
                var listServiceByPayId = _contractPayDAL.GetDetailContractPayById(contractPayId,
                   StoreProcedureConstant.SP_GetListServiceByPayId).ToList<ContractPayViewModel>();
                foreach (var item in listServiceByPayId)
                {
                    item.ServiceIdParent = item.ServiceId;
                    listSubServiceByPayId.Add(item);
                }
                listSubServiceByPayId.AsParallel().ForAll(item =>
                {
                    if (item.ServiceType == (int)SubServiceType.Tour)
                        item.ServiceType = (int)ServiceType.Tour;
                    if (item.ServiceType == (int)SubServiceType.PRODUCT_FLY_TICKET)
                        item.ServiceType = (int)ServiceType.PRODUCT_FLY_TICKET;
                    if (item.ServiceType == (int)SubServiceType.BOOK_HOTEL_ROOM_VIN)
                        item.ServiceType = (int)ServiceType.BOOK_HOTEL_ROOM_VIN;
                    if (item.ServiceType == (int)SubServiceType.Other)
                        item.ServiceType = (int)ServiceType.Other;
                });
                contractPayViewModel.ContractPayDetail = listSubServiceByPayId;
            }

            return contractPayViewModel;
        }
        public int CreateContractPay(ContractPayViewModel model)
        {
            var entity = _contractPayDAL.GetByBillNo(model.BillNo);
            if (entity != null)
                return -2;
            return _contractPayDAL.CreateContractPay(model);
        }
        public long CountPaymentRequest()
        {
            return _contractPayDAL.CountPaymentRequest();
        }

        public int UpdateContractPay(ContractPayViewModel model)
        {
            return _contractPayDAL.UpdateContractPay(model);
        }
    }
}

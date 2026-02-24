using DAL;
using DAL.Funding;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class PaymentRequestRepository : IPaymentRequestRepository
    {
        private readonly PaymentRequestDAL paymentRequestDAL;
        private readonly OrderDAL orderDAL;
        public PaymentRequestRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            paymentRequestDAL = new PaymentRequestDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
            orderDAL = new OrderDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }
        public int CreatePaymentRequest(PaymentRequestViewModel model)
        {
            var paymentRequest = paymentRequestDAL.GetByPaymentCode(model.PaymentCode);
            if (paymentRequest != null)
                return -2;
            return paymentRequestDAL.CreatePaymentRequest(model);
        }

        public List<PaymentRequestViewModel> GetPaymentRequests(PaymentRequestSearchModel searchModel, out long total, int currentPage = 1, int pageSize = 20)
        {
            total = 0;
            try
            {
                var listPaymentRequests = paymentRequestDAL.GetPagingList(searchModel, currentPage, pageSize,
                StoreProcedureConstant.SP_GetListPaymentRequest).ToList<PaymentRequestViewModel>();
                if (listPaymentRequests.FirstOrDefault() != null)
                    total = listPaymentRequests.FirstOrDefault().TotalRow;
                var listServiceFlyStr = new List<string>();
                foreach (var item in listPaymentRequests)
                {
                    if (!string.IsNullOrEmpty(item.PaymentVoucherNo))
                        item.PaymentVoucherNo = item.PaymentVoucherNo.Replace(",", "").Trim();
                    if (string.IsNullOrEmpty(item.ListServiceCode)) continue;
                    var listServiceCode = item.ListServiceCode.Split(",");
                    foreach (string serviceCode in listServiceCode)
                    {
                        listServiceFlyStr.Add(serviceCode);
                    }
                }
                //var listServiceFly = flyBookingDetailDAL.GetByServiceCodes(listServiceFlyStr);
                //var listPaymentVoucher = paymentVoucherDAL.GetByPaymentCodes(listPaymentRequests.Where(n => !string.IsNullOrEmpty(n.PaymentVoucherNo)).Select(n => n.PaymentVoucherNo.Replace(",", " ")).ToList());
                var listOrder = orderDAL.GetByOrderNos(listPaymentRequests.Where(n => !string.IsNullOrEmpty(n.OrderNo)).Select(n => n.OrderNo).ToList());
                foreach (var item in listPaymentRequests)
                {
                    //if (!string.IsNullOrEmpty(item.PaymentVoucherNo))
                    //{
                    //    var paymentVoucher = listPaymentVoucher.FirstOrDefault(n => item.PaymentVoucherNo.Replace(",", "").Contains(n.PaymentCode));
                    //    item.PaymentVoucherId = paymentVoucher != null ? paymentVoucher.Id : 0;
                    //}
                    if (!string.IsNullOrEmpty(item.OrderNo))
                    {
                        var order = listOrder.FirstOrDefault(n => n.OrderNo == item.OrderNo);
                        item.OrderId = order != null ? (int)order.OrderId : 0;
                    }
                    if (string.IsNullOrEmpty(item.ListServiceCode)) continue;
                    var listServiceCode = item.ListServiceCode.Split(",");
                    var listServiceId = item.ListServiceId.Split(",");
                    item.ListServiceCodeAndType = new List<CountStatus>();
                    int index = 0;
                    foreach (var serviceCode in listServiceCode)
                    {
                        CountStatus countStatus = new CountStatus();
                        //var flyService = listServiceFly.FirstOrDefault(n => n.ServiceCode == serviceCode);
                        countStatus.DataNo = serviceCode;
                        countStatus.DataId = int.Parse(listServiceId[index]);
                        //if (flyService != null)
                        //{
                        //    countStatus.DataIdFly = flyService.GroupBookingId.Replace(",", "_");
                        //}
                        if (serviceCode.Contains("TOUR"))
                            countStatus.ServiceType = (int)ServiceType.Tour;
                        if (serviceCode.Contains("FLIGHT"))
                            countStatus.ServiceType = (int)ServiceType.PRODUCT_FLY_TICKET;
                        if (serviceCode.Contains("HOTEL"))
                            countStatus.ServiceType = (int)ServiceType.BOOK_HOTEL_ROOM;
                        if (serviceCode.Contains("OTHER"))
                            countStatus.ServiceType = (int)ServiceType.Other;
                        if (serviceCode.Contains("VINWONDER"))
                            countStatus.ServiceType = (int)ServiceType.VinWonder;
                        item.ListServiceCodeAndType.Add(countStatus);
                        index++;
                    }
                }
                return listPaymentRequests;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPaymentRequests - PaymentRequestRepository: " + ex);
                return new List<PaymentRequestViewModel>();
            }
        }
        public int ApprovePaymentRequest(string paymentRequestNo, int userId, int status)
        {
            var entity = paymentRequestDAL.GetByRequestNo(paymentRequestNo);
            entity.UpdatedBy = userId;
            entity.Status = status;
            return paymentRequestDAL.ApproveRequest(entity);
        }
        public PaymentRequest GetByRequestNo(string paymentRequestNo)
        {
            try
            {
                return paymentRequestDAL.GetByRequestNo(paymentRequestNo);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByRequestNo - PaymentRequestRepository: " + ex);
                return null;
            }
        }
        public PaymentRequestViewModel GetById(int paymentRequestId)
         {
            try
            {
                var requestInfos = paymentRequestDAL.GetRequestDetail(paymentRequestId,
                    StoreProcedureConstant.sp_GetDetailPaymentRequest).ToList<PaymentRequestViewModel>();
                var requestInfo = requestInfos.FirstOrDefault();
                //if (requestInfo.PaymentVoucherId != null && requestInfo.PaymentVoucherId != 0)
                //{
                //    requestInfo.AttachFile = attachFileDAL.GetListByDataID(requestInfo.PaymentVoucherId, (int)AttachmentType.Payment_Voucher).Result;
                //}
                requestInfo.PaymentDateStr = DateUtil.DateToString(requestInfo.PaymentDate);
                requestInfo.RelateData = new List<PaymentRequestDetailViewModel>();
                if (requestInfo.Type == (int)PAYMENT_VOUCHER_TYPE.HOAN_TRA_KHACH_HANG)
                {
                    foreach (var item in requestInfos)
                    {
                        PaymentRequestDetailViewModel model = new PaymentRequestDetailViewModel();
                        item.CopyProperties(model);
                        model.OrderId = item.OrderId;
                        model.OrderNo = item.OrderNo;
                        model.Amount = item.Amount;
                        model.OrderAmount = item.OrderAmount;
                        model.OrderAmountPay = item.OrderAmountPay;
                        model.ServiceId = (int)item.ServiceId;
                        model.UserCreateFullName = item.UserCreateFullName;
                        model.DepartmentName = item.DepartmentName;
                        model.ServiceId = (int)item.ServiceId;
                        model.BankIdName = item.BankIdName;
                        model.AccountNumber = item.AccountNumber;
                        model.ServiceId = (int)item.ServiceId;
                        var serviceInfo = paymentRequestDAL.GetDetailServiceById(item.ServiceId, item.ServiceType,
                          StoreProcedureConstant.Sp_GetDetailServiceById).ToList<PaymentRequestViewModel>().FirstOrDefault();
                        model.ServiceAmount = serviceInfo != null ? serviceInfo.Amount : 0;
                        model.ServicePrice = serviceInfo != null ? serviceInfo.Price : 0;
                        model.CreatedBy = item.CreatedBy;
                        requestInfo.RelateData.Add(model);
                    }
                }
                if (requestInfo.Type == (int)PAYMENT_VOUCHER_TYPE.THANH_TOAN_DICH_VU || requestInfo.Type == (int)PAYMENT_VOUCHER_TYPE.CHI_PHI_MARKETING)
                {
                    var requestServiceDetails = paymentRequestDAL.GetRequestDetail(paymentRequestId,
                        StoreProcedureConstant.sp_GetAllServiceByRequestiD).ToList<PaymentRequestViewModel>();
                    foreach (var item in requestServiceDetails)
                    {
                        PaymentRequestDetailViewModel model = new PaymentRequestDetailViewModel();
                        item.CopyProperties(model);
                        model.OrderId = item.OrderId;
                        model.ServiceId = (int)item.ServiceId;
                        var serviceInfo = paymentRequestDAL.GetDetailServiceById(item.ServiceId, item.ServiceType,
                        StoreProcedureConstant.Sp_GetDetailServiceById).ToList<PaymentRequestViewModel>().FirstOrDefault();
                        model.ServiceAmount = serviceInfo != null ? serviceInfo.Amount : 0;
                        double servicePrice = 0;
                        //if (requestInfo.Type == (int)PAYMENT_VOUCHER_TYPE.THANH_TOAN_DICH_VU)
                        //{
                        //    servicePrice = GetAmontRequestForSupplier(item.ServiceId, item.ServiceType, requestInfo.SupplierId.Value, serviceInfo.Price);
                        //}
                        //model.ServicePrice = servicePrice;
                        model.CreatedBy = requestInfo.CreatedBy;
                        requestInfo.RelateData.Add(model);
                    }
                }
                //foreach (var item in requestInfo.RelateData)
                //{
                //    var serviceInfo = paymentRequestDAL.GetDetailServiceById(item.ServiceId, item.ServiceType,
                //        StoreProcedureConstant.sp_GetAllServiceByRequestiD).ToList<PaymentRequestViewModel>().FirstOrDefault();
                //    item.ServiceAmount = serviceInfo != null ? serviceInfo.Amount : 0;
                //    item.ServicePrice = serviceInfo != null ? serviceInfo.Price : 0;
                //}
                requestInfo.IsIncludeService = requestInfo.IsServiceIncluded == true ? 1 : 0;
                return requestInfo;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - PaymentRequestRepository: " + ex);
                return null;
            }
        }
        public List<PaymentRequestViewModel> GetRequestByClientId(long clientId, long orderid = 0)
        {
            try
            {
                var listPaymentRequest = paymentRequestDAL.GetListPaymentRequestByClientId(clientId, 3,
                    StoreProcedureConstant.SP_GetListPaymentRequestByClientId).ToList<PaymentRequestViewModel>();
                foreach (var item in listPaymentRequest)
                {
                    item.ListServiceCodeAndType = new List<CountStatus>();
                    //if (!string.IsNullOrEmpty(item.PaymentVoucherCode))
                    //{
                    //    var listPaymentVoucher = paymentVoucherDAL.GetByPaymentCodes(item.PaymentVoucherCode.Split(",").ToList());
                    //    foreach (var paymentVoucher in listPaymentVoucher)
                    //    {
                    //        CountStatus countStatus = new CountStatus();
                    //        countStatus.DataNo = paymentVoucher.PaymentCode;
                    //        countStatus.DataId = paymentVoucher.Id;
                    //        item.ListServiceCodeAndType.Add(countStatus);
                    //    }
                    //}
                }
                if (orderid > 0)
                {
                    var listTemp = new List<PaymentRequestViewModel>();
                    foreach (var item in listPaymentRequest)
                    {
                        var requestDetails = paymentRequestDAL.GetByPaymentRequestId((int)item.Id);
                        if (requestDetails.FirstOrDefault(n => n.OrderId == orderid) != null)
                            listTemp.Add(item);
                    }
                    listPaymentRequest = listTemp;
                }
                return listPaymentRequest;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByServiceId - PaymentRequestRepository: " + ex);
            }
            return new List<PaymentRequestViewModel>();
        }

        public List<CountStatus> GetCountStatus(PaymentRequestSearchModel searchModel)
        {
            try
            {
                var listPaymentRequests = paymentRequestDAL.GetCountStatus(searchModel,
                StoreProcedureConstant.SP_CountPaymentRequestByStatus).ToList<CountStatus>();
                return listPaymentRequests;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetCountStatus - PaymentRequestRepository: " + ex);
                return new List<CountStatus>();
            }
        }
        public int UpdatePaymentRequest(PaymentRequestViewModel model)
        {
            var entity = paymentRequestDAL.GetById2(model.Id);
            var result = paymentRequestDAL.UpdatePaymentRequest(model);
            if (result > 0 && entity.Status == (int)PAYMENT_REQUEST_STATUS.TU_CHOI && model.Amount != entity.Amount && model.IsServiceIncluded.Value)
            {
                var detailRequests = paymentRequestDAL.GetByPaymentRequestId((int)model.Id);
                foreach (var item in detailRequests)
                {
                    item.Amount = model.Amount;
                    paymentRequestDAL.UpdateRequestDetail(item);
                }
            }
            if (result > 0 && model.IsAdminEdit && model.Amount != entity.Amount && model.IsServiceIncluded.Value)
            {
                var detailRequests = paymentRequestDAL.GetByPaymentRequestId((int)model.Id);
                foreach (var item in detailRequests)
                {
                    item.Amount = model.Amount;
                    paymentRequestDAL.UpdateRequestDetail(item);
                }
                //tính lại số tiền phiếu chi nếu có
                var amountChange = entity.Amount - model.Amount;
                //var paymentVoucher = paymentVoucherDAL.CheckExistsPaymentRequest(new List<long>() { model.Id },
                //    StoreProcedureConstant.SP_CheckExistsPaymentVoucherByRequestId);
                //if (paymentVoucher.Count > 0)
                //{
                //    foreach (var item in paymentVoucher)
                //    {
                //        item.Amount = item.Amount - amountChange;
                //        paymentVoucherDAL.Update(item);
                //    }
                //}
            }
            return result;
        }
        public List<PaymentRequestViewModel> GetServiceListBySupplierId(long supplierId, int requestId = 0, int serviceId = 0)
        {
            try
            {
                var listService = new List<PaymentRequestViewModel>();
                var listServiceOutput = paymentRequestDAL.GetServiceListBySupplierId(supplierId,
                    StoreProcedureConstant.SP_GetAllServiceBySupplierId).ToList<PaymentRequestViewModel>();
                var listServiceId = listServiceOutput.Select(n => Convert.ToInt64(n.ServiceId)).ToList();
                var listRequestDetail = paymentRequestDAL.GetByDataIds(listServiceId);
                if (requestId == 0)
                    listRequestDetail = listRequestDetail.Where(n => n.Status != (int)PAYMENT_REQUEST_STATUS.TU_CHOI).ToList();
                if (serviceId != 0)
                    listServiceOutput = listServiceOutput.Where(n => n.ServiceId == serviceId).ToList();
                foreach (var item in listServiceOutput)
                {
                    item.TotalAmount = item.Amount;
                    var detail = listRequestDetail.Where(n => n.OrderId == item.OrderId && n.RequestId == requestId && n.ServiceId == item.ServiceId).FirstOrDefault();
                    if (detail != null)
                    {
                        item.IsChecked = true;
                        item.Id = detail.Id;
                        item.AmountPayment = detail.Amount;
                    }
                    item.TotalDisarmed = item.AmountPay;
                    item.TotalNeedPayment = item.Amount - item.TotalDisarmed;
                    if (item.TotalNeedPayment < 0)
                        item.TotalNeedPayment = 0;
                    PaymentRequestViewModel model = new PaymentRequestViewModel();
                    item.CopyProperties(model);
                    if (requestId > 0)
                    {
                        if (listService.FirstOrDefault(n => n.ServiceId == item.ServiceId) == null)
                        {
                            if (detail != null)
                            {
                                item.IsDisabled = true;
                                item.IsChecked = true;
                            }
                            listService.Add(item);
                        }
                    }
                    else
                    {
                        if (listService.FirstOrDefault(n => n.ServiceId == item.ServiceId) == null && item.TotalNeedPayment > 0)
                            listService.Add(item);
                    }
                }
                return listService;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetServiceListBySupplierId - PaymentRequestRepository: " + ex);
            }
            return new List<PaymentRequestViewModel>();
        }
        public List<OrderPaymentRequest> GetListPaymentRequestByOrderId(int Orderid)
        {
            try
            {
                var dt = paymentRequestDAL.GetListPaymentRequestByOrderId(Orderid);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<OrderPaymentRequest>();
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListPaymentRequestByOrderId - PaymentRequestRepository: " + ex);
            }
            return null;
        }
        public List<PaymentRequestViewModel> GetByClientId(long clientId, long Type, int paymentVoucherId = 0)
        {
            try
            {
                var listPaymentRequestByClientId = paymentRequestDAL.GetListPaymentRequestByClientId(clientId, Type, StoreProcedureConstant.SP_GetListPaymentRequestByClientId).ToList<PaymentRequestViewModel>();
                var listPaymentRequest = listPaymentRequestByClientId.Where(n => n.Status == (int)PAYMENT_REQUEST_STATUS.CHO_CHI).ToList();
                var listPaymentRequestOutput = new List<PaymentRequestViewModel>();
                var listPaymentRequestExists = paymentRequestDAL.GetPaymentRequestExists(listPaymentRequest.Select(n => n.Id).ToList(),
                    StoreProcedureConstant.SP_CheckCreatePaymentVoucher).ToList<PaymentRequestViewModel>();
                var listRequetIdExists = new List<long>();
                foreach (var item in listPaymentRequestExists)
                {
                    var requestIds = item.RequestIds.Split(",");
                    foreach (var requestId in requestIds)
                    {
                        if (!string.IsNullOrEmpty(requestId))
                            listRequetIdExists.Add(int.Parse(requestId));
                    }
                }
                if (listPaymentRequestExists.Count > 0)
                {
                    foreach (var item in listPaymentRequest)
                    {
                        PaymentRequestViewModel model = new PaymentRequestViewModel();
                        var exists = listRequetIdExists.Contains(item.Id);
                        if (!exists)
                        {
                            item.CopyProperties(model);
                            listPaymentRequestOutput.Add(model);
                        }
                    }
                }
                else
                {
                    listPaymentRequestOutput = listPaymentRequest;
                }
                //var paymentVoucher = paymentVoucherDAL.GetById(paymentVoucherId);
                //if (paymentVoucher != null)
                //{
                //    var listRequest = paymentRequestDAL.GetByIds(
                //   paymentVoucher.RequestId.Split(",").Select(n => Convert.ToInt64(n)).ToList());
                //    if (paymentVoucherId > 0)
                //    {
                //        foreach (var item in listRequest)
                //        {
                //            PaymentRequestViewModel model = new PaymentRequestViewModel();
                //            item.CopyProperties(model);
                //            model.IsChecked = true;
                //            listPaymentRequestOutput.Add(model);

                //        }
                //    }
                //}
                return listPaymentRequestOutput;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByClientId - PaymentRequestRepository: " + ex);
            }
            return new List<PaymentRequestViewModel>();
        }

        public List<PaymentRequestViewModel> GetBySupplierId(long supplierId, int paymentVoucherId = 0, string requestType = "1,2")
        {
            try
            {
                var listPaymentRequest = paymentRequestDAL.GetServiceListBySupplierId(supplierId,
                    StoreProcedureConstant.SP_GetListPaymentRequestBySupplierId, requestType).ToList<PaymentRequestViewModel>();
                var listPaymentRequestOutput = new List<PaymentRequestViewModel>();
                var listPaymentRequestExists = paymentRequestDAL.GetPaymentRequestExists(listPaymentRequest.Select(n => n.Id).ToList(),
                    StoreProcedureConstant.SP_CheckCreatePaymentVoucher).ToList<PaymentRequestViewModel>();
                var listRequetIdExists = new List<long>();
                foreach (var item in listPaymentRequestExists)
                {
                    var requestIds = item.RequestIds.Split(",");
                    foreach (var requestId in requestIds)
                    {
                        if (!string.IsNullOrEmpty(requestId))
                            listRequetIdExists.Add(int.Parse(requestId));
                    }
                }
                if (listPaymentRequestExists.Count > 0)
                {
                    foreach (var item in listPaymentRequest)
                    {
                        PaymentRequestViewModel model = new PaymentRequestViewModel();
                        var exists = listRequetIdExists.Contains(item.Id);
                        if (!exists)
                        {
                            item.CopyProperties(model);
                            listPaymentRequestOutput.Add(model);
                        }
                    }
                }
                else
                {
                    listPaymentRequestOutput = listPaymentRequest;
                }
                //var paymentVoucher = paymentVoucherDAL.GetById(paymentVoucherId);
                //if (paymentVoucher != null && !string.IsNullOrEmpty(paymentVoucher.RequestId))
                //{
                //    var listRequest = paymentRequestDAL.GetByIds(
                //   paymentVoucher.RequestId.Split(",").Select(n => Convert.ToInt64(n)).ToList());
                //    if (paymentVoucherId > 0)
                //    {
                //        foreach (var item in listRequest)
                //        {
                //            PaymentRequestViewModel model = new PaymentRequestViewModel();
                //            item.CopyProperties(model);
                //            model.IsChecked = true;
                //            listPaymentRequestOutput.Add(model);
                //        }
                //    }
                //}

                return listPaymentRequestOutput;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetBySupplierId - PaymentRequestRepository: " + ex);
            }
            return new List<PaymentRequestViewModel>();
        }
        public List<PaymentRequestViewModel> GetByServiceId(long serviceId, int type, int requestType = 0)
        {
            try
            {
                var listServiceOutput = paymentRequestDAL.GetListPaymentRequestByServiceId(serviceId, type,
                    StoreProcedureConstant.sp_GetListPaymentRequestByServiceId, requestType).ToList<PaymentRequestViewModel>();
                //foreach (var item in listServiceOutput)
                //{
                //    item.ListServiceCodeAndType = new List<CountStatus>();
                //    if (!string.IsNullOrEmpty(item.PaymentVoucherCode))
                //    {
                //        var listPaymentVoucher = paymentVoucherDAL.GetByPaymentCodes(item.PaymentVoucherCode.Split(",").ToList());
                //        foreach (var paymentVoucher in listPaymentVoucher)
                //        {
                //            CountStatus countStatus = new CountStatus();
                //            countStatus.DataNo = paymentVoucher.PaymentCode;
                //            countStatus.DataId = paymentVoucher.Id;
                //            item.ListServiceCodeAndType.Add(countStatus);
                //        }
                //    }
                //}
                return listServiceOutput;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByServiceId - PaymentRequestRepository: " + ex);
            }
            return new List<PaymentRequestViewModel>();
        }
    }
}

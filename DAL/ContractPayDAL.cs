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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ultilities.Constants;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class ContractPayDAL : GenericService<ContractPay>
    {
        private static DbWorker _DbWorker;
        private static string _connection;
        private static OrderDAL orderDAL;
        public ContractPayDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
            _connection = connection;
            orderDAL = new OrderDAL(connection);
        }
        public DataTable GetServiceDetail(string serviceCode, string proc)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ServiceCode", serviceCode);
                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetContractPayServiceListBySupplierId - ContractPayDAL: " + ex);
            }
            return null;
        }
        public DataTable GetByPayId(long payId, string proc)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@PayId", payId);
                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByPayId - ContractPayDAL: " + ex);
            }
            return null;
        }
        public DataTable GetDetailContractPayById(long payId, string proc)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@PayId", payId);
                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetDetailContractPayById - ContractPayDAL: " + ex);
            }
            return null;
        }
        public ContractPay GetByBillNo(string billNo)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.ContractPays.AsNoTracking().FirstOrDefault(x => x.BillNo == billNo);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByBillNo - ContractPayDAL: " + ex);
                return null;
            }
        }
        public long CountPaymentRequest()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.PaymentRequests.AsNoTracking().Count();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CountPaymentVoucherInYear - ContractPayDAL: " + ex.ToString());
                return -1;
            }
        }
        public int CreateContractPay(ContractPayViewModel model)
        {
            int id = 0;
            List<int> detailIds = new List<int>();
            try
            {
                SqlParameter[] objParam_contractPay = new SqlParameter[16];
                objParam_contractPay[0] = new SqlParameter("@BillNo", model.BillNo);
                if (model.ClientId == null || model.ClientId == 0)
                    objParam_contractPay[1] = new SqlParameter("@ClientId", DBNull.Value);
                else
                    objParam_contractPay[1] = new SqlParameter("@ClientId", model.ClientId);
                objParam_contractPay[2] = new SqlParameter("@Note", string.IsNullOrEmpty(model.Note) ? DBNull.Value.ToString() :
                    model.Note);
                if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_NCC_HOAN_TRA
                    || model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_HOA_HONG_NCC)
                {
                    objParam_contractPay[3] = new SqlParameter("@Amount", model.ContractPayDetails.Sum(n => n.Amount));
                }
                else
                {
                    objParam_contractPay[3] = new SqlParameter("@Amount", model.Amount);
                }
                objParam_contractPay[4] = new SqlParameter("@Type", model.Type);
                objParam_contractPay[5] = new SqlParameter("@PayType", model.PayType);
                if (model.PayType == (int)DepositHistoryConstant.CONTRACT_PAYMENT_TYPE.CHUYEN_KHOAN)
                {
                    objParam_contractPay[6] = new SqlParameter("@BankingAccountId", model.BankingAccountId);
                }
                else
                {
                    objParam_contractPay[6] = new SqlParameter("@BankingAccountId", DBNull.Value);
                }
                objParam_contractPay[7] = new SqlParameter("@Description", string.IsNullOrEmpty(model.Description)
                    ? DBNull.Value.ToString() : model.Description);
                objParam_contractPay[8] = new SqlParameter("@AttatchmentFile", string.IsNullOrEmpty(model.AttatchmentFile)
                    ? DBNull.Value.ToString() : model.AttatchmentFile);
                objParam_contractPay[9] = new SqlParameter("@ExportDate", DateTime.Now);
                objParam_contractPay[10] = new SqlParameter("@PayStatus", (int)DepositHistoryConstant.CONTRACT_PAY_STATUS.KE_TOAN_DUYET);
                objParam_contractPay[11] = new SqlParameter("@CreatedBy", model.CreatedBy);
                objParam_contractPay[12] = new SqlParameter("@CreatedDate", DateTime.Now);
                objParam_contractPay[13] = new SqlParameter("@SupplierId", Convert.ToInt32(model.SupplierId));
                objParam_contractPay[14] = new SqlParameter("@ObjectType", Convert.ToInt32(model.ObjectType));
                objParam_contractPay[15] = new SqlParameter("@EmployeeId", Convert.ToInt32(model.EmployeeId));
                id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertContractPay, objParam_contractPay);
                if (id > 0)
                {
                    foreach (var item in model.ContractPayDetails)
                    {
                        var detailId = 0;
                        SqlParameter[] objParam_contractPayDetail = new SqlParameter[8];
                        objParam_contractPayDetail[0] = new SqlParameter("@PayId", id);
                        if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY)
                        {
                            objParam_contractPayDetail[1] = new SqlParameter("@DataId", item.Id);
                        }
                        else
                        {
                            objParam_contractPayDetail[1] = new SqlParameter("@DataId", item.OrderId);

                        }
                        objParam_contractPayDetail[2] = new SqlParameter("@CreatedBy", model.CreatedBy);
                        objParam_contractPayDetail[3] = new SqlParameter("@Amount", item.Amount);
                        objParam_contractPayDetail[4] = new SqlParameter("@CreatedDate", DateTime.Now);
                        objParam_contractPayDetail[5] = new SqlParameter("@ServiceId", Convert.ToInt64(item.ServiceId));
                        objParam_contractPayDetail[6] = new SqlParameter("@ServiceType", Convert.ToInt32(item.ServiceType));
                        if (string.IsNullOrEmpty(item.ServiceCode))
                            objParam_contractPayDetail[7] = new SqlParameter("@ServiceCode", DBNull.Value);
                        else
                            objParam_contractPayDetail[7] = new SqlParameter("@ServiceCode", item.ServiceCode);

                        detailId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertContractPayDetail, objParam_contractPayDetail);
                        if (detailId > 0)
                            detailIds.Add(detailId);
                        if (detailId <= 0)
                        {
                            using (var _DbContext = new EntityDataContext(_connection))
                            {
                                var entity = _DbContext.ContractPays.Find(id);
                                _DbContext.ContractPays.Remove(entity);
                                foreach (var idDetail in detailIds)
                                {
                                    var detail = _DbContext.ContractPayDetails.Find(idDetail);
                                    _DbContext.ContractPayDetails.Remove(detail);
                                }
                                _DbContext.SaveChanges();
                            }
                            return -1;
                        }
                        //nếu thanh toán đủ thì cập nhật trạng thái của đơn hàng - Đơn hàng đã thanh toán
                        var status = (int)OrderStatus2.WAITING_FOR_OPERATOR;
                        var orderInfo = orderDAL.GetByOrderId(item.OrderId);
                        if (orderInfo != null && orderInfo.OrderStatus != (int)OrderStatus2.CREATED_ORDER
                             && orderInfo.OrderStatus != (int)OrderStatus2.CONFIRMED_SALE)
                        {
                            status = orderInfo.OrderStatus.Value;
                        }
                        if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG && item.Amount >= item.TotalNeedPayment)
                        {
                            SqlParameter[] objParam_updateFinishPayment = new SqlParameter[5];
                            objParam_updateFinishPayment[0] = new SqlParameter("@OrderId", item.OrderId);
                            objParam_updateFinishPayment[1] = new SqlParameter("@IsFinishPayment", true);
                            objParam_updateFinishPayment[2] = new SqlParameter("@PaymentStatus", (int)PaymentStatus.PAID);
                            objParam_updateFinishPayment[3] = new SqlParameter("@Status", status);
                            objParam_updateFinishPayment[4] = new SqlParameter("@DebtStatus", (int)DepositHistoryConstant.ORDER_DEBT_STATUS.GACH_NO_DU);
                            _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderFinishPayment, objParam_updateFinishPayment);
                        }
                        if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG && item.Amount < item.TotalNeedPayment)
                        {

                            SqlParameter[] objParam_updateFinishPayment = new SqlParameter[5];
                            objParam_updateFinishPayment[0] = new SqlParameter("@OrderId", item.OrderId);
                            objParam_updateFinishPayment[1] = new SqlParameter("@IsFinishPayment", false);
                            objParam_updateFinishPayment[2] = new SqlParameter("@PaymentStatus", (int)PaymentStatus.PAID_NOT_ENOUGH);
                            objParam_updateFinishPayment[3] = new SqlParameter("@Status", status);
                            objParam_updateFinishPayment[4] = new SqlParameter("@DebtStatus", (int)DepositHistoryConstant.ORDER_DEBT_STATUS.GACH_NO_CHUA_DU);
                            _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderFinishPayment, objParam_updateFinishPayment);
                        }
                        if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG)
                        {
                            orderDAL.UpdateOrderStatus(item.OrderId, status, model.CreatedBy.Value, model.CreatedBy.Value).Wait();
                        }
                        //nếu thanh toán đủ thì cập nhật trạng thái của nạp quỹ - Chờ duyệt
                        if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY)
                        {
                            SqlParameter[] objParam_updateFinishPayment = new SqlParameter[3];
                            objParam_updateFinishPayment[0] = new SqlParameter("@DepositHistoryId", item.Id);
                            objParam_updateFinishPayment[1] = new SqlParameter("@IsFinishPayment", true);
                            objParam_updateFinishPayment[2] = new SqlParameter("@Status", (int)DepositHistoryConstant.DEPOSIT_STATUS.CHO_DUYET);
                            _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateDepositFinishPayment, objParam_updateFinishPayment);
                        }
                    }

                    SqlParameter[] objParam_UpdateContractPayDebtStatus = new SqlParameter[3];
                    objParam_UpdateContractPayDebtStatus[0] = new SqlParameter("@PayId", id);
                    objParam_UpdateContractPayDebtStatus[1] = new SqlParameter("@DebtStatus", model.ContractPayDetails.Sum(n => n.Amount) >= model.Amount ?
                        (int)DepositHistoryConstant.CONTRACTPAY_DEBT_STATUS.DA_GACH_HET :
                         (int)DepositHistoryConstant.CONTRACTPAY_DEBT_STATUS.CHUA_GACH_HET);
                    objParam_UpdateContractPayDebtStatus[2] = new SqlParameter("@UpdatedBy", model.CreatedBy);
                    _DbWorker.ExecuteNonQuery(StoreProcedureConstant.Sp_UpdateDebtStatusByPayId, objParam_UpdateContractPayDebtStatus);
                }
                return id;
            }
            catch (Exception ex)
            {
                //DeleteContractPayFail(id, detailIds);
                LogHelper.InsertLogTelegram("CreateContactPay - ContractPayDAL. " + ex);
                return -1;
            }
        }
        public int UpdateContractPay(ContractPayViewModel model)
        {
            try
            {
                int id = 0;
                SqlParameter[] objParam_contractPay = new SqlParameter[16];
                objParam_contractPay[0] = new SqlParameter("@BillNo", model.BillNo);
                if (model.ClientId == null || model.ClientId == 0)
                    objParam_contractPay[1] = new SqlParameter("@ClientId", Convert.ToInt32(0));
                else
                    objParam_contractPay[1] = new SqlParameter("@ClientId", model.ClientId);
                objParam_contractPay[2] = new SqlParameter("@Note", model.Note);
                objParam_contractPay[3] = new SqlParameter("@Amount", model.Amount);
                objParam_contractPay[4] = new SqlParameter("@Type", model.Type);
                objParam_contractPay[5] = new SqlParameter("@PayType", model.PayType);
                if (model.PayType == (int)DepositHistoryConstant.CONTRACT_PAYMENT_TYPE.CHUYEN_KHOAN)
                {
                    objParam_contractPay[6] = new SqlParameter("@BankingAccountId", model.BankingAccountId);
                }
                else
                {
                    objParam_contractPay[6] = new SqlParameter("@BankingAccountId", DBNull.Value);
                }
                objParam_contractPay[7] = new SqlParameter("@Description", string.IsNullOrEmpty(model.Description)
                    ? DBNull.Value.ToString() : model.Description);
                objParam_contractPay[8] = new SqlParameter("@AttatchmentFile", !string.IsNullOrEmpty(model.AttatchmentFile) ?
                    model.AttatchmentFile : DBNull.Value.ToString());
                objParam_contractPay[9] = new SqlParameter("@ExportDate", DateTime.Now);
                objParam_contractPay[10] = new SqlParameter("@PayStatus", model.PayStatus);
                objParam_contractPay[11] = new SqlParameter("@UpdatedBy", model.UpdatedBy);
                objParam_contractPay[12] = new SqlParameter("@PayId", model.PayId);
                objParam_contractPay[13] = new SqlParameter("@SupplierId", Convert.ToInt32(model.SupplierId));
                objParam_contractPay[14] = new SqlParameter("@ObjectType", Convert.ToInt32(model.ObjectType));
                objParam_contractPay[15] = new SqlParameter("@EmployeeId", Convert.ToInt32(model.EmployeeId));
                id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateContractPay, objParam_contractPay);
                foreach (var item in model.ContractPayDetails)
                {
                    var detailId = 0;
                    if (item.Id == 0)
                    {
                        SqlParameter[] objParam_contractPayDetail = new SqlParameter[8];
                        objParam_contractPayDetail[0] = new SqlParameter("@PayId", id);
                        if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY)
                        {
                            objParam_contractPayDetail[1] = new SqlParameter("@DataId", item.Id);
                        }
                        else
                        {
                            objParam_contractPayDetail[1] = new SqlParameter("@DataId", item.OrderId);
                        }
                        objParam_contractPayDetail[2] = new SqlParameter("@CreatedBy", model.UpdatedBy);
                        objParam_contractPayDetail[3] = new SqlParameter("@Amount", item.Amount);
                        objParam_contractPayDetail[4] = new SqlParameter("@CreatedDate", DateTime.Now);
                        objParam_contractPayDetail[5] = new SqlParameter("@ServiceId", Convert.ToInt64(item.ServiceId));
                        objParam_contractPayDetail[6] = new SqlParameter("@ServiceType", Convert.ToInt32(item.ServiceType));
                        if (string.IsNullOrEmpty(item.ServiceCode))
                            objParam_contractPayDetail[7] = new SqlParameter("@ServiceCode", DBNull.Value);
                        else
                            objParam_contractPayDetail[7] = new SqlParameter("@ServiceCode", item.ServiceCode);
                        detailId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertContractPayDetail, objParam_contractPayDetail);
                        item.PayDetailId = detailId;
                        item.Id = detailId;
                        if (detailId <= 0)
                        {
                            return -1;
                        }
                        //if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY)
                        //{
                        //    var listContractPayDetail = GetByContractPayId(id);
                        //    var contractPayDetails = listContractPayDetail.Where(n => n.DataId != item.Id).ToList();
                        //    foreach (var contractPayDetail in contractPayDetails)
                        //    {
                        //        DeleteContractPayDetail(contractPayDetail);
                        //        SqlParameter[] objParam_Detail = new SqlParameter[3];
                        //        objParam_Detail[0] = new SqlParameter("@DepositHistoryId", contractPayDetail.DataId);
                        //        objParam_Detail[1] = new SqlParameter("@IsFinishPayment", false);
                        //        objParam_Detail[2] = new SqlParameter("@Status", (int)DepositHistoryConstant.DEPOSIT_STATUS.CHO_DUYET);
                        //        detailId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateDepositFinishPayment, objParam_Detail);
                        //    }
                        //}
                    }
                    else
                    {
                        if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG
                            || model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_NCC_HOAN_TRA
                            || model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_HOA_HONG_NCC)
                        {
                            SqlParameter[] objParam_contractPayDetail = new SqlParameter[8];
                            objParam_contractPayDetail[0] = new SqlParameter("@Id", item.Id);
                            objParam_contractPayDetail[1] = new SqlParameter("@PayId", id);
                            if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG || (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_NCC_HOAN_TRA))
                            {
                                objParam_contractPayDetail[2] = new SqlParameter("@DataId", item.OrderId);
                            }
                            if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY)
                            {
                                objParam_contractPayDetail[2] = new SqlParameter("@DataId", item.Id);
                            }

                            if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG || (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_NCC_HOAN_TRA))
                            {
                                objParam_contractPayDetail[3] = new SqlParameter("@Amount", item.Amount);
                            }
                            if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY)
                            {
                                objParam_contractPayDetail[3] = new SqlParameter("@Amount", model.Amount);
                            }
                            objParam_contractPayDetail[3] = new SqlParameter("@Amount", item.Amount);
                            objParam_contractPayDetail[4] = new SqlParameter("@UpdatedBy", model.UpdatedBy);
                            objParam_contractPayDetail[5] = new SqlParameter("@ServiceId", Convert.ToInt64(item.ServiceId));
                            objParam_contractPayDetail[6] = new SqlParameter("@ServiceType", Convert.ToInt32(item.ServiceType));
                            if (string.IsNullOrEmpty(item.ServiceCode))
                                objParam_contractPayDetail[7] = new SqlParameter("@ServiceCode", DBNull.Value);
                            else
                                objParam_contractPayDetail[7] = new SqlParameter("@ServiceCode", item.ServiceCode);
                            detailId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateContractPayDetail, objParam_contractPayDetail);
                            if (detailId <= 0)
                            {
                                return -1;
                            }
                        }
                    }

                    //nếu thanh toán đủ thì cập nhật trạng thái của đơn hàng - Đơn hàng đã thanh toán
                    var status = (int)OrderStatus2.WAITING_FOR_OPERATOR;
                    var orderInfo = orderDAL.GetByOrderId(item.OrderId);
                    if (orderInfo != null && orderInfo.OrderStatus != (int)OrderStatus2.CREATED_ORDER
                         && orderInfo.OrderStatus != (int)OrderStatus2.CONFIRMED_SALE)
                    {
                        status = orderInfo.OrderStatus.Value;
                    }
                    if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG && item.Amount >= item.TotalNeedPayment)
                    {
                        SqlParameter[] objParam_contractPayDetail = new SqlParameter[5];
                        objParam_contractPayDetail[0] = new SqlParameter("@OrderId", item.OrderId);
                        objParam_contractPayDetail[1] = new SqlParameter("@IsFinishPayment", true);
                        objParam_contractPayDetail[2] = new SqlParameter("@Status", status);
                        objParam_contractPayDetail[3] = new SqlParameter("@DebtStatus", DBNull.Value);
                        objParam_contractPayDetail[4] = new SqlParameter("@PaymentStatus", (int)PaymentStatus.PAID);

                        detailId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderFinishPayment, objParam_contractPayDetail);
                    }
                    else
                    {

                        SqlParameter[] objParam_contractPayDetail = new SqlParameter[5];
                        objParam_contractPayDetail[0] = new SqlParameter("@OrderId", item.OrderId);
                        objParam_contractPayDetail[1] = new SqlParameter("@IsFinishPayment", false);
                        objParam_contractPayDetail[2] = new SqlParameter("@Status", status);
                        objParam_contractPayDetail[3] = new SqlParameter("@DebtStatus", DBNull.Value);
                        objParam_contractPayDetail[4] = new SqlParameter("@PaymentStatus", (int)PaymentStatus.PAID_NOT_ENOUGH);
                        detailId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateOrderFinishPayment, objParam_contractPayDetail);
                    }
                    if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_DON_HANG)
                    {
                        orderDAL.UpdateOrderStatus(item.OrderId, status, model.UpdatedBy.Value, model.UpdatedBy.Value).Wait();
                    }
                    //nếu thanh toán đủ thì cập nhật trạng thái của nạp quỹ - Chờ duyệt
                    if (model.Type == (int)DepositHistoryConstant.CONTRACT_PAY_TYPE.THU_TIEN_KY_QUY)
                    {
                        SqlParameter[] objParam_contractPayDetail = new SqlParameter[3];
                        objParam_contractPayDetail[0] = new SqlParameter("@DepositHistoryId", item.Id);
                        objParam_contractPayDetail[1] = new SqlParameter("@IsFinishPayment", true);
                        objParam_contractPayDetail[2] = new SqlParameter("@Status", (int)DepositHistoryConstant.DEPOSIT_STATUS.CHO_DUYET);
                        detailId = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateDepositFinishPayment, objParam_contractPayDetail);
                        if (detailId <= 0)
                        {
                            return -1;
                        }
                    }
                }

                SqlParameter[] objParam_UpdateContractPayDebtStatus = new SqlParameter[3];
                objParam_UpdateContractPayDebtStatus[0] = new SqlParameter("@PayId", model.PayId);
                objParam_UpdateContractPayDebtStatus[1] = new SqlParameter("@DebtStatus", model.ContractPayDetails.Sum(n => n.Amount) >= model.Amount ?
                    (int)DepositHistoryConstant.CONTRACTPAY_DEBT_STATUS.DA_GACH_HET :
                     (int)DepositHistoryConstant.CONTRACTPAY_DEBT_STATUS.CHUA_GACH_HET);
                objParam_UpdateContractPayDebtStatus[2] = new SqlParameter("@UpdatedBy", model.UpdatedBy);
                _DbWorker.ExecuteNonQuery(StoreProcedureConstant.Sp_UpdateDebtStatusByPayId, objParam_UpdateContractPayDebtStatus);
                //var listContractPay = GetByContractPayId(model.PayId);
                var listDetai = GetByContractPayIds(new List<int>() { model.PayId });
                foreach (var item in listDetai)
                {
                    var exists = model.ContractPayDetails.FirstOrDefault(n => n.PayDetailId == item.Id);
                    if (exists == null)
                    {
                        //DeleteContractPayDetail(item);
                    }
                }
                //if (listContractPay != null && listContractPay.Count > 0)
                //{
                //    var id_ContractPayDetai = model.ContractPayDetails.Select(s => s.Id).ToList();
                //    string ids = string.Join(',', id_ContractPayDetai);
                //    listContractPay = listContractPay.Where(s => ids.Contains(s.Id.ToString()) != true).ToList();
                //    foreach (var item in listContractPay)
                //    {
                //        DeleteContractPayDetail(item);
                //    }
                //}

                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateContactPay - ContractPayDAL. " + ex);
                return -1;
            }
        }

        public ContractPay GetById(long contractPayId)
        {
            try
            {

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.ContractPays.AsNoTracking().FirstOrDefault(x => x.PayId == contractPayId);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - ContractPayDAL: " + ex);
                return null;
            }
        }
        public long CountContractPayInYear()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.ContractPays.AsNoTracking().Where(x => ((DateTime)x.CreatedDate).Year == DateTime.Now.Year).Count();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CountContractPayInYear - ContractPayDAL: " + ex.ToString());
                return -1;
            }
        }
        public DataTable GetPagingList(ContractPaySearchModel searchModel, int currentPage, int pageSize, string proc)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[13];
                objParam[0] = new SqlParameter("@BillNo", searchModel.BillNo);
                objParam[1] = new SqlParameter("@Description", searchModel.Content);
                objParam[2] = new SqlParameter("@ContractPayType", searchModel.Type);
                objParam[3] = new SqlParameter("@PayType", searchModel.PayType);
                objParam[4] = new SqlParameter("@ClientID", searchModel.ClientId);
                if (searchModel.CreateByIds == null || searchModel.CreateByIds.Count == 0)
                {
                    objParam[5] = new SqlParameter("@UserCreate", DBNull.Value);
                }
                else
                {
                    objParam[5] = new SqlParameter("@UserCreate", string.Join(",", searchModel.CreateByIds));
                }
                objParam[6] = new SqlParameter("@CreateDateFrom", searchModel.FromCreateDate);
                objParam[7] = new SqlParameter("@CreateDateTo", searchModel.ToCreateDate);
                if (pageSize == -1)
                {
                    objParam[8] = new SqlParameter("@PageIndex", -1);
                    objParam[9] = new SqlParameter("@PageSize", DBNull.Value);
                }
                else
                {
                    objParam[8] = new SqlParameter("@PageIndex", currentPage);
                    objParam[9] = new SqlParameter("@PageSize", pageSize);
                }
                if (searchModel.SupplierId == 0)
                    objParam[10] = new SqlParameter("@SupplerId", DBNull.Value);
                else
                    objParam[10] = new SqlParameter("@SupplerId", searchModel.SupplierId);
                if (searchModel.EmployeeId == 0)
                    objParam[11] = new SqlParameter("@SalerId", DBNull.Value);
                else
                    objParam[11] = new SqlParameter("@SalerId", searchModel.EmployeeId);
                if (string.IsNullOrEmpty(searchModel.ServiceCode))
                    objParam[12] = new SqlParameter("@ServiceCode", DBNull.Value);
                else
                    objParam[12] = new SqlParameter("@ServiceCode", searchModel.ServiceCode);
                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ContractPayDAL: " + ex);
            }
            return null;
        }

        public async Task<string> getContractPayByBillNo(string bill_no)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {

                    var data = _DbContext.ContractPays.AsNoTracking().FirstOrDefault(s => s.BillNo == bill_no);
                    return data == null ? "" : data.BillNo;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getContractPayByBillNo - ContractPayDAL: " + ex);
                return "";
            }
        }

        public async Task<long> CountInvoiceRequest()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.InvoiceRequests.Count();
                    
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CountInvoiceRequest - ContractPayDAL. " + ex);
                return 0;
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
        public double GetTotalAmountContractPayByServiceId(string ServiceId, long ServiceType, long ContractPayType)
        {

            try
            {


                SqlParameter[] objParam_contractPay = new SqlParameter[3];
                objParam_contractPay[0] = new SqlParameter("@ServiceId", ServiceId);
                objParam_contractPay[1] = new SqlParameter("@ServiceType", ServiceType);
                objParam_contractPay[2] = new SqlParameter("@ContractPayType", ContractPayType);

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetTotalAmountContractPayByServiceId, objParam_contractPay);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var amount = Convert.ToDouble(dt.Rows[0]["Amount"]);
                    return amount;
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteContractPayDetailByIds - ContractPayDAL. " + ex);
                return 0;
            }
        }
        public List<ContractPayDetail> GetByContractPayIds(List<int> contractPayIds)
        {
            try
            {

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var contractPays = _DbContext.ContractPayDetails.AsNoTracking().Where(x => contractPayIds.Contains(x.PayId)
                    && (x.ServiceId == 0 || x.ServiceId == null)).ToList();
                    if (contractPays != null)
                    {
                        return contractPays;
                    }
                }
                return new List<ContractPayDetail>();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByContractPayIds - ContractPayDAL: " + ex);
                return new List<ContractPayDetail>();
            }
        }
        public List<ContractPayDetail> GetByContractDataIds(List<long> dataIds)
        {
            try
            {

                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.ContractPayDetails.AsNoTracking().Where(x => x.DataId != null
                    && dataIds.Contains(x.DataId.Value) && (x.ServiceId == 0 || x.ServiceId == null)).ToList();
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return new List<ContractPayDetail>();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetByContractDataIds - ContractPayDAL: " + ex);
                return new List<ContractPayDetail>();
            }
        }

    }
}

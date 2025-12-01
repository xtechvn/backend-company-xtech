//using DAL.Generic;
//using DAL.StoreProcedure;
//using Entities.Models;
//using Entities.ViewModels;
//using Entities.ViewModels.Funding;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Text;
//using Utilities;
//using Utilities.Contants;

//namespace DAL.Funding
//{
//    public class DebtStatisticDAL : GenericService<DebtStatistic>
//    {
//        private static DbWorker _DbWorker;
//        public DebtStatisticDAL(string connection) : base(connection)
//        {
//            _connection = connection;
//            _DbWorker = new DbWorker(connection);
//        }

//        public DataTable GetPagingList(DebtStatisticViewModel searchModel, int currentPage, int pageSize, string proc)
//        {
//            try
//            {
//                SqlParameter[] objParam = new SqlParameter[12];
//                objParam[0] = new SqlParameter("@Code", searchModel.Code);
//                if (searchModel.StatusMulti == null || searchModel.StatusMulti.Count == 0)
//                    objParam[1] = new SqlParameter("@Status", DBNull.Value);
//                else
//                    objParam[1] = new SqlParameter("@Status", string.Join(",", searchModel.StatusMulti));
//                if (searchModel.ClientId == 0)
//                    objParam[2] = new SqlParameter("@ClientId", DBNull.Value);
//                else
//                    objParam[2] = new SqlParameter("@ClientId", searchModel.ClientId);
//                objParam[3] = new SqlParameter("@FromDateFrom", searchModel.FromDate);
//                objParam[4] = new SqlParameter("@FromDateTo", searchModel.ToDate);
//                objParam[5] = new SqlParameter("@ToDateFrom", searchModel.FromDate);
//                objParam[6] = new SqlParameter("@ToDateTo", searchModel.ToDate);
//                if (searchModel.CreateByIds == null || searchModel.CreateByIds.Count == 0)
//                {
//                    objParam[7] = new SqlParameter("@UserCreate", DBNull.Value);
//                }
//                else
//                {
//                    objParam[7] = new SqlParameter("@UserCreate", string.Join(",", searchModel.CreateByIds));
//                }
//                objParam[8] = new SqlParameter("@CreateDateFrom", searchModel.FromCreateDate);
//                objParam[9] = new SqlParameter("@CreateDateTo", searchModel.ToCreateDate);
//                if (pageSize == -1)
//                {
//                    objParam[10] = new SqlParameter("@PageIndex", -1);
//                    objParam[11] = new SqlParameter("@PageSize", DBNull.Value);
//                }
//                else
//                {
//                    objParam[10] = new SqlParameter("@PageIndex", currentPage);
//                    objParam[11] = new SqlParameter("@PageSize", pageSize);
//                }
//                return _DbWorker.GetDataTable(proc, objParam);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetPagingList - DebtStatisticDAL: " + ex);
//            }
//            return null;
//        }

//        public DataTable GetCountStatus(DebtStatisticViewModel searchModel, string proc)
//        {
//            try
//            {
//                SqlParameter[] objParam = new SqlParameter[10];
//                objParam[0] = new SqlParameter("@Code", searchModel.Code);
//                if (searchModel.StatusMulti == null || searchModel.StatusMulti.Count == 0)
//                    objParam[1] = new SqlParameter("@Status", DBNull.Value);
//                else
//                    objParam[1] = new SqlParameter("@Status", string.Join(",", searchModel.StatusMulti));
//                if (searchModel.ClientId == 0)
//                    objParam[2] = new SqlParameter("@ClientId", DBNull.Value);
//                else
//                    objParam[2] = new SqlParameter("@ClientId", searchModel.ClientId);
//                objParam[3] = new SqlParameter("@FromDateFrom", searchModel.FromDate);
//                objParam[4] = new SqlParameter("@FromDateTo", searchModel.ToDate);
//                objParam[5] = new SqlParameter("@ToDateFrom", searchModel.FromDate);
//                objParam[6] = new SqlParameter("@ToDateTo", searchModel.ToDate);
//                if (searchModel.CreateByIds == null || searchModel.CreateByIds.Count == 0)
//                {
//                    objParam[7] = new SqlParameter("@UserCreate", DBNull.Value);
//                }
//                else
//                {
//                    objParam[7] = new SqlParameter("@UserCreate", string.Join(",", searchModel.CreateByIds));
//                }
//                objParam[8] = new SqlParameter("@CreateDateFrom", searchModel.FromCreateDate);
//                objParam[9] = new SqlParameter("@CreateDateTo", searchModel.ToCreateDate);
//                return _DbWorker.GetDataTable(proc, objParam);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetCountStatus - DebtStatisticDAL: " + ex);
//            }
//            return null;
//        }

//        public int CreateDebtStatistic(DebtStatisticViewModel model)
//        {
//            int id = 0;
//            try
//            {
//                SqlParameter[] objParam_paymentRequest = new SqlParameter[13];
//                objParam_paymentRequest[0] = new SqlParameter("@Code", model.Code);
//                objParam_paymentRequest[1] = new SqlParameter("@FromDate", model.FromDate);
//                objParam_paymentRequest[2] = new SqlParameter("@ToDate", model.ToDate);
//                objParam_paymentRequest[3] = new SqlParameter("@ClientId", Convert.ToInt32(model.ClientId));
//                objParam_paymentRequest[4] = new SqlParameter("@Amount", model.Amount);
//                objParam_paymentRequest[5] = new SqlParameter("@Description", string.IsNullOrEmpty(model.Description)
//                    ? DBNull.Value.ToString() : model.Description);
//                objParam_paymentRequest[6] = new SqlParameter("@Note", string.IsNullOrEmpty(model.Note)
//                    ? DBNull.Value.ToString() : model.Note);
//                if (model.IsSend == 1)
//                    objParam_paymentRequest[7] = new SqlParameter("@Status", Convert.ToInt32((int)DEBT_STATISTIC_STATUS.CHO_KE_TOAN_XAC_NHAN));
//                else
//                    objParam_paymentRequest[7] = new SqlParameter("@Status", Convert.ToInt32((int)DEBT_STATISTIC_STATUS.LUU_NHAP));
//                objParam_paymentRequest[8] = new SqlParameter("@Currency", "VND");
//                objParam_paymentRequest[9] = new SqlParameter("@OrderIds", model.OrderIds);
//                objParam_paymentRequest[10] = new SqlParameter("@DeclineReason", DBNull.Value);
//                objParam_paymentRequest[11] = new SqlParameter("@CreateBy", model.CreateBy);
//                objParam_paymentRequest[12] = new SqlParameter("@CreatedDate", DateTime.Now);
//                id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertDebtStatistic, objParam_paymentRequest);
//                return id;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("CreateDebtStatistic - DebtStatisticDAL. " + ex);
//                return -1;
//            }
//        }

//        public int UpdateDebtStatistic(DebtStatisticViewModel model)
//        {
//            int id = 0;
//            try
//            {
//                SqlParameter[] objParam_paymentRequest = new SqlParameter[15];
//                objParam_paymentRequest[0] = new SqlParameter("@Code", model.Code);
//                objParam_paymentRequest[1] = new SqlParameter("@FromDate", model.FromDate);
//                objParam_paymentRequest[2] = new SqlParameter("@ToDate", model.ToDate);
//                objParam_paymentRequest[3] = new SqlParameter("@ClientId", Convert.ToInt32(model.ClientId));
//                objParam_paymentRequest[4] = new SqlParameter("@Amount", model.Amount);
//                objParam_paymentRequest[5] = new SqlParameter("@Description", string.IsNullOrEmpty(model.Description)
//                    ? DBNull.Value.ToString() : model.Description);
//                objParam_paymentRequest[6] = new SqlParameter("@Note", string.IsNullOrEmpty(model.Note)
//                    ? DBNull.Value.ToString() : model.Note);
//                if (model.IsSend == 1)
//                    objParam_paymentRequest[7] = new SqlParameter("@Status", Convert.ToInt32((int)DEBT_STATISTIC_STATUS.CHO_KE_TOAN_XAC_NHAN));
//                else
//                    objParam_paymentRequest[7] = new SqlParameter("@Status", Convert.ToInt32((int)DEBT_STATISTIC_STATUS.LUU_NHAP));
//                objParam_paymentRequest[8] = new SqlParameter("@Currency", "VND");
//                objParam_paymentRequest[9] = new SqlParameter("@OrderIds", model.OrderIds);
//                objParam_paymentRequest[10] = new SqlParameter("@DeclineReason", DBNull.Value);
//                objParam_paymentRequest[11] = new SqlParameter("@UpdatedBy", model.UpdatedBy);
//                objParam_paymentRequest[12] = new SqlParameter("@Id", model.Id);
//                objParam_paymentRequest[13] = new SqlParameter("@VerifyDate", model.VerifyDate);
//                objParam_paymentRequest[14] = new SqlParameter("@UserVerify", model.UserVerify);

//                id = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateDebtStatistic, objParam_paymentRequest);
//                return id;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("UpdateDebtStatistic - DebtStatisticDAL. " + ex);
//                return -1;
//            }
//        }

//        public int UpdateDebtStatisticStatus(DebtStatistic model)
//        {
//            try
//            {
//                using (var _DbContext = new EntityDataContext(_connection))
//                {
//                    _DbContext.DebtStatistic.Update(model);
//                    _DbContext.SaveChanges();
//                }
//                return 1;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("UpdateDebtStatisticStatus - DebtStatisticDAL. " + ex);
//                return -1;
//            }
//        }

//        public DataTable GetRequestDetail(long requestId, string proc)
//        {
//            try
//            {
//                SqlParameter[] objParam = new SqlParameter[1];
//                objParam[0] = new SqlParameter("@Id", requestId);
//                return _DbWorker.GetDataTable(proc, objParam);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetRequestDetail - DebtStatisticDAL: " + ex);
//            }
//            return null;
//        }

//        public int RemoveDebtStatistic(DebtStatistic model)
//        {
//            try
//            {
//                using (var _DbContext = new EntityDataContext(_connection))
//                {
//                    _DbContext.DebtStatistic.Remove(model);
//                    _DbContext.SaveChanges();
//                }
//                return 1;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("RemoveDebtStatistic - DebtStatisticDAL. " + ex);
//                return -1;
//            }
//        }

//        public DebtStatistic GetById(long requestId)
//        {
//            try
//            {
//                using (var _DbContext = new EntityDataContext(_connection))
//                {
//                    var detail = _DbContext.DebtStatistic.AsNoTracking().FirstOrDefault(x => x.Id == requestId);
//                    if (detail != null)
//                    {
//                        return detail;
//                    }
//                }
//                return null;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetById - DebtStatisticDAL: " + ex);
//                return null;
//            }
//        }

//        public DebtStatistic GetByCode(string code)
//        {
//            try
//            {
//                using (var _DbContext = new EntityDataContext(_connection))
//                {
//                    var detail = _DbContext.DebtStatistic.AsNoTracking().FirstOrDefault(x => x.Code == code);
//                    if (detail != null)
//                    {
//                        return detail;
//                    }
//                }
//                return null;
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetByPaymentCode - PaymentRequestDAL: " + ex);
//                return null;
//            }
//        }

//        public DataTable GetOrderListByClientId(long clientId, DateTime fromDate, DateTime toDate, string proc, bool isDetail = false)
//        {
//            try
//            {
//                SqlParameter[] objParam = new SqlParameter[6];
//                objParam[0] = new SqlParameter("@ClientId", clientId);
//                objParam[1] = new SqlParameter("@CreateDateFrom", fromDate);
//                objParam[2] = new SqlParameter("@CreateDateTo", toDate);
//                objParam[2] = new SqlParameter("@CreateDateTo", toDate);
//                objParam[3] = new SqlParameter("@PageIndex", DBNull.Value);
//                objParam[4] = new SqlParameter("@PageSize", 1000);
//                if (!isDetail)
//                    objParam[5] = new SqlParameter("@Module", 1);
//                else
//                    objParam[5] = new SqlParameter("@Module", DBNull.Value);
//                return _DbWorker.GetDataTable(proc, objParam);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("GetOrderListByClientId - DebtStatisticDAL: " + ex);
//            }
//            return null;
//        }

//        public DataTable CheckApproveDebtStatistic(string orderIds, string proc)
//        {
//            try
//            {
//                SqlParameter[] objParam = new SqlParameter[1];
//                objParam[0] = new SqlParameter("@OrderId", orderIds);
//                return _DbWorker.GetDataTable(proc, objParam);
//            }
//            catch (Exception ex)
//            {
//                LogHelper.InsertLogTelegram("CheckApproveDebtStatistic - DebtStatisticDAL: " + ex);
//            }
//            return null;
//        }
//    }
//}

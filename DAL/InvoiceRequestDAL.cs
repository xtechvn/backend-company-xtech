using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.CustomerManager;
using Entities.ViewModels.InvoiceRequest;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class InvoiceRequestDAL
    {
        private static DbWorker _DbWorker;
        public InvoiceRequestDAL(string connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<GenericViewModel<InvoiceRequestViewModel>> GetListInvoiceRequest(InvoiceSearchViewModel request) 
        {
            var model = new GenericViewModel<InvoiceRequestViewModel>();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
               {
                    new SqlParameter("@InvoiceRequestNo", request.InvoiceRequestNo != null ? request.InvoiceRequestNo : DBNull.Value),
                    new SqlParameter("@PlanDateFrom", request.PlanDateFrom != DateTime.MinValue ? request.PlanDateFrom : DBNull.Value),
                    new SqlParameter("@PlanDateTo", request.PlanDateTo != DateTime.MinValue ? request.PlanDateTo : DBNull.Value),
                    new SqlParameter("@InvoiceNo", request.InvoiceNo != null ? request.InvoiceNo : DBNull.Value),
                    new SqlParameter("@InvoiceCode", request.InvoiceCode != null ? request.InvoiceCode : DBNull.Value),
                    new SqlParameter("@ExportDateFrom", request.ExportDateFrom != DateTime.MinValue ? request.ExportDateFrom : DBNull.Value),
                    new SqlParameter("@ExportDateTo", request.ExportDateTo != DateTime.MinValue ? request.ExportDateTo : DBNull.Value),
                    new SqlParameter("@InvoiceRequestStatus", request.InvoiceRequestStatus != "-1" ? request.InvoiceRequestStatus : DBNull.Value),
                    new SqlParameter("@IsHasBill", request.IsHasBill != null? request.IsHasBill : DBNull.Value),
                    new SqlParameter("@ClientId", request.ClientId != 0 ? request.ClientId : DBNull.Value),
                    new SqlParameter("@UserCreate", request.UserCreate != null ? request.UserCreate : DBNull.Value),
                    new SqlParameter("@CreateDateFrom", request.CreateDateFrom != DateTime.MinValue ? request.CreateDateFrom : DBNull.Value),
                    new SqlParameter("@CreateDateTo", request.CreateDateTo != DateTime.MinValue ? request.CreateDateTo : DBNull.Value),
                    new SqlParameter("@UserVerify", request.UserVerify != null ? request.UserVerify : DBNull.Value),
                    new SqlParameter("@VerifyDateFrom", request.VerifyDateFrom != DateTime.MinValue ? request.VerifyDateFrom : DBNull.Value),
                    new SqlParameter("@VerifyDateTo", request.VerifyDateTo != DateTime.MinValue ? request.VerifyDateTo : DBNull.Value),
                    new SqlParameter("@PageIndex", request.PageIndex),
                    new SqlParameter("@PageSize", request.PageSize),
               };
                DataTable dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListInvoiceRequest, sqlParameter);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    var data = dataTable.ToList<InvoiceRequestViewModel>();
                    model.ListData = data;
                    model.CurrentPage = request.PageIndex;
                    model.PageSize = request.PageSize;
                    model.TotalRecord = Convert.ToInt32(dataTable.Rows[0]["TotalRow"]);
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListInvoiceRequest - InvoiceRequestDAL: " + ex.ToString());
            }
            return null;
        }

        public async Task<List<InvoiceRequestViewModel>> GetListInvoiceRequestByOrderId(int OrderId) 
        {
            try 
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
               {
                    new SqlParameter("@OrderId", OrderId)
               };
                DataTable dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.sp_GetListInvoiceRequestByOrderId, sqlParameter);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    var data = dataTable.ToList<InvoiceRequestViewModel>();
                    return data;
                }
                return null;
            }
            catch(Exception ex) 
            {
                LogHelper.InsertLogTelegram("GetListInvoiceRequestByOrderId - InvoiceRequestDAL: " + ex.ToString());
            }
            return null;
        }

        public async Task<List<InvoiceRequestViewModel>> GetInvoiceRequestById(int OrderId)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
               {
                    new SqlParameter("@Id", OrderId)
               };
                DataTable dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetInvoiceRequestById, sqlParameter);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    var data = dataTable.ToList<InvoiceRequestViewModel>();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetInvoiceRequestById - InvoiceRequestDAL: " + ex.ToString());
            }
            return null;
        }

        public async Task<int> SetUpInvoiceRequest(InvoiceRequest request) // chưa xong
        {
            try
            {
                if (request.Id != 0)
                {
                    SqlParameter[] sqlParameter = new SqlParameter[]
                    {
                    new SqlParameter("@Id", request.Id),
                    new SqlParameter("@InvoiceRequestNo", request.InvoiceRequestNo != null ? request.InvoiceRequestNo : DBNull.Value),
                    new SqlParameter("@ClientId", request.ClientId != null ? request.ClientId : DBNull.Value),
                    new SqlParameter("@PlanDate",request.PlanDate != null ? request.PlanDate : DBNull.Value),
                    new SqlParameter("@TaxNo", request.TaxNo != null ? request.TaxNo : DBNull.Value),
                    new SqlParameter("@CompanyName", request.CompanyName != null ? request.CompanyName : DBNull.Value),
                    new SqlParameter("@Address", request.Address != null ? request.Address : DBNull.Value),
                    new SqlParameter("@OrderId", request.OrderId != null ? request.OrderId : DBNull.Value),
                    new SqlParameter("@AttachFile", request.AttachFile != null? request.AttachFile : DBNull.Value),
                    new SqlParameter("@UserVerify", request.UserVerify != null ? request.UserVerify : DBNull.Value),
                    new SqlParameter("@VerifyDate", request.VerifyDate != null ? request.VerifyDate : DBNull.Value),
                    new SqlParameter("@DeclineReason", request.DeclineReason != null ? request.DeclineReason : DBNull.Value),
                    new SqlParameter("@UpdatedBy", request.UpdatedBy != null ? request.UpdatedBy : DBNull.Value),
                    new SqlParameter("@IsDelete", request.IsDelete != null ? request.IsDelete : DBNull.Value),
                    new SqlParameter("@Status", request.Status != null ? request.Status : DBNull.Value),
                    new SqlParameter("@Note", request.Note != null ? request.Note : DBNull.Value)
                    };
                    return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateInvoiceRequest, sqlParameter);
                }
                else
                {
                    SqlParameter[] sqlParameter = new SqlParameter[]
                    {
                    new SqlParameter("@InvoiceRequestNo", request.InvoiceRequestNo != null ? request.InvoiceRequestNo : DBNull.Value),
                    new SqlParameter("@ClientId", request.ClientId != null ? request.ClientId : DBNull.Value),
                    new SqlParameter("@PlanDate",request.PlanDate != null ? request.PlanDate : DBNull.Value),
                    new SqlParameter("@TaxNo", request.TaxNo != null ? request.TaxNo : DBNull.Value),
                    new SqlParameter("@CompanyName", request.CompanyName != null ? request.CompanyName : DBNull.Value),
                    new SqlParameter("@Address", request.Address != null ? request.Address : DBNull.Value),
                    new SqlParameter("@OrderId", request.OrderId != null ? request.OrderId : DBNull.Value),
                    new SqlParameter("@AttachFile", request.AttachFile != null? request.AttachFile : DBNull.Value),
                    new SqlParameter("@UserVerify", request.UserVerify != null ? request.UserVerify : DBNull.Value),
                    new SqlParameter("@VerifyDate", request.VerifyDate != null ? request.VerifyDate : DBNull.Value),
                    new SqlParameter("@CreatedBy", request.CreatedBy != null ? request.CreatedBy : DBNull.Value),
                    new SqlParameter("@CreatedDate", request.CreatedDate != null ? request.CreatedDate : DBNull.Value),
                    new SqlParameter("@Status", request.Status != null ? request.Status : DBNull.Value),
                    new SqlParameter("@Note", request.Note != null ? request.Note : DBNull.Value)
                    };
                    return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertInvoiceRequest, sqlParameter);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUpInvoiceRequest - InvoiceRequestDAL: " + ex.ToString());
                return -1;
            }
        }
    }
}

using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels.InvoiceRequest;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class InvoiceRequestDetailDAL
    {
        private static DbWorker _DbWorker;
        public InvoiceRequestDetailDAL(string connection)
        {
            _DbWorker = new DbWorker(connection);
        }

        public async Task<List<InvoiceRequestDetail>> GetListInvoiceRequestDetailByInvoiceRequestId(int OrderId)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
               {
                    new SqlParameter("@InvoiceRequestId", OrderId)
               };
                DataTable dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListInvoiceRequestDetailByInvoiceRequestId, sqlParameter);
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    var data = dataTable.ToList<InvoiceRequestDetail>();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListInvoiceRequestDetailByInvoiceRequestId - InvoiceRequestDetailDAL: " + ex.ToString());
            }
            return null;
        }

        public async Task<int> SetUpInvoiceRequestDetail(InvoiceRequestDetail request) // chưa xong
        {
            try
            {
                if (request.Id > 0)
                {
                    
                    SqlParameter[] sqlParameter = new SqlParameter[]
                    {
                    new SqlParameter("@Id", request.Id),
                    new SqlParameter("@InvoiceRequestId", request.InvoiceRequestId != null ? request.InvoiceRequestId : DBNull.Value),
                    new SqlParameter("@ProductName", request.ProductName != null ? request.ProductName : DBNull.Value),
                    new SqlParameter("@Unit",request.Unit != null ? request.Unit : DBNull.Value),
                    new SqlParameter("@Quantity", request.Quantity != null ? request.Quantity : request.Quantity),
                    new SqlParameter("@Price", request.Price != null ? (long)request.Price : DBNull.Value),
                    new SqlParameter("@PriceVat", request.PriceVat != null ? request.PriceVat : DBNull.Value),
                    new SqlParameter("@PriceExtra", request.PriceExtra != null ? request.PriceExtra : DBNull.Value),
                    new SqlParameter("@PriceExtraExport", request.PriceExtraExport != null? request.PriceExtraExport : DBNull.Value),
                    new SqlParameter("@VAT", request.Vat != null ? request.Vat : DBNull.Value),
                    new SqlParameter("@UpdatedBy" , request.UpdatedBy != null ? request.UpdatedBy : DBNull.Value)
                    };
                    return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateInvoiceRequestDetail, sqlParameter);
                }
                else
                {
                    SqlParameter[] sqlParameter = new SqlParameter[]
                    {
                    new SqlParameter("@InvoiceRequestId", request.InvoiceRequestId != null ? request.InvoiceRequestId : DBNull.Value),
                    new SqlParameter("@ProductName", request.ProductName != null ? request.ProductName : DBNull.Value),
                    new SqlParameter("@Unit",request.Unit != null ? request.Unit : DBNull.Value),
                    new SqlParameter("@Quantity", request.Quantity != null ? request.Quantity : request.Quantity),
                    new SqlParameter("@Price", request.Price != null ? (long)request.Price : DBNull.Value),
                    new SqlParameter("@PriceVat", request.PriceVat != null ? request.PriceVat : DBNull.Value),
                    new SqlParameter("@PriceExtra", request.PriceExtra != null ? request.PriceExtra : DBNull.Value),
                    new SqlParameter("@PriceExtraExport", request.PriceExtraExport != null? request.PriceExtraExport : DBNull.Value),
                    new SqlParameter("@VAT", request.Vat != null ? request.Vat : DBNull.Value),
                    new SqlParameter("@CreatedBy", request.CreatedBy != null ? request.CreatedBy : DBNull.Value),
                    new SqlParameter("@CreatedDate", request.CreatedDate != null ? request.CreatedDate : DBNull.Value)
                    };
                    return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertInvoiceRequestDetail, sqlParameter);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUpInvoiceRequestDetail - InvoiceRequestDetailDAL: " + ex.ToString());
                return -1;
            }
        }

        public async Task<int> DeleteInvoiceRequestDetailById(int Id)
        {
            try
            {
                if (Id != 0)
                {
                    SqlParameter[] sqlParameter = new SqlParameter[]
                    {
                        new SqlParameter("@Id", Id),
                    };
                    return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_DeleteInvoiceRequestDetailById, sqlParameter);
                }
                return -1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteInvoiceRequestDetailById - InvoiceRequestDetailDAL: " + ex.ToString());
                return -1;
            }
        }
    }
}

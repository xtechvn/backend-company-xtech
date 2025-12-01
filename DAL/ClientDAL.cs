using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.CustomerManager;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System.Data;
using System.Globalization;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class ClientDAL : GenericService<Client>
    {
        private static DbWorker _DbWorker;
        private static string _connection;
        public ClientDAL(string connection) : base(connection)
        {
            _connection = connection;
            _DbWorker = new DbWorker(connection);
        }
        public async Task<AccountClient> GetAccountClientByID(long id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.AccountClients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                    if (detail != null)
                    {
                        return detail;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAccountClientByID - ClientDAL: " + ex.ToString());
                return null;
            }
        }
        public List<Client> GetClientByIds(List<long> clientIds)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var clients = _DbContext.Client.AsNoTracking().Where(x => clientIds.Contains(x.Id)).ToList();
                    if (clients != null)
                    {
                        return clients;
                    }
                }
                return new List<Client>();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByIds - ClientDAL: " + ex.ToString());
                return new List<Client>();
            }
        }
        public async Task<List<Client>> GetClientInfo(List<long> listIdAccountClient)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var listAccountClient = _DbContext.AccountClients.AsNoTracking().Where(n => listIdAccountClient.Contains(n.Id)).ToList();
                    var listClientId = listAccountClient.Select(n => n.ClientId).ToList();
                    var listClient = _DbContext.Client.AsNoTracking().Where(n => listClientId.Contains(n.Id)).ToList();
                    foreach (var item in listClient)
                    {
                        var accountClient = listAccountClient.FirstOrDefault(n => n.ClientId == item.Id);
                        item.ClientMapId = accountClient?.Id;
                    }
                    return listClient;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientInfo - ClientDAL: " + ex);
                return new List<Client>();
            }
        }
        public async Task<int> UpdateApproachStatus(Client client)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@Id", client.Id),
                    new SqlParameter("@ApproachType", client.ApproachType),
                };           
                return _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_UpdateApproachType_Client, sqlParameter); 
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateApproachStatus - ClientDAL: " + ex.ToString());
                return -1;
            }
        }

        /// <summary>
        /// Lấy list client suggestion theo txt_search từ DB (bảng Client)
        /// </summary>
        public List<CustomerViewModel> GetClientSuggesstion(string txt_search)
        {
            var result = new List<CustomerViewModel>();

            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var query = _DbContext.Client.AsNoTracking();

                    // Nếu không search gì thì trả top 30 mới nhất
                    if (string.IsNullOrWhiteSpace(txt_search))
                    {
                        result = query
                            .OrderByDescending(x => x.UpdateTime ?? x.JoinDate)
                            .Take(30)
                            .Select(x => new CustomerViewModel
                            {
                                Id = x.Id,
                                ClientName = x.ClientName,
                                Email = x.Email,
                                Phone = x.Phone,
                                ClientCode = x.ClientCode,
                                Status = x.Status
                            })
                            .ToList();

                        return result;
                    }

                    txt_search = txt_search.Trim();

                    // Điều kiện search giống bên ES: phone / email / name / code
                    query = query.Where(x =>
                        (x.Phone != null && x.Phone.Contains(txt_search)) ||
                        (x.Email != null && x.Email.Contains(txt_search)) ||
                        (x.ClientName != null && x.ClientName.Contains(txt_search)) ||
                        (x.ClientCode != null && x.ClientCode.Contains(txt_search))
                    );

                    int top = 4000;

                    result = query
                        .OrderByDescending(x => x.UpdateTime ?? x.JoinDate)
                        .Take(top)
                        .Select(x => new CustomerViewModel
                        {
                            Id = x.Id,
                            ClientName = x.ClientName,
                            Email = x.Email,
                            Phone = x.Phone,
                            ClientCode = x.ClientCode,
                            Status = x.Status
                        })
                        .ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientSuggesstion - ClientDAL: " + ex);
                return new List<CustomerViewModel>();
            }
        }

        public async Task<Client> GetClientByID(int Id) 
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@ClientID", Id)
                };
                var dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.GetClientByID, sqlParameter);
                if (dataTable != null && dataTable.Rows.Count > 0) 
                {
                    var clients = dataTable.ToList<Client>();
                    return clients[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByID - ClientDAL: " + ex.ToString());
                return null;
            }
        }
       
        public int SetUpClient(Client model)
        {
            try
            {

                if (model.Id == 0)
                {
                    var check = GetClientByClientCode(model.ClientCode);
                    if (check == null)
                    {
                        return 2;
                    }
                    else
                    {
                        int status = 0;
                        SqlParameter[] objParam = new SqlParameter[] 
                        {
                        new SqlParameter("@ClientMapId", model.ClientMapId != null ? model.ClientMapId : DBNull.Value),
                        new SqlParameter("@SaleMapId", model.SaleMapId != null ? model.SaleMapId : DBNull.Value),
                        new SqlParameter("@ClientType", model.ClientType != null ? model.ClientType : DBNull.Value),
                        new SqlParameter("@ClientName", model.ClientName != null ? model.ClientName : DBNull.Value),
                        new SqlParameter("@ApproachType", model.ApproachType != null ? model.ApproachType : DBNull.Value),
                        new SqlParameter("@Email", model.Email != null ? model.Email : DBNull.Value),
                        new SqlParameter("@Gender", model.Gender != null ? model.Gender : DBNull.Value),
                        new SqlParameter("@Status", status),
                        new SqlParameter("@Note", model.Note != null ? model.Note : DBNull.Value),
                        new SqlParameter("@Avartar", model.Avartar != null ? model.Avartar : DBNull.Value),
                        new SqlParameter("@JoinDate", model.JoinDate != null ? model.JoinDate : DBNull.Value),
                        new SqlParameter("@isReceiverInfoEmail  ", model.IsReceiverInfoEmail != null ? model.IsReceiverInfoEmail : DBNull.Value),
                        new SqlParameter("@Phone", model.Phone != null ? model.Phone : DBNull.Value),
                        new SqlParameter("@Birthday", model.Birthday != null ? model.Birthday : DBNull.Value),
                        new SqlParameter("@UpdateTime", DateTime.Now),
                        new SqlParameter("@TaxNo", model.TaxNo != null ? model.TaxNo : DBNull.Value),
                        new SqlParameter("@AgencyType", model.AgencyType != null ? model.AgencyType : DBNull.Value),
                        new SqlParameter("@PermisionType", model.PermisionType != null ? model.PermisionType : DBNull.Value),
                        new SqlParameter("@BusinessAddress", model.BusinessAddress != null ? model.BusinessAddress : DBNull.Value),
                        new SqlParameter("@ExportBillAddress", model.ExportBillAddress != null ? model.ExportBillAddress : DBNull.Value),
                        new SqlParameter("@ClientCode", model.ClientCode != null ? model.ClientCode : DBNull.Value),
                        new SqlParameter("@IsRegisterAffiliate", model.IsRegisterAffiliate != null ? model.IsRegisterAffiliate : DBNull.Value),
                        new SqlParameter("@ReferralId", model.ReferralId != null ? model.ReferralId : DBNull.Value)
                    };


                        var dt = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.SP_InsertClient, objParam);


                        return 1;
                    }

                }
                else
                {
                    var data2 = GetClientByEmail(model.Email);


                    if (data2 != null && data2.Id == model.Id)
                    {

                        SqlParameter[] objParam = new SqlParameter[] 
                        {
                            new SqlParameter("@Id", model.ClientMapId != null ? model.ClientMapId : DBNull.Value),
                            new SqlParameter("@ClientMapId", model.ClientMapId != null ? model.ClientMapId : DBNull.Value),
                        new SqlParameter("@SaleMapId", model.SaleMapId != null ? model.SaleMapId : DBNull.Value),
                        new SqlParameter("@ClientType", model.ClientType != null ? model.ClientType : DBNull.Value),
                        new SqlParameter("@ClientName", model.ClientName != null ? model.ClientName : DBNull.Value),
                        new SqlParameter("@ApproachType", model.ApproachType != null ? model.ApproachType : DBNull.Value),
                        new SqlParameter("@Email", model.Email != null ? model.Email : DBNull.Value),
                        new SqlParameter("@Gender", model.Gender != null ? model.Gender : DBNull.Value),
                        new SqlParameter("@Status", model.Status),
                        new SqlParameter("@Note", model.Note != null ? model.Note : DBNull.Value),
                        new SqlParameter("@Avartar", model.Avartar != null ? model.Avartar : DBNull.Value),
                        new SqlParameter("@JoinDate", model.JoinDate != DateTime.MinValue ? model.JoinDate : DBNull.Value),
                        new SqlParameter("@isReceiverInfoEmail  ", model.IsReceiverInfoEmail != null ? model.IsReceiverInfoEmail : DBNull.Value),
                        new SqlParameter("@Phone", model.Phone != null ? model.Phone : DBNull.Value),
                        new SqlParameter("@Birthday", model.Birthday != null ? model.Birthday : DBNull.Value),
                        new SqlParameter("@UpdateTime", DateTime.Now),
                        new SqlParameter("@TaxNo", model.TaxNo != null ? model.TaxNo : DBNull.Value),
                        new SqlParameter("@AgencyType", model.AgencyType != null ? model.AgencyType : DBNull.Value),
                        new SqlParameter("@PermisionType", model.PermisionType != null ? model.PermisionType : DBNull.Value),
                        new SqlParameter("@BusinessAddress", model.BusinessAddress != null ? model.BusinessAddress : DBNull.Value),
                        new SqlParameter("@ExportBillAddress", model.ExportBillAddress != null ? model.ExportBillAddress : DBNull.Value),
                        new SqlParameter("@ClientCode", model.ClientCode != null ? model.ClientCode : DBNull.Value),
                        new SqlParameter("@IsRegisterAffiliate", model.IsRegisterAffiliate != null ? model.IsRegisterAffiliate : DBNull.Value),
                        new SqlParameter("@ReferralId", model.ReferralId != null ? model.ReferralId : DBNull.Value)
                        };
                        var dt = _DbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_UpdateClient, objParam);

                    }
                    else
                    {
                        return 2;
                    }

                }
                return 1;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SetUpClient - ClientDAL: " + ex.ToString());
                return 0;
            }
        }
        public Client GetClientByEmail(string email)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@Email", email);
                objParam[1] = new SqlParameter("@TaxNo", DBNull.Value);

                var dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetClientByEmail, objParam);
                if (dataTable != null && dataTable.Rows.Count > 0) 
                {
                    var client = dataTable.ToList<Client>();
                    return client[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByEmail - ClientDAL: " + ex);
                return null;
            }
        }
        
        public async Task<DataTable> GetPagingList(CustomerManagerViewSearchModel searchModel, int currentPage, int pageSize, string proc)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[16];
                objParam[0] = new SqlParameter("@MaKH", searchModel.MaKH);
                objParam[1] = new SqlParameter("@TenKH", searchModel.TenKH);
                objParam[2] = new SqlParameter("@Email", searchModel.Email);
                objParam[3] = new SqlParameter("@Phone", searchModel.Phone);
                objParam[4] = new SqlParameter("@AgencyType", searchModel.AgencyType);
                objParam[5] = new SqlParameter("@ClientType", searchModel.ClientType);
                objParam[6] = new SqlParameter("@PermissionType", searchModel.PermissionType);
                objParam[7] = new SqlParameter("@PageIndex", currentPage);
                objParam[8] = new SqlParameter("@PageSize", pageSize);
                objParam[9] = new SqlParameter("@UserId", searchModel.UserId);
                objParam[10] = (CheckDate(searchModel.CreateDate) == DateTime.MinValue) ? new SqlParameter("@CreateDate", DBNull.Value) : new SqlParameter("@CreateDate", CheckDate(searchModel.CreateDate));
                objParam[11] = (CheckDate(searchModel.EndDate) == DateTime.MinValue) ? new SqlParameter("@EndDate", DBNull.Value) : new SqlParameter("@EndDate", CheckDate(searchModel.EndDate));
                objParam[12] = new SqlParameter("@MinAmount", searchModel.MinAmount);
                objParam[13] = new SqlParameter("@MaxAmount", searchModel.MaxAmount);
                objParam[14] = new SqlParameter("@CreatedBy", searchModel.CreatedBy);
                objParam[15] = new SqlParameter("@SalerPermission", searchModel.SalerPermission);
                return _DbWorker.GetDataTable(proc, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetPagingList - ClientDAL: " + ex);
            }
            return null;
        }
        private DateTime CheckDate(string dateTime)
        {
            DateTime _date = DateTime.MinValue;
            if (!string.IsNullOrEmpty(dateTime))
            {
                _date = DateTime.ParseExact(dateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            return _date != DateTime.MinValue ? _date : DateTime.MinValue;
        }
       
        public async Task<DataTable> getClientById(long Client)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ClientID", (int)Client);

                return _DbWorker.GetDataTable(StoreProcedureConstant.GetClientByID, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getClientid - ClientDal: " + ex);
            }
            return null;
        }
        public async Task<DataTable> GetAmountRemainOfContractByClientId(long Client)
        {
            try
            {

                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ClientId", Client);

                return _DbWorker.GetDataTable(StoreProcedureConstant.Sp_GetAmountRemainOfContractPayByClientId, objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAmountRemainOfContractByClientId - ClientDal: " + ex);
            }
            return null;
        }
        public async Task<Client> GetClientByClientCode(string client_code) // fix
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@ClientCode", client_code)
                };
            
                var dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetClientbyClientCode, sqlParameter);
                Client? client = dataTable.Rows.Cast<DataRow>().Select(row => new Client
                {
                    Id = row.Field<int>("Id"),
                    ClientMapId = row.Field<int>("ClientMapId"),
                    ClientType = row.Field<int>("ClientType"),
                    SaleMapId = row.Field<int>("SaleMapId"),
                    ClientName = row.Field<string>("ClientName"),
                    Email = row.Field<string>("Email"),
                    Gender = row.Field<int>("Gender"),
                    Status = row.Field<byte>("Status"),
                    Note = row.Field<string>("Note"),
                    Avartar = row.Field<string>("Avartar"),
                    JoinDate = row.Field<DateTime>("JoinDate"),
                    Phone = row.Field<string>("Phone"),
                    IsReceiverInfoEmail = row.Field<bool>("IsReceiverInfoEmail"),
                    Birthday = row.Field<DateTime>("Birthday"),
                    UpdateTime = row.Field<DateTime>("Birthday"),
                    TaxNo = row.Field<string>("TaxNo"),
                    AgencyType = row.Field<int>("AgencyType"),
                    PermisionType = row.Field<int>("PermisionType"),
                    BusinessAddress = row.Field<string>("BusinessAddress"),
                    ExportBillAddress = row.Field<string>("ExportBillAddress"),
                    ClientCode = row.Field<string>("ClientCode"),
                    IsRegisterAffiliate = row.Field<bool>("IsRegisterAffiliate"),
                    ReferralId = row.Field<string>("ReferralId"),
                }).FirstOrDefault();
                return client;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByClientCode - ClientDAL: " + ex);
                return null;
            }
        }
        public int countClientTypeUse(int client_type)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@ClientType", client_type);

                DataTable tb = new DataTable();
                return _DbWorker.ExecuteNonQuery("Sp_CountClientByType", objParam);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("countClientTypeUse - ClientDAL: " + ex.ToString());
                return -1;
            }
        }

        public List<Client> GetAll()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var clients = _DbContext.Clients.AsNoTracking().OrderByDescending(s => s.JoinDate).ToList();
                    if (clients != null)
                    {
                        return clients;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAll in GenericService" + ex);
                return null;
            }
        }
        public async Task<Client> GetClientDetail(long clientId)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = await _DbContext.Client.AsNoTracking().FirstOrDefaultAsync(x => x.Id == clientId);
                    if (detail != null)
                    {
                        return detail;
                    } 
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientDetail - ClientDAL: " + ex.ToString());
                return null;
            }
        }
    }
}

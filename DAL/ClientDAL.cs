using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
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
    public class ClientDAL
    {
        private static DbWorker _DbWorker;
        private static string _connection;
        public ClientDAL(string connection)
        {
            _connection = connection;
            _DbWorker = new DbWorker(connection);
        }


       /* public async Task<AccountClient> GetAccountClientByClientId(long clientId) //fix
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[]
                {
                    new SqlParameter("@ClientId", clientId)
                };
                var dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAccountClientByClientId, sqlParameter);
                AccountClient? accountClient = dataTable.Rows.Cast<DataRow>().Select(row => new AccountClient
                {
                    Id = row.Field<int>("Id"),
                    ClientId = row.Field<int>("ClientId"),
                    ClientType = row.Field<int>("ClientType"),
                    UserName = row.Field<string>("Username"),
                    Password = row.Field<string>("Password"),
                    PasswordBackup = row.Field<string>("PasswordBackup"),
                    ForgotPasswordToken = row.Field<string>("ForgotPasswordToken"),
                    Status = row.Field<byte>("Status")
                }).FirstOrDefault();
                return accountClient;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAccountClientByClientId - ClientDAL: " + ex.ToString());
                return null;
            }
        }*/

        /*public async Task<AccountClient> GetAccountClientByID(long id) //fix
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[] 
                {
                    new SqlParameter("@ClientId", id)
                };
                var dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetAccountClientById, sqlParameter);
                AccountClient? accountClient = dataTable.Rows.Cast<DataRow>().Select(row => new AccountClient 
                {
                    Id = row.Field<int>("Id"),
                    ClientId = row.Field<int>("ClientId"),
                    ClientType = row.Field<int>("ClientType"),
                    UserName = row.Field<string>("Username"),
                    Password = row.Field<string>("Password"),
                    PasswordBackup = row.Field<string>("PasswordBackup"),
                    ForgotPasswordToken = row.Field<string>("ForgotPasswordToken"),
                    Status = row.Field<byte>("Status"),
                }).FirstOrDefault();
                return accountClient;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAccountClientByID - ClientDAL: " + ex.ToString());
                return null;
            }
        }*/
        /*public async Task<List<AccountClient>> GetAccountClientAsync(string name) //fix
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@Ids", name)
                };

                var lstObj = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListClientByName, sqlParameters);
                List<AccountClient> accountClients = lstObj.AsEnumerable().Select(row => new AccountClient
                {
                    Id = row.Field<int>("Id"),
                    ClientId = row.Field<int>("ClientId"),
                    ClientType = row.Field<int>("ClientType"),
                    UserName = row.Field<string>("Username"),
                    Password = row.Field<string>("Password"),
                    PasswordBackup = row.Field<string>("PasswordBackup"),
                    ForgotPasswordToken = row.Field<string>("ForgotPasswordToken"),
                    Status = row.Field<byte>("Status")
                }).ToList();

                return accountClients;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAccountClientAsync - ClientDAL: " + ex.ToString());
                return new List<AccountClient>();
            }
        }*/

        public async Task<Client> GetClientByID(int Id) //fix
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

       /* public List<Client> GetClientByIds(List<long> Ids) //fix
        {
            try
            {

                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@Ids", Ids)
                };
                var dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListClientByClientIds, sqlParameters);
                List<Client>? clients = dataTable.AsEnumerable().Select(row => new Client
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
                    ReferralId = row.Field<string>("ReferralId")
                }).ToList();
                return clients;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByIds - ClientDAL: " + ex.ToString());
                return new List<Client>();
            }
        }*/

        /*public async Task<List<Client>> GetClientsbyClientType(int Type) //fix
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@Type", Type)
                };
                var dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListClientByClientIds, sqlParameters);
                List<Client>? clients = dataTable.AsEnumerable().Select(row => new Client
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
                    ReferralId = row.Field<string>("ReferralId")
                }).ToList();
                return clients;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientsbyClientType - ClientDAL: " + ex.ToString());
                return null;
            }
        }*/

        /*public async Task<List<Client>> GetClientInfo(List<long> listIdAccountClient)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[]
                  {
                    new SqlParameter("@listIdAccountClient", listIdAccountClient)
                  };
                var dataTable = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListClientByIdAccountClient, sqlParameters);
                List<Client>? clients = dataTable.AsEnumerable().Select(row => new Client
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
                    ReferralId = row.Field<string>("ReferralId")
                }).ToList();
                return clients;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientInfo - ClientDAL: " + ex);
                return new List<Client>();
            }
        }*/
       
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

                        SqlParameter[] objParam = new SqlParameter[23];
                        objParam[0] = new SqlParameter("@Id ", model.Id);
                        objParam[1] = new SqlParameter("@ClientMapId", model.ClientMapId);
                        objParam[2] = new SqlParameter("@SaleMapId", model.SaleMapId);
                        objParam[3] = new SqlParameter("@ClientType", model.ClientType);
                        objParam[4] = new SqlParameter("@ClientName", model.ClientName);
                        objParam[5] = new SqlParameter("@Email", model.Email);
                        objParam[6] = new SqlParameter("@Gender", model.Gender);
                        objParam[7] = new SqlParameter("@Status", model.Status);
                        objParam[8] = new SqlParameter("@Note", model.Note);
                        objParam[9] = new SqlParameter("@Avartar", model.Avartar);
                        objParam[10] = new SqlParameter("@JoinDate", model.JoinDate);
                        objParam[11] = new SqlParameter("@isReceiverInfoEmail  ", model.IsReceiverInfoEmail);
                        objParam[12] = new SqlParameter("@Phone", model.Phone);
                        objParam[13] = new SqlParameter("@Birthday", model.Birthday);
                        objParam[14] = new SqlParameter("@UpdateTime", model.UpdateTime);
                        objParam[15] = new SqlParameter("@TaxNo", model.TaxNo);
                        objParam[16] = new SqlParameter("@AgencyType", model.AgencyType);
                        objParam[17] = new SqlParameter("@PermisionType", model.PermisionType);
                        objParam[18] = new SqlParameter("@BusinessAddress", model.BusinessAddress);
                        objParam[19] = new SqlParameter("@ExportBillAddress", model.ExportBillAddress);
                        objParam[20] = new SqlParameter("@ClientCode", model.ClientCode);
                        objParam[21] = new SqlParameter("@IsRegisterAffiliate", model.IsRegisterAffiliate);
                        objParam[22] = new SqlParameter("@ReferralId", model.ReferralId);


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
        /*public Client GetClientByTaxNo(string TaxNo)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[4];
                objParam[0] = new SqlParameter("@Email", DBNull.Value);
                objParam[1] = new SqlParameter("@TaxNo", TaxNo);

                DataTable dt = _DbWorker.GetDataTable(StoreProcedureConstant.SP_GetListClient, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<Client>();
                    return data[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetClientByTaxNo - ClientDAL: " + ex);
                return null;
            }
        }*/
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
    }
}

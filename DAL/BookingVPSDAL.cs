using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using Entities.Models;
using Entities.ViewModels.Galaxy;

namespace DAL
{
    public class BookingVPSDAL : GenericService<BookingVp>
    {

        private DbWorker dbWorker;

        public BookingVPSDAL(string connection) : base(connection)
        {
            dbWorker = new DbWorker(connection);
        }
        public int InsertBookingvps(GalaxyViewModel data)
        {
            try
            {

                SqlParameter[] objParam_order = new SqlParameter[11];
                objParam_order[0] = new SqlParameter("@Mem", data.Memory);
                objParam_order[1] = new SqlParameter("@Cpu", data.CPU);
                objParam_order[2] = new SqlParameter("@Ssd", data.SSD);
                objParam_order[3] = new SqlParameter("@Net", data.net);
                objParam_order[4] = new SqlParameter("@Nip", data.nip);
                objParam_order[5] = new SqlParameter("@NMonth", data.nMonth);
                objParam_order[6] = new SqlParameter("@Quantity", data.quantity);
                objParam_order[7] = new SqlParameter("@ClientId", data.Clientid);
                objParam_order[8] = new SqlParameter("@Amount", data.Amount);
                objParam_order[9] = new SqlParameter("@CreatedBy", data.Clientid);
                objParam_order[10] = new SqlParameter("@CreatedDate", DBNull.Value);

                var id = dbWorker.ExecuteNonQuery(StoreProcedureConstant.sp_InsertBookingVPS, objParam_order);
                
                return id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("InsertBookingvps - BookingVPSDAL. " + ex);
                return -1;
            }
        }
    }
}


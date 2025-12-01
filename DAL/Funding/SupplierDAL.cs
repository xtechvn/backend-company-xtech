using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Funding;

using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Contants;

namespace DAL.Funding
{
    public class SupplierDAL : GenericService<Supplier>
    {

        private static string _connection;


        public SupplierDAL(string connection) : base(connection)
        {
            _connection = connection;

        }


        public Supplier CheckExistName(int id, string name)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var detail = _DbContext.Supplier.FirstOrDefault(x => x.SupplierId != id && name.ToLower() == x.FullName.ToLower());
                    return detail;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckExistName - SupplierDAL: " + ex);
                return null;
            }
        }
    }
}

using DAL.Generic;
using DAL.StoreProcedure;
using Entities.Models;
using System.Globalization;

namespace DAL
{
    public class DepartmentDAL : GenericService<Department>
    {
        private static DbWorker DbWorker;
        public DepartmentDAL(string connection) : base(connection)
        {
            DbWorker = new DbWorker(connection);
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
      
    }
}

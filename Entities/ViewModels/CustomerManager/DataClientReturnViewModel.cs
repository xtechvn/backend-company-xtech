using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.CustomerManager
{
    public class DataClientReturnViewModel
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string Email {  get; set; }
        public string ReturnUrl { get; set; }
    }
}

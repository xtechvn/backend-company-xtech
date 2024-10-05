using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.CustomerManager
{
    public class DataClientReturnViewModel
    {
        public int IdClient { get; set; }
        public int IdAccount { get; set; }
        public int status { get; set; }
        public string UserName { get; set; }
        public string Email {  get; set; }
        public string ReturnUrl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.UserAgent
{
    public class UserAgentViewModel : Entities.Models.UserAgent
    {
        public string UserId_Name { get; set; }
        public string Avata { get; set; }
    }
}

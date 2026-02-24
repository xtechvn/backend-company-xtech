using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Ultilities.Constants;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;



namespace WEB.Adavigo.CMS.Controllers
{
    [CustomAuthorize]

    public class ContractController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly IAllCodeRepository _allCodeRepository;
       
       

        private IClientRepository _clientRepository;
       
        public ContractController(IConfiguration configuration, IAllCodeRepository allCodeRepository, 
            IClientRepository clientRepository, ICustomerManagerRepository customerManagerRepository)
        {

            _configuration = configuration;
            _allCodeRepository = allCodeRepository;
           
            _clientRepository = clientRepository;
            
        }
       
        public async Task<IActionResult> ClientSuggestion(string txt_search)
        {

            try
            {

                
                var data = await _clientRepository.GetClientSuggesstion(txt_search);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = data,
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ClientSuggestion - ContractController: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    data = new List<CustomerViewModel>()
                });
            }

        }
        

    }
}

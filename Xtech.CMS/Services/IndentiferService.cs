using DAL;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;

namespace WEB.Adavigo.CMS.Service
{
    public class IndentiferService
    {
        private readonly IConfiguration _configuration;
        private readonly IIdentifierServiceRepository _identifierServiceRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IContractPayRepository _contractPayRepository;

        public IndentiferService(IConfiguration configuration, IIdentifierServiceRepository identifierServiceRepository, IOrderRepository orderRepository, IContractPayRepository contractPayRepository)
        {
            _configuration = configuration;
            _identifierServiceRepository = identifierServiceRepository;
            _orderRepository = orderRepository;
            _contractPayRepository = contractPayRepository;
        }

        
        public async Task<string> BuildPaymentRequest()
        {
            string bill_no = string.Empty;
            try
            {
                var months = new Dictionary<int, string> { { 1, "A" }, { 2, "B" }, { 3, "C" }, { 4, "D" }, { 5, "E" }, { 6, "F" }, { 7, "G" }, { 8, "H" }, { 9, "K" }, { 10, "L" }, { 11, "M" }, { 12, "N" } };

                var current_date = DateTime.Now;
                bill_no = "YCC";

                // 2 số cuối của năm
                bill_no += current_date.Year.ToString().Substring(current_date.Year.ToString().Length - 2, 2);

                //Tháng hiện tại
                bill_no += months[current_date.Month];

                //2. Số thứ tự đã dùng.
                long bill_count = _contractPayRepository.CountPaymentRequest();

                //format numb
                string s_bill_new = string.Format(String.Format("{0,5:00000}", bill_count + 1));

                bill_no += s_bill_new;

                return bill_no;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("BuildPaymentRequest - IdentifierServiceRepository" + ex.ToString());
                //Trả mã random
                var rd = new Random();
                var contract_pay_default = rd.Next(DateTime.Now.Day, DateTime.Now.Year) + rd.Next(1, 999);
                bill_no = "PYCC-" + contract_pay_default;
                return bill_no;
            }
        }
    }
}

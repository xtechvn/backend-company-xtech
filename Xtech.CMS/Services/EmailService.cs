using Repositories.IRepositories;
using System.Net.Mail;
using System.Net;
using Utilities.Contants;
using Utilities;
using Xtech.CMS.Services.ServiceInterface;

namespace Xtech.CMS.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        public EmailService(IConfiguration configuration, IUserRepository userRepository, IProjectTaskRepository projectTaskRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _projectTaskRepository = projectTaskRepository;
        }
        public async Task<bool> SendEmail(long UserId, long old_AssigneeId, long TaskId)
        {
            bool ressult = true;
            try
            {
                //AccountClient orderInfo = JsonConvert.DeserializeObject<AccountClient>(objectStr);
                if (UserId <= 0)
                {
                    return false;
                }
                MailMessage message = new MailMessage();
                message.Subject = "assigned to you";

                //config send email
                string from_mail = _configuration["MAIL_CONFIG:FROM_MAIL"];
                string account = _configuration["MAIL_CONFIG:USERNAME"];
                string password = _configuration["MAIL_CONFIG:PASSWORD"];
                string host = _configuration["MAIL_CONFIG:HOST"];
                string port = _configuration["MAIL_CONFIG:PORT"];
                var task = await _projectTaskRepository.GetById(TaskId);
                var detail_user = await _userRepository.GetById(UserId);
                var name_old_user = "Chưa được chỉ định";
                if (old_AssigneeId > 0)
                {
                    var detail_olduser = await _userRepository.GetById(old_AssigneeId);
                    name_old_user = detail_olduser != null ? detail_olduser.FullName : name_old_user;
                }

                if (detail_user != null)
                {
                    message.Subject = "assigned " + task.Title + " to you";
                }

                if (detail_user != null && (detail_user.Email == null || detail_user.Email == ""))
                {
                    return false;
                }
                message.IsBodyHtml = true;
                message.From = new MailAddress(from_mail, _configuration["MAIL_CONFIG:STMP_USERNAME_Email"]);
                var Template = "<body class=\"min-h-screen flex items-center justify-center p-4\">\r\n" +
                    "<!-- BEGIN: Notification Container -->\r\n" +
                    "<main class=\"w-full max-w-3xl bg-white shadow-lg rounded-custom overflow-hidden border border-gray-200\" data-purpose=\"notification-card\">\r\n" +
                    "<!-- BEGIN: Header Section -->\r\n<header class=\"p-6 border-b border-gray-100\">\r\n<div class=\"flex items-center space-x-2 text-gray-700\">\r\n" +
                    "<span class=\"font-medium\">Bình Định Thanh</span>\r\n<span class=\"font-bold\">assigned</span>\r\n<span>this work item to you</span>\r\n</div>\r\n</header>\r\n" +
                    "<!-- END: Header Section -->\r\n<!-- BEGIN: Content Section -->\r\n<section class=\"p-6 space-y-6\">\r\n<!-- Project Context -->\r\n" +
                    "<div class=\"text-gray-500 text-sm font-medium\" data-purpose=\"project-breadcrumb\">\r\n        Không gian Scrum của tôi / " + task != null ? task.Title : "" + "\r\n   " +
                    "   </div>\r\n<!-- Item Title -->\r\n<h1 class=\"text-3xl text-blue-600 font-normal\" data-purpose=\"work-item-title\">\r\n        bài kiểm tra 1\r\n      </h1>\r\n" +
                    "<!-- Author Information -->\r\n<div class=\"flex items-center space-x-3\" data-purpose=\"author-details\">\r\n" +
                    "</div>\r\n<!-- Assignment Status Change -->\r\n<div class=\"flex items-center space-x-2 text-gray-700 py-2\" data-purpose=\"assignment-status\">\r\n<span>Người được chỉ định:</span>\r\n" +
                    "<span class=\"px-2 py-0.5 bg-red-50 text-gray-500 line-through rounded\">" + name_old_user + "</span>\r\n<span class=\"text-gray-400\">→</span>\r\n" +
                    "<span class=\"px-2 py-0.5 bg-green-50 text-green-700 rounded\">" + detail_user != null ? detail_user.FullName : "" + "</span>\r\n</div>\r\n<!-- Action Button -->\r\n<div class=\"pt-4\">\r\n" +
                    "<button href=\"http://be.x-tech.vn/TaskManagement?projectId=2\" class=\"bg-primary hover:bg-opacity-90 text-white font-medium py-3 px-8 rounded-md transition-colors shadow-sm\" data-purpose=\"view-item-button\">\r\n          Xem mục công việc\r\n        </button>\r\n</div>\r\n</section>\r\n<!-- END: Content Section -->\r\n<!-- BEGIN: Footer Section -->\r\n<footer class=\"px-6 py-4 bg-gray-50 border-t border-gray-100 flex justify-between items-center text-xs text-gray-400\">\r\n<div>Enterprise Record Management System</div>\r\n<div class=\"space-x-4\">\r\n<a class=\"hover:underline\" href=\"#\">Privacy Policy</a>\r\n<a class=\"hover:underline\" href=\"#\">Notifications Settings</a>\r\n</div>\r\n</footer>\r\n<!-- END: Footer Section -->\r\n</main>\r\n<!-- END: Notification Container -->\r\n</body>";
                message.Body = Template;
                //attachment 

                string sendEmailsFrom = account;
                string sendEmailsFromPassword = password;
                SmtpClient smtp = new SmtpClient(host, Convert.ToInt32(port));
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential(sendEmailsFrom, sendEmailsFromPassword);
                smtp.Timeout = 50000;

                message.To.Add(detail_user.Email);


                smtp.Send(message);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SendEmailpaymentVoucher - MailService: " + ex);
                ressult = false;
            }
            return ressult;
        }
    }
}

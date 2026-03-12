namespace Xtech.CMS.Services.ServiceInterface
{
    public interface IEmailService
    {
        Task<bool> SendEmail(long UserId,long old_AssigneeId, long TaskId);
    }
}

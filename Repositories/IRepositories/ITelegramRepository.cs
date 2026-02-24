using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModels;

namespace Repositories.IRepositories
{
    public interface  ITelegramRepository
    {
        List<TeleBotServer> GetAllcodeTelegram();
        GenericViewModel<TeleBotServer> GetTelegramPagingList(string name, int status, int currentPage, int pageSize);
        Task<int> AddTelegram(TeleBotServer telegrammodel);
      
        TeleBotServer GetTelegrambyid(int id);
        
    }
}

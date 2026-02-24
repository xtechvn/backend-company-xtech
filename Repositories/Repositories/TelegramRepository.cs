using System;
using System.Collections.Generic;
using System.Text;
using Repositories.IRepositories;
using Entities.Models;
using DAL;
using Entities.ConfigModels;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Entities.ViewModels;
using Utilities;

namespace Repositories.Repositories
{
    public class TelegramRepository : ITelegramRepository
    {
        private readonly TelegramDAL _telegramDAL;

        public TelegramRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _telegramDAL = new TelegramDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        // TokenName -> name (search theo Name)
        // Projectmodel bỏ (model mới đâu còn)
        // statusmodel -> status (lọc theo Status)
        public GenericViewModel<TeleBotServer> GetTelegramPagingList(string name, int status, int currentPage, int pageSize)
        {
            var model = new GenericViewModel<TeleBotServer>();

            try
            {
                model.ListData = _telegramDAL.GetTelegramPagingList(name, status, currentPage, pageSize, out int totalRecord);
                model.PageSize = pageSize;
                model.CurrentPage = currentPage;
                model.TotalRecord = totalRecord;
                model.TotalPage = (int)Math.Ceiling((double)totalRecord / pageSize);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTelegramPagingList - TelegramRepository: " + ex);
            }

            return model;
        }

        // Trước là List<TeleBotServer>, giờ đổi sang TeleBotServer
        public List<TeleBotServer> GetAllcodeTelegram()
        {
            return _telegramDAL.GetAll();
        }

        // Trước là AddTelegram(TeleBotServer), giờ xài TeleBotServer
        public Task<int> AddTelegram(TeleBotServer telegrammodel)
        {
            var result = _telegramDAL.AddOrUpdate(telegrammodel);
            return result;
        }

        // Get by id theo model mới
        public TeleBotServer GetTelegrambyid(int id)
        {
            return _telegramDAL.GetById(id);
        }

       
    }
}

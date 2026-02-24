using DAL.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entities.Models;
using Utilities;

namespace DAL
{
    public class TelegramDAL : GenericService<TeleBotServer>
    {
        public TelegramDAL(string connection) : base(connection)
        {
        }

        // Thêm param status
        // Thêm param status
        public List<TeleBotServer> GetTelegramPagingList(string name, int status, int currentPage, int pageSize, out int totalRecord)
        {
            totalRecord = 0;

            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var datalist = _DbContext.TeleBotServer.AsQueryable();

                    // --- FILTER ---
                    if (!string.IsNullOrEmpty(name))
                    {
                        datalist = datalist.Where(s => s.Name.Contains(name));
                    }

                    if (status >= 0)
                    {
                        datalist = datalist.Where(s => s.Status == status);
                    }

                    // ĐẶT SORT THEO ID DESC → record mới luôn đứng đầu
                    datalist = datalist.OrderByDescending(x => x.Id);

                    totalRecord = datalist.Count();

                    // --- LẤY DANH SÁCH ---
                    var list = datalist
                        .Skip((currentPage - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    bool needSave = false;
                    var today = DateTime.Today;

                    // --- CHECK QUÁ HẠN & TỰ ĐỘNG UPDATE STATUS + BẮN LOG ---
                    foreach (var item in list)
                    {
                        // chỉ xử lý khi đang "Đang chạy" (Status = 0)
                        if (item.Status == 0
                            && item.EndDate.HasValue
                            && item.EndDate.Value.Date < today)
                        {
                            item.Status = 1;   // Khóa/tạm dừng
                            _DbContext.TeleBotServer.Update(item);
                            needSave = true;

                            // Bắn log về Telegram: biết con server nào hết hạn
                            try
                            {
                                string msg = $"Server [{item.Name}] đã hết hạn vào ngày {item.EndDate.Value:dd/MM/yyyy}";
                                LogHelper.InsertLogTelegram(msg);
                            }
                            catch (Exception ex)
                            {
                                // Nếu log lỗi thì không được văng, chỉ ghi thêm 1 dòng log lỗi
                                LogHelper.InsertLogTelegram("Error send expire log - TelegramDAL: " + ex);
                            }
                        }
                    }

                    if (needSave)
                    {
                        _DbContext.SaveChanges();
                    }

                    return list;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTelegramPagingList - TelegramDAL: " + ex);
            }

            return null;
        }


        public List<TeleBotServer> GetAll()
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.TeleBotServer
                        .Where(s => s.Id != 0)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetAll - TelegramDAL: " + ex);
                return null;
            }
        }

        public TeleBotServer GetById(int id)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    return _DbContext.TeleBotServer.FirstOrDefault(s => s.Id == id);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - TelegramDAL: " + ex);
            }

            return null;
        }

        public async Task<int> AddOrUpdate(TeleBotServer model)
        {
            try
            {
                using (var _DbContext = new EntityDataContext(_connection))
                {
                    var today = DateTime.Today;

                    int? oldStatus = null;
                    string oldName = null;

                    if (model.Id != 0)
                    {
                        // Lấy bản cũ từ DB để so sánh trạng thái
                        var old = await _DbContext.TeleBotServer
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == model.Id);

                        if (old != null)
                        {
                            oldStatus = old.Status;
                            oldName = old.Name;
                        }
                    }

                    // Nếu chưa có CreatedAt thì set luôn khi tạo
                    if (!model.CreatedAt.HasValue)
                    {
                        model.CreatedAt = DateTime.Now;
                    }

                    // TỰ ĐỘNG TÍNH LẠI STATUS DỰA THEO EndDate
                    if (model.EndDate.HasValue && model.EndDate.Value.Date < today)
                    {
                        // Hết hạn → Khóa/Tạm dừng
                        model.Status = 1;
                    }
                    else
                    {
                        // Chưa hết hạn hoặc không có EndDate → Đang chạy
                        model.Status = 0;
                    }

                    // Lưu trạng thái mới
                    int newStatus = model.Status;

                    if (model.Id == 0)
                    {
                        // Add
                        _DbContext.TeleBotServer.Add(model);
                    }
                    else
                    {
                        // Update
                        _DbContext.TeleBotServer.Update(model);
                    }

                    await _DbContext.SaveChangesAsync();

                    // 🔔 SAU KHI LƯU XONG MỚI GỬI LOG (TRÁNH LỖI TRANSACTION)
                    try
                    {
                        // Case cập nhật: từ Đang chạy (0) sang Khóa/Tạm dừng (1)
                        if (oldStatus.HasValue && oldStatus.Value == 0 && newStatus == 1)
                        {
                            string serverName = !string.IsNullOrEmpty(model.Name) ? model.Name : oldName ?? ("ID=" + model.Id);
                            string msg = $"Server [{serverName}] đã được chuyển sang trạng thái Khóa/Tạm dừng (update).";
                            LogHelper.InsertLogTelegram(msg);
                        }

                        // Case tạo mới mà đã ở trạng thái khóa (ít gặp, nhưng vẫn log nếu muốn)
                        if (!oldStatus.HasValue && newStatus == 1)
                        {
                            string serverName = !string.IsNullOrEmpty(model.Name) ? model.Name : ("ID=" + model.Id);
                            string msg = $"Server [{serverName}] được tạo mới với trạng thái Khóa/Tạm dừng.";
                            LogHelper.InsertLogTelegram(msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.InsertLogTelegram("AddOrUpdate - SendStatusChangeLog ERROR: " + ex);
                    }

                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddOrUpdate - TelegramDAL: " + ex);
            }

            return -1;
        }


    }
}

using Aspose.Cells;
using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Repositories.Repositories.BaseRepos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.IRepositories
{
    public class DepartmentRepository : BaseRepository, IDepartmentRepository
    {
        private readonly DepartmentDAL _DepartmentDAL;
        private const string DEPARTMENT_CODE_FORMAT = "DEPARTMENT.{0}.{1}";

        public DepartmentRepository(IHttpContextAccessor context, IOptions<DataBaseConfig> dataBaseConfig, IUserRepository userRepository, IConfiguration configuration) : base(context, dataBaseConfig, configuration, userRepository)
        {
            _DepartmentDAL = new DepartmentDAL(_SqlServerConnectString);
        }

        public async Task<long> Create(Department model)
        {
            try
            {
                await CheckExistName(model);
                Department parent_model = null;

                if (model.ParentId != null && model.ParentId.Value > 0) parent_model = await GetById(model.ParentId.Value);

                model.FullParent = parent_model != null ? $"{(!String.IsNullOrEmpty(parent_model.FullParent) ? $"{parent_model.FullParent}," : String.Empty)}{parent_model.Id}" : String.Empty;
                model.Status = 0;
                model.CreatedDate = DateTime.Now;
                model.CreatedBy = _SysUserModel.Id;

                return await _DepartmentDAL.CreateAsync(model);
            }
            catch
            {
                throw;
            }
        }

        public async Task<long> Update(Department model)
        {
            try
            {
                await CheckExistName(model);
                Department parent_model = null;

                if (model.ParentId != null && model.ParentId.Value > 0) parent_model = await GetById(model.ParentId.Value);

                var data = await GetById(model.Id);

                data.Description = model.Description;
                data.DepartmentName = model.DepartmentName;
                data.FullParent = parent_model != null ? ($"{(!String.IsNullOrEmpty(parent_model.FullParent) ? $"{parent_model.FullParent}," : data.FullParent)}{parent_model.Id}") : data.FullParent;
                data.DepartmentCode = model.DepartmentCode;
                data.UpdatedDate = DateTime.Now;
                data.UpdatedBy = _SysUserModel.Id;
                data.Branch = model.Branch;

                await _DepartmentDAL.UpdateAsync(data);
                return model.Id;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<Department>> GetAll(string name)
        {
            try
            {
                var datas = await _DepartmentDAL.GetByConditionAsync(s => s.IsDelete == false);


                if (!String.IsNullOrEmpty(name))
                {
                    List<Department> result = datas.Where(s => s.DepartmentName.ToLower().Contains(name.ToLower())).ToList();
                    var full_parent_ids = result.Select(s => s.FullParent).Where(s => !string.IsNullOrEmpty(s))
                        .SelectMany(s => s.Split(',')).Select(s => int.Parse(s)).Distinct().ToList();

                    result.AddRange(datas.Where(s => full_parent_ids.Contains(s.Id)));

                    return result.Distinct();
                }

                return datas;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Department> GetById(int id)
        {
            return await _DepartmentDAL.FindAsync(id);
        }

        public async Task<long> Delete(int id)
        {
            try
            {
                var child_datas = await _DepartmentDAL.GetByConditionAsync(s => s.ParentId == id && s.IsDelete == false);
                if (child_datas != null && child_datas.Any())
                    throw new Exception("Phòng ban đang chứa phòng ban con đang hoạt động. Bạn không thể xóa phòng ban đã chọn.");

                var data = await GetById(id);
                data.IsDelete = true;
                await _DepartmentDAL.UpdateAsync(data);
                return id;
            }
            catch
            {
                throw;
            }
        }

        private async Task<bool> CheckExistName(Department model)
        {
            try
            {
                var datas = await _DepartmentDAL.GetByConditionAsync(s => s.DepartmentName.ToLower() == model.DepartmentName.ToLower()
                && s.ParentId == model.ParentId && s.Id != model.Id);

                if (datas != null && datas.Any())
                {
                    throw new Exception("Tên phòng ban đã tồn tại trong cùng phòng ban Cha");
                }

                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<GenericViewModel<SearchReportDepartmentViewModel>> GetReportDepartment(ReportDepartmentViewModel searchModel)
        {
            var model = new GenericViewModel<SearchReportDepartmentViewModel>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetListRevenueByDepartment(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    model.ListData = dt.ToList<SearchReportDepartmentViewModel>();
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalRecord = dt.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalRow"].ToString());
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetReportDepartment - DepartmentRepository: " + ex);
            }
            return model;
        }
        public async Task<GenericViewModel<ListReportDepartmentViewModel>> GetReportDepartmentsaler(ReportDepartmentViewModel searchModel)
        {
            var model = new GenericViewModel<ListReportDepartmentViewModel>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetListRevenueByDepartment(searchModel);

                DataTable dt2 = await _DepartmentDAL.GetListRevenueBySaler(searchModel);
                var data = new List<SearchReportDepartmentViewModel>();
                var ListReportDepartment = new List<ListReportDepartmentViewModel>();
                var data2 = new List<SearchReportDepartmentViewModel>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    data = dt.ToList<SearchReportDepartmentViewModel>();
                }
                if (dt2 != null && dt2.Rows.Count > 0)
                {
                    data2 = dt2.ToList<SearchReportDepartmentViewModel>();
                    model.TotalRecord = dt2.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt2.Rows[0]["TotalRow"].ToString());
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        var ReportDepartment = new ListReportDepartmentViewModel();
                        var listParentDepartmentId = data2.Where(s => s.ParentDepartmentId == item.ParentDepartmentId && s.DepartmentId == item.DepartmentId).ToList();
                        ReportDepartment.listReportDepartment = listParentDepartmentId;

                        ReportDepartment.ParentDepartmentAmount = listParentDepartmentId.Sum(s => s.Amount);
                        ReportDepartment.ParentDepartmentAmountVat = listParentDepartmentId.Sum(s => s.AmountVat);
                        ReportDepartment.ParentDepartmentComission = listParentDepartmentId.Sum(s => s.Comission);
                        if (listParentDepartmentId.Sum(s => s.Profit) > 0)
                        {
                            ReportDepartment.ParentDepartmentPercent = (listParentDepartmentId.Sum(s => s.Profit) / listParentDepartmentId.Sum(s => s.Amount)) * 100;
                        }
                        else
                        {
                            ReportDepartment.ParentDepartmentPercent = 0;
                        }
                        ReportDepartment.ParentDepartmentTotalOrder = listParentDepartmentId.Sum(s => s.TotalOrder);
                        ReportDepartment.ParentDepartmentPrice = listParentDepartmentId.Sum(s => s.Price);
                        ReportDepartment.ParentDepartmentPriceVat = listParentDepartmentId.Sum(s => s.PriceVat);
                        ReportDepartment.ParentDepartmentProfitVat = listParentDepartmentId.Sum(s => s.ProfitVat);
                        ReportDepartment.ParentDepartmentProfit = listParentDepartmentId.Sum(s => s.Profit);
                        ReportDepartment.ParentDepartmentName = item.ParentDepartmentName;
                        ReportDepartment.DepartmentName = item.DepartmentName;

                        ListReportDepartment.Add(ReportDepartment);
                    }
                }

                model.ListData = ListReportDepartment;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetReportDepartment - DepartmentRepository: " + ex);
            }
            return model;
        }
        public async Task<GenericViewModel<SearchReportDepartmentSupplier>> GetRevenueBySupplier(ReportDepartmentViewModel searchModel)
        {
            var model = new GenericViewModel<SearchReportDepartmentSupplier>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetListRevenueBySupplier(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    model.ListData = dt.ToList<SearchReportDepartmentSupplier>();
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalRecord = dt.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalRow"].ToString());
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetRevenueBySupplier - DepartmentRepository: " + ex);
            }
            return model;
        }
        public async Task<GenericViewModel<SearchReportDepartmentClient>> GetRevenueByClient(ReportDepartmentViewModel searchModel)
        {
            var model = new GenericViewModel<SearchReportDepartmentClient>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetListRevenueByClient(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    model.ListData = dt.ToList<SearchReportDepartmentClient>();
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalRecord = dt.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalRow"].ToString());
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetRevenueByClient - DepartmentRepository: " + ex);
            }
            return model;
        }
        public async Task<GenericViewModel<DetailRevenueByDepartmentViewModel>> GetListDetailRevenueByDepartment(ReportDepartmentViewModel searchModel)
        {
            var model = new GenericViewModel<DetailRevenueByDepartmentViewModel>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetListDetailRevenueByDepartment(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    model.ListData = dt.ToList<DetailRevenueByDepartmentViewModel>();
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalRecord = dt.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalRow"].ToString());
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetReportDepartment - DepartmentRepository: " + ex);
            }
            return model;
        }
        public async Task<GenericViewModel<ListDetailRevenueByDepartmentViewModel>> GetListDetailRevenueBySaler(ReportDepartmentViewModel searchModel)
        {
            var model = new GenericViewModel<ListDetailRevenueByDepartmentViewModel>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetListDetailRevenueByDepartment(searchModel);
                DataTable dt2 = await _DepartmentDAL.GetListDetailRevenueBySaler(searchModel);
                var data = new List<DetailRevenueByDepartmentViewModel>();
                var ListReportDepartment = new List<ListDetailRevenueByDepartmentViewModel>();
                var data2 = new List<DetailRevenueByDepartmentViewModel>();

                if (dt != null && dt.Rows.Count > 0)
                {
                    data = dt.ToList<DetailRevenueByDepartmentViewModel>();
                }
                if (dt2 != null && dt2.Rows.Count > 0)
                {
                    data2 = dt2.ToList<DetailRevenueByDepartmentViewModel>();
                    model.TotalRecord = dt2.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt2.Rows[0]["TotalRow"].ToString());
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        var ReportDepartment = new ListDetailRevenueByDepartmentViewModel();
                        var listParentDepartmentId = data2.Where(s => s.ParentDepartmentId == item.ParentDepartmentId && s.DepartmentId == item.DepartmentId).ToList();
                        ReportDepartment.listReportDepartment = listParentDepartmentId;

                        ReportDepartment.ParentDepartmentAmount = listParentDepartmentId.Sum(s => s.Amount);
                        ReportDepartment.ParentDepartmentAmountVat = listParentDepartmentId.Sum(s => s.AmountVat);
                        ReportDepartment.ParentDepartmentComission = listParentDepartmentId.Sum(s => s.Comission);
                        ReportDepartment.ParentDepartmentPercent = (listParentDepartmentId.Sum(s => s.Profit) / listParentDepartmentId.Sum(s => s.Amount)) * 100;
                        ReportDepartment.ParentDepartmentTotalOrder = listParentDepartmentId.Sum(s => s.TotalOrder);
                        ReportDepartment.ParentDepartmentPrice = listParentDepartmentId.Sum(s => s.Price);
                        ReportDepartment.ParentDepartmentPriceVat = listParentDepartmentId.Sum(s => s.PriceVat);
                        ReportDepartment.ParentDepartmentProfitVat = listParentDepartmentId.Sum(s => s.ProfitVat);
                        ReportDepartment.ParentDepartmentProfit = listParentDepartmentId.Sum(s => s.Profit);

                        ReportDepartment.DepartmentFlyBookingAmount = listParentDepartmentId.Sum(s => s.FlyBookingAmount);
                        ReportDepartment.DepartmentFlyBookingPrice = listParentDepartmentId.Sum(s => s.FlyBookingPrice);
                        ReportDepartment.DepartmentFlyBookingProfit = listParentDepartmentId.Sum(s => s.FlyBookingProfit);
                        ReportDepartment.DepartmentHotelBookingAmount = listParentDepartmentId.Sum(s => s.HotelBookingAmount);
                        ReportDepartment.DepartmentHotelBookingPrice = listParentDepartmentId.Sum(s => s.HotelBookingPrice);
                        ReportDepartment.DepartmentHotelBookingProfit = listParentDepartmentId.Sum(s => s.HotelBookingProfit);
                        ReportDepartment.DepartmentTourAmount = listParentDepartmentId.Sum(s => s.TourAmount);
                        ReportDepartment.DepartmentTourPrice = listParentDepartmentId.Sum(s => s.TourPrice);
                        ReportDepartment.DepartmentTourProfit = listParentDepartmentId.Sum(s => s.TourProfit);
                        ReportDepartment.DepartmentOtherBookingAmount = listParentDepartmentId.Sum(s => s.OtherBookingAmount);
                        ReportDepartment.DepartmentOtherBookingPrice = listParentDepartmentId.Sum(s => s.OtherBookingPrice);
                        ReportDepartment.DepartmentOtherBookingProfit = listParentDepartmentId.Sum(s => s.OtherBookingProfit);
                        ReportDepartment.DepartmentVinWonderAmount = listParentDepartmentId.Sum(s => s.VinWonderAmount);
                        ReportDepartment.DepartmentVinWonderPrice = listParentDepartmentId.Sum(s => s.VinWonderPrice);
                        ReportDepartment.DepartmentVinWonderProfit = listParentDepartmentId.Sum(s => s.VinWonderProfit);

                        ReportDepartment.ParentDepartmentName = item.ParentDepartmentName;
                        ReportDepartment.DepartmentName = item.DepartmentName;

                        ReportDepartment.DepartmentName = item.DepartmentName;

                        ListReportDepartment.Add(ReportDepartment);
                    }
                }

                model.ListData = ListReportDepartment;

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetReportDepartment - DepartmentRepository: " + ex);
            }
            return model;
        }

        public async Task<GenericViewModel<DetailRevenueByDepartmentViewModel>> GetListDetailRevenueBySupplier(ReportDepartmentViewModel searchModel)
        {
            var model = new GenericViewModel<DetailRevenueByDepartmentViewModel>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetListDetailRevenueBySupplier(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    model.ListData = dt.ToList<DetailRevenueByDepartmentViewModel>();
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalRecord = dt.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalRow"].ToString());
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListDetailRevenueBySupplier - DepartmentRepository: " + ex);
            }
            return model;
        }
        public async Task<GenericViewModel<DetailRevenueByDepartmentViewModel>> GetListDetailRevenueByClient(ReportDepartmentViewModel searchModel)
        {
            var model = new GenericViewModel<DetailRevenueByDepartmentViewModel>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetListDetailRevenueByClient(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    model.ListData = dt.ToList<DetailRevenueByDepartmentViewModel>();
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalRecord = dt.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalRow"].ToString());
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListDetailRevenueByClient - DepartmentRepository: " + ex);
            }
            return model;
        }
        public async Task<GenericViewModel<OrderViewModel>> GetListOrder(ReportDepartmentViewModel searchModel)
        {
            var model = new GenericViewModel<OrderViewModel>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetListOrder(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    model.ListData = dt.ToList<OrderViewModel>();
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalRecord = dt.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalRow"].ToString());
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListOrder - DepartmentRepository: " + ex);
            }
            return model;
        }

        public async Task<string> ExportDeposit(ReportDepartmentViewModel searchModel, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                searchModel.PageIndex = -1;
                if (searchModel.Type == 1)
                {
                    switch (Convert.ToInt32(searchModel.DepartmentType))
                    {
                        case (int)DepartmentType.RevenueByDepartment:
                            {
                                var data = new List<SearchReportDepartmentViewModel>();
                                DataTable dt = await _DepartmentDAL.GetListRevenueByDepartment(searchModel);
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    data = dt.ToList<SearchReportDepartmentViewModel>();
                                }
                                if (data != null && data.Count > 0)
                                {
                                    Workbook wb = new Workbook();
                                    Worksheet ws = wb.Worksheets[0];
                                    ws.Name = "Doanh thu tổng theo PBBH";
                                    Cells cell = ws.Cells;

                                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                                    StyleFlag st = new StyleFlag();
                                    st.All = true;
                                    int Index = 1;
                                    Style style = ws.Cells["A1"].GetStyle();

                                    #region Header
                                    range = cell.CreateRange(0, 0, 1, 15);
                                    style = ws.Cells["A1"].GetStyle();
                                    style.Font.IsBold = true;
                                    style.IsTextWrapped = true;
                                    //style.ForegroundColor = Color.FromArgb(33, 88, 103);
                                    //style.BackgroundColor = Color.FromArgb(33, 88, 103);
                                    style.Pattern = BackgroundType.Solid;
                                    //style.Font.Color = Color.White;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    range.ApplyStyle(style, st);

                                    // Set column width
                                    cell.SetColumnWidth(0, 8);
                                    cell.SetColumnWidth(1, 20);
                                    cell.SetColumnWidth(2, 20);
                                    cell.SetColumnWidth(3, 20);
                                    cell.SetColumnWidth(4, 20);
                                    cell.SetColumnWidth(5, 30);
                                    cell.SetColumnWidth(6, 30);
                                    cell.SetColumnWidth(7, 25);
                                    cell.SetColumnWidth(8, 25);
                                    cell.SetColumnWidth(9, 25);
                                    cell.SetColumnWidth(10, 25);
                                    cell.SetColumnWidth(11, 25);
                                    cell.SetColumnWidth(12, 25);
                                    cell.SetColumnWidth(13, 25);
                                    cell.SetColumnWidth(14, 25);
                                    cell.SetColumnWidth(15, 25);


                                    // Set header value
                                    cell.Merge(0, 3, 1, 2);
                                    ws.Cells["D1"].PutValue("Tổng doanh thu (có VAT)");
                                    cell.Merge(0, 8, 1, 3);
                                    ws.Cells["I1"].PutValue("Tổng tính lương (chưa VAT)");


                                    ws.Cells["A2"].PutValue("STT");
                                    ws.Cells["B2"].PutValue("Phòng ban");
                                    ws.Cells["C2"].PutValue("Số lượng đơn hàng");
                                    ws.Cells["D2"].PutValue("Giá thu khách hàng");
                                    ws.Cells["E2"].PutValue("Giá thanh toán NCC");
                                    ws.Cells["F2"].PutValue("Hoa hồng CTV");
                                    ws.Cells["G2"].PutValue("Lợi nhuận (có VAT)");
                                    ws.Cells["H2"].PutValue("Tỷ suất");
                                    ws.Cells["I2"].PutValue("Doanh thu");
                                    ws.Cells["J2"].PutValue("Giá vốn");
                                    ws.Cells["k2"].PutValue("Lợi nhuận");

                                    ws.Cells["L2"].PutValue("Ngày In bắt đầu");
                                    ws.Cells["M2"].PutValue("Ngày In kết thúc");
                                    ws.Cells["N2"].PutValue("Ngày Out bắt đầu");
                                    ws.Cells["O2"].PutValue("Ngày Out kết thúc");

                                    #endregion

                                    #region Body

                                    range = cell.CreateRange(1, 0, data.Count + 2, 15);
                                    style = ws.Cells["A3"].GetStyle();
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    range.ApplyStyle(style, st);

                                    Style alignCenterStyle = ws.Cells["A3"].GetStyle();
                                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                                    Style numberStyle = ws.Cells["A2"].GetStyle();
                                    numberStyle.Number = 3;
                                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                                    int RowIndex = 3;



                                    ws.Cells["B" + RowIndex].PutValue("Tổng cộng");
                                    ws.Cells["C" + RowIndex].PutValue(data.Sum(s => s.TotalOrder));
                                    ws.Cells["C" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["D" + RowIndex].PutValue(data.Sum(s => s.Amount));
                                    ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["E" + RowIndex].PutValue(data.Sum(s => s.Price));
                                    ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["F" + RowIndex].PutValue(data.Sum(s => s.Comission));
                                    ws.Cells["F" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["G" + RowIndex].PutValue(data.Sum(s => s.Profit));
                                    ws.Cells["G" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["H" + RowIndex].PutValue(data.Sum(s => s.Profit) <= 0 ? "0" : ((data.Sum(s => s.Profit) / data.Sum(s => s.Amount)) * 100).ToString("N2") + "%");
                                    ws.Cells["H" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["I" + RowIndex].PutValue(data.Sum(s => s.AmountVat));
                                    ws.Cells["I" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["J" + RowIndex].PutValue(data.Sum(s => s.PriceVat));
                                    ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["k" + RowIndex].PutValue(data.Sum(s => s.ProfitVat));
                                    ws.Cells["K" + RowIndex].SetStyle(numberStyle);

                                    foreach (var item in data)
                                    {

                                        RowIndex++;
                                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 3);
                                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                                        ws.Cells["B" + RowIndex].PutValue(item.DepartmentName);
                                        ws.Cells["C" + RowIndex].PutValue(item.TotalOrder);
                                        ws.Cells["D" + RowIndex].PutValue(item.Amount);
                                        ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["E" + RowIndex].PutValue(item.Price);
                                        ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["F" + RowIndex].PutValue(item.Comission);
                                        ws.Cells["F" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["G" + RowIndex].PutValue(item.Profit);
                                        ws.Cells["G" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["H" + RowIndex].PutValue(item.Percent.ToString("N2") + "%");
                                        ws.Cells["I" + RowIndex].PutValue(item.AmountVat);
                                        ws.Cells["I" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["J" + RowIndex].PutValue(item.PriceVat);
                                        ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["k" + RowIndex].PutValue(item.ProfitVat);
                                        ws.Cells["k" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["L" + RowIndex].PutValue(searchModel.StartDateFromStr);
                                        ws.Cells["M" + RowIndex].PutValue(searchModel.StartDateToStr);
                                        ws.Cells["N" + RowIndex].PutValue(searchModel.EndDateFromStr);
                                        ws.Cells["O" + RowIndex].PutValue(searchModel.EndDateToStr);

                                    }

                                    #endregion
                                    wb.Save(FilePath);
                                    pathResult = FilePath;
                                }
                            }
                            break;
                        case (int)DepartmentType.RevenueByDepartmentsaler:
                            {

                                DataTable dt = await _DepartmentDAL.GetListRevenueByDepartment(searchModel);
                                DataTable dt2 = await _DepartmentDAL.GetListRevenueBySaler(searchModel);
                                var data = new List<SearchReportDepartmentViewModel>();
                                var List_ReportDepartment = new List<ListReportDepartmentViewModel>();
                                var data2 = new List<SearchReportDepartmentViewModel>();

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    data = dt.ToList<SearchReportDepartmentViewModel>();
                                }
                                if (dt2 != null && dt2.Rows.Count > 0)
                                {
                                    data2 = dt2.ToList<SearchReportDepartmentViewModel>();

                                }
                                if (data != null && data.Count > 0)
                                {
                                    foreach (var item in data)
                                    {
                                        var ReportDepartment = new ListReportDepartmentViewModel();
                                        var listParentDepartmentId = data2.Where(s => s.ParentDepartmentId == item.ParentDepartmentId && s.DepartmentId == item.DepartmentId).ToList();
                                        ReportDepartment.listReportDepartment = listParentDepartmentId;

                                        ReportDepartment.ParentDepartmentAmount = listParentDepartmentId.Sum(s => s.Amount);
                                        ReportDepartment.ParentDepartmentAmountVat = listParentDepartmentId.Sum(s => s.AmountVat);
                                        ReportDepartment.ParentDepartmentComission = listParentDepartmentId.Sum(s => s.Comission);
                                        ReportDepartment.ParentDepartmentPercent = listParentDepartmentId.Sum(s => s.Percent);
                                        ReportDepartment.ParentDepartmentTotalOrder = listParentDepartmentId.Sum(s => s.TotalOrder);
                                        ReportDepartment.ParentDepartmentPrice = listParentDepartmentId.Sum(s => s.Price);
                                        ReportDepartment.ParentDepartmentPriceVat = listParentDepartmentId.Sum(s => s.PriceVat);
                                        ReportDepartment.ParentDepartmentProfitVat = listParentDepartmentId.Sum(s => s.ProfitVat);
                                        ReportDepartment.ParentDepartmentProfit = listParentDepartmentId.Sum(s => s.Profit);
                                        ReportDepartment.ParentDepartmentName = item.ParentDepartmentName;
                                        ReportDepartment.DepartmentName = item.DepartmentName;

                                        List_ReportDepartment.Add(ReportDepartment);
                                    }
                                }
                                if (List_ReportDepartment != null && List_ReportDepartment.Count > 0)
                                {
                                    Workbook wb = new Workbook();
                                    Worksheet ws = wb.Worksheets[0];
                                    ws.Name = "Doanh thu tổng theo NV";
                                    Cells cell = ws.Cells;

                                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                                    StyleFlag st = new StyleFlag();
                                    st.All = true;
                                    Style style = ws.Cells["A1"].GetStyle();

                                    #region Header
                                    range = cell.CreateRange(0, 0, 1, 15);
                                    style = ws.Cells["A1"].GetStyle();
                                    style.Font.IsBold = true;
                                    style.IsTextWrapped = true;
                                    /*  style.ForegroundColor = Color.FromArgb(33, 88, 103);
                                      style.BackgroundColor = Color.FromArgb(33, 88, 103);*/
                                    style.Pattern = BackgroundType.Solid;
                                    /*style.Font.Color = Color.White;*/
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    range.ApplyStyle(style, st);

                                    // Set column width
                                    cell.SetColumnWidth(0, 8);
                                    cell.SetColumnWidth(1, 20);
                                    cell.SetColumnWidth(2, 20);
                                    cell.SetColumnWidth(3, 20);
                                    cell.SetColumnWidth(4, 20);
                                    cell.SetColumnWidth(5, 30);
                                    cell.SetColumnWidth(6, 30);
                                    cell.SetColumnWidth(7, 25);
                                    cell.SetColumnWidth(8, 25);
                                    cell.SetColumnWidth(9, 25);
                                    cell.SetColumnWidth(10, 25);
                                    cell.SetColumnWidth(11, 25);
                                    cell.SetColumnWidth(12, 25);
                                    cell.SetColumnWidth(13, 25);
                                    cell.SetColumnWidth(14, 25);
                                    cell.SetColumnWidth(15, 25);


                                    // Set header value
                                    cell.Merge(0, 3, 1, 2);
                                    ws.Cells["D1"].PutValue("Tổng doanh thu (có VAT)");
                                    cell.Merge(0, 8, 1, 3);
                                    ws.Cells["I1"].PutValue("Tổng tính lương (chưa VAT)");

                                    ws.Cells["A2"].PutValue("STT");
                                    ws.Cells["B2"].PutValue("Phòng ban");
                                    ws.Cells["C2"].PutValue("Số lượng đơn hàng");
                                    ws.Cells["D2"].PutValue("Giá thu khách hàng");
                                    ws.Cells["E2"].PutValue("Giá thanh toán NCC");
                                    ws.Cells["F2"].PutValue("Hoa hồng CTV");
                                    ws.Cells["G2"].PutValue("Lợi nhuận (có VAT)");
                                    ws.Cells["H2"].PutValue("Tỷ suất");
                                    ws.Cells["I2"].PutValue("Doanh thu");
                                    ws.Cells["J2"].PutValue("Giá vốn");
                                    ws.Cells["k2"].PutValue("Lợi nhuận");
                                    ws.Cells["L2"].PutValue("Ngày In bắt đầu");
                                    ws.Cells["M2"].PutValue("Ngày In kết thúc");
                                    ws.Cells["N2"].PutValue("Ngày Out bắt đầu");
                                    ws.Cells["O2"].PutValue("Ngày Out kết thúc");

                                    #endregion

                                    #region Body
                                   
                                    range = cell.CreateRange(1, 0, List_ReportDepartment.Sum(s => s.listReportDepartment.Count()) + data.Count + 2, 16);
                                    style = ws.Cells["A3"].GetStyle();
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    range.ApplyStyle(style, st);

                                    Style alignCenterStyle = ws.Cells["A3"].GetStyle();
                                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                                    Style numberStyle = ws.Cells["A2"].GetStyle();
                                    numberStyle.Number = 3;
                                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                                    int RowIndex = 3;
                                    ws.Cells["B" + RowIndex].PutValue("Tổng cộng");
                                    ws.Cells["C" + RowIndex].PutValue(data.Sum(s => s.TotalOrder));
                                    ws.Cells["C" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["D" + RowIndex].PutValue(data.Sum(s => s.Amount));
                                    ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["E" + RowIndex].PutValue(data.Sum(s => s.Price));
                                    ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["F" + RowIndex].PutValue(data.Sum(s => s.Comission));
                                    ws.Cells["F" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["G" + RowIndex].PutValue(data.Sum(s => s.Profit));
                                    ws.Cells["G" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["H" + RowIndex].PutValue(data.Sum(s => s.Profit) <= 0 ? "0" : ((data.Sum(s => s.Profit) / data.Sum(s => s.Amount)) * 100).ToString("N2") + "%");
                                    ws.Cells["H" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["I" + RowIndex].PutValue(data.Sum(s => s.AmountVat));
                                    ws.Cells["I" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["J" + RowIndex].PutValue(data.Sum(s => s.PriceVat));
                                    ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                                    ws.Cells["k" + RowIndex].PutValue(data.Sum(s => s.ProfitVat));
                                    ws.Cells["K" + RowIndex].SetStyle(numberStyle);
                                    foreach (var item in List_ReportDepartment)
                                    {

                                        RowIndex++;
                                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 3);
                                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                                        ws.Cells["B" + RowIndex].PutValue(item.DepartmentName);
                                        ws.Cells["C" + RowIndex].PutValue(item.ParentDepartmentTotalOrder);
                                        ws.Cells["D" + RowIndex].PutValue(item.ParentDepartmentAmount);
                                        ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["E" + RowIndex].PutValue(item.ParentDepartmentPrice);
                                        ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["F" + RowIndex].PutValue(item.ParentDepartmentComission);
                                        ws.Cells["F" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["G" + RowIndex].PutValue(item.ParentDepartmentProfit);
                                        ws.Cells["G" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["H" + RowIndex].PutValue(item.ParentDepartmentPercent.ToString("N2") + "%");
                                        ws.Cells["I" + RowIndex].PutValue(item.ParentDepartmentAmountVat);
                                        ws.Cells["I" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["J" + RowIndex].PutValue(item.ParentDepartmentPriceVat);
                                        ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["k" + RowIndex].PutValue(item.ParentDepartmentProfitVat);
                                        ws.Cells["k" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["L" + RowIndex].PutValue(searchModel.StartDateFromStr);
                                        ws.Cells["M" + RowIndex].PutValue(searchModel.StartDateToStr);
                                        ws.Cells["N" + RowIndex].PutValue(searchModel.EndDateFromStr);
                                        ws.Cells["O" + RowIndex].PutValue(searchModel.EndDateToStr);
                                        foreach (var item2 in item.listReportDepartment)
                                        {
                                            RowIndex++;
                                            ws.Cells["A" + RowIndex].PutValue(RowIndex - 3);
                                            ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                                            ws.Cells["B" + RowIndex].PutValue(item2.UserName + ": " + item2.FullName);
                                            ws.Cells["C" + RowIndex].PutValue(item2.TotalOrder);
                                            ws.Cells["D" + RowIndex].PutValue(item2.Amount);
                                            ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["E" + RowIndex].PutValue(item2.Price);
                                            ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["F" + RowIndex].PutValue(item2.Comission);
                                            ws.Cells["F" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["G" + RowIndex].PutValue(item2.Profit);
                                            ws.Cells["G" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["H" + RowIndex].PutValue(item2.Percent.ToString("N2") + "%");
                                            ws.Cells["I" + RowIndex].PutValue(item2.AmountVat);
                                            ws.Cells["I" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["J" + RowIndex].PutValue(item2.PriceVat);
                                            ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["k" + RowIndex].PutValue(item2.ProfitVat);
                                            ws.Cells["k" + RowIndex].SetStyle(numberStyle);

                                            ws.Cells["L" + RowIndex].PutValue(searchModel.StartDateFromStr);
                                            ws.Cells["M" + RowIndex].PutValue(searchModel.StartDateToStr);
                                            ws.Cells["N" + RowIndex].PutValue(searchModel.EndDateFromStr);
                                            ws.Cells["O" + RowIndex].PutValue(searchModel.EndDateToStr);
                                        }

                                    }

                                    #endregion
                                    wb.Save(FilePath);
                                    pathResult = FilePath;
                                }

                            }
                            break;
                        case (int)DepartmentType.RevenueByDepartmentSupplier:
                            {
                                var data = new List<SearchReportDepartmentSupplier>();
                                DataTable dt = await _DepartmentDAL.GetListRevenueBySupplier(searchModel);
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    data = dt.ToList<SearchReportDepartmentSupplier>();
                                }
                                if (data != null && data.Count > 0)
                                {
                                    Workbook wb = new Workbook();
                                    Worksheet ws = wb.Worksheets[0];
                                    ws.Name = "Doanh thu tổng theo NCC";
                                    Cells cell = ws.Cells;

                                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                                    StyleFlag st = new StyleFlag();
                                    st.All = true;
                                    Style style = ws.Cells["A1"].GetStyle();

                                    #region Header
                                    range = cell.CreateRange(0, 0, 1, 5);
                                    style = ws.Cells["A1"].GetStyle();
                                    style.Font.IsBold = true;
                                    style.IsTextWrapped = true;
                                    /*    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                                        style.BackgroundColor = Color.FromArgb(33, 88, 103);*/
                                    style.Pattern = BackgroundType.Solid;
                                    /*   style.Font.Color = Color.White;*/
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    range.ApplyStyle(style, st);

                                    // Set column width
                                    cell.SetColumnWidth(0, 8);
                                    cell.SetColumnWidth(1, 20);
                                    cell.SetColumnWidth(2, 40);
                                    cell.SetColumnWidth(3, 20);
                                    cell.SetColumnWidth(4, 20);



                                    // Set header value
                                    cell.Merge(0, 3, 1, 2);
                                    ws.Cells["D1"].PutValue("Tổng doanh thu (có VAT)");

                                    ws.Cells["A2"].PutValue("STT");
                                    ws.Cells["B2"].PutValue("Nhà cung cấp");
                                    ws.Cells["C2"].PutValue("Giá thu khách hàng");
                                    ws.Cells["D2"].PutValue("Giá thanh toán NCC");
                                    ws.Cells["E2"].PutValue("Lợi nhuận (có VAT)");

                                    ws.Cells["F2"].PutValue("Ngày In bắt đầu");
                                    ws.Cells["G2"].PutValue("Ngày In kết thúc");
                                    ws.Cells["H2"].PutValue("Ngày Out bắt đầu");
                                    ws.Cells["I2"].PutValue("Ngày Out kết thúc");

                                    #endregion

                                    #region Body

                                    range = cell.CreateRange(1, 0, data.Count + 2, 11);
                                    style = ws.Cells["A3"].GetStyle();
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    range.ApplyStyle(style, st);

                                    Style alignCenterStyle = ws.Cells["A3"].GetStyle();
                                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                                    Style numberStyle = ws.Cells["I3"].GetStyle();
                                    numberStyle.Number = 3;
                                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                                    int RowIndex = 2;

                                    foreach (var item in data)
                                    {

                                        RowIndex++;
                                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 2);
                                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                                        ws.Cells["B" + RowIndex].PutValue(item.FullName);
                                        ws.Cells["C" + RowIndex].PutValue(item.Amount);
                                        ws.Cells["C" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["D" + RowIndex].PutValue(item.Price);
                                        ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["E" + RowIndex].PutValue(item.Profit);
                                        ws.Cells["E" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["F" + RowIndex].PutValue(searchModel.StartDateFromStr);
                                        ws.Cells["G" + RowIndex].PutValue(searchModel.StartDateToStr);
                                        ws.Cells["H" + RowIndex].PutValue(searchModel.EndDateFromStr);
                                        ws.Cells["I" + RowIndex].PutValue(searchModel.EndDateToStr);

                                    }

                                    #endregion
                                    wb.Save(FilePath);
                                    pathResult = FilePath;
                                }
                            }
                            break;
                        case (int)DepartmentType.RevenueByDepartmentClient:
                            {
                                var data = new List<SearchReportDepartmentClient>();
                                DataTable dt = await _DepartmentDAL.GetListRevenueByClient(searchModel);
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    data = dt.ToList<SearchReportDepartmentClient>();
                                }
                                if (data != null && data.Count > 0)
                                {
                                    Workbook wb = new Workbook();
                                    Worksheet ws = wb.Worksheets[0];
                                    ws.Name = "Doanh thu tổng theo Khách hàng";
                                    Cells cell = ws.Cells;

                                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                                    StyleFlag st = new StyleFlag();
                                    st.All = true;
                                    Style style = ws.Cells["A1"].GetStyle();

                                    #region Header
                                    range = cell.CreateRange(0, 0, 1, 11);
                                    style = ws.Cells["A1"].GetStyle();
                                    style.Font.IsBold = true;
                                    style.IsTextWrapped = true;
                                    /*  style.ForegroundColor = Color.FromArgb(33, 88, 103);
                                      style.BackgroundColor = Color.FromArgb(33, 88, 103);*/
                                    style.Pattern = BackgroundType.Solid;
                                    /*    style.Font.Color = Color.White;*/
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    range.ApplyStyle(style, st);

                                    // Set column width
                                    cell.SetColumnWidth(0, 8);
                                    cell.SetColumnWidth(1, 20);
                                    cell.SetColumnWidth(2, 40);
                                    cell.SetColumnWidth(3, 20);
                                    cell.SetColumnWidth(4, 20);
                                    cell.SetColumnWidth(5, 30);
                                    cell.SetColumnWidth(6, 40);
                                    cell.SetColumnWidth(7, 20);
                                    cell.SetColumnWidth(8, 20);
                                    cell.SetColumnWidth(9, 30);




                                    // Set header value
                                    cell.Merge(0, 3, 1, 2);
                                    ws.Cells["D1"].PutValue("Tổng doanh thu (có VAT)");

                                    ws.Cells["A2"].PutValue("STT");
                                    ws.Cells["B2"].PutValue("Phòng ban");
                                    ws.Cells["C2"].PutValue("Số lượng đơn hàng");
                                    ws.Cells["D2"].PutValue("Giá thu khách hàng");
                                    ws.Cells["E2"].PutValue("Giá thanh toán NCC");
                                    ws.Cells["F2"].PutValue("Lợi nhuận (có VAT)");
                                    ws.Cells["G2"].PutValue("Ngày In bắt đầu");
                                    ws.Cells["G2"].PutValue("Ngày In kết thúc");
                                    ws.Cells["I2"].PutValue("Ngày Out bắt đầu");
                                    ws.Cells["J2"].PutValue("Ngày Out kết thúc");

                                    #endregion

                                    #region Body

                                    range = cell.CreateRange(1, 0, data.Count, 10);
                                    style = ws.Cells["A3"].GetStyle();
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    range.ApplyStyle(style, st);

                                    Style alignCenterStyle = ws.Cells["A3"].GetStyle();
                                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                                    Style numberStyle = ws.Cells["I3"].GetStyle();
                                    numberStyle.Number = 3;
                                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                                    int RowIndex = 2;

                                    foreach (var item in data)
                                    {

                                        RowIndex++;
                                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 1);
                                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                                        ws.Cells["B" + RowIndex].PutValue(item.ClientName);
                                        ws.Cells["C" + RowIndex].PutValue(item.TotalOrder);
                                        ws.Cells["D" + RowIndex].PutValue(item.Amount);
                                        ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["E" + RowIndex].PutValue(item.Price);
                                        ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["F" + RowIndex].PutValue(item.Profit);
                                        ws.Cells["F" + RowIndex].SetStyle(numberStyle);


                                        ws.Cells["g" + RowIndex].PutValue(searchModel.StartDateFromStr);
                                        ws.Cells["H" + RowIndex].PutValue(searchModel.StartDateToStr);
                                        ws.Cells["I" + RowIndex].PutValue(searchModel.EndDateFromStr);
                                        ws.Cells["J" + RowIndex].PutValue(searchModel.EndDateToStr);

                                    }

                                    #endregion
                                    wb.Save(FilePath);
                                    pathResult = FilePath;
                                }
                            }
                            break;
                    }
                }
                else
                {
                    switch (Convert.ToInt32(searchModel.DepartmentType))
                    {
                        case (int)DepartmentType.RevenueByDepartment:
                            {
                                var data = new List<DetailRevenueByDepartmentViewModel>();
                                DataTable dt = await _DepartmentDAL.GetListDetailRevenueByDepartment(searchModel);
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    data = dt.ToList<DetailRevenueByDepartmentViewModel>();
                                }
                                if (data != null && data.Count > 0)
                                {
                                    Workbook wb = new Workbook();
                                    Worksheet ws = wb.Worksheets[0];
                                    ws.Name = "Doanh thu CTDV theo PBBH";
                                    Cells cell = ws.Cells;

                                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                                    StyleFlag st = new StyleFlag();
                                    st.All = true;
                                    Style style = ws.Cells["A1"].GetStyle();

                                    #region Header
                                    range = cell.CreateRange(0, 0, 1, 30);
                                    style = ws.Cells["A1"].GetStyle();
                                    style.Font.IsBold = true;
                                    style.IsTextWrapped = true;
                                    //style.ForegroundColor = Color.FromArgb(33, 88, 103);
                                    //style.BackgroundColor = Color.FromArgb(33, 88, 103);
                                    style.Pattern = BackgroundType.Solid;
                                    //style.Font.Color = Color.White;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    range.ApplyStyle(style, st);

                                    // Set column width
                                    cell.SetColumnWidth(0, 8);
                                    cell.SetColumnWidth(1, 20);
                                    cell.SetColumnWidth(2, 40);
                                    cell.SetColumnWidth(3, 20);
                                    cell.SetColumnWidth(4, 20);
                                    cell.SetColumnWidth(5, 30);
                                    cell.SetColumnWidth(6, 30);
                                    cell.SetColumnWidth(7, 25);
                                    cell.SetColumnWidth(8, 25);
                                    cell.SetColumnWidth(9, 25);
                                    cell.SetColumnWidth(10, 25);
                                    cell.SetColumnWidth(11, 25);
                                    cell.SetColumnWidth(12, 25);
                                    cell.SetColumnWidth(13, 25);
                                    cell.SetColumnWidth(14, 25);
                                    cell.SetColumnWidth(15, 25);
                                    cell.SetColumnWidth(16, 25);
                                    cell.SetColumnWidth(17, 25);
                                    cell.SetColumnWidth(18, 25);
                                    cell.SetColumnWidth(19, 25);
                                    cell.SetColumnWidth(20, 25);
                                    cell.SetColumnWidth(21, 25);
                                    cell.SetColumnWidth(22, 25);
                                    cell.SetColumnWidth(23, 25);
                                    cell.SetColumnWidth(24, 25);
                                    cell.SetColumnWidth(25, 25);
                                    cell.SetColumnWidth(26, 25);
                                    cell.SetColumnWidth(27, 25);
                                    cell.SetColumnWidth(28, 25);
                                    cell.SetColumnWidth(29, 25);


                                    // Set header value
                                    cell.Merge(0, 3, 1, 2);
                                    ws.Cells["D1"].PutValue("Tổng doanh thu (có VAT)");
                                    cell.Merge(0, 8, 1, 3);
                                    ws.Cells["I1"].PutValue("Khách sạn (chưa VAT)");
                                    cell.Merge(0, 11, 1, 3);
                                    ws.Cells["L1"].PutValue("Vé máy bay (chưa VAT)");
                                    cell.Merge(0, 14, 1, 3);
                                    ws.Cells["O1"].PutValue("Tour du lịch (chưa VAT)");
                                    cell.Merge(0, 17, 1, 3);
                                    ws.Cells["R1"].PutValue("Dịch vụ khác (chưa VAT)");
                                    cell.Merge(0, 20, 1, 3);
                                    ws.Cells["U1"].PutValue("VinWonder (chưa VAT)");
                                    cell.Merge(0, 23, 1, 3);
                                    ws.Cells["X1"].PutValue("Tổng tính lương (chưa VAT)");

                                    ws.Cells["A2"].PutValue("STT");
                                    ws.Cells["B2"].PutValue("Phòng ban");
                                    ws.Cells["C2"].PutValue("Số lượng đơn hàng");
                                    ws.Cells["D2"].PutValue("Giá thu khách hàng");
                                    ws.Cells["E2"].PutValue("Giá thanh toán NCC");
                                    ws.Cells["F2"].PutValue("Hoa hồng CTV");
                                    ws.Cells["G2"].PutValue("Lợi nhuận (có VAT)");
                                    ws.Cells["H2"].PutValue("Tỷ suất");

                                    ws.Cells["I2"].PutValue("Doanh thu");
                                    ws.Cells["J2"].PutValue("Giá vốn");
                                    ws.Cells["k2"].PutValue("Lợi nhuận");

                                    ws.Cells["L2"].PutValue("Doanh thu");
                                    ws.Cells["M2"].PutValue("Giá vốn");
                                    ws.Cells["N2"].PutValue("Lợi nhuận");

                                    ws.Cells["O2"].PutValue("Doanh thu");
                                    ws.Cells["P2"].PutValue("Giá vốn");
                                    ws.Cells["Q2"].PutValue("Lợi nhuận");

                                    ws.Cells["R2"].PutValue("Doanh thu");
                                    ws.Cells["S2"].PutValue("Giá vốn");
                                    ws.Cells["T2"].PutValue("Lợi nhuận");

                                    ws.Cells["U2"].PutValue("Doanh thu");
                                    ws.Cells["V2"].PutValue("Giá vốn");
                                    ws.Cells["W2"].PutValue("Lợi nhuận");

                                    ws.Cells["X2"].PutValue("Doanh thu");
                                    ws.Cells["Y2"].PutValue("Giá vốn");
                                    ws.Cells["Z2"].PutValue("Lợi nhuận");

                                    ws.Cells["AA2"].PutValue("Ngày In bắt đầu");
                                    ws.Cells["AB2"].PutValue("Ngày In kết thúc");
                                    ws.Cells["AC2"].PutValue("Ngày Out bắt đầu");
                                    ws.Cells["AD2"].PutValue("Ngày Out kết thúc");


                                    #endregion

                                    #region Body

                                    range = cell.CreateRange(1, 0, data.Count + 1, 30);
                                    style = ws.Cells["A3"].GetStyle();
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    range.ApplyStyle(style, st);

                                    Style alignCenterStyle = ws.Cells["A3"].GetStyle();
                                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                                    Style numberStyle = ws.Cells["I3"].GetStyle();
                                    numberStyle.Number = 3;
                                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                                    int RowIndex = 2;

                                    foreach (var item in data)
                                    {

                                        RowIndex++;
                                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 2);
                                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                                        ws.Cells["B" + RowIndex].PutValue(item.DepartmentName);
                                        ws.Cells["C" + RowIndex].PutValue(item.TotalOrder);
                                        ws.Cells["D" + RowIndex].PutValue(item.Amount);
                                        ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["E" + RowIndex].PutValue(item.Price);
                                        ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["F" + RowIndex].PutValue(item.Comission);
                                        ws.Cells["F" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["G" + RowIndex].PutValue(item.Profit);
                                        ws.Cells["G" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["H" + RowIndex].PutValue(item.Percent.ToString("N2") + "%");

                                        ws.Cells["I" + RowIndex].PutValue(item.HotelBookingAmount);
                                        ws.Cells["I" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["J" + RowIndex].PutValue(item.HotelBookingPrice);
                                        ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["k" + RowIndex].PutValue(item.HotelBookingProfit);
                                        ws.Cells["k" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["L" + RowIndex].PutValue(item.FlyBookingAmount);
                                        ws.Cells["L" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["M" + RowIndex].PutValue(item.FlyBookingPrice);
                                        ws.Cells["M" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["N" + RowIndex].PutValue(item.FlyBookingProfit);
                                        ws.Cells["N" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["O" + RowIndex].PutValue(item.TourAmount);
                                        ws.Cells["O" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["P" + RowIndex].PutValue(item.TourPrice);
                                        ws.Cells["P" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["Q" + RowIndex].PutValue(item.TourProfit);
                                        ws.Cells["Q" + RowIndex].SetStyle(numberStyle);


                                        ws.Cells["R" + RowIndex].PutValue(item.OtherBookingAmount);
                                        ws.Cells["R" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["S" + RowIndex].PutValue(item.OtherBookingPrice);
                                        ws.Cells["S" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["T" + RowIndex].PutValue(item.OtherBookingProfit);
                                        ws.Cells["T" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["U" + RowIndex].PutValue(item.VinWonderAmount);
                                        ws.Cells["U" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["V" + RowIndex].PutValue(item.VinWonderPrice);
                                        ws.Cells["V" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["W" + RowIndex].PutValue(item.VinWonderProfit);
                                        ws.Cells["W" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["X" + RowIndex].PutValue(item.AmountVat);
                                        ws.Cells["X" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["Y" + RowIndex].PutValue(item.PriceVat);
                                        ws.Cells["Y" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["Z" + RowIndex].PutValue(item.ProfitVat);
                                        ws.Cells["Z" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["AA" + RowIndex].PutValue(searchModel.StartDateFromStr);
                                        ws.Cells["AB" + RowIndex].PutValue(searchModel.StartDateToStr);
                                        ws.Cells["AC" + RowIndex].PutValue(searchModel.EndDateFromStr);
                                        ws.Cells["AD" + RowIndex].PutValue(searchModel.EndDateToStr);
                                    }

                                    #endregion
                                    wb.Save(FilePath);
                                    pathResult = FilePath;
                                }
                            }
                            break;
                        case (int)DepartmentType.RevenueByDepartmentsaler:
                            {
                                DataTable dt = await _DepartmentDAL.GetListDetailRevenueByDepartment(searchModel);
                                DataTable dt2 = await _DepartmentDAL.GetListDetailRevenueBySaler(searchModel);
                                var data = new List<DetailRevenueByDepartmentViewModel>();
                                var List_ReportDepartment = new List<ListDetailRevenueByDepartmentViewModel>();
                                var data2 = new List<DetailRevenueByDepartmentViewModel>();

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    data = dt.ToList<DetailRevenueByDepartmentViewModel>();
                                }
                                if (dt2 != null && dt2.Rows.Count > 0)
                                {
                                    data2 = dt2.ToList<DetailRevenueByDepartmentViewModel>();

                                }
                                if (data != null && data.Count > 0)
                                {
                                    foreach (var item in data)
                                    {
                                        var ReportDepartment = new ListDetailRevenueByDepartmentViewModel();
                                        var listParentDepartmentId = data2.Where(s => s.ParentDepartmentId == item.ParentDepartmentId && s.DepartmentId == item.DepartmentId).ToList();
                                        ReportDepartment.listReportDepartment = listParentDepartmentId;

                                        ReportDepartment.ParentDepartmentAmount = listParentDepartmentId.Sum(s => s.Amount);
                                        ReportDepartment.ParentDepartmentAmountVat = listParentDepartmentId.Sum(s => s.AmountVat);
                                        ReportDepartment.ParentDepartmentComission = listParentDepartmentId.Sum(s => s.Comission);
                                        ReportDepartment.ParentDepartmentPercent = (listParentDepartmentId.Sum(s => s.Profit) / listParentDepartmentId.Sum(s => s.Amount)) * 100;
                                        ReportDepartment.ParentDepartmentTotalOrder = listParentDepartmentId.Sum(s => s.TotalOrder);
                                        ReportDepartment.ParentDepartmentPrice = listParentDepartmentId.Sum(s => s.Price);
                                        ReportDepartment.ParentDepartmentPriceVat = listParentDepartmentId.Sum(s => s.PriceVat);
                                        ReportDepartment.ParentDepartmentProfitVat = listParentDepartmentId.Sum(s => s.ProfitVat);
                                        ReportDepartment.ParentDepartmentProfit = listParentDepartmentId.Sum(s => s.Profit);

                                        ReportDepartment.DepartmentFlyBookingAmount = listParentDepartmentId.Sum(s => s.FlyBookingAmount);
                                        ReportDepartment.DepartmentFlyBookingPrice = listParentDepartmentId.Sum(s => s.FlyBookingPrice);
                                        ReportDepartment.DepartmentFlyBookingProfit = listParentDepartmentId.Sum(s => s.FlyBookingProfit);
                                        ReportDepartment.DepartmentHotelBookingAmount = listParentDepartmentId.Sum(s => s.HotelBookingAmount);
                                        ReportDepartment.DepartmentHotelBookingPrice = listParentDepartmentId.Sum(s => s.HotelBookingPrice);
                                        ReportDepartment.DepartmentHotelBookingProfit = listParentDepartmentId.Sum(s => s.HotelBookingProfit);
                                        ReportDepartment.DepartmentTourAmount = listParentDepartmentId.Sum(s => s.TourAmount);
                                        ReportDepartment.DepartmentTourPrice = listParentDepartmentId.Sum(s => s.TourPrice);
                                        ReportDepartment.DepartmentTourProfit = listParentDepartmentId.Sum(s => s.TourProfit);
                                        ReportDepartment.DepartmentOtherBookingAmount = listParentDepartmentId.Sum(s => s.OtherBookingAmount);
                                        ReportDepartment.DepartmentOtherBookingPrice = listParentDepartmentId.Sum(s => s.OtherBookingPrice);
                                        ReportDepartment.DepartmentOtherBookingProfit = listParentDepartmentId.Sum(s => s.OtherBookingProfit);
                                        ReportDepartment.DepartmentVinWonderAmount = listParentDepartmentId.Sum(s => s.VinWonderAmount);
                                        ReportDepartment.DepartmentVinWonderPrice = listParentDepartmentId.Sum(s => s.VinWonderPrice);
                                        ReportDepartment.DepartmentVinWonderProfit = listParentDepartmentId.Sum(s => s.VinWonderProfit);

                                        ReportDepartment.ParentDepartmentName = item.ParentDepartmentName;
                                        ReportDepartment.DepartmentName = item.DepartmentName;

                                        ReportDepartment.DepartmentName = item.DepartmentName;

                                        List_ReportDepartment.Add(ReportDepartment);
                                    }
                                }
                                if (List_ReportDepartment != null && List_ReportDepartment.Count > 0)
                                {
                                    Workbook wb = new Workbook();
                                    Worksheet ws = wb.Worksheets[0];

                                    ws.Name = "Doanh thu CTDV theo nhân viên";
                                    Cells cell = ws.Cells;
                                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                                    StyleFlag st = new StyleFlag();
                                    st.All = true;
                                    Style style = ws.Cells["A1"].GetStyle();

                                    #region Header
                                    range = cell.CreateRange(0, 0, 1, 30);
                                    style = ws.Cells["A1"].GetStyle();
                                    style.Font.IsBold = true;
                                    style.IsTextWrapped = true;
                                    //style.ForegroundColor = Color.FromArgb(33, 88, 103);
                                    //style.BackgroundColor = Color.FromArgb(33, 88, 103);
                                    style.Pattern = BackgroundType.Solid;
                                    //style.Font.Color = Color.White;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    range.ApplyStyle(style, st);

                                    cell.SetColumnWidth(0, 8);
                                    cell.SetColumnWidth(1, 20);
                                    cell.SetColumnWidth(2, 40);
                                    cell.SetColumnWidth(3, 20);
                                    cell.SetColumnWidth(4, 20);
                                    cell.SetColumnWidth(5, 30);
                                    cell.SetColumnWidth(6, 30);
                                    cell.SetColumnWidth(7, 25);
                                    cell.SetColumnWidth(8, 25);
                                    cell.SetColumnWidth(9, 25);
                                    cell.SetColumnWidth(10, 25);
                                    cell.SetColumnWidth(11, 25);
                                    cell.SetColumnWidth(12, 25);
                                    cell.SetColumnWidth(13, 25);
                                    cell.SetColumnWidth(14, 25);
                                    cell.SetColumnWidth(15, 25);
                                    cell.SetColumnWidth(16, 25);
                                    cell.SetColumnWidth(17, 25);
                                    cell.SetColumnWidth(18, 25);
                                    cell.SetColumnWidth(19, 25);
                                    cell.SetColumnWidth(20, 25);
                                    cell.SetColumnWidth(21, 25);
                                    cell.SetColumnWidth(22, 25);
                                    cell.SetColumnWidth(23, 25);
                                    cell.SetColumnWidth(24, 25);
                                    cell.SetColumnWidth(25, 25);
                                    cell.SetColumnWidth(26, 25);
                                    cell.SetColumnWidth(27, 25);
                                    cell.SetColumnWidth(28, 25);
                                    cell.SetColumnWidth(29, 25);


                                    // Set header value

                                    cell.Merge(0, 3, 1, 2);
                                    ws.Cells["D1"].PutValue("Tổng doanh thu (có VAT)");
                                    cell.Merge(0, 8, 1, 3);
                                    ws.Cells["I1"].PutValue("Khách sạn (chưa VAT)");
                                    cell.Merge(0, 11, 1, 3);
                                    ws.Cells["L1"].PutValue("Vé máy bay (chưa VAT)");
                                    cell.Merge(0, 14, 1, 3);
                                    ws.Cells["O1"].PutValue("Tour du lịch (chưa VAT)");
                                    cell.Merge(0, 17, 1, 3);
                                    ws.Cells["R1"].PutValue("Dịch vụ khác (chưa VAT)");
                                    cell.Merge(0, 20, 1, 3);
                                    ws.Cells["U1"].PutValue("VinWonder (chưa VAT)");
                                    cell.Merge(0, 23, 1, 3);
                                    ws.Cells["X1"].PutValue("Tổng tính lương (chưa VAT)");

                                    ws.Cells["A2"].PutValue("STT");
                                    ws.Cells["B2"].PutValue("Nhân viên");
                                    ws.Cells["C2"].PutValue("Số lượng đơn hàng");
                                    ws.Cells["D2"].PutValue("Giá thu khách hàng");
                                    ws.Cells["E2"].PutValue("Giá thanh toán NCC");
                                    ws.Cells["F2"].PutValue("Hoa hồng CTV");
                                    ws.Cells["G2"].PutValue("Lợi nhuận (có VAT)");
                                    ws.Cells["H2"].PutValue("Tỷ suất");

                                    ws.Cells["I2"].PutValue("Doanh thu");
                                    ws.Cells["J2"].PutValue("Giá vốn");
                                    ws.Cells["k2"].PutValue("Lợi nhuận");

                                    ws.Cells["L2"].PutValue("Doanh thu");
                                    ws.Cells["M2"].PutValue("Giá vốn");
                                    ws.Cells["N2"].PutValue("Lợi nhuận");

                                    ws.Cells["O2"].PutValue("Doanh thu");
                                    ws.Cells["P2"].PutValue("Giá vốn");
                                    ws.Cells["Q2"].PutValue("Lợi nhuận");
                                    ws.Cells["R2"].PutValue("Doanh thu");
                                    ws.Cells["S2"].PutValue("Giá vốn");
                                    ws.Cells["T2"].PutValue("Lợi nhuận");

                                    ws.Cells["U2"].PutValue("Doanh thu");
                                    ws.Cells["V2"].PutValue("Giá vốn");
                                    ws.Cells["W2"].PutValue("Lợi nhuận");

                                    ws.Cells["X2"].PutValue("Doanh thu");
                                    ws.Cells["Y2"].PutValue("Giá vốn");
                                    ws.Cells["Z2"].PutValue("Lợi nhuận");
                                    ws.Cells["AA2"].PutValue("Ngày In bắt đầu");
                                    ws.Cells["AB2"].PutValue("Ngày In kết thúc");
                                    ws.Cells["AC2"].PutValue("Ngày Out bắt đầu");
                                    ws.Cells["AD2"].PutValue("Ngày Out kết thúc");


                                    #endregion

                                    #region Body
                                    
                                    range = cell.CreateRange(1, 0, List_ReportDepartment.Sum(s=>s.listReportDepartment.Count()) + data.Count() + 2, 30);
                                    style = ws.Cells["A3"].GetStyle();
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    range.ApplyStyle(style, st);

                                    Style alignCenterStyle = ws.Cells["A3"].GetStyle();
                                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                                    Style numberStyle = ws.Cells["I3"].GetStyle();
                                    numberStyle.Number = 3;
                                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                                    int RowIndex = 2;

                                    foreach (var item in List_ReportDepartment)
                                    {

                                        RowIndex++;
                                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 2);
                                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                                        ws.Cells["B" + RowIndex].PutValue(item.DepartmentName);
                                        ws.Cells["C" + RowIndex].PutValue(item.ParentDepartmentTotalOrder);
                                        ws.Cells["D" + RowIndex].PutValue(item.ParentDepartmentAmount);
                                        ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["E" + RowIndex].PutValue(item.ParentDepartmentPrice);
                                        ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["F" + RowIndex].PutValue(item.ParentDepartmentComission);
                                        ws.Cells["F" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["G" + RowIndex].PutValue(item.ParentDepartmentProfit);
                                        ws.Cells["G" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["H" + RowIndex].PutValue(item.ParentDepartmentPercent.ToString("N2") + "%");
                                        ws.Cells["I" + RowIndex].PutValue(item.DepartmentHotelBookingAmount);
                                        ws.Cells["I" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["J" + RowIndex].PutValue(item.DepartmentHotelBookingPrice);
                                        ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["k" + RowIndex].PutValue(item.DepartmentHotelBookingProfit);
                                        ws.Cells["k" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["L" + RowIndex].PutValue(item.DepartmentFlyBookingAmount);
                                        ws.Cells["L" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["M" + RowIndex].PutValue(item.DepartmentFlyBookingPrice);
                                        ws.Cells["M" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["N" + RowIndex].PutValue(item.DepartmentFlyBookingProfit);
                                        ws.Cells["N" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["O" + RowIndex].PutValue(item.DepartmentTourAmount);
                                        ws.Cells["O" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["P" + RowIndex].PutValue(item.DepartmentTourPrice);
                                        ws.Cells["P" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["Q" + RowIndex].PutValue(item.DepartmentTourProfit);
                                        ws.Cells["Q" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["R" + RowIndex].PutValue(item.DepartmentOtherBookingAmount);
                                        ws.Cells["R" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["S" + RowIndex].PutValue(item.DepartmentOtherBookingPrice);
                                        ws.Cells["S" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["T" + RowIndex].PutValue(item.DepartmentOtherBookingProfit);
                                        ws.Cells["T" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["U" + RowIndex].PutValue(item.DepartmentVinWonderAmount);
                                        ws.Cells["U" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["V" + RowIndex].PutValue(item.DepartmentVinWonderPrice);
                                        ws.Cells["V" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["W" + RowIndex].PutValue(item.DepartmentVinWonderProfit);
                                        ws.Cells["W" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["X" + RowIndex].PutValue(item.ParentDepartmentAmountVat);
                                        ws.Cells["X" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["Y" + RowIndex].PutValue(item.ParentDepartmentPriceVat);
                                        ws.Cells["Y" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["Z" + RowIndex].PutValue(item.ParentDepartmentProfitVat);
                                        ws.Cells["Z" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["AA" + RowIndex].PutValue(searchModel.StartDateFromStr);
                                        ws.Cells["AB" + RowIndex].PutValue(searchModel.StartDateToStr);
                                        ws.Cells["AC" + RowIndex].PutValue(searchModel.EndDateFromStr);
                                        ws.Cells["AD" + RowIndex].PutValue(searchModel.EndDateToStr);

                                        foreach (var item2 in item.listReportDepartment)
                                        {
                                            RowIndex++;
                                            ws.Cells["A" + RowIndex].PutValue(RowIndex - 2);
                                            ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                                            ws.Cells["B" + RowIndex].PutValue(item2.UserName + ": " + item2.FullName);
                                            ws.Cells["C" + RowIndex].PutValue(item2.TotalOrder);
                                            ws.Cells["D" + RowIndex].PutValue(item2.Amount);
                                            ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["E" + RowIndex].PutValue(item2.Price);
                                            ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["F" + RowIndex].PutValue(item2.Comission);
                                            ws.Cells["F" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["G" + RowIndex].PutValue(item2.Profit);
                                            ws.Cells["G" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["H" + RowIndex].PutValue(item2.Percent.ToString("N2") + "%");
                                            ws.Cells["I" + RowIndex].PutValue(item2.HotelBookingAmount);
                                            ws.Cells["I" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["J" + RowIndex].PutValue(item2.HotelBookingPrice);
                                            ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["k" + RowIndex].PutValue(item2.HotelBookingProfit);
                                            ws.Cells["k" + RowIndex].SetStyle(numberStyle);

                                            ws.Cells["L" + RowIndex].PutValue(item2.FlyBookingAmount);
                                            ws.Cells["L" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["M" + RowIndex].PutValue(item2.FlyBookingPrice);
                                            ws.Cells["M" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["N" + RowIndex].PutValue(item2.FlyBookingProfit);
                                            ws.Cells["N" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["O" + RowIndex].PutValue(item2.TourAmount);
                                            ws.Cells["O" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["P" + RowIndex].PutValue(item2.TourPrice);
                                            ws.Cells["P" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["Q" + RowIndex].PutValue(item2.TourProfit);
                                            ws.Cells["Q" + RowIndex].SetStyle(numberStyle);


                                            ws.Cells["R" + RowIndex].PutValue(item2.OtherBookingAmount);
                                            ws.Cells["R" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["S" + RowIndex].PutValue(item2.OtherBookingPrice);
                                            ws.Cells["S" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["T" + RowIndex].PutValue(item2.OtherBookingProfit);
                                            ws.Cells["T" + RowIndex].SetStyle(numberStyle);

                                            ws.Cells["U" + RowIndex].PutValue(item2.VinWonderAmount);
                                            ws.Cells["U" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["V" + RowIndex].PutValue(item2.VinWonderPrice);
                                            ws.Cells["V" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["W" + RowIndex].PutValue(item2.VinWonderProfit);
                                            ws.Cells["W" + RowIndex].SetStyle(numberStyle);

                                            ws.Cells["X" + RowIndex].PutValue(item2.AmountVat);
                                            ws.Cells["X" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["Y" + RowIndex].PutValue(item2.PriceVat);
                                            ws.Cells["Y" + RowIndex].SetStyle(numberStyle);
                                            ws.Cells["Z" + RowIndex].PutValue(item2.ProfitVat);
                                            ws.Cells["Z" + RowIndex].SetStyle(numberStyle);

                                            ws.Cells["AA" + RowIndex].PutValue(searchModel.StartDateFromStr);
                                            ws.Cells["AB" + RowIndex].PutValue(searchModel.StartDateToStr);
                                            ws.Cells["AC" + RowIndex].PutValue(searchModel.EndDateFromStr);
                                            ws.Cells["AD" + RowIndex].PutValue(searchModel.EndDateToStr);
                                        }
                                    }

                                    #endregion
                                    wb.Save(FilePath);
                                    pathResult = FilePath;
                                }
                            }
                            break;
                        case (int)DepartmentType.RevenueByDepartmentSupplier:
                            {
                                var data = new List<DetailRevenueByDepartmentViewModel>();
                                DataTable dt = await _DepartmentDAL.GetListDetailRevenueBySupplier(searchModel);
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    data = dt.ToList<DetailRevenueByDepartmentViewModel>();
                                }
                                if (data != null && data.Count > 0)
                                {
                                    Workbook wb = new Workbook();
                                    Worksheet ws = wb.Worksheets[0];
                                    ws.Name = "Doanh thu CTDV theo NCC";
                                    Cells cell = ws.Cells;

                                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                                    StyleFlag st = new StyleFlag();
                                    st.All = true;
                                    Style style = ws.Cells["A1"].GetStyle();

                                    #region Header
                                    range = cell.CreateRange(0, 0, 1, 21);
                                    style = ws.Cells["A1"].GetStyle();
                                    style.Font.IsBold = true;
                                    style.IsTextWrapped = true;
                                    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                                    style.BackgroundColor = Color.FromArgb(33, 88, 103);
                                    style.Pattern = BackgroundType.Solid;
                                    style.Font.Color = Color.White;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    range.ApplyStyle(style, st);

                                    cell.SetColumnWidth(0, 8);
                                    cell.SetColumnWidth(1, 20);
                                    cell.SetColumnWidth(2, 40);
                                    cell.SetColumnWidth(3, 20);
                                    cell.SetColumnWidth(4, 20);
                                    cell.SetColumnWidth(5, 30);
                                    cell.SetColumnWidth(6, 30);
                                    cell.SetColumnWidth(7, 25);
                                    cell.SetColumnWidth(8, 25);
                                    cell.SetColumnWidth(9, 25);
                                    cell.SetColumnWidth(10, 25);
                                    cell.SetColumnWidth(11, 25);
                                    cell.SetColumnWidth(12, 25);
                                    cell.SetColumnWidth(13, 25);
                                    cell.SetColumnWidth(14, 25);
                                    cell.SetColumnWidth(15, 25);
                                    cell.SetColumnWidth(16, 25);
                                    cell.SetColumnWidth(17, 25);
                                    cell.SetColumnWidth(18, 25);
                                    cell.SetColumnWidth(19, 25);
                                    cell.SetColumnWidth(20, 25);



                                    // Set header value
                                    ws.Cells["A1"].PutValue("STT");
                                    ws.Cells["B1"].PutValue("khách hàng");
                                    ws.Cells["D1"].PutValue("Giá thu khách hàng");
                                    ws.Cells["E1"].PutValue("Giá thanh toán NCC");
                                    ws.Cells["G1"].PutValue("Lợi nhuận (có VAT)");


                                    ws.Cells["I1"].PutValue("Doanh thu");
                                    ws.Cells["J1"].PutValue("Giá vốn");
                                    ws.Cells["k1"].PutValue("Lợi nhuận");

                                    ws.Cells["L1"].PutValue("Doanh thu");
                                    ws.Cells["M1"].PutValue("Giá vốn");
                                    ws.Cells["N1"].PutValue("Lợi nhuận");

                                    ws.Cells["O1"].PutValue("Doanh thu");
                                    ws.Cells["P1"].PutValue("Giá vốn");
                                    ws.Cells["Q1"].PutValue("Lợi nhuận");


                                    ws.Cells["R1"].PutValue("Ngày In bắt đầu");
                                    ws.Cells["S1"].PutValue("Ngày In kết thúc");
                                    ws.Cells["T1"].PutValue("Ngày Out bắt đầu");
                                    ws.Cells["U1"].PutValue("Ngày Out kết thúc");
                                    #endregion

                                    #region Body

                                    range = cell.CreateRange(1, 0, data.Count + 1, 21);
                                    style = ws.Cells["A2"].GetStyle();
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    range.ApplyStyle(style, st);

                                    Style alignCenterStyle = ws.Cells["A2"].GetStyle();
                                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                                    Style numberStyle = ws.Cells["I2"].GetStyle();
                                    numberStyle.Number = 3;
                                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                                    int RowIndex = 1;

                                    foreach (var item in data)
                                    {

                                        RowIndex++;
                                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 1);
                                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                                        ws.Cells["B" + RowIndex].PutValue(item.ClientName);

                                        ws.Cells["D" + RowIndex].PutValue(item.Amount);
                                        ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["E" + RowIndex].PutValue(item.Price);
                                        ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["G" + RowIndex].PutValue(item.Profit);
                                        ws.Cells["G" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["I" + RowIndex].PutValue(item.HotelBookingAmount);
                                        ws.Cells["I" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["J" + RowIndex].PutValue(item.HotelBookingPrice);
                                        ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["k" + RowIndex].PutValue(item.HotelBookingProfit);
                                        ws.Cells["k" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["L" + RowIndex].PutValue(item.FlyBookingAmount);
                                        ws.Cells["L" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["M" + RowIndex].PutValue(item.FlyBookingPrice);
                                        ws.Cells["M" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["N" + RowIndex].PutValue(item.FlyBookingProfit);
                                        ws.Cells["N" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["O" + RowIndex].PutValue(item.TourAmount);
                                        ws.Cells["O" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["P" + RowIndex].PutValue(item.TourPrice);
                                        ws.Cells["P" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["Q" + RowIndex].PutValue(item.TourProfit);
                                        ws.Cells["Q" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["R" + RowIndex].PutValue(searchModel.StartDateFromStr);
                                        ws.Cells["S" + RowIndex].PutValue(searchModel.StartDateToStr);
                                        ws.Cells["Y" + RowIndex].PutValue(searchModel.EndDateFromStr);
                                        ws.Cells["U" + RowIndex].PutValue(searchModel.EndDateToStr);

                                    }

                                    #endregion
                                    wb.Save(FilePath);
                                    pathResult = FilePath;
                                }

                            }
                            break;
                        case (int)DepartmentType.RevenueByDepartmentClient:
                            {
                                var data = new List<DetailRevenueByDepartmentViewModel>();
                                DataTable dt = await _DepartmentDAL.GetListDetailRevenueByClient(searchModel);
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    data = dt.ToList<DetailRevenueByDepartmentViewModel>();
                                }
                                if (data != null && data.Count > 0)
                                {
                                    Workbook wb = new Workbook();
                                    Worksheet ws = wb.Worksheets[0];
                                    ws.Name = "Doanh thu CTDV theo khách hàng";
                                    Cells cell = ws.Cells;

                                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                                    StyleFlag st = new StyleFlag();
                                    st.All = true;
                                    Style style = ws.Cells["A1"].GetStyle();

                                    #region Header
                                    range = cell.CreateRange(0, 0, 1, 28);
                                    style = ws.Cells["A1"].GetStyle();
                                    style.Font.IsBold = true;
                                    style.IsTextWrapped = true;
                                    /* style.ForegroundColor = Color.FromArgb(33, 88, 103);
                                     style.BackgroundColor = Color.FromArgb(33, 88, 103);*/
                                    style.Pattern = BackgroundType.Solid;
                                    /*style.Font.Color = Color.White;*/
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    range.ApplyStyle(style, st);

                                    cell.SetColumnWidth(0, 8);
                                    cell.SetColumnWidth(1, 20);
                                    cell.SetColumnWidth(2, 40);
                                    cell.SetColumnWidth(3, 20);
                                    cell.SetColumnWidth(4, 20);
                                    cell.SetColumnWidth(5, 30);
                                    cell.SetColumnWidth(6, 30);
                                    cell.SetColumnWidth(7, 25);
                                    cell.SetColumnWidth(8, 25);
                                    cell.SetColumnWidth(9, 25);
                                    cell.SetColumnWidth(10, 25);
                                    cell.SetColumnWidth(11, 25);
                                    cell.SetColumnWidth(12, 25);
                                    cell.SetColumnWidth(13, 25);
                                    cell.SetColumnWidth(14, 25);
                                    cell.SetColumnWidth(15, 25);
                                    cell.SetColumnWidth(16, 25);
                                    cell.SetColumnWidth(17, 25);
                                    cell.SetColumnWidth(18, 25);
                                    cell.SetColumnWidth(19, 25);
                                    cell.SetColumnWidth(20, 25);
                                    cell.SetColumnWidth(21, 25);
                                    cell.SetColumnWidth(22, 25);
                                    cell.SetColumnWidth(23, 25);
                                    cell.SetColumnWidth(24, 25);
                                    cell.SetColumnWidth(25, 25);
                                    cell.SetColumnWidth(26, 25);



                                    // Set header value
                                    cell.Merge(0, 3, 1, 2);
                                    ws.Cells["D1"].PutValue("Tổng doanh thu (có VAT)");
                                    cell.Merge(0, 6, 1, 3);
                                    ws.Cells["G1"].PutValue("Khách sạn (chưa VAT)");
                                    cell.Merge(0, 9, 1, 3);
                                    ws.Cells["J1"].PutValue("Vé máy bay (chưa VAT)");
                                    cell.Merge(0, 12, 1, 3);
                                    ws.Cells["M1"].PutValue("Tour du lịch (chưa VAT)");
                                    cell.Merge(0, 15, 1, 3);
                                    ws.Cells["P1"].PutValue("Dịch vụ khác (chưa VAT)");
                                    cell.Merge(0, 18, 1, 3);
                                    ws.Cells["S1"].PutValue("VinWonder (chưa VAT)");

                                    ws.Cells["A2"].PutValue("STT");
                                    ws.Cells["B2"].PutValue("khách hàng");
                                    ws.Cells["C2"].PutValue("Số lượng đơn hàng");
                                    ws.Cells["D2"].PutValue("Giá thu khách hàng");
                                    ws.Cells["E2"].PutValue("Giá thanh toán NCC");

                                    ws.Cells["F2"].PutValue("Lợi nhuận (có VAT)");


                                    ws.Cells["G2"].PutValue("Doanh thu");
                                    ws.Cells["H2"].PutValue("Giá vốn");
                                    ws.Cells["I2"].PutValue("Lợi nhuận");

                                    ws.Cells["J2"].PutValue("Doanh thu");
                                    ws.Cells["K2"].PutValue("Giá vốn");
                                    ws.Cells["L2"].PutValue("Lợi nhuận");

                                    ws.Cells["M2"].PutValue("Doanh thu");
                                    ws.Cells["N2"].PutValue("Giá vốn");
                                    ws.Cells["O2"].PutValue("Lợi nhuận");

                                    ws.Cells["P2"].PutValue("Doanh thu");
                                    ws.Cells["Q2"].PutValue("Giá vốn");
                                    ws.Cells["R2"].PutValue("Lợi nhuận");

                                    ws.Cells["S2"].PutValue("Doanh thu");
                                    ws.Cells["T2"].PutValue("Giá vốn");
                                    ws.Cells["U2"].PutValue("Lợi nhuận");

                                    ws.Cells["V2"].PutValue("Ngày In bắt đầu");
                                    ws.Cells["W2"].PutValue("Ngày In kết thúc");
                                    ws.Cells["X2"].PutValue("Ngày Out bắt đầu");
                                    ws.Cells["Y2"].PutValue("Ngày Out kết thúc");
                                    #endregion

                                    #region Body

                                    range = cell.CreateRange(1, 0, data.Count + 2, 27);
                                    style = ws.Cells["A3"].GetStyle();
                                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                                    style.VerticalAlignment = TextAlignmentType.Center;
                                    range.ApplyStyle(style, st);

                                    Style alignCenterStyle = ws.Cells["A3"].GetStyle();
                                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                                    Style numberStyle = ws.Cells["I3"].GetStyle();
                                    numberStyle.Number = 3;
                                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                                    int RowIndex = 2;

                                    foreach (var item in data)
                                    {

                                        RowIndex++;
                                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 2);
                                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                                        ws.Cells["B" + RowIndex].PutValue(item.ClientName);
                                        ws.Cells["C" + RowIndex].PutValue(item.TotalOrder);
                                        ws.Cells["D" + RowIndex].PutValue(item.Amount);
                                        ws.Cells["D" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["E" + RowIndex].PutValue(item.Price);
                                        ws.Cells["E" + RowIndex].SetStyle(numberStyle);
                                        //ws.Cells["F" + RowIndex].PutValue(item.Comission);
                                        ws.Cells["F" + RowIndex].PutValue(item.Profit);
                                        ws.Cells["F" + RowIndex].SetStyle(numberStyle);
                                        //ws.Cells["H" + RowIndex].PutValue(item.Percent.ToString("N2") + "%");
                                        ws.Cells["G" + RowIndex].PutValue(item.HotelBookingAmount);
                                        ws.Cells["G" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["H" + RowIndex].PutValue(item.HotelBookingPrice);
                                        ws.Cells["H" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["I" + RowIndex].PutValue(item.HotelBookingProfit);
                                        ws.Cells["I" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["J" + RowIndex].PutValue(item.FlyBookingAmount);
                                        ws.Cells["J" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["K" + RowIndex].PutValue(item.FlyBookingPrice);
                                        ws.Cells["K" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["L" + RowIndex].PutValue(item.FlyBookingProfit);
                                        ws.Cells["L" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["M" + RowIndex].PutValue(item.TourAmount);
                                        ws.Cells["M" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["N" + RowIndex].PutValue(item.TourPrice);
                                        ws.Cells["N" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["O" + RowIndex].PutValue(item.TourProfit);
                                        ws.Cells["O" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["P" + RowIndex].PutValue(item.OtherBookingAmount);
                                        ws.Cells["P" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["Q" + RowIndex].PutValue(item.OtherBookingPrice);
                                        ws.Cells["Q" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["R" + RowIndex].PutValue(item.OtherBookingProfit);
                                        ws.Cells["R" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["S" + RowIndex].PutValue(item.VinWonderAmount);
                                        ws.Cells["S" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["T" + RowIndex].PutValue(item.VinWonderPrice);
                                        ws.Cells["T" + RowIndex].SetStyle(numberStyle);
                                        ws.Cells["U" + RowIndex].PutValue(item.VinWonderProfit);
                                        ws.Cells["U" + RowIndex].SetStyle(numberStyle);

                                        ws.Cells["V" + RowIndex].PutValue(searchModel.StartDateFromStr);
                                        ws.Cells["W" + RowIndex].PutValue(searchModel.StartDateToStr);
                                        ws.Cells["X" + RowIndex].PutValue(searchModel.EndDateFromStr);
                                        ws.Cells["Y" + RowIndex].PutValue(searchModel.EndDateToStr);


                                    }

                                    #endregion
                                    wb.Save(FilePath);
                                    pathResult = FilePath;
                                }
                            }
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportDeposit - DepartmentRepository: " + ex);
            }
            return pathResult;
        }
        public async Task<string> ExportDepositListOrder(ReportDepartmentViewModel searchModel, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                searchModel.PageIndex = -1;
                var data = new List<OrderViewModel>();

                DataTable dt = await _DepartmentDAL.GetListOrder(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    data = dt.ToList<OrderViewModel>();

                }

                if (data != null && data.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Danh sách đơn hàng";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header


                    // Set column width
                    cell.SetColumnWidth(0, 8);
                    cell.SetColumnWidth(1, 20);
                    cell.SetColumnWidth(2, 40);
                    cell.SetColumnWidth(3, 40);
                    cell.SetColumnWidth(4, 40);
                    cell.SetColumnWidth(5, 40);
                    cell.SetColumnWidth(6, 40);
                    cell.SetColumnWidth(7, 40);
                    cell.SetColumnWidth(8, 40);
                    cell.SetColumnWidth(9, 40);
                    cell.SetColumnWidth(10, 40);
                    cell.SetColumnWidth(11, 40);
                    cell.SetColumnWidth(12, 40);



                    // Set header value

                    ws.Cells["A1"].PutValue("STT");
                    ws.Cells["B1"].PutValue("Mã đơn");
                    ws.Cells["C1"].PutValue("Ngày bắt đầu");
                    ws.Cells["D1"].PutValue("Ngày kết thúc");
                    ws.Cells["E1"].PutValue("Khách hàng");
                    ws.Cells["F1"].PutValue("Nhãn đơn");
                    ws.Cells["G1"].PutValue("Thanh toán");
                    ws.Cells["H1"].PutValue("Lợi nhuận");
                    ws.Cells["I1"].PutValue("Trạng thái");
                    ws.Cells["J1"].PutValue("Ngày tạo");
                    ws.Cells["k1"].PutValue("Nhân viên chính");
                    ws.Cells["L1"].PutValue("Hình thức thanh toán");





                    range = cell.CreateRange(0, 0, 1, 12);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;
                    style.ForegroundColor = Color.FromArgb(33, 88, 103);
                    style.BackgroundColor = Color.FromArgb(33, 88, 103);
                    style.Pattern = BackgroundType.Solid;
                    style.Font.Color = Color.White;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    range.ApplyStyle(style, st);


                    // Set header value

                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, data.Count, data.Count + 1);
                    style = ws.Cells["A2"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);


                    Style numberStyle = ws.Cells["A2"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    Style alignCenterStyle = ws.Cells["A2"].GetStyle();
                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;
                    int RowIndex = 1;
                    foreach (var item in data)
                    {

                        RowIndex++;
                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 1);
                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["B" + RowIndex].PutValue(item.OrderCode);
                        ws.Cells["C" + RowIndex].PutValue(Convert.ToDateTime(item.StartDate).ToString("dd/MM/yyyy HH:ss"));
                        ws.Cells["D" + RowIndex].PutValue(Convert.ToDateTime(item.EndDate).ToString("dd/MM/yyyy HH:ss"));
                        ws.Cells["E" + RowIndex].PutValue(item.ClientName + " - " + item.ClientEmail);
                        ws.Cells["F" + RowIndex].PutValue(item.Note);
                        ws.Cells["G" + RowIndex].PutValue(item.Payment + "/" + (item.Amount == 0 ? 0 : item.Amount));
                        ws.Cells["H" + RowIndex].PutValue(item.Profit);
                        ws.Cells["H" + RowIndex].SetStyle(numberStyle);
                        ws.Cells["I" + RowIndex].PutValue(item.Status);
                        ws.Cells["J" + RowIndex].PutValue(Convert.ToDateTime(item.CreateDate).ToString("dd/MM/yyyy HH:ss"));
                        ws.Cells["k" + RowIndex].PutValue(item.SalerName + " - " + item.SalerEmail);
                        ws.Cells["L" + RowIndex].PutValue((item.PermisionTypeName == null || item.PermisionTypeName.Trim() == "" ? "Không công nợ" : item.PermisionTypeName) + " - " + item.PaymentStatusName);


                    }

                    #endregion
                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportDepositListOrder - DepartmentRepository: " + ex);
            }
            return pathResult;
        }
        public async Task<GenericViewModel<SearchRevenueBySupplierViewModel>> GetListTotalDebtRevenueBySupplier(RevenueBySupplierViewModel searchModel)
        {
            var model = new GenericViewModel<SearchRevenueBySupplierViewModel>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetTableTotalDebtRevenueBySupplier(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    model.ListData = dt.ToList<SearchRevenueBySupplierViewModel>();
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalRecord = dt.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalRow"].ToString());
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTotalDebtRevenueBySupplier - DepartmentRepository: " + ex);
            }
            return model;
        }
        public async Task<GenericViewModel<SearchDetailRevenueBySupplierViewModel>> GetListDetailRevenueBySupplier(RevenueBySupplierViewModel searchModel)
        {
            var model = new GenericViewModel<SearchDetailRevenueBySupplierViewModel>();
            try
            {
                DataTable dt = await _DepartmentDAL.GetTableDetailRevenueBySupplier(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    model.ListData = dt.ToList<SearchDetailRevenueBySupplierViewModel>();
                    model.PageSize = (int)searchModel.PageSize;
                    model.CurrentPage = (int)searchModel.PageIndex;
                    model.TotalRecord = dt.Rows[0]["TotalRow"].Equals(DBNull.Value) ? 0 : Convert.ToInt32(dt.Rows[0]["TotalRow"].ToString());
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListDetailRevenueBySupplier - DepartmentRepository: " + ex);
            }
            return model;
        }
        public async Task<string> ExportDeposit(RevenueBySupplierViewModel searchModel, string FilePath)
        {
            var pathResult = string.Empty;
            try
            {
                var data = new List<SearchRevenueBySupplierViewModel>();

                DataTable dt = await _DepartmentDAL.GetTableTotalDebtRevenueBySupplier(searchModel);
                if (dt != null && dt.Rows.Count > 0)
                {

                    data = dt.ToList<SearchRevenueBySupplierViewModel>();

                }
                if (data != null && data.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Danh sách công nợ phải trả NCC";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header
                    range = cell.CreateRange(0, 0, 1, 10);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;

                    style.Pattern = BackgroundType.Solid;

                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    range.ApplyStyle(style, st);

                    // Set column width
                    cell.SetColumnWidth(0, 8);
                    cell.SetColumnWidth(1, 20);
                    cell.SetColumnWidth(2, 40);
                    cell.SetColumnWidth(3, 20);
                    cell.SetColumnWidth(4, 20);
                    cell.SetColumnWidth(5, 30);
                    cell.SetColumnWidth(6, 30);
                    cell.SetColumnWidth(7, 25);
                    cell.SetColumnWidth(8, 25);
                    cell.SetColumnWidth(9, 25);
                    cell.SetColumnWidth(10, 25);

                    cell.Merge(0, 4, 1, 2);
                    ws.Cells["E1"].PutValue("Số dư đầu kỳ");
                    cell.Merge(0, 6, 1, 2);
                    ws.Cells["G1"].PutValue("Phát sinh trong kỳ");
                    cell.Merge(0, 8, 1, 2);
                    ws.Cells["I1"].PutValue("Số dư cuối kỳ");
                    // Set header value
                    ws.Cells["A2"].PutValue("STT");
                    ws.Cells["B2"].PutValue("Mã nhà cung cấp");
                    ws.Cells["C2"].PutValue("Tên nhà cung cấp");
                    ws.Cells["D2"].PutValue("TK công nợ");
                    ws.Cells["E2"].PutValue("Nợ");
                    ws.Cells["F2"].PutValue("Có");
                    ws.Cells["G2"].PutValue("Nợ");
                    ws.Cells["H2"].PutValue("Có");
                    ws.Cells["I2"].PutValue("Nợ");
                    ws.Cells["J2"].PutValue("Có");


                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, data.Count, 12);
                    style = ws.Cells["A3"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;

                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;

                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;

                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;

                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);

                    Style alignCenterStyle = ws.Cells["A3"].GetStyle();
                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                    Style numberStyle = ws.Cells["I3"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    int RowIndex = 2;

                    foreach (var item in data)
                    {
                        string ttchitiet = string.Empty;

                        RowIndex++;
                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 2);
                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["B" + RowIndex].PutValue(item.SupplierId);
                        ws.Cells["C" + RowIndex].PutValue(item.FullName);
                        ws.Cells["D" + RowIndex].PutValue(item.DebtAccount);
                        ws.Cells["E" + RowIndex].PutValue(item.AmountOpeningBalanceCredit);
                        ws.Cells["E" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["F" + RowIndex].PutValue(item.AmountOpeningBalanceDebit);
                        ws.Cells["F" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["G" + RowIndex].PutValue(item.AmountDebit);
                        ws.Cells["G" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["H" + RowIndex].PutValue(item.AmountCredit);
                        ws.Cells["H" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["I" + RowIndex].PutValue(item.AmountClosingBalanceDebit);
                        ws.Cells["I" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["J" + RowIndex].PutValue(item.AmountClosingBalanceCredit);
                        ws.Cells["J" + RowIndex].SetStyle(alignCenterStyle);
                    }

                    #endregion
                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportPaymentRequest - DepartmentRepository: " + ex);
            }
            return pathResult;
        }
        public async Task<string> ExportDepositSupplier(RevenueBySupplierViewModel searchModel, string FilePath, double amount)
        {
            var pathResult = string.Empty;
            try
            {
                var data = new List<SearchDetailRevenueBySupplierViewModel>();

                DataTable dt = await _DepartmentDAL.GetTableDetailRevenueBySupplier(searchModel);

                if (dt != null && dt.Rows.Count > 0)
                {

                    data = dt.ToList<SearchDetailRevenueBySupplierViewModel>();

                }
                if (data != null && data.Count > 0)
                {
                    Workbook wb = new Workbook();
                    Worksheet ws = wb.Worksheets[0];
                    ws.Name = "Danh sách CTCN phải trả NCC";
                    Cells cell = ws.Cells;

                    var range = ws.Cells.CreateRange(0, 0, 1, 1);
                    StyleFlag st = new StyleFlag();
                    st.All = true;
                    Style style = ws.Cells["A1"].GetStyle();

                    #region Header
                    range = cell.CreateRange(0, 0, 1, 10);
                    style = ws.Cells["A1"].GetStyle();
                    style.Font.IsBold = true;
                    style.IsTextWrapped = true;


                    style.Pattern = BackgroundType.Solid;

                    style.VerticalAlignment = TextAlignmentType.Center;
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;

                    range.ApplyStyle(style, st);

                    // Set column width
                    cell.SetColumnWidth(0, 8);
                    cell.SetColumnWidth(1, 20);
                    cell.SetColumnWidth(2, 40);
                    cell.SetColumnWidth(3, 20);
                    cell.SetColumnWidth(4, 20);
                    cell.SetColumnWidth(5, 30);
                    cell.SetColumnWidth(6, 30);
                    cell.SetColumnWidth(7, 25);
                    cell.SetColumnWidth(8, 25);
                    cell.SetColumnWidth(9, 25);
                    cell.SetColumnWidth(10, 25);
                    cell.SetColumnWidth(11, 25);
                    cell.SetColumnWidth(12, 25);

                    cell.Merge(0, 8, 1, 2);
                    ws.Cells["I1"].PutValue("Phát sinh");
                    cell.Merge(0, 10, 1, 2);
                    ws.Cells["K1"].PutValue("Số dư");

                    // Set header value
                    ws.Cells["A2"].PutValue("STT");
                    ws.Cells["B2"].PutValue("Ngày hạch toán");
                    ws.Cells["C2"].PutValue("Ngày chứng từ");
                    ws.Cells["D2"].PutValue("Số chứng từ");
                    ws.Cells["E2"].PutValue("Số hóa đơn");
                    ws.Cells["F2"].PutValue("Diễn giải");
                    ws.Cells["G2"].PutValue("TK công nợ");
                    ws.Cells["H2"].PutValue("Ngân hàng");
                    ws.Cells["I2"].PutValue("Nợ");
                    ws.Cells["J2"].PutValue("Có");
                    ws.Cells["K2"].PutValue("Nợ");
                    ws.Cells["L2"].PutValue("Có");


                    #endregion

                    #region Body

                    range = cell.CreateRange(1, 0, data.Count + 2, 12);
                    style = ws.Cells["A3"].GetStyle();
                    style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.TopBorder].Color = Color.Black;
                    style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.BottomBorder].Color = Color.Black;
                    style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.LeftBorder].Color = Color.Black;
                    style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
                    style.Borders[BorderType.RightBorder].Color = Color.Black;
                    style.VerticalAlignment = TextAlignmentType.Center;
                    range.ApplyStyle(style, st);

                    Style alignCenterStyle = ws.Cells["A3"].GetStyle();
                    alignCenterStyle.HorizontalAlignment = TextAlignmentType.Center;

                    Style numberStyle = ws.Cells["I3"].GetStyle();
                    numberStyle.Number = 3;
                    numberStyle.HorizontalAlignment = TextAlignmentType.Right;
                    numberStyle.VerticalAlignment = TextAlignmentType.Center;

                    int RowIndex = 3;

                    ws.Cells["E" + RowIndex].PutValue("Số dư đầu kỳ");

                    ws.Cells["L" + RowIndex].PutValue(amount);
                    foreach (var item in data)
                    {
                        string ttchitiet = string.Empty;

                        RowIndex++;
                        ws.Cells["A" + RowIndex].PutValue(RowIndex - 3);
                        ws.Cells["A" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["B" + RowIndex].PutValue(Convert.ToDateTime(item.CreatedDate).ToString("dd/MM/yyyy"));
                        ws.Cells["C" + RowIndex].PutValue(Convert.ToDateTime(item.CreatedDate).ToString("dd/MM/yyyy"));
                        ws.Cells["D" + RowIndex].PutValue(item.PaymentCode);
                        ws.Cells["E" + RowIndex].PutValue("");
                        ws.Cells["F" + RowIndex].PutValue(item.Description);
                        ws.Cells["G" + RowIndex].PutValue(item.DebtAccount);
                        ws.Cells["H" + RowIndex].PutValue(item.BankName + " - " + item.BankAccount);
                        ws.Cells["I" + RowIndex].PutValue(item.AmountDebit);
                        ws.Cells["I" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["J" + RowIndex].PutValue(item.AmountCredit);
                        ws.Cells["J" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["K" + RowIndex].PutValue(item.AmountRemain);
                        ws.Cells["K" + RowIndex].SetStyle(alignCenterStyle);
                        ws.Cells["L" + RowIndex].PutValue((amount + item.AmountCredit - item.AmountDebit));
                        ws.Cells["L" + RowIndex].SetStyle(alignCenterStyle);
                        amount = amount + item.AmountCredit - item.AmountDebit;
                    }

                    #endregion
                    wb.Save(FilePath);
                    pathResult = FilePath;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ExportPaymentRequest - DepartmentRepository: " + ex);
            }
            return pathResult;
        }
    }
}

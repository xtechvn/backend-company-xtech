using Entities.ViewModels;
using Entities.ViewModels.DashBoard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repositories.IRepositories;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class DashBoardController : Controller
    {
        private readonly IDashboardRepository _DashboardRepository;
        private readonly IOrderRepository _OrderRepository;
        private ManagementUser _ManagementUser;

        public DashBoardController(ManagementUser managementUser,
            IDashboardRepository dashboardRepository,
            IOrderRepository orderRepository)
        {
            _DashboardRepository = dashboardRepository;
            _ManagementUser = managementUser;
            _OrderRepository = orderRepository;
        }


        [HttpPost]
        public IActionResult GetNewClientByDay(DashboardSearchModel model)
        {
            try
            {
                var data = _DashboardRepository.GetNewClientByDay(model.from_date.Date, model.to_date);
                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetNewClientByDay - DashBoardController: " + ex.ToString());
                return Content("");
            }
        }

        [HttpPost]
        public IActionResult GetRevenueOrderByDay(DashboardSearchModel model)
        {
            try
            {
                var data = _DashboardRepository.GetRevenueOrderByDay(model.from_date, model.to_date, model.status);
                if (data != null && data.Rows.Count > 0)
                {
                    return new JsonResult(data.Rows[0]);
                }
                else
                {
                    return new JsonResult(new
                    {
                        TotalOrder = 0,
                        TotalRevenue = 0
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetRevenueOrderByDay - DashBoardController: " + ex.ToString());
                return Content("");
            }

        }

        [HttpPost]
        public IActionResult GetRevenueOrderGroupBySale(DashboardSearchModel model)
        {
            try
            {
                var dataTable = _DashboardRepository.GetRevenueOrderGroupBySale(model.from_date, model.to_date, model.type);

                switch (model.type)
                {
                    case 1:
                        var data = dataTable.AsEnumerable().Select(s => new
                        {
                            date = DateTime.Parse(s["Date"].ToString()),
                            revenue = decimal.Parse(s["TotalRevenue"].ToString())
                        });

                        if (model.date_type == 1)
                        {
                            return new JsonResult(new
                            {
                                label = data.Select(s => s.date.ToString("dd/MM/yyyy")),
                                value = data.Select(s => s.revenue)
                            });
                        }
                        else
                        {
                            var months = data.GroupBy(s => new { s.date.Month, s.date.Year }).Select(s => new
                            {
                                label = s.First().date.ToString("MM/yyyy"),
                                revenue = s.Sum(m => m.revenue)
                            });

                            return new JsonResult(new
                            {
                                label = months.Select(s => s.label),
                                value = months.Select(s => s.revenue)
                            });
                        }
                    case 2:
                    case 3:
                        var datas = dataTable.AsEnumerable().Select(s => new
                        {
                            label = s["DimName"].ToString(),
                            revenue = decimal.Parse(s["TotalRevenue"].ToString())
                        });

                        return new JsonResult(new
                        {
                            label = datas.Select(s => s.label),
                            value = datas.Select(s => s.revenue)
                        });
                }

                return new JsonResult(null);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetRevenueOrderGroupBySale - DashBoardController: " + ex.ToString());
                return Content("");
            }

        }

        [HttpPost]
        public async Task<IActionResult> GetOrderStatistic()
        {
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();

                var searchModel = new OrderViewSearchModel();
                if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
                {
                    var list = current_user.Role.Split(',');

                    foreach (var item in list)
                    {
                        switch (Convert.ToInt32(item))
                        {
                            case (int)RoleType.SaleOnl:
                            case (int)RoleType.SaleTour:
                            case (int)RoleType.SaleKd:
                            case (int)RoleType.TPDHKS:
                            case (int)RoleType.TPDHVe:
                            case (int)RoleType.TPDHTour:
                            case (int)RoleType.TPKS:
                            case (int)RoleType.TPTour:
                            case (int)RoleType.TPVe:
                            case (int)RoleType.DHPQ:
                            case (int)RoleType.DHTour:
                            case (int)RoleType.DHVe:
                            case (int)RoleType.DHKS:
                            case (int)RoleType.GDHN:
                            case (int)RoleType.GDHPQ:
                                {
                                    if (searchModel.SalerPermission == null || searchModel.SalerPermission.Trim() == "")
                                    {
                                        searchModel.SalerPermission = current_user.UserUnderList;
                                    }
                                    else
                                    {
                                        searchModel.SalerPermission += "," + current_user.UserUnderList;

                                    }
                                }
                                break;
                            case (int)RoleType.Admin:
                            case (int)RoleType.KT:
                            case (int)RoleType.GD:
                                {
                                    searchModel.Sale = null;
                                }
                                break;
                        }
                    }
                    searchModel.PermisionType = null;
                    searchModel.PaymentStatus = null;

                    var dataOrder = await _OrderRepository.GetTotalCountOrder(searchModel, 1, 10);
                    return new JsonResult(dataOrder);
                }
                else
                {
                    return new JsonResult(new
                    {
                        totalrecordErr = 0
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderStatistic - DashBoardController: " + ex.ToString());
                return Content("");
            }
        }
    }
}
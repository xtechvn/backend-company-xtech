using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers.WorkManagement
{
    [CustomAuthorize]
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;

        public ProjectController(IProjectRepository projectRepository, ISprintRepository sprintRepository, IProjectTaskRepository projectTaskRepository)
        {
            _projectRepository = projectRepository;
            _sprintRepository = sprintRepository;
            _projectTaskRepository = projectTaskRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search()
        {
            var projects = await _projectRepository.GetAllProjects();
            return PartialView(projects);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Projects model)
        {
            try
            {
                var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                model.CreatedBy = userId;
                var id = await _projectRepository.Upsert(model);
                return Json(new { isSuccess = id > 0, id = id });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Create - ProjectController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] Projects model)
        {
            try
            {
                var id = await _projectRepository.Upsert(model);
                return Json(new { isSuccess = id > 0, id = id });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Update - ProjectController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // 1. Create Project
                var project = new Projects
                {
                    ProjectName = "Dự án mẫu (XTech)",
                    Description = "Dự án dùng để test các tính năng Backlog và Board.",
                    CreatedBy = userId
                };
                var projectId = await _projectRepository.Upsert(project);

                if (projectId > 0)
                {
                    // 2. Create Sprint
                    var sprint = new Sprint
                    {
                        SprintName = "Sprint 1 - Thiết kế UI",
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddDays(14),
                        Goal = "Hoàn thành giao diện cơ bản",
                        Status = 1, // Active
                        ProjectId = projectId,
                        CreatedBy = userId
                    };
                    var sprintId = await _sprintRepository.Upsert(sprint);

                    // 3. Create Tasks
                    // Task in Sprint (In Progress)
                    await _projectTaskRepository.Upsert(new ProjectTask
                    {
                        Title = "Thiết kế Homepage",
                        Description = "Thiết kế giao diện trang chủ theo style modern.",
                        ProjectId = projectId,
                        SprintId = sprintId,
                        StatusId = 1, // In Progress
                        PriorityId = 2, // High
                        CreatedBy = (int?)userId
                    });

                    // Task in Sprint (To Do)
                    await _projectTaskRepository.Upsert(new ProjectTask
                    {
                        Title = "Cài đặt database",
                        Description = "Thiết kế schema và migration database.",
                        ProjectId = projectId,
                        SprintId = sprintId,
                        StatusId = 0, // To Do
                        PriorityId = 1, // Medium
                        CreatedBy = (int?)userId
                    });

                    // Task in Backlog
                    await _projectTaskRepository.Upsert(new ProjectTask
                    {
                        Title = "Viết API Authentication",
                        Description = "Triển khai JWT và login flow.",
                        ProjectId = projectId,
                        SprintId = null,
                        StatusId = 0,
                        PriorityId = 1,
                        CreatedBy = (int?)userId
                    });
                }

                return Json(new { isSuccess = true });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SeedData - ProjectController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}

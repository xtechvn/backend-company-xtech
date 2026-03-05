using Entities.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using WEB.CMS.Customize;
using Ultilities.Constants;
using Entities.ViewModels;

namespace WEB.CMS.Controllers.WorkManagement
{
    [CustomAuthorize]
    public class TaskManagementController : Controller
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TaskManagementController(ISprintRepository sprintRepository, IProjectTaskRepository projectTaskRepository, IProjectRepository projectRepository, IUserRepository userRepository, IWebHostEnvironment webHostEnvironment)
        {
            _sprintRepository = sprintRepository;
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(long? projectId)
        {
            ViewBag.ProjectId = projectId;
            return View();
        }

        public async Task<IActionResult> Backlog(long? projectId)
        {
            var sprints = await _sprintRepository.GetAllSprints(projectId);
            var backlogTasks = await _projectTaskRepository.GetTasksBySprint(null, projectId);
            
            // Fetch tasks for each sprint
            var sprintTasks = new Dictionary<long, List<ProjectTask>>();
            if (sprints != null)
            {
                foreach (var sprint in sprints)
                {
                    var tasks = await _projectTaskRepository.GetTasksBySprint(sprint.Id, projectId);
                    sprintTasks[sprint.Id] = tasks;
                }
            }

            var projects = await _projectRepository.GetAllProjects();
            var users = _userRepository.GetAll();
            ViewBag.ProjectId = projectId;
            ViewBag.BacklogTasks = backlogTasks;
            ViewBag.Sprints = sprints;
            ViewBag.SprintTasks = sprintTasks;
            ViewBag.Projects = projects;
            ViewBag.Users = users;
            return PartialView();
        }

        public async Task<IActionResult> Board(long? projectId)
        {
            var activeSprint = (await _sprintRepository.GetAllSprints(projectId)).FirstOrDefault(s => s.Status == 1); // Active
            var tasks = activeSprint != null ? await _projectTaskRepository.GetTasksBySprint(activeSprint.Id, projectId) : new List<ProjectTask>();
            var users = _userRepository.GetAll();
            ViewBag.ProjectId = projectId;
            ViewBag.ActiveSprint = activeSprint;
            ViewBag.Tasks = tasks;
            ViewBag.Users = users;
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSprint([FromBody] Sprint model)
        {
            try
            {
                var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                model.CreatedBy = userId;
                model.Status = 0; // Future
                // Note: model.ProjectId should be sent from frontend
                var id = await _sprintRepository.Upsert(model);
                return Json(new { isSuccess = id > 0, id = id });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateSprint - TaskManagementController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSprintDetail(long id)
        {
            var detail = await _sprintRepository.GetById(id);
            return Json(new { isSuccess = detail != null, data = detail });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSprint(long id)
        {
            try
            {
                var success = await _sprintRepository.Delete(id);
                return Json(new { isSuccess = success });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromForm] ProjectTaskViewModel model)
        {
            try
            {
                var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                var task = new ProjectTask
                {
                    Id = model.Id ?? 0,
                    Title = model.Title,
                    Description = model.Description,
                    AssigneeId = (int?)model.AssigneeId,
                    ReporterId = (int?)model.ReporterId,
                    PriorityId = model.Priority,
                    SprintId = model.SprintId,
                    StoryPoint = model.StoryPoint,
                    DueDate = model.DueDate,
                    TaskOrder = model.TaskOrder ?? model.Order,
                    CreatedBy = (int?)userId,
                    StatusId = model.Status ?? 0,
                    ProjectId = model.ProjectId,
                    TaskType = model.TaskType
                };

                if (model.AttachmentFile != null)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(model.AttachmentFile.FileName);
                    string uploadFolder = @"uploads/tasks/" + userId;
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, uploadFolder);

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    string filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AttachmentFile.CopyToAsync(stream);
                    }
                    task.Attachment = "/" + uploadFolder + "/" + fileName;
                }

                var id = await _projectTaskRepository.Upsert(task);
                return Json(new { isSuccess = id > 0, id = id });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CreateTask - TaskManagementController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetTaskDetail(long id)
        {
            var detail = await _projectTaskRepository.GetById(id);
            return Json(new { isSuccess = detail != null, data = detail });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(long id)
        {
            try
            {
                var success = await _projectTaskRepository.Delete(id);
                return Json(new { isSuccess = success });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MoveTask(long taskId, long? sprintId)
        {
            try
            {
                var success = await _projectTaskRepository.UpdateSprint(taskId, sprintId);
                return Json(new { isSuccess = success });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("MoveTask - TaskManagementController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MoveTasks([FromForm] List<long> taskIds, long? sprintId)
        {
            try
            {
                var success = await _projectTaskRepository.UpdateSprints(taskIds, sprintId);
                return Json(new { isSuccess = success });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("MoveTasks - TaskManagementController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> StartSprint(long sprintId)
        {
            try
            {
                var success = await _sprintRepository.UpdateStatus(sprintId, 1);
                return Json(new { isSuccess = success });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("StartSprint - TaskManagementController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(long taskId, int status)
        {
            try
            {
                var success = await _projectTaskRepository.UpdateStatus(taskId, status);
                return Json(new { isSuccess = success });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateTaskStatus - TaskManagementController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }

    public class ProjectTaskViewModel
    {
        public long? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long? AssigneeId { get; set; }
        public long? ReporterId { get; set; }
        public int? Priority { get; set; }
        public long? SprintId { get; set; }
        public int? StoryPoint { get; set; }
        public DateTime? DueDate { get; set; }
        public string Label { get; set; }
        public IFormFile AttachmentFile { get; set; }
        public long? ProjectId { get; set; }
        public int? Status { get; set; }
        public int? TaskType { get; set; }
        public int? Order { get; set; }
        public int? TaskOrder { get; set; }
    }
}

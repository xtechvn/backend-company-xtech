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
using Nest;
using Repositories.Repositories;

namespace WEB.CMS.Controllers.WorkManagement
{
    [CustomAuthorize]
    public class TaskManagementController : Controller
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IProjectTaskRepository _projectTaskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITaskCommentRepository _taskCommentRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAllCodeRepository _allCodeRepository;

        public TaskManagementController(ISprintRepository sprintRepository, IProjectTaskRepository projectTaskRepository, IProjectRepository projectRepository, 
            IUserRepository userRepository, ITaskCommentRepository taskCommentRepository, IWebHostEnvironment webHostEnvironment, IAllCodeRepository allCodeRepository)
        {
            _sprintRepository = sprintRepository;
            _projectTaskRepository = projectTaskRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _taskCommentRepository = taskCommentRepository;
            _webHostEnvironment = webHostEnvironment;
            _allCodeRepository = allCodeRepository;
        }

        public async Task<IActionResult> Index(long? projectId)
        {
            // Get all projects for dropdown
            var projects = await _projectRepository.GetAllProjects();
            ViewBag.Projects = projects;

            // If no projectId provided, try to get from session (default project)
            if (!projectId.HasValue)
            {
                var defaultProjectId = HttpContext.Session.GetInt32("DefaultProjectId");
                
                // Log to verify
                System.Diagnostics.Debug.WriteLine($"Reading DefaultProjectId from Session: {defaultProjectId}");
                
                if (defaultProjectId.HasValue)
                {
                    projectId = defaultProjectId.Value;
                    System.Diagnostics.Debug.WriteLine($"Using default project from Session: {projectId}");
                }
                else if (projects != null && projects.Any())
                {
                    // If no default project in session, use first project
                    projectId = projects.First().Id;
                    System.Diagnostics.Debug.WriteLine($"No default project in Session, using first project: {projectId}");
                }
                
                // Redirect to Index with the selected project
                if (projectId.HasValue)
                {
                    return RedirectToAction("Index", new { projectId = projectId });
                }
            }

            ViewBag.ProjectId = projectId;

            // Get current project details if projectId is provided
            if (projectId.HasValue)
            {
                var currentProject = projects.FirstOrDefault(p => p.Id == projectId.Value);
                ViewBag.CurrentProject = currentProject;
            }

            return View();
        }

        [HttpPost]
        public IActionResult SetDefaultProject(long projectId)
        {
            try
            {
                // Save default project to session
                HttpContext.Session.SetInt32("DefaultProjectId", (int)projectId);
                
                // Log to verify
                System.Diagnostics.Debug.WriteLine($"Saved DefaultProjectId to Session: {projectId}");
                
                return Json(new { isSuccess = true, message = "Đã đặt dự án mặc định" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving DefaultProjectId: {ex.Message}");
                return Json(new { isSuccess = false, message = ex.Message });
            }
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
            var taskStatuses = _allCodeRepository.GetListByType("TASK_STATUS");
            ViewBag.ProjectId = projectId;
            ViewBag.BacklogTasks = backlogTasks;
            ViewBag.Sprints = sprints;
            ViewBag.SprintTasks = sprintTasks;
            ViewBag.Projects = projects;
            ViewBag.Users = users;
            ViewBag.TaskStatuses = taskStatuses;
            return PartialView();
        }

        public async Task<IActionResult> Board(long? projectId)
        {
            var activeSprint = (await _sprintRepository.GetAllSprints(projectId)).FirstOrDefault(s => s.Status == 1); // Active
            var allSprints = await _sprintRepository.GetAllSprints(projectId);
            if (allSprints != null && allSprints.Count > 0)
            {
                allSprints = allSprints.Where(s => s.Status == (int)SprintStatus.start).ToList();
            }
            // Get tasks for each sprint
            var sprintTasks = new Dictionary<long, List<ProjectTask>>();
            if (allSprints != null)
            {
                foreach (var sprint in allSprints)
                {
                    var tasks = await _projectTaskRepository.GetTasksBySprint(sprint.Id, projectId);
                    sprintTasks[sprint.Id] = tasks;
                }
            }
            var taskStatuses = _allCodeRepository.GetListByType("TASK_STATUS");
            var users = _userRepository.GetAll();
            var projects = await _projectRepository.GetAllProjects();
            ViewBag.ProjectId = projectId;
            ViewBag.ActiveSprint = activeSprint;
            ViewBag.Sprints = allSprints;
            ViewBag.SprintTasks = sprintTasks;
            ViewBag.Users = users;
            ViewBag.Projects = projects;
            ViewBag.TaskStatuses = taskStatuses;
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> TaskDetail(long id)
        {
            var task = await _projectTaskRepository.GetById(id);
            var users = _userRepository.GetAll();
            var comments = await _taskCommentRepository.GetCommentsByTaskId(id);
            var taskStatuses = _allCodeRepository.GetByType("TASK_STATUS");
            ViewBag.Task = task;
            ViewBag.Users = users;
            ViewBag.Comments = comments;
            ViewBag.TaskStatuses = taskStatuses;
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] TaskComment model)
        {
            try
            {
                var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                model.UserId = userId;
                model.CreatedDate = DateTime.Now;

                var id = await _taskCommentRepository.CreateComment(model);

                // Get user info for response
                var user = _userRepository.GetAll().FirstOrDefault(u => u.Id == userId);

                return Json(new
                {
                    isSuccess = id > 0,
                    id = id,
                    userName = user?.FullName ?? "User",
                    createdDate = model.CreatedDate?.ToString("dd/MM/yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddComment - TaskManagementController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(long id)
        {
            try
            {
                var success = await _taskCommentRepository.DeleteComment(id);
                return Json(new { isSuccess = success });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteComment - TaskManagementController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
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
        public async Task<IActionResult> StartSprint(long sprintId, int status)
        {
            try
            {
                var success = await _sprintRepository.UpdateStatus(sprintId, status);
                return Json(new { isSuccess = success });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("StartSprint - TaskManagementController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteSprint(long sprintId, long? targetSprintId)
        {
            try
            {
                // Get all tasks in the sprint that are not completed (status != 5)
                var tasks = await _projectTaskRepository.GetTasksBySprint(sprintId, null);
                var incompleteTasks = tasks.Where(t => t.StatusId != 5).Select(t => t.Id).ToList();

                // Move incomplete tasks to target sprint or backlog
                if (incompleteTasks.Count > 0)
                {
                    await _projectTaskRepository.UpdateSprints(incompleteTasks, targetSprintId);
                }

                // Update sprint status to completed (2)
                var success = await _sprintRepository.UpdateStatus(sprintId, 2);
                
                return Json(new { isSuccess = success });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CompleteSprint - TaskManagementController: " + ex);
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

        [HttpPost]
        public async Task<IActionResult> UpdateTaskDetails([FromBody] UpdateTaskDetailsModel model)
        {
            try
            {
                var task = await _projectTaskRepository.GetById(model.TaskId);
                if (task == null)
                {
                    return Json(new { isSuccess = false, message = "Task not found" });
                }

                var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                task.Title = model.Title ?? task.Title;
                task.Description = model.Description ?? task.Description;
                task.AssigneeId = model.AssigneeId.HasValue ? (int?)model.AssigneeId.Value : task.AssigneeId;
                task.ReporterId = model.ReporterId.HasValue ? (int?)model.ReporterId.Value : task.ReporterId;
                task.ModifiedBy = (int?)userId;
                task.ModifiedDate = DateTime.Now;

                var id = await _projectTaskRepository.Upsert(task);
                return Json(new { isSuccess = id > 0 });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateTaskDetails - TaskManagementController: " + ex);
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }

    public class UpdateTaskDetailsModel
    {
        public long TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long? AssigneeId { get; set; }
        public long? ReporterId { get; set; }
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

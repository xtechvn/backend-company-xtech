var taskManagement = {
    projectId: null,

    initTabs: function (projectId) {
        this.projectId = projectId;
        $(".task-tabs .nav-link").click(function () {
            $(".task-tabs .nav-link").removeClass("active");
            $(this).addClass("active");
            var target = $(this).data("target");
            taskManagement.loadContent(target);
        });

        // Load default tab
        taskManagement.loadContent("backlog");
    },

    loadContent: function (tab) {
        $("#taskManagementContent").html('<div class="text-center p-5"><div class="spinner-border text-primary"></div></div>');
        var url = "/TaskManagement/Backlog?projectId=" + (this.projectId || "");
        if (tab === "board") url = "/TaskManagement/Board?projectId=" + (this.projectId || "");

        $.get(url, function (res) {
            $("#taskManagementContent").html(res);
            if (tab === "backlog") taskManagement.initBacklog();
            else taskManagement.initBoard();
        });
    },

    initBacklog: function () {
        // Multi-select logic using Checkboxes
        $(document).off("change", ".task-checkbox").on("change", ".task-checkbox", function (e) {
            var card = $(this).closest(".task-card");
            if ($(this).is(":checked")) {
                card.addClass("border-primary bg-blue-50 selected-task");
            } else {
                card.removeClass("border-primary bg-blue-50 selected-task");
            }

            // Sync "Select All" checkbox state
            var list = card.closest('.task-list');
            var allCheckboxes = list.find('.task-checkbox');
            var checkedCheckboxes = list.find('.task-checkbox:checked');
            var selectAllCb = list.parent().find('.sprint-select-all');
            if (selectAllCb.length > 0) {
                selectAllCb.prop('checked', allCheckboxes.length > 0 && allCheckboxes.length === checkedCheckboxes.length);
            }
        });

        // "Select All" logic
        $(document).off("change", ".sprint-select-all").on("change", ".sprint-select-all", function (e) {
            var isChecked = $(this).is(":checked");
            var targetWrap = $(this).closest('.backlog-item, .sprint-item');
            var checkboxes = targetWrap.find('.task-checkbox');

            checkboxes.prop("checked", isChecked);
            checkboxes.each(function () {
                var card = $(this).closest(".task-card");
                if (isChecked) {
                    card.addClass("border-primary bg-blue-50 selected-task");
                } else {
                    card.removeClass("border-primary bg-blue-50 selected-task");
                }
            });
        });

        $(".task-list").sortable({
            connectWith: ".task-list",
            placeholder: "ui-state-highlight mb-2 rounded border-dashed border-2 border-primary h-10",
            helper: function (e, item) {
                if (!item.hasClass('selected-task')) {
                    item.addClass('selected-task border-primary bg-blue-50');
                    item.find('.task-checkbox').prop('checked', true);
                }
                var selected = $('.selected-task');
                var helper = $('<div class="multiple-drag-helper"></div>').append(selected.clone());
                item.data('multidrag', selected);
                return helper;
            },
            start: function (event, ui) {
                var selected = ui.item.data('multidrag');
                selected.not(ui.item).hide();
            },
            stop: function (event, ui) {
                var selected = ui.item.data('multidrag');
                selected.not(ui.item).show().insertAfter(ui.item);
                $('.task-card').removeClass('selected-task border-primary bg-blue-50');
                $('.task-checkbox').prop('checked', false);
                $('.sprint-select-all').prop('checked', false);
            },
            receive: function (event, ui) {
                var sprintId = $(this).data("sprint-id");
                var selected = ui.item.data('multidrag');
                var taskIds = [];
                selected.each(function () {
                    taskIds.push($(this).data("id"));
                });

                if (taskIds.length > 0) {
                    taskManagement.moveTasks(taskIds, sprintId);
                }
            }
        }).disableSelection();
    },

    initBoard: function () {
        $(".task-drop-zone").sortable({
            connectWith: ".task-drop-zone",
            placeholder: "ui-state-highlight mb-2 rounded border-dashed border-2 border-primary h-20",
            receive: function (event, ui) {
                var taskId = ui.item.data("id");
                var status = $(this).data("status");
                taskManagement.updateStatus(taskId, status);
            }
        }).disableSelection();
    },

    showCreateSprintModal: function () {
        $("#sprint-id-hidden").val("");
        $("#sprint-name").val("");
        $("#sprint-start").val("");
        $("#sprint-end").val("");
        $("#sprint-goal").val("");
        $("#modal-sprint .modal-title").text("Create Sprint");
        $("#modal-sprint").modal("show");
    },

    showCreateTaskModal: function (sprintId) {
        $("#task-id-hidden").val("");
        $("#task-status-hidden").val("0");
        $("#task-title").val("");
        $("#task-desc").val("");
        $("#task-assignee").val("").trigger("change");
        $("#task-reporter").val("").trigger("change");
        $("#task-priority").val("1");
        $("#task-sprint-id-modal").val(sprintId || "");
        $("#task-story-point").val("");
        $("#task-due-date").val("");
        $("#task-label").val("");
        $("#task-project-id").val(this.projectId || "");
        $("#task-type").val("0");
        $("#modal-task .modal-title").text("Create Task");
        $("#modal-task").modal("show");
    },

    saveSprint: function () {
        var data = {
            Id: $("#sprint-id-hidden").val() || 0,
            SprintName: $("#sprint-name").val(),
            StartDate: $("#sprint-start").val(),
            EndDate: $("#sprint-end").val(),
            Goal: $("#sprint-goal").val(),
            Status: 0,
            ProjectId: this.projectId
        };

        if (!data.SprintName) {
            toastr.error("Please enter sprint name");
            return;
        }

        $.ajax({
            url: "/TaskManagement/CreateSprint",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (res) {
                if (res.isSuccess) {
                    taskManagement.loadContent("backlog");
                    $.magnificPopup.close();
                    toastr.success("Sprint saved successfully");
                } else {
                    toastr.error("Failed to save sprint");
                }
            }
        });
    },

    editSprint: function (id) {
        $.post("/TaskManagement/GetSprintDetail", { id: id }, function (res) {
            if (res.isSuccess) {
                var d = res.data;
                $("#sprint-id-hidden").val(d.id);
                $("#sprint-name").val(d.sprintName);
                if (d.startDate) $("#sprint-start").val(d.startDate.split('T')[0]);
                if (d.endDate) $("#sprint-end").val(d.endDate.split('T')[0]);
                $("#sprint-goal").val(d.goal);
                $("#modal-sprint .modal-title").text("Edit Sprint");
                $("#modal-sprint").modal("show");
            }
        });
    },

    deleteSprint: function (id) {
        Swal.fire({
            title: 'Delete Sprint?',
            text: "This will permanently delete the sprint.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                $.post("/TaskManagement/DeleteSprint", { id: id }, function (res) {
                    if (res.isSuccess) {
                        toastr.success("Sprint deleted");
                        taskManagement.loadContent("backlog");
                    } else {
                        toastr.error(res.message || "Failed to delete sprint");
                    }
                });
            }
        });
    },

    saveTask: function () {
        var formData = new FormData();
        formData.append("Id", $("#task-id-hidden").val() || "");
        formData.append("Title", $("#task-title").val());
        formData.append("Description", $("#task-desc").val());
        formData.append("AssigneeId", $("#task-assignee").val() || "");
        formData.append("ReporterId", $("#task-reporter").val() || "");
        formData.append("Priority", $("#task-priority").val());
        formData.append("SprintId", $("#task-sprint-id-modal").val() || "");
        formData.append("StoryPoint", $("#task-story-point").val() || "");
        formData.append("DueDate", $("#task-due-date").val() || "");
        formData.append("Label", $("#task-label").val() || "");
        formData.append("ProjectId", $("#task-project-id").val() || this.projectId || "");
        formData.append("TaskType", $("#task-type").val() || 0);
        formData.append("Status", $("#task-status-hidden").val() || 0);

        var fileInput = document.getElementById('task-attachment');
        if (fileInput.files.length > 0) {
            formData.append("AttachmentFile", fileInput.files[0]);
        }

        if (!$("#task-title").val()) {
            toastr.error("Please enter task title");
            return;
        }

        $.ajax({
            url: "/TaskManagement/CreateTask",
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
                if (res.isSuccess) {
                    taskManagement.loadContent("backlog");
                    $.magnificPopup.close();
                    toastr.success("Task saved successfully");
                } else {
                    toastr.error(res.message || "Failed to save task");
                }
            }
        });
    },

    editTask: function (id) {
        $.post("/TaskManagement/GetTaskDetail", { id: id }, function (res) {
            if (res.isSuccess) {
                var d = res.data;
                $("#task-id-hidden").val(d.id);
                $("#task-status-hidden").val(d.statusId);
                $("#task-title").val(d.title);
                $("#task-desc").val(d.description);
                if (d.assigneeId) {
                    var newOption = new Option(d.assigneeName || "User " + d.assigneeId, d.assigneeId, true, true);
                    $("#task-assignee").append(newOption).trigger('change');
                }
                if (d.reporterId) {
                    var newOption = new Option(d.reporterName || "User " + d.reporterId, d.reporterId, true, true);
                    $("#task-reporter").append(newOption).trigger('change');
                }
                $("#task-priority").val(d.priorityId);
                $("#task-sprint-id-modal").val(d.sprintId || "");
                $("#task-story-point").val(d.storyPoint);
                if (d.dueDate) $("#task-due-date").val(d.dueDate.split('T')[0]);
                $("#task-label").val(d.label);
                $("#task-project-id").val(d.projectId || "");
                $("#task-type").val(d.taskType || 0);
                $("#modal-task .modal-title").text("Edit Task");
                $("#modal-task").modal("show");
            }
        });
    },

    deleteTask: function (id) {
        Swal.fire({
            title: 'Delete Task?',
            text: "This will permanently delete the task.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                $.post("/TaskManagement/DeleteTask", { id: id }, function (res) {
                    if (res.isSuccess) {
                        toastr.success("Task deleted");
                        taskManagement.loadContent("backlog");
                    } else {
                        toastr.error(res.message || "Failed to delete task");
                    }
                });
            }
        });
    },

    moveTask: function (taskId, sprintId) {
        $.post("/TaskManagement/MoveTask", { taskId: taskId, sprintId: sprintId }, function (res) {
            if (res.isSuccess) {
                toastr.success("Task moved successfully");
            } else {
                toastr.error(res.message || "Failed to move task");
            }
        });
    },

    moveTasks: function (taskIds, sprintId) {
        $.ajax({
            url: "/TaskManagement/MoveTasks",
            type: "POST",
            data: { taskIds: taskIds, sprintId: sprintId },
            success: function (res) {
                if (res.isSuccess) {
                    toastr.success(taskIds.length + " task(s) moved successfully");
                } else {
                    toastr.error(res.message || "Failed to move tasks");
                }
            }
        });
    },

    startSprint: function (sprintId,status) {
        Swal.fire({
            title: 'Start Sprint?',
            text: "This will set the sprint as active.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Yes, start it!'
        }).then((result) => {
            if (result.isConfirmed || result.value) {
                $.post("/TaskManagement/StartSprint", { sprintId: sprintId, status: status }, function (res) {
                    if (res.isSuccess) {
                        toastr.success("Sprint started");
                        location.reload();
                    }
                });
            }
        });
    },

    updateStatus: function (taskId, status) {
        $.post("/TaskManagement/UpdateTaskStatus", { taskId: taskId, status: status }, function (res) {
            if (res.isSuccess) {
               // toastr.success("Status updated");
            } else {
                toastr.error("Failed to update status");
            }
        });
    }
};

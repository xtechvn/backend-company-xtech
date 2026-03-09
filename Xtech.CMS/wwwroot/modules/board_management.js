var boardTaskManagement = {
    init: function() {
        this.initDragDrop();
        this.initEventHandlers();
        this.initHorizontalScroll();
    },

    initHorizontalScroll: function() {
        // Enable horizontal scroll by dragging
        $('.board-container').each(function() {
            const slider = this;
            let isDown = false;
            let startX;
            let scrollLeft;

            $(slider).on('mousedown', function(e) {
                // Only start drag if clicking on the container itself, not on task cards
                if ($(e.target).closest('.task-card, .task-drop-zone').length) {
                    return;
                }
                
                isDown = true;
                slider.classList.add('active-scroll');
                startX = e.pageX - slider.offsetLeft;
                scrollLeft = slider.scrollLeft;
                e.preventDefault();
            });

            $(slider).on('mouseleave', function() {
                isDown = false;
                slider.classList.remove('active-scroll');
            });

            $(slider).on('mouseup', function() {
                isDown = false;
                slider.classList.remove('active-scroll');
            });

            $(slider).on('mousemove', function(e) {
                if (!isDown) return;
                e.preventDefault();
                const x = e.pageX - slider.offsetLeft;
                const walk = (x - startX) * 2; // Scroll speed multiplier
                slider.scrollLeft = scrollLeft - walk;
            });
        });
    },

    initDragDrop: function() {
        // Initialize Drag & Drop with jQuery UI Sortable for each sprint separately
        $(".task-drop-zone").each(function() {
            var sprintId = $(this).data("sprint");
            
            $(this).sortable({
                connectWith: ".task-drop-zone[data-sprint='" + sprintId + "']",
                cursor: "move",
                placeholder: "task-placeholder",
                tolerance: "pointer",
                helper: "clone",
                opacity: 0.8,
                delay: 150,
                distance: 5,
                start: function(event, ui) {
                    ui.item.addClass("dragging");
                    ui.placeholder.height(ui.item.height());
                },
                stop: function(event, ui) {
                    ui.item.removeClass("dragging");
                },
                update: function(event, ui) {
                    // Only trigger on the receiving list
                    if (this === ui.item.parent()[0]) {
                        var taskId = ui.item.data("id");
                        var newStatus = parseInt($(this).data("status"));
                        var oldStatus = ui.sender ? parseInt(ui.sender.data("status")) : newStatus;
                        
                        // Only update if status actually changed
                        if (oldStatus !== newStatus) {
                            boardTaskManagement.updateTaskStatus(taskId, newStatus, ui.item);
                        }
                    }
                }
            }).disableSelection();
        });
    },

    initEventHandlers: function() {
        // Task card click event
        $(document).off("click", ".task-card").on("click", ".task-card", function(e) {
            // Prevent opening modal when dragging or clicking action buttons
            if($(e.target).closest('.avatar-circle, button').length || $(this).hasClass('ui-sortable-helper')) return;
            
            var taskId = $(this).data("id");
            // Open task detail using magnific popup
            boardTaskManagement.openTaskDetail(taskId);
        });
    },

    updateTaskStatus: function(taskId, newStatus, taskCard) {
        $.ajax({
            url: '/TaskManagement/UpdateTaskStatus',
            type: 'POST',
            data: {
                taskId: taskId,
                status: newStatus
            },
            success: function(response) {
                if(response.isSuccess) {
                    toastr.success("Task moved successfully!");
                    
                    // Update card styling based on new status
                    taskCard.removeClass('border-l-4 border-l-blue-500 opacity-75');
                    taskCard.find('.text-sm.mb-2').removeClass('line-through text-gray-500');
                    taskCard.find('.bi-check-circle-fill').remove();
                    
                    if(newStatus === 1) {
                        // IN PROGRESS styling
                        taskCard.addClass('border-l-4 border-l-blue-500');
                        if(!taskCard.find('.avatar-circle').length) {
                            taskCard.find('.flex.justify-between .text-xs').next().replaceWith('<div class="avatar-circle">US</div>');
                        }
                    } else if(newStatus === 2) {
                        // DONE styling
                        taskCard.addClass('opacity-75');
                        taskCard.find('.text-sm.mb-2').addClass('line-through text-gray-500');
                        taskCard.find('.avatar-circle').replaceWith('<i class="bi bi-check-circle-fill text-success"></i>');
                    } else {
                        // TO DO styling (default)
                        if(!taskCard.find('.avatar-circle').length) {
                            taskCard.find('.flex.justify-between .text-xs').next().replaceWith('<div class="avatar-circle">US</div>');
                        }
                    }
                    
                    // Update column counts
                    $('.board-column').each(function() {
                        var count = $(this).find('.task-card').length;
                        $(this).find('.column-header span.bg-gray-200').text(count);
                    });
                } else {
                    toastr.error(response.message || "Failed to update task status!");
                    // Revert the move on failure
                    location.reload();
                }
            },
            error: function() {
                toastr.error("An error occurred while updating task!");
                // Revert the move on error
                location.reload();
            }
        });
    },

    openTaskDetail: function(id) {
        let title = 'TASK-' + id;
        let url = '/TaskManagement/TaskDetail';
        let param = { id: id };
        
        _magnific.OpenSmallPopup(title, url, param);
    },
    
    showCreateTaskModal: function() {
        // Reset form
        $("#task-title-create").val("");
        $("#task-desc").val("");
        $("#task-assignee-create").val("");
        $("#task-reporter-create").val("");
        $("#task-priority").val("1");
        $("#task-status-create").val("0");
        $("#task-story-point").val("");
        $("#task-due-date").val("");
        $("#task-label").val("");
        $("#task-attachment").val("");
        
        // Show modal
        $("#modal-create-task").modal("show");
    },
    
    saveTask: function() {
        var title = $("#task-title-create").val().trim();
        var projectId = $("#task-project-id").val();
        var sprintId = $("#task-sprint-id").val();
        
        if (!title) {
            toastr.error("Please enter task title!");
            return;
        }
        
        if (!projectId) {
            toastr.error("Please select a project!");
            return;
        }
        
        var formData = new FormData();
        formData.append("Title", title);
        formData.append("Description", $("#task-desc").val());
        formData.append("AssigneeId", $("#task-assignee-create").val() || "");
        formData.append("ReporterId", $("#task-reporter-create").val() || "");
        formData.append("Priority", $("#task-priority").val());
        formData.append("Status", $("#task-status-create").val());
        formData.append("StoryPoint", $("#task-story-point").val() || "");
        formData.append("DueDate", $("#task-due-date").val() || "");
        formData.append("Label", $("#task-label").val() || "");
        formData.append("ProjectId", projectId);
        formData.append("SprintId", sprintId);
        formData.append("TaskType", $("#task-type").val());
        
        var fileInput = document.getElementById("task-attachment");
        if (fileInput.files.length > 0) {
            formData.append("AttachmentFile", fileInput.files[0]);
        }
        
        $.ajax({
            url: '/TaskManagement/CreateTask',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (response.isSuccess) {
                    toastr.success("Task created successfully!");
                    $("#modal-create-task").modal("hide");
                    // Reload page to show new task
                    location.reload();
                } else {
                    toastr.error(response.message || "Failed to create task!");
                }
            },
            error: function() {
                toastr.error("An error occurred while creating task!");
            }
        });
    },

    showCompleteSprintModal: function(sprintId) {
        $("#complete-sprint-id").val(sprintId);
        $("#complete-sprint-target").val("");
        $("#modal-complete-sprint").modal("show");
    },

    completeSprint: function() {
        var sprintId = $("#complete-sprint-id").val();
        var targetSprintId = $("#complete-sprint-target").val() || null;
        
        if (!sprintId) {
            toastr.error("Sprint ID not found");
            return;
        }

        $.ajax({
            url: "/TaskManagement/CompleteSprint",
            type: "POST",
            data: {
                sprintId: sprintId,
                targetSprintId: targetSprintId
            },
            success: function(res) {
                if (res.isSuccess) {
                    $("#modal-complete-sprint").modal("hide");
                    
                    setTimeout(function() {
                        $(".modal-backdrop").remove();
                        $("body").removeClass("modal-open").css("padding-right", "");
                        
                        toastr.success("Sprint completed successfully");
                        location.reload();
                    }, 300);
                } else {
                    toastr.error(res.message || "Failed to complete sprint");
                }
            },
            error: function() {
                toastr.error("An error occurred while completing sprint");
            }
        });
    }
};

// Initialize when document is ready
$(document).ready(function() {
    boardTaskManagement.init();
});

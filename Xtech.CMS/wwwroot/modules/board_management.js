var boardTaskManagement = {
    // Khởi tạo TinyMCE với hỗ trợ paste ảnh chụp màn hình
    initTinyMCEWithPaste: function (selector, initialContent) {
        var editorId = selector.replace('#', '');
        if (typeof tinymce !== 'undefined' && tinymce.get(editorId)) {
            tinymce.get(editorId).remove();
        }

        var useDarkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;

        tinymce.init({
            selector: selector,
            plugins: 'print preview paste importcss searchreplace autolink autosave save directionality code visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap quickbars emoticons',
            menubar: 'file edit view insert format tools table help',
            toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent | numlist bullist | forecolor backcolor removeformat | pagebreak | charmap emoticons | fullscreen preview save print | insertfile image media template link anchor codesample | ltr rtl',
            toolbar_sticky: true,
            height: 300,
            image_caption: true,
            quickbars_selection_toolbar: 'bold italic | quicklink h2 h3 blockquote quickimage quicktable',
            noneditable_noneditable_class: 'mceNonEditable',
            toolbar_mode: 'sliding',
            contextmenu: 'link image imagetools table',
            skin: useDarkMode ? 'oxide-dark' : 'oxide',
            content_css: useDarkMode ? 'dark' : 'default',
            content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px } img { max-width: 100%; height: auto; }',

            paste_data_images: true,
            images_upload_handler: function (blobInfo, success, failure) {
                var base64 = 'data:' + blobInfo.blob().type + ';base64,' + blobInfo.base64();
                success(base64);
            },

            setup: function (editor) {
                editor.on('init', function () {
                    if (initialContent) {
                        editor.setContent(initialContent);
                    }
                });
            }
        });
    },

    init: function() {
        this.initDragDrop();
        this.initEventHandlers();
        this.initHorizontalScroll();
    },

    initHorizontalScroll: function() {
        // Bật tính năng cuộn ngang bằng cách kéo chuột
        $('.board-container').each(function() {
            const slider = this;
            let isDown = false;
            let startX;
            let scrollLeft;

            $(slider).on('mousedown', function(e) {
                // Chỉ bắt đầu kéo nếu click vào container, không phải task card
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
                const walk = (x - startX) * 2; // Tốc độ cuộn
                slider.scrollLeft = scrollLeft - walk;
            });
        });
    },

    initDragDrop: function() {
        // Khởi tạo Drag & Drop với jQuery UI Sortable cho từng sprint riêng biệt
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
                    // Chỉ kích hoạt trên danh sách nhận
                    if (this === ui.item.parent()[0]) {
                        var taskId = ui.item.data("id");
                        var newStatus = parseInt($(this).data("status"));
                        var oldStatus = ui.sender ? parseInt(ui.sender.data("status")) : newStatus;
                        
                        // Chỉ cập nhật nếu trạng thái thực sự thay đổi
                        if (oldStatus !== newStatus) {
                            boardTaskManagement.updateTaskStatus(taskId, newStatus, ui.item);
                        }
                    }
                }
            }).disableSelection();
        });
    },

    initEventHandlers: function() {
        // Sự kiện click vào task card
        $(document).off("click", ".task-card").on("click", ".task-card", function(e) {
            // Ngăn mở modal khi đang kéo hoặc click vào nút action
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
                    toastr.success("Đã chuyển task thành công!");
                    
                    // Cập nhật style của card dựa trên trạng thái mới
                    taskCard.removeClass('border-l-4 border-l-blue-500 opacity-75');
                    taskCard.find('.text-sm.mb-2').removeClass('line-through text-gray-500');
                    taskCard.find('.bi-check-circle-fill').remove();
                    
                    if(newStatus === 1) {
                        // Style cho IN PROGRESS
                        taskCard.addClass('border-l-4 border-l-blue-500');
                        if(!taskCard.find('.avatar-circle').length) {
                            taskCard.find('.flex.justify-between .text-xs').next().replaceWith('<div class="avatar-circle">US</div>');
                        }
                    } else if(newStatus === 2) {
                        // Style cho DONE
                        taskCard.addClass('opacity-75');
                        taskCard.find('.text-sm.mb-2').addClass('line-through text-gray-500');
                        taskCard.find('.avatar-circle').replaceWith('<i class="bi bi-check-circle-fill text-success"></i>');
                    } else {
                        // Style cho TO DO (mặc định)
                        if(!taskCard.find('.avatar-circle').length) {
                            taskCard.find('.flex.justify-between .text-xs').next().replaceWith('<div class="avatar-circle">US</div>');
                        }
                    }
                    
                    // Cập nhật số lượng task trong mỗi cột
                    $('.board-column').each(function() {
                        var count = $(this).find('.task-card').length;
                        $(this).find('.column-header span.bg-gray-200').text(count);
                    });
                } else {
                    toastr.error(response.message || "Không thể cập nhật trạng thái task!");
                    // Hoàn tác thao tác khi thất bại
                    location.reload();
                }
            },
            error: function() {
                toastr.error("Đã xảy ra lỗi khi cập nhật task!");
                // Hoàn tác thao tác khi có lỗi
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
        // Đặt ngày hết hạn là ngày hiện tại
        var today = new Date().toISOString().split('T')[0];
        $("#task-due-date").val(today);
        $("#task-label").val("");
        $("#task-attachment").val("");
        
        // Hiển thị modal trước
        $("#modal-create-task").modal("show");
        
        // Đợi modal hiển thị xong rồi mới khởi tạo TinyMCE
        $("#modal-create-task").on('shown.bs.modal', function () {
            boardTaskManagement.initTinyMCEWithPaste('#task-desc');
            $(this).off('shown.bs.modal');
        });
    },
    
    saveTask: function() {
        var title = $("#task-title-create").val().trim();
        var projectId = $("#task-project-id").val();
        var sprintId = $("#task-sprint-id").val();
        
        if (!title) {
            toastr.error("Vui lòng nhập tên công việc!");
            return;
        }
        
        if (!projectId) {
            toastr.error("Vui lòng chọn dự án!");
            return;
        }
        
        // Lấy nội dung từ TinyMCE editor
        var description = "";
        if (typeof tinymce !== 'undefined' && tinymce.get('task-desc')) {
            description = tinymce.get('task-desc').getContent();
        } else {
            description = $("#task-desc").val();
        }
        
        var formData = new FormData();
        formData.append("Title", title);
        formData.append("Description", description);
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
        if (fileInput && fileInput.files.length > 0) {
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
                    toastr.success("Tạo task thành công!");
                    $("#modal-create-task").modal("hide");
                    
                    // Đợi modal đóng xong
                    setTimeout(function() {
                        $(".modal-backdrop").remove();
                        $("body").removeClass("modal-open").css("padding-right", "");
                        
                        // Hủy TinyMCE instance nếu tồn tại
                        if (typeof tinymce !== 'undefined' && tinymce.get('task-desc')) {
                            tinymce.get('task-desc').remove();
                        }
                        
                        // Tải lại trang để hiển thị task mới
                        location.reload();
                    }, 300);
                } else {
                    toastr.error(response.message || "Không thể tạo task!");
                }
            },
            error: function() {
                toastr.error("Đã xảy ra lỗi khi tạo task!");
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

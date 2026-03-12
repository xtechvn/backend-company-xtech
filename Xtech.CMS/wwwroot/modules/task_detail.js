var taskDetailManagement = {
    taskId: null,
    originalTitle: "",
    originalDescription: "",
    isSaving: false,

    init: function (taskId, title, description) {
        this.taskId = taskId;
        this.originalTitle = title || "";
        this.originalDescription = description || "";

        this.initTinyMCE(description);
        this.initTitleHandlers();
        this.initDescriptionHandlers();
        this.initAssigneeReporterHandlers();
        this.initStatusHandler();
        this.initCommentHandlers();
        this.initKeyboardShortcuts();
    },

    initTinyMCE: function (description) {
        var self = this;

        // Remove Magnific Popup's focus enforcement for TinyMCE
        if ($.magnificPopup && $.magnificPopup.instance) {
            var origOnFocusIn = $.magnificPopup.instance._onFocusIn;
            $.magnificPopup.instance._onFocusIn = function (e) {
                // Return true if the event target is inside the TinyMCE UI
                if ($(e.target).closest('.tox-tinymce, .tox-tinymce-aux, .moxman-window, .tam-assetmanager-root, .tox-dialog, .tox-dialog-wrap').length) {
                    return true;
                }
                // Otherwise call original focus event
                if (origOnFocusIn) {
                    origOnFocusIn.call(this, e);
                }
            };
        }

        // Xóa tất cả TinyMCE instances cũ
        if (typeof tinymce !== 'undefined') {
            tinymce.remove();
        }

        // Đợi một chút để đảm bảo instance cũ đã bị xóa hoàn toàn
        setTimeout(function () {
            // Đảm bảo textarea có nội dung trước khi khởi tạo TinyMCE
            $('#detail-description').val(description || "");

            // Khởi tạo TinyMCE mới - TinyMCE sẽ tự động lấy nội dung từ textarea
            if (typeof _common !== 'undefined' && typeof _common.tinyMce === 'function') {
                _common.tinyMce('#detail-description');
            }
        }, 100);
    },

    initTitleHandlers: function () {
        var self = this;

        $("#detail-task-title").focus(function () {
            $("#title-actions").css("display", "flex");
        });

        $("#btn-cancel-title").click(function () {
            $("#detail-task-title").val(self.originalTitle);
            $("#title-actions").css("display", "none");
        });

        $("#btn-save-title").click(function () {
            var newTitle = $("#detail-task-title").val().trim();
            var $btn = $(this);

            if (!newTitle) {
                toastr.error("Title cannot be empty!");
                return;
            }

            if (newTitle !== self.originalTitle && !self.isSaving) {
                $btn.prop('disabled', true).text('Saving...');

                self.updateTaskField("Title", newTitle, function () {
                    self.originalTitle = newTitle;
                    $("#title-actions").css("display", "none");
                    $btn.prop('disabled', false).text('Save');
                    toastr.success("Title updated!");
                }, function () {
                    $btn.prop('disabled', false).text('Save');
                });
            } else {
                $("#title-actions").css("display", "none");
            }
        });
    },

    initDescriptionHandlers: function () {
        var self = this;

        // Xử lý sự kiện focus cho TinyMCE editor
        // Đợi TinyMCE khởi tạo xong
        var checkTinyMCE = setInterval(function () {
            if (typeof tinymce !== 'undefined' && tinymce.get('detail-description')) {
                clearInterval(checkTinyMCE);

                // Lắng nghe sự kiện focus trên TinyMCE editor
                tinymce.get('detail-description').on('focus', function () {
                    $("#desc-actions").css("display", "flex");
                });
            }
        }, 100);

        // Fallback cho trường hợp không có TinyMCE
        $("#detail-description").focus(function () {
            $("#desc-actions").css("display", "flex");
        });

        $("#btn-cancel-description").click(function () {
            // Khôi phục nội dung gốc
            if (typeof tinymce !== 'undefined' && tinymce.get('detail-description')) {
                tinymce.get('detail-description').setContent(self.originalDescription);
            } else {
                $("#detail-description").val(self.originalDescription);
            }
            $("#desc-actions").css("display", "none");
        });

        $("#btn-save-description").click(function () {
            // Lấy nội dung từ TinyMCE hoặc textarea
            var newDescription = "";
            if (typeof tinymce !== 'undefined' && tinymce.get('detail-description')) {
                newDescription = tinymce.get('detail-description').getContent();
            } else {
                newDescription = $("#detail-description").val().trim();
            }

            var $btn = $(this);

            if (newDescription !== self.originalDescription && !self.isSaving) {
                $btn.prop('disabled', true).text('Đang lưu...');

                self.updateTaskField("Description", newDescription, function () {
                    self.originalDescription = newDescription;
                    $("#desc-actions").css("display", "none");
                    $btn.prop('disabled', false).text('Lưu');
                    toastr.success("Đã cập nhật mô tả!");
                }, function () {
                    $btn.prop('disabled', false).text('Lưu');
                });
            } else {
                $("#desc-actions").css("display", "none");
            }
        });
    },

    initAssigneeReporterHandlers: function () {
        var self = this;

        $("#detail-assignee").on("change", function () {
            var newAssigneeId = $(this).val() ? parseInt($(this).val()) : null;
            var $this = $(this);

            if (!self.isSaving) {
                $this.prop('disabled', true);
                self.updateTaskField("AssigneeId", newAssigneeId, function () {
                    $this.prop('disabled', false);
                    toastr.success("Assignee updated!");
                }, function () {
                    $this.prop('disabled', false);
                });
            }
        });

        $("#detail-reporter").on("change", function () {
            var newReporterId = $(this).val() ? parseInt($(this).val()) : null;
            var $this = $(this);

            if (!self.isSaving) {
                $this.prop('disabled', true);
                self.updateTaskField("ReporterId", newReporterId, function () {
                    $this.prop('disabled', false);
                    toastr.success("Reporter updated!");
                }, function () {
                    $this.prop('disabled', false);
                });
            }
        });
    },

    initStatusHandler: function () {
        var self = this;

        $("#detail-status").on("change", function () {
            var newStatus = parseInt($(this).val());

            if (!self.taskId) {
                toastr.error("Task ID not found!");
                return;
            }

            $.ajax({
                url: '/TaskManagement/UpdateTaskStatus',
                type: 'POST',
                data: {
                    taskId: self.taskId,
                    status: newStatus
                },
                success: function (response) {
                    if (response.isSuccess) {
                        toastr.success("Status updated successfully!");

                        setTimeout(() => {
                            $.magnificPopup.close();
                            location.reload();
                        }, 1000);
                    } else {
                        toastr.error(response.message || "Failed to update status!");
                    }
                },
                error: function () {
                    toastr.error("An error occurred while updating status!");
                }
            });
        });
    },

    initCommentHandlers: function () {
        var self = this;

        $("#comment-input").focus(function () {
            $(this).attr("rows", "3");
            $("#comment-actions").css("display", "flex");
        });

        $("#btn-cancel-comment").click(function () {
            $("#comment-input").val("");
            $("#comment-input").attr("rows", "1");
            $("#comment-actions").css("display", "none");
        });

        $("#btn-save-comment").click(function () {
            var val = $("#comment-input").val().trim();
            if (!val) return;

            if (!self.taskId) {
                toastr.error("Task ID not found!");
                return;
            }

            var $btn = $(this);
            $btn.prop('disabled', true).text('Saving...');

            $.ajax({
                url: '/TaskManagement/AddComment',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    TaskId: self.taskId,
                    Content: val
                }),
                success: function (response) {
                    if (response.isSuccess) {
                        var newComment = `
                            <div class="d-flex mb10 comment-item" style="display: flex; gap: 10px;" data-comment-id="${response.id}">
                                <div class="avatar-circle-sm flex-shrink-0" style="width: 32px; height: 32px; font-size: 14px;">U</div>
                                <div style="flex: 1;">
                                    <div style="display: flex; align-items: center; gap: 10px; margin-bottom: 5px;">
                                        <span style="font-weight: bold; font-size: 14px; color: #333;">${response.userName}</span>
                                        <span style="font-size: 12px; color: #999;">${response.createdDate}</span>
                                    </div>
                                    <div style="font-size: 14px; color: #555; white-space: pre-wrap;">${val}</div>
                                    <div style="font-size: 12px; color: #999; margin-top: 5px; font-weight: 500; cursor: pointer;">
                                        <span class="delete-comment" data-id="${response.id}">Delete</span>
                                    </div>
                                </div>
                            </div>
                        `;

                        var list = $("#task-comments-list");
                        if (list.find(".italic").length) list.empty();

                        list.append(newComment);

                        $("#btn-cancel-comment").click();
                        $btn.prop('disabled', false).text('Save');
                        toastr.success("Comment added!");
                    } else {
                        $btn.prop('disabled', false).text('Save');
                        toastr.error(response.message || "Failed to add comment!");
                    }
                },
                error: function () {
                    $btn.prop('disabled', false).text('Save');
                    toastr.error("An error occurred while adding comment!");
                }
            });
        });

        $(document).on("click", ".delete-comment", function () {
            var commentId = $(this).data("id");
            var commentItem = $(this).closest(".comment-item");

            if (!confirm("Are you sure you want to delete this comment?")) {
                return;
            }

            $.ajax({
                url: '/TaskManagement/DeleteComment',
                type: 'POST',
                data: {
                    id: commentId
                },
                success: function (response) {
                    if (response.isSuccess) {
                        commentItem.fadeOut(300, function () {
                            $(this).remove();

                            if ($("#task-comments-list .comment-item").length === 0) {
                                $("#task-comments-list").html('<div style="font-size: 13px; color: #999; font-style: italic;">No comments yet.</div>');
                            }
                        });
                        toastr.success("Comment deleted!");
                    } else {
                        toastr.error(response.message || "Failed to delete comment!");
                    }
                },
                error: function () {
                    toastr.error("An error occurred while deleting comment!");
                }
            });
        });
    },

    initKeyboardShortcuts: function () {
        $(document).on("keypress", function (e) {
            if (e.which === 109 || e.which === 77) { // 'm' or 'M'
                if (!$(e.target).is('input, textarea')) {
                    e.preventDefault();
                    $("#comment-input").focus();
                }
            }
        });
    },

    updateTaskField: function (fieldName, fieldValue, successCallback, errorCallback) {
        var self = this;

        if (!this.taskId) {
            toastr.error("Task ID not found!");
            if (errorCallback) errorCallback();
            return;
        }

        var data = {
            TaskId: this.taskId
        };
        data[fieldName] = fieldValue;

        $.ajax({
            url: '/TaskManagement/UpdateTaskDetails',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                if (response.isSuccess) {
                    if (successCallback) successCallback();
                } else {
                    toastr.error(response.message || "Failed to update " + fieldName + "!");
                    if (errorCallback) errorCallback();
                }
            },
            error: function () {
                toastr.error("An error occurred while updating " + fieldName + "!");
                if (errorCallback) errorCallback();
            }
        });
    }
};

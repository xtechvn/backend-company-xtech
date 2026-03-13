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

        // Override Magnific Popup focus trap cho TinyMCE
        if ($.magnificPopup && $.magnificPopup.instance) {
            var origOnFocusIn = $.magnificPopup.instance._onFocusIn;
            $.magnificPopup.instance._onFocusIn = function (e) {
                if ($(e.target).closest('.tox-tinymce, .tox-tinymce-aux, .moxman-window, .tam-assetmanager-root, .tox-dialog, .tox-dialog-wrap').length) {
                    return true;
                }
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
            $('#detail-description').val(description || "");

            var useDarkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;

            tinymce.init({
                selector: '#detail-description',
                plugins: 'print preview paste importcss searchreplace autolink autosave save directionality code visualblocks visualchars fullscreen image link media template codesample table charmap hr pagebreak nonbreaking anchor toc insertdatetime advlist lists wordcount imagetools textpattern noneditable help charmap quickbars emoticons',
                imagetools_cors_hosts: ['picsum.photos'],
                menubar: 'file edit view insert format tools table help',
                toolbar: 'undo redo | bold italic underline strikethrough | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | outdent indent |  numlist bullist | forecolor backcolor removeformat | pagebreak | charmap emoticons | fullscreen  preview save print | insertfile image media template link anchor codesample | ltr rtl',
                toolbar_sticky: true,
                autosave_ask_before_unload: false,
                autosave_interval: '30s',
                autosave_prefix: '{path}{query}-{id}-',
                autosave_restore_when_empty: false,
                autosave_retention: '2m',
                image_advtab: true,
                importcss_append: true,
                height: 300,
                image_caption: true,
                quickbars_selection_toolbar: 'bold italic | quicklink h2 h3 blockquote quickimage quicktable',
                noneditable_noneditable_class: 'mceNonEditable',
                toolbar_mode: 'sliding',
                contextmenu: 'link image imagetools table',
                skin: useDarkMode ? 'oxide-dark' : 'oxide',
                content_css: useDarkMode ? 'dark' : 'default',
                content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px } img { max-width: 100%; height: auto; }',

                // ====== CHO PHÉP PASTE ẢNH CHỤP MÀN HÌNH ======
                paste_data_images: true,

                // Xử lý upload ảnh khi paste (chuyển blob thành base64 inline)
                images_upload_handler: function (blobInfo, success, failure) {
                    // Chuyển ảnh paste thành base64 để chèn trực tiếp
                    var base64 = 'data:' + blobInfo.blob().type + ';base64,' + blobInfo.base64();
                    success(base64);
                },

                // Sự kiện khi TinyMCE khởi tạo xong
                setup: function (editor) {
                    editor.on('focus', function () {
                        $("#desc-actions").css("display", "flex");
                    });

                    // Thông báo khi paste ảnh thành công
                    editor.on('PastePostProcess', function (e) {
                        var imgs = e.node.querySelectorAll('img');
                        if (imgs.length > 0) {
                            toastr.info('Đã dán ' + imgs.length + ' ảnh vào mô tả');
                        }
                    });
                }
            });
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
                toastr.error("Tiêu đề không được để trống!");
                return;
            }

            if (newTitle !== self.originalTitle && !self.isSaving) {
                $btn.prop('disabled', true).text('Đang lưu...');

                self.updateTaskField("Title", newTitle, function () {
                    self.originalTitle = newTitle;
                    $("#title-actions").css("display", "none");
                    $btn.prop('disabled', false).text('Lưu');
                    toastr.success("Đã cập nhật tiêu đề!");
                }, function () {
                    $btn.prop('disabled', false).text('Lưu');
                });
            } else {
                $("#title-actions").css("display", "none");
            }
        });
    },

    initDescriptionHandlers: function () {
        var self = this;

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
                    toastr.success("Đã cập nhật người thực hiện!");
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
                    toastr.success("Đã cập nhật người báo cáo!");
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
                toastr.error("Không tìm thấy Task ID!");
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
                        toastr.success("Đã cập nhật trạng thái!");

                        setTimeout(function () {
                            $.magnificPopup.close();
                            location.reload();
                        }, 1000);
                    } else {
                        toastr.error(response.message || "Cập nhật trạng thái thất bại!");
                    }
                },
                error: function () {
                    toastr.error("Đã xảy ra lỗi khi cập nhật trạng thái!");
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
                toastr.error("Không tìm thấy Task ID!");
                return;
            }

            var $btn = $(this);
            $btn.prop('disabled', true).text('Đang lưu...');

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
                        var newComment = '<div class="d-flex mb10 comment-item" style="display: flex; gap: 10px;" data-comment-id="' + response.id + '">'
                            + '<div class="avatar-circle-sm flex-shrink-0" style="width: 32px; height: 32px; font-size: 14px;">U</div>'
                            + '<div style="flex: 1;">'
                            + '<div style="display: flex; align-items: center; gap: 10px; margin-bottom: 5px;">'
                            + '<span style="font-weight: bold; font-size: 14px; color: #333;">' + response.userName + '</span>'
                            + '<span style="font-size: 12px; color: #999;">' + response.createdDate + '</span>'
                            + '</div>'
                            + '<div style="font-size: 14px; color: #555; white-space: pre-wrap;">' + val + '</div>'
                            + '<div style="font-size: 12px; color: #999; margin-top: 5px; font-weight: 500; cursor: pointer;">'
                            + '<span class="delete-comment" data-id="' + response.id + '">Xóa</span>'
                            + '</div>'
                            + '</div>'
                            + '</div>';

                        var list = $("#task-comments-list");
                        if (list.find(".italic").length) list.empty();

                        list.append(newComment);

                        $("#btn-cancel-comment").click();
                        $btn.prop('disabled', false).text('Lưu');
                        toastr.success("Đã thêm bình luận!");
                    } else {
                        $btn.prop('disabled', false).text('Lưu');
                        toastr.error(response.message || "Thêm bình luận thất bại!");
                    }
                },
                error: function () {
                    $btn.prop('disabled', false).text('Lưu');
                    toastr.error("Đã xảy ra lỗi khi thêm bình luận!");
                }
            });
        });

        $(document).on("click", ".delete-comment", function () {
            var commentId = $(this).data("id");
            var commentItem = $(this).closest(".comment-item");

            if (!confirm("Bạn có chắc muốn xóa bình luận này?")) {
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
                                $("#task-comments-list").html('<div style="font-size: 13px; color: #999; font-style: italic;">Chưa có bình luận nào.</div>');
                            }
                        });
                        toastr.success("Đã xóa bình luận!");
                    } else {
                        toastr.error(response.message || "Xóa bình luận thất bại!");
                    }
                },
                error: function () {
                    toastr.error("Đã xảy ra lỗi khi xóa bình luận!");
                }
            });
        });
    },

    initKeyboardShortcuts: function () {
        $(document).on("keypress", function (e) {
            if (e.which === 109 || e.which === 77) { // 'm' or 'M'
                if (!$(e.target).is('input, textarea, [contenteditable]')) {
                    e.preventDefault();
                    $("#comment-input").focus();
                }
            }
        });
    },

    updateTaskField: function (fieldName, fieldValue, successCallback, errorCallback) {
        var self = this;

        if (!this.taskId) {
            toastr.error("Không tìm thấy Task ID!");
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
                    toastr.error(response.message || "Cập nhật " + fieldName + " thất bại!");
                    if (errorCallback) errorCallback();
                }
            },
            error: function () {
                toastr.error("Đã xảy ra lỗi khi cập nhật " + fieldName + "!");
                if (errorCallback) errorCallback();
            }
        });
    }
};

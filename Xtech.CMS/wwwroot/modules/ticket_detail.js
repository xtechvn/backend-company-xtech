// ticket_detail.js - CMS
// SSE thay SignalR + CloseTicket AJAX + xử lý status_changed

var ticketDetail = {
    eventSource: null,
    ticketId: null,
    currentStatus: null, // theo dõi status hiện tại

    init: function () {
        this.ticketId = ($('#TicketId').val() || '').trim();
        this.currentStatus = parseInt($('#CurrentStatus').val() || '0');
        this.initSSE();
        this.renderStatusUI();

        // Ctrl+Enter gửi
        $('#replyEditor').on('keydown', function (e) {
            if (e.ctrlKey && e.key === 'Enter') {
                if (window.replyEditor && replyEditor.beforeSend) replyEditor.beforeSend();
                ticketDetail.sendReply();
                e.preventDefault();
            }
        });
    },

    // =========================================================================
    // SSE
    // =========================================================================
    initSSE: function () {
        this.eventSource = new EventSource('/TicketSSE/Stream?ticketId=' + this.ticketId);

        this.eventSource.onopen = function () {
            console.log('[SSE] CMS connected');
        };

        this.eventSource.onmessage = function (event) {
            try {
                var data = JSON.parse(event.data);

                // ✅ Xử lý status_changed (WebUser reopen)
                if (data.type === 'status_changed') {
                    ticketDetail.handleStatusChanged(data);
                    return;
                }

                // Attachments
                if (data.type === 'attachments') {
                    if (data.messageId && data.attachFiles && data.attachFiles.length)
                        ticketDetail.updateMessageAttachments(data.messageId, data.attachFiles);
                    return;
                }

                // Message thường
                var msg = ticketDetail.normalizeMessage(data);
                if (!msg) return;
                if ((msg.ticketId + '').toLowerCase() !== (ticketDetail.ticketId + '').toLowerCase()) return;
                ticketDetail.appendMessage(msg);

            } catch (e) {
                console.error('[SSE] parse error:', e);
            }
        };

        this.eventSource.onerror = function () {
            console.warn('[SSE] Connection error. Reconnecting...');
        };
    },

    // =========================================================================
    // Xử lý status thay đổi từ SSE
    // =========================================================================
    handleStatusChanged: function (data) {
        ticketDetail.currentStatus = data.status;
        ticketDetail.renderStatusUI();
    },

    // =========================================================================
    // Render UI theo status
    // =========================================================================
    renderStatusUI: function () {
        var isClosed = ticketDetail.currentStatus === 3; // TicketStatus.Closed

        // Badge status
        var badgeHtml = isClosed
            ? '<span class="badge bg-danger">Closed</span>'
            : '<span class="badge bg-success">Open</span>';
        $('#statusBadge').html(badgeHtml);

        // Nút Close/Reopen
        if (isClosed) {
            $('#btnCloseTicket').hide();
            $('#btnReopenTicket').show();
            // Ẩn reply editor khi đã closed
            $('#replySection').hide();
        } else {
            $('#btnCloseTicket').show();
            $('#btnReopenTicket').hide();
            $('#replySection').show();
        }
    },

    // =========================================================================
    // Close Ticket (AJAX) - CMS staff đóng ticket
    // =========================================================================
    closeTicket: function () {
        if (!confirm('Bạn có chắc muốn đóng ticket này không?')) return;

        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: window.ticketEndpoints.changeStatusUrl
                ? '/Ticket/CloseTicket'
                : '/Ticket/CloseTicket',
            type: 'POST',
            data: {
                ticketId: ticketDetail.ticketId,
                __RequestVerificationToken: token
            },
            success: function (res) {
                if (res && res.success) {
                    ticketDetail.currentStatus = 3; // Closed
                    ticketDetail.renderStatusUI();
                } else {
                    alert(res?.message || 'Đóng ticket thất bại');
                }
            },
            error: function (xhr) {
                alert('HTTP ' + xhr.status + ': Close ticket failed');
            }
        });
    },

    // =========================================================================
    // Reopen Ticket (AJAX) - CMS staff mở lại
    // =========================================================================
    reopenTicket: function () {
        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: '/Ticket/ReopenTicket',
            type: 'POST',
            data: {
                ticketId: ticketDetail.ticketId,
                __RequestVerificationToken: token
            },
            success: function (res) {
                if (res && res.success) {
                    ticketDetail.currentStatus = 0; // Open
                    ticketDetail.renderStatusUI();
                } else {
                    alert(res?.message || 'Mở lại ticket thất bại');
                }
            },
            error: function (xhr) {
                alert('HTTP ' + xhr.status + ': Reopen ticket failed');
            }
        });
    },

    // =========================================================================
    // Send Reply (AJAX)
    // =========================================================================
    sendReply: function () {
        var content = ($('#txtReplyContent').val() || '').trim();
        var files = window.__replyFiles || [];

        if (!content && (!files || files.length === 0)) {
            alert('Vui lòng nhập nội dung hoặc đính kèm file.');
            return;
        }

        var fd = new FormData();
        fd.append('TicketId', this.ticketId);
        fd.append('ContentHtml', content);
        fd.append('Content', content);
        fd.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());

        if (files && files.length)
            for (var i = 0; i < files.length; i++) fd.append('AttachFiles', files[i]);

        $.ajax({
            url: window.ticketEndpoints.replyUrl,
            type: 'POST',
            data: fd,
            processData: false,
            contentType: false,
            success: function (res) {
                if (res && res.success) {
                    if (res.data) {
                        var msg = ticketDetail.normalizeMessage(res.data);
                        if (msg) ticketDetail.appendMessage(msg);
                    }
                    if (window.replyEditor && replyEditor.reset) replyEditor.reset();
                    window.__replyFiles = [];
                    $('#txtReplyContent').val('');
                } else {
                    alert(res?.message || 'Gửi trả lời thất bại');
                }
            },
            error: function (xhr) {
                alert('HTTP ' + xhr.status + ': ' + (xhr.responseText || 'Yêu cầu thất bại'));
            }
        });
    },

    // =========================================================================
    // Append Message
    // =========================================================================
    appendMessage: function (m) {
        if (!m) return;
        if (m.id && $('#msg-' + m.id).length > 0) return;

        var senderType = (m.senderType || '').toLowerCase();
        var isStaff = senderType === 'agent' || senderType === 'staff';
        var senderTitle = isStaff ? ('Staff - ' + (m.senderId || '')) : 'Customer';
        var contentHtml = m.contentHtml
            ? m.contentHtml
            : this.escapeHtml(m.content || '').replace(/\n/g, '<br/>');

        var card = `
      <div class="card shadow-sm mb-4" ${m.id ? `id="msg-${m.id}"` : ''}>
        <div class="card-header d-flex justify-content-between align-items-center bg-body-tertiary">
          <div class="d-flex align-items-center gap-3">
            ${isStaff
                ? `<div class="avatar bg-secondary-subtle text-secondary overflow-hidden"><i class="bi bi-person-fill"></i></div>`
                : `<div class="avatar avatar-primary">C</div>`}
            <div>
              <div class="fw-bold small mb-0">${this.escapeHtml(senderTitle)}</div>
              <div class="text-secondary" style="font-size:10px;">${this.escapeHtml(m.createdAt || '')}</div>
            </div>
          </div>
          ${isStaff ? `<span class="badge bg-secondary-subtle text-secondary fw-bold" style="font-size:10px;">STAFF</span>` : ''}
        </div>
        <div class="card-body">
          <div class="text-secondary-emphasis mb-3">${contentHtml}</div>
          ${this.renderAttachments(m.attachFiles)}
        </div>
      </div>`;

        $('#chatContainer').append(card);
        window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
    },

    // Thông báo hệ thống (status changed...)
    appendSystemMessage: function (text) {
        var html = `
        <div class="text-center my-3">
            <span class="badge bg-light text-secondary border px-3 py-2" style="font-size:12px;">
                ${text}
            </span>
        </div>`;
        $('#chatContainer').append(html);
        window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
    },

    updateMessageAttachments: function (messageId, files) {
        var $msg = $('#msg-' + messageId);
        if ($msg.length === 0) return;
        $msg.find('.attachments').remove();
        $msg.find('.card-body').append(this.renderAttachments(files));
    },

    renderAttachments: function (files) {
        if (!files || !files.length) return '';
        return `<div class="attachments mt-3">${files.map(f => this.renderAttachmentItem(f)).join('')}</div>`;
    },

    renderAttachmentItem: function (file) {
        var name = file.Name || file.name || '';
        var url = file.Url || file.url || '#';
        var lower = (name || url).toLowerCase();
        var isImg = lower.endsWith('.jpg') || lower.endsWith('.jpeg') ||
            lower.endsWith('.png') || lower.endsWith('.gif') || lower.endsWith('.webp');

        if (isImg) return `<a class="att-img" href="${url}" target="_blank" rel="noopener"><img src="${url}" alt="${this.escapeHtml(name)}"/></a>`;

        var ext = name ? (name.split('.').pop() || '').toUpperCase() : 'FILE';
        return `<a class="att-file" href="${url}" download><div class="meta"><span class="ext">${ext}</span><span class="name">${this.escapeHtml(name || url)}</span></div><i class="bi bi-download"></i></a>`;
    },

    normalizeMessage: function (m) {
        if (!m) return null;
        var ticketId = m.ticketId || m.TicketId;
        if (!ticketId) return null;
        return {
            id: m.id || m.Id || null,
            ticketId: ticketId,
            senderType: m.senderType || m.SenderType || '',
            senderId: m.senderId || m.SenderId || '',
            content: m.content || m.Content || '',
            contentHtml: m.contentHtml || m.ContentHtml || '',
            createdAt: m.createdAt || m.CreatedAt || '',
            attachFiles: m.attachFiles || m.AttachFiles || []
        };
    },

    escapeHtml: function (s) {
        if (!s) return '';
        return (s + '').replace(/[&<>"']/g, c =>
            ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' })[c]);
    }
};

$(document).ready(function () {
    ticketDetail.init();
});
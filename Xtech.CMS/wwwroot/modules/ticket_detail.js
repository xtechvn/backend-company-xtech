// ticket_detail.js - CMS
// Thay SignalR bằng SSE (Server-Sent Events)
// Không còn phụ thuộc vào hubUrl, không bị Mixed Content

var ticketDetail = {
    eventSource: null,
    ticketId: null,

    init: function () {
        this.ticketId = ($('#TicketId').val() || '').trim();
        this.initSSE();

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
    // SSE: thay thế hoàn toàn SignalR
    // Kết nối tới /Ticket/Stream?ticketId=xxx (cùng domain CMS)
    // =========================================================================
    initSSE: function () {
        // SSE chỉ dùng HTTP GET thuần, không cần WebSocket
        // Cùng domain be.x-tech.vn => không bao giờ bị Mixed Content
        this.eventSource = new EventSource(
            '/TicketSSE/Stream?ticketId=' + this.ticketId
        );

        this.eventSource.onopen = function () {
            console.log('[SSE] Connected');
        };

        this.eventSource.onmessage = function (event) {
            try {
                var data = JSON.parse(event.data);

                // Phân biệt message thường và attachments
                if (data.type === 'attachments') {
                    if (data.messageId && data.attachFiles && data.attachFiles.length) {
                        ticketDetail.updateMessageAttachments(data.messageId, data.attachFiles);
                    }
                    return;
                }

                // Message thường
                var msg = ticketDetail.normalizeMessage(data);
                if (!msg) return;

                // Chỉ xử lý message thuộc ticket này
                if ((msg.ticketId + '').toLowerCase() !== (ticketDetail.ticketId + '').toLowerCase()) return;

                ticketDetail.appendMessage(msg);

            } catch (e) {
                console.error('[SSE] parse error:', e);
            }
        };

        this.eventSource.onerror = function () {
            console.warn('[SSE] Connection error. Browser sẽ tự reconnect...');
            // EventSource tự reconnect (built-in browser behavior)
        };
    },

    // =========================================================================
    // Send Reply (AJAX - không đổi)
    // =========================================================================
    sendReply: function () {
        var content = ($('#txtReplyContent').val() || '').trim();
        var files = window.__replyFiles || [];

        if (!content && (!files || files.length === 0)) {
            alert('Please enter content or attach file.');
            return;
        }

        var fd = new FormData();
        fd.append('TicketId', this.ticketId);
        fd.append('ContentHtml', content);
        fd.append('Content', content);
        fd.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());

        if (files && files.length) {
            for (var i = 0; i < files.length; i++) {
                fd.append('AttachFiles', files[i]);
            }
        }

        $.ajax({
            url: window.ticketEndpoints.replyUrl,
            type: 'POST',
            data: fd,
            processData: false,
            contentType: false,
            success: function (res) {
                if (res && res.success) {
                    // Append ngay từ HTTP response (không chờ SSE để tránh duplicate)
                    if (res.data) {
                        var msg = ticketDetail.normalizeMessage(res.data);
                        if (msg) ticketDetail.appendMessage(msg);
                    }

                    if (window.replyEditor && replyEditor.reset) replyEditor.reset();
                    window.__replyFiles = [];
                    $('#txtReplyContent').val('');
                } else {
                    alert(res?.message || 'Reply failed');
                }
            },
            error: function (xhr) {
                var msg = xhr?.responseJSON?.message || ('HTTP ' + xhr.status + ': ' + (xhr.responseText || 'Request failed'));
                alert(msg);
            }
        });
    },

    // =========================================================================
    // Append Message
    // =========================================================================
    appendMessage: function (m) {
        if (!m) return;

        // Dedup: nếu message đã có trong DOM thì bỏ qua
        if (m.id && $('#msg-' + m.id).length > 0) return;

        var senderType = (m.senderType || '').toLowerCase();
        var isStaff = senderType === 'agent' || senderType === 'staff';
        var senderTitle = isStaff ? ('Staff - ' + (m.senderId || '')) : 'Customer';
        var createdAt = m.createdAt || '';
        var contentHtml = m.contentHtml
            ? m.contentHtml
            : this.escapeHtml(m.content || '').replace(/\n/g, '<br/>');

        var attHtml = this.renderAttachments(m.attachFiles);

        var card = `
      <div class="card shadow-sm mb-4" ${m.id ? `id="msg-${m.id}"` : ''}>
        <div class="card-header d-flex justify-content-between align-items-center bg-body-tertiary">
          <div class="d-flex align-items-center gap-3">
            ${isStaff
                ? `<div class="avatar bg-secondary-subtle text-secondary overflow-hidden"><i class="bi bi-person-fill"></i></div>`
                : `<div class="avatar avatar-primary">C</div>`}
            <div>
              <div class="fw-bold small mb-0">${this.escapeHtml(senderTitle)}</div>
              <div class="text-secondary" style="font-size:10px;">${this.escapeHtml(createdAt)}</div>
            </div>
          </div>
          ${isStaff
                ? `<span class="badge bg-secondary-subtle text-secondary fw-bold" style="font-size:10px;">STAFF</span>`
                : ''}
        </div>
        <div class="card-body">
          <div class="text-secondary-emphasis mb-3">${contentHtml}</div>
          ${attHtml}
        </div>
      </div>`;

        $('#chatContainer').append(card);
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
        return `
      <div class="attachments mt-3">
        ${files.map(f => this.renderAttachmentItem(f)).join('')}
      </div>`;
    },

    renderAttachmentItem: function (file) {
        var name = file.Name || file.name || '';
        var url = file.Url || file.url || '#';
        var lower = (name || url).toLowerCase();

        var isImg = lower.endsWith('.jpg') || lower.endsWith('.jpeg') ||
            lower.endsWith('.png') || lower.endsWith('.gif') ||
            lower.endsWith('.webp');

        if (isImg) {
            return `
        <a class="att-img" href="${url}" target="_blank" rel="noopener">
          <img src="${url}" alt="${this.escapeHtml(name)}"/>
        </a>`;
        }

        var ext = '';
        if (name) { var parts = name.split('.'); ext = (parts.length > 1 ? parts.pop() : '').toUpperCase(); }

        return `
      <a class="att-file" href="${url}" download>
        <div class="meta">
          <span class="ext">${ext || 'FILE'}</span>
          <span class="name">${this.escapeHtml(name || url)}</span>
        </div>
        <i class="bi bi-download"></i>
      </a>`;
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
        return (s + '').replace(/[&<>"']/g, function (c) {
            return ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' })[c];
        });
    }
};

$(document).ready(function () {
    ticketDetail.init();
});
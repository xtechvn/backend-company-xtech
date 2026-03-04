var ticketDetail = {
    connection: null,
    ticketId: null,

    init: function () {
        this.ticketId = ($('#TicketId').val() || '').trim();
        this.initSignalR();

        // Ctrl + Enter gửi
        $('#replyEditor').on('keydown', function (e) {
            if (e.ctrlKey && e.key === 'Enter') {
                if (window.replyEditor && replyEditor.beforeSend) replyEditor.beforeSend();
                ticketDetail.sendReply();
                e.preventDefault();
            }
        });
    },

    initSignalR: function () {
        if (typeof signalR === 'undefined') {
            console.warn('SignalR client not loaded. Realtime disabled.');
            return;
        }

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(window.ticketEndpoints.hubUrl)
            .withAutomaticReconnect()
            .build();

        // message event
        this.connection.on('ReceiveMessage', function (m) {
            var msg = ticketDetail.normalizeMessage(m);
            if (!msg) return;

            if ((msg.ticketId + '').toLowerCase() !== (ticketDetail.ticketId + '').toLowerCase()) return;

            ticketDetail.appendMessage(msg);
        });

        // attachments event (đến sau message)
        this.connection.on('ReceiveAttachments', function (p) {
            if (!p) return;
            var messageId = p.messageId || p.MessageId || p.id || p.Id;
            var files = p.attachFiles || p.AttachFiles || [];
            if (!messageId || !files.length) return;

            ticketDetail.updateMessageAttachments(messageId, files);
        });

        this.connection.start()
            .then(() => this.connection.invoke('JoinTicket', ticketDetail.ticketId))
            .catch(err => console.error('SignalR connect error:', err));
    },

    // ===== Send Reply (AJAX) =====
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

        // anti-forgery
        fd.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());

        // attachments
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
                    // ✅ append ngay theo response (khỏi phụ thuộc realtime)
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

    // ===== Append Message =====
    appendMessage: function (m) {
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
          ${isStaff ? `<span class="badge bg-secondary-subtle text-secondary fw-bold" style="font-size:10px;">STAFF</span>` : ``}
        </div>

        <div class="card-body">
          <div class="text-secondary-emphasis mb-3">${contentHtml}</div>
          ${attHtml}
        </div>
      </div>
    `;

        $('#chatContainer').append(card);
        window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
    },

    // ===== Update attachments for an existing message =====
    updateMessageAttachments: function (messageId, files) {
        var $msg = $('#msg-' + messageId);
        if ($msg.length === 0) return;

        $msg.find('.attachments').remove();
        $msg.find('.card-body').append(this.renderAttachments(files));
    },

    // ===== Render attachments =====
    renderAttachments: function (files) {
        if (!files || !files.length) return '';

        return `
      <div class="attachments mt-3">
        ${files.map(f => this.renderAttachmentItem(f)).join('')}
      </div>
    `;
    },

    renderAttachmentItem: function (file) {
        var name = file.Name || file.name || '';
        var url = file.Url || file.url || '#';
        var lower = (name || url).toLowerCase();

        var isImg =
            lower.endsWith('.jpg') || lower.endsWith('.jpeg') ||
            lower.endsWith('.png') || lower.endsWith('.gif') ||
            lower.endsWith('.webp');

        if (isImg) {
            return `
        <a class="att-img" href="${url}" target="_blank" rel="noopener">
          <img src="${url}" alt="${this.escapeHtml(name)}"/>
        </a>
      `;
        }

        var ext = '';
        if (name) {
            var parts = name.split('.');
            ext = (parts.length > 1 ? parts.pop() : '').toUpperCase();
        }

        return `
      <a class="att-file" href="${url}" download>
        <div class="meta">
          <span class="ext">${ext || 'FILE'}</span>
          <span class="name">${this.escapeHtml(name || url)}</span>
        </div>
        <i class="bi bi-download"></i>
      </a>
    `;
    },

    // ===== Normalize =====
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
    },

    // stub: bạn có closeTicket thì implement ở đây
    closeTicket: function (ticketId) {
        alert('TODO: closeTicket ' + ticketId);
    }
};

$(document).ready(function () {
    ticketDetail.init();
});
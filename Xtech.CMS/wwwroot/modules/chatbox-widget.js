
    (function () {
  // ======================== CONFIG ========================
  const tenant_id = "01JNGDB23ZF2MNEN4GCX8WSZ21";
    const chatIconUrl = "https://static-image.adavigo.com/uploads/icons/icon-chat.png";
    const avatarUrl = "https://static-image.adavigo.com/uploads/icons/avatar.png";

    // ======================== STYLE =========================
    const css = `
    * {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
    font-family: Arial, sans-serif
}

.widget-chat .chat-circle {
    position: fixed;
    bottom: 20px;
    right: 20px;
    color: #fff;
    cursor: pointer;
    z-index: 1
}

.widget-chat {
    font-size: 14px
}

.widget-chat .chat-box {
    display: none;
    position: fixed;
    right: 30px;
    width: 400px;
    z-index: 1;
    height: 600px;
    max-width: calc(100% - 30px);
    max-height: 100vh;
    bottom: 30px;
    border-radius: 10px;
    border: 1px solid #E3EBF3;
    background: #FFF;
    box-shadow: 0 4px 34px 0 rgb(0 0 0 / .15);
    border-radius: 5px
}

@media (max-width:767px) {
    .widget-chat .chat-box {
        right: 10px
    }
}

.widget-chat .chat-box-toggle {
    cursor: pointer;
    content: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"><path d="M21.375 12C21.375 12.2984 21.2565 12.5845 21.0455 12.7955C20.8345 13.0065 20.5484 13.125 20.25 13.125H3.75C3.45163 13.125 3.16548 13.0065 2.9545 12.7955C2.74353 12.5845 2.625 12.2984 2.625 12C2.625 11.7016 2.74353 11.4155 2.9545 11.2045C3.16548 10.9935 3.45163 10.875 3.75 10.875H20.25C20.5484 10.875 20.8345 10.9935 21.0455 11.2045C21.2565 11.4155 21.375 11.7016 21.375 12Z" fill="%23ffffff"/></svg>')
}

.widget-chat .chat-box-header {
    background: linear-gradient(90deg, #070BA0 26.71%, #6B6FFE 102.09%);
    border-top-left-radius: 5px;
    border-top-right-radius: 5px;
    color: #fff;
    font-size: 16px;
    height: 40px;
    padding: 7px;
    display: flex;
    align-items: center;
    justify-content: space-between
}

.widget-chat .chat-input-name {
    text-align: center;
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: center;
    height: calc(100% - 50px);
    padding: 20px
}

.widget-chat .chat-box-body {
    position: relative;
    height: calc(100% - 40px);
    padding-bottom: 50px;
    border: 1px solid #ccc;
    overflow: hidden
}

.widget-chat .chat-box-body:after {
    content: "";
    background-image: url(data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgdmlld0JveD0iMCAwIDIwMCAyMDAiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+PGcgdHJhbnNmb3JtPSJ0cmFuc2xhdGUoMTAgOCkiIGZpbGw9Im5vbmUiIGZpbGwtcnVsZT0iZXZlbm9kZCI+PGNpcmNsZSBzdHJva2U9IiMwMDAiIHN0cm9rZS13aWR0aD0iMS4yNSIgY3g9IjE3NiIgY3k9IjEyIiByPSI0Ii8+PHBhdGggZD0iTTIwLjUuNWwyMyAxMW0tMjkgODRsLTMuNzkgMTAuMzc3TTI3LjAzNyAxMzEuNGw1Ljg5OCAyLjIwMy0zLjQ2IDUuOTQ3IDYuMDcyIDIuMzkyLTMuOTMzIDUuNzU4bTEyOC43MzMgMzUuMzdsLjY5My05LjMxNiAxMC4yOTIuMDUyLjQxNi05LjIyMiA5LjI3NC4zMzJNLjUgNDguNXM2LjEzMSA2LjQxMyA2Ljg0NyAxNC44MDVjLjcxNSA4LjM5My0yLjUyIDE0LjgwNi0yLjUyIDE0LjgwNk0xMjQuNTU1IDkwcy03LjQ0NCAwLTEzLjY3IDYuMTkyYy02LjIyNyA2LjE5Mi00LjgzOCAxMi4wMTItNC44MzggMTIuMDEybTIuMjQgNjguNjI2cy00LjAyNi05LjAyNS0xOC4xNDUtOS4wMjUtMTguMTQ1IDUuNy0xOC4xNDUgNS43IiBzdHJva2U9IiMwMDAiIHN0cm9rZS13aWR0aD0iMS4yNSIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+PHBhdGggZD0iTTg1LjcxNiAzNi4xNDZsNS4yNDMtOS41MjFoMTEuMDkzbDUuNDE2IDkuNTIxLTUuNDEgOS4xODVIOTAuOTUzbC01LjIzNy05LjE4NXptNjMuOTA5IDE1LjQ3OWgxMC43NXYxMC43NWgtMTAuNzV6IiBzdHJva2U9IiMwMDAiIHN0cm9rZS13aWR0aD0iMS4yNSIvPjxjaXJjbGUgZmlsbD0iIzAwMCIgY3g9IjcxLjUiIGN5PSI3LjUiIHI9IjEuNSIvPjxjaXJjbGUgZmlsbD0iIzAwMCIgY3g9IjE3MC41IiBjeT0iOTUuNSIgcj0iMS41Ii8+PGNpcmNsZSBmaWxsPSIjMDAwIiBjeD0iODEuNSIgY3k9IjEzNC41IiByPSIxLjUiLz48Y2lyY2xlIGZpbGw9IiMwMDAiIGN4PSIxMy41IiBjeT0iMjMuNSIgcj0iMS41Ii8+PHBhdGggZmlsbD0iIzAwMCIgZD0iTTkzIDcxaDN2M2gtM3ptMzMgODRoM3YzaC0zem0tODUgMThoM3YzaC0zeiIvPjxwYXRoIGQ9Ik0zOS4zODQgNTEuMTIybDUuNzU4LTQuNDU0IDYuNDUzIDQuMjA1LTIuMjk0IDcuMzYzaC03Ljc5bC0yLjEyNy03LjExNHpNMTMwLjE5NSA0LjAzbDEzLjgzIDUuMDYyLTEwLjA5IDcuMDQ4LTMuNzQtMTIuMTF6bS04MyA5NWwxNC44MyA1LjQyOS0xMC44MiA3LjU1Ny00LjAxLTEyLjk4N3pNNS4yMTMgMTYxLjQ5NWwxMS4zMjggMjAuODk3TDIuMjY1IDE4MGwyLjk0OC0xOC41MDV6IiBzdHJva2U9IiMwMDAiIHN0cm9rZS13aWR0aD0iMS4yNSIvPjxwYXRoIGQ9Ik0xNDkuMDUgMTI3LjQ2OHMtLjUxIDIuMTgzLjk5NSAzLjM2NmMxLjU2IDEuMjI2IDguNjQyLTEuODk1IDMuOTY3LTcuNzg1LTIuMzY3LTIuNDc3LTYuNS0zLjIyNi05LjMzIDAtNS4yMDggNS45MzYgMCAxNy41MSAxMS42MSAxMy43MyAxMi40NTgtNi4yNTcgNS42MzMtMjEuNjU2LTUuMDczLTIyLjY1NC02LjYwMi0uNjA2LTE0LjA0MyAxLjc1Ni0xNi4xNTcgMTAuMjY4LTEuNzE4IDYuOTIgMS41ODQgMTcuMzg3IDEyLjQ1IDIwLjQ3NiAxMC44NjYgMy4wOSAxOS4zMzEtNC4zMSAxOS4zMzEtNC4zMSIgc3Ryb2tlPSIjMDAwIiBzdHJva2Utd2lkdGg9IjEuMjUiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPjwvZz48L3N2Zz4=);
    opacity: .1;
    top: 0;
    left: 0;
    bottom: 0;
    right: 0;
    height: 100%;
    position: absolute;
    z-index: -1
}

.widget-chat .chat-input-name h2 {
    margin: 20px 0
}

.widget-chat .box-name input {
    border-radius: 10px;
    border: 1px solid #E3EBF3;
    background: #F1F5F9;
    color: #262626;
    font-size: 14px;
    font-weight: 400;
    width: 100%;
    padding: 8px;
    line-height: 130%;
    margin-top: 10px;
    text-align: center;
    outline: 0 !important
}

.widget-chat .box-name .btn-box {
    border-radius: 10px;
    background: linear-gradient(90deg, #070BA0 26.71%, #6B6FFE 102.09%);
    text-align: center;
    padding: 9px;
    color: #fff;
    border: 0;
    outline: 0;
    font-size: 14px;
    display: block;
    margin-top: 8px;
    line-height: 130%;
    width: 100%
}

.widget-chat .chat-input {
    width: 100%;
    bottom: 0;
    position: absolute;
    height: 50px;
    font-size: 14px;
    padding-top: 10px;
    padding-right: 50px;
    padding-bottom: 10px;
    padding-left: 15px;
    border: none;
    resize: none;
    outline: none;
    color: #262626;
    font-weight: 400;
    border-top: none;
    border-bottom-right-radius: 5px;
    border-bottom-left-radius: 5px;
    overflow: hidden
}

.widget-chat .chat-input>form {
    margin-bottom: 0
}

.widget-chat .chat-input::-webkit-input-placeholder {
    color: #ccc
}

.widget-chat .chat-input::-moz-placeholder {
    color: #ccc
}

.widget-chat .chat-input:-ms-input-placeholder {
    color: #ccc
}

.widget-chat .chat-input:-moz-placeholder {
    color: #ccc
}

.widget-chat .chat-submit {
    position: absolute;
    z-index: 1;
    bottom: 14px;
    right: 10px;
    box-shadow: none;
    border: none;
    border-radius: 50%;
    color: #5A5EB9;
    width: 25px;
    cursor: pointer;
    height: 25px;
    background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"><path d="M22.4999 11.9897C22.5006 12.2569 22.4298 12.5195 22.295 12.7502C22.1602 12.981 21.9661 13.1715 21.733 13.3022L5.99049 22.3031C5.76458 22.4311 5.50954 22.4989 5.24987 22.5C5.01061 22.4987 4.77513 22.4402 4.56308 22.3293C4.35104 22.2185 4.16859 22.0586 4.03095 21.8628C3.89332 21.6671 3.8045 21.4413 3.77191 21.2043C3.73932 20.9673 3.76391 20.7259 3.84362 20.5003L6.37487 13.005C6.3996 12.9317 6.4464 12.8679 6.50883 12.8222C6.57126 12.7766 6.64628 12.7513 6.72362 12.75H13.4999C13.6027 12.7502 13.7044 12.7293 13.7988 12.6885C13.8932 12.6478 13.9782 12.588 14.0486 12.513C14.1189 12.438 14.1731 12.3494 14.2077 12.2526C14.2423 12.1558 14.2567 12.0529 14.2499 11.9503C14.2329 11.7574 14.1436 11.5781 14 11.4482C13.8564 11.3184 13.6691 11.2476 13.4755 11.25H6.72549C6.64703 11.25 6.57055 11.2254 6.50681 11.1796C6.44307 11.1339 6.39528 11.0693 6.37018 10.995L3.83893 3.5006C3.73818 3.21334 3.72721 2.90223 3.80749 2.60859C3.88777 2.31495 4.05548 2.05269 4.28836 1.85664C4.52124 1.66059 4.80825 1.54004 5.11128 1.51099C5.4143 1.48195 5.71899 1.54579 5.98487 1.69403L21.7349 10.6837C21.9667 10.814 22.1597 11.0036 22.2941 11.2331C22.4286 11.4626 22.4996 11.7237 22.4999 11.9897Z" fill="%23B7CCD9"/></svg>') no-repeat center/contain
}

.widget-chat .chat-logs {
    padding: 15px;
    max-height: 100%;
    overflow-y: auto
}

.widget-chat .chat-logs::-webkit-scrollbar-track {
    background-color: #fff
}

.widget-chat .chat-logs::-webkit-scrollbar {
    width: 5px;
    background-color: #eee
}

.widget-chat .chat-logs::-webkit-scrollbar-thumb {
    background-color: #eee
}

.widget-chat .chat-msg {
    display: flex;
    align-items: start;
    gap: 10px;
    margin-bottom: 20px
}

.widget-chat .chat-msg.user>.msg-avatar img {
    width: 30px;
    height: 30px;
    object-fit: contain;
    flex-shrink: 0
}

.widget-chat .chat-msg.self>.msg-avatar img {
    width: 30px;
    height: 30px;
    object-fit: contain;
    flex-shrink: 0
}

.widget-chat .chat-msg.self>.msg-avatar {
    order: 1
}

.widget-chat .chat-msg.self {
    justify-content: end
}

.widget-chat .cm-msg-text {
    background: #f4f4f5;
    padding: 5px 10px;
    color: #262626;
    max-width: 75%;
    position: relative;
    line-height: 160%;
    border-radius: 10px
}

.widget-chat .chat-msg {
    clear: both
}

.widget-chat .chat-msg.self>.cm-msg-text {
    background: linear-gradient(90deg, #070BA0 26.71%, #6B6FFE 102.09%);
    color: #fff
}

.widget-chat .cm-msg-button>ul>li {
    list-style: none;
    float: left;
    width: 50%
}

.widget-chat .cm-msg-button {
    clear: both;
    margin-bottom: 70px
}

.widget-chat .tab-select {
    display: block;
    margin-top: 8px
}

.widget-chat .tab-select li {
    display: block;
    padding: 5px;
    border-radius: 6px;
    border: 1px solid #E3EBF3;
    background: #FFF;
    color: #070BA0;
    cursor: pointer
}

.widget-chat .tab-select li:not(:last-child) {
    margin-bottom: 8px
}

.hint-chat {
    display: inline-block;
    color: #fff;
    position: fixed;
    bottom: 30px;
    right: 90px;
    cursor: pointer;
    align-items: center;
    padding: 15px 30px;
    background: linear-gradient(90deg, #070BA0 26.71%, #6B6FFE 102.09%);
    gap: 0.5rem;
    border-radius: 9999px;
    animation: 1.6s ease 0s infinite normal none running moveLeftAndBack-6870bd72;
}

@keyframes moveLeftAndBack-6870bd72 {
    0% {
        transform: translate(0)
    }

    50% {
        transform: translate(-16px)
    }

    to {
        transform: translate(0)
    }
}

.greeting[data-v-6870bd72] {
    display: flex;
    cursor: pointer;
    align-items: center;
    gap: .5rem;
    border-radius: 9999px;
    padding-top: .625rem;
    padding-bottom: .625rem;
    animation: moveLeftAndBack-6870bd72 1.6s infinite
}
.widget-chat .chat-box,
.widget-chat .chat-circle,
.widget-chat .hint-chat {
    z-index: 9999 !important;
    position: fixed !important;
}
.typing-dots {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    height: 18px;
    gap: 4px;
}

    .typing-dots span {
        display: block;
        width: 6px;
        height: 6px;
        background-color: #999;
        border-radius: 50%;
        animation: bounce 1.2s infinite ease-in-out;
    }

        .typing-dots span:nth-child(2) {
            animation-delay: 0.2s;
        }

        .typing-dots span:nth-child(3) {
            animation-delay: 0.4s;
        }

@keyframes bounce {
    0%, 80%, 100% {
        transform: translateY(0);
        opacity: 0.5;
    }

    40% {
        transform: translateY(-6px);
        opacity: 1;
    }
}

    `;
    const style = document.createElement("style");
    style.innerHTML = css;
    document.head.appendChild(style);

    // ======================== HTML ==========================
    const html = `
    <div class="widget-chat">
        <div id="body">
            <span class="hint-chat">Tôi có thể giúp gì cho bạn ?</span>

            <div id="chat-circle" class="btn btn-raised chat-circle">
                <img src="${chatIconUrl}" width="60" alt="Chat" />
            </div>

            <div class="chat-box">
                <div class="chat-box-header">
                    ChatBot
                    <span class="chat-box-toggle"></span>
                </div>

                <div class="chat-box-body">
                    <div class="chat-box-overlay"></div>

                    <!-- Vùng hiển thị các tin nhắn -->
                    <div class="chat-logs">
                        <!-- Tin nhắn từ AI (bot) -->
                        <div class="chat-msg user">
                            <span class="msg-avatar">
                                <img src="${chatIconUrl}">
                            </span>
                            <div class="cm-msg-text">
                                Chào mừng bạn đến với vRace! Tôi là vRace Assistant và nhiệm vụ của tôi là hỗ trợ giải đáp các thắc mắc về quy định, quy chế giải chạy trên vRace. Đồng thời tôi cũng sẵn sàng giúp đỡ bạn xử lý vấn đề gặp phải trong quá trình sử dụng dịch vụ của VRace. Hãy chia sẻ với tôi bất kỳ câu hỏi nào bạn có và tôi sẽ cố gắng hết sức để trả lời cho bạn.
                                <ul class="tab-select">
                                    <li>Thông tin tour</li>
                                    <li>Thời gian chạy</li>
                                    <li>Hướng dẫn đăng ký</li>
                                </ul>
                            </div>
                        </div>
                    </div>

                    <!-- Ô nhập tin nhắn -->
                    <div class="chat-input-block">
                        <form id="chat-form">
                            <input type="text" id="chat-input" class="chat-input" placeholder="Nhập tin nhắn...">
                            <button type="submit" class="chat-submit" id="chat-submit"></button>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
    `;
    const container = document.createElement("div");
    container.innerHTML = html;
    document.body.appendChild(container);

    // ======================== JS ============================
        $("#chat-circle, .chat-box-toggle").click(function () {
            $("#chat-circle").toggle('scale');
            $(".chat-box").toggle('scale');
            $(".hint-chat").toggle('scale');
        });

        // const tenantMap = {
        //     "xtech.vn": "01JNGDB23ZF2MNEN4GCX8WSZ21",
        //     "abc.vn": "TENANT_ABC_01",
        //     "mytravel.vn": "TENANT_TRAVEL_03"
        // };

        const currentDomain = window.location.hostname;
        console.log(currentDomain);
        // const tenant_id = tenantMap[currentDomain] || "TENANT_DEFAULT";

        // Gửi bằng nút Submit
        $("#chat-form").submit(function (e) {
            e.preventDefault();

            const msg = $("#chat-input").val().trim();
            if (!msg) return;

            $("#chat-input").val(""); // 👉 Xoá input ngay khi gửi
            sendMessage(msg);
        });

        // Gửi khi click vào các tab gợi ý
        $(document).on("click", ".tab-select li", function () {
            const text = $(this).text().trim();
            $("#chat-input").val(""); // 👉 Xoá input nếu đang gõ dở
            sendMessage(text);
        });

        // Hàm xử lý gửi + hiển thị
        function sendMessage(msg) {
            // Hiển thị tin nhắn người dùng
            $(".chat-logs").append(`
                <div class="chat-msg self">
                    <div class="cm-msg-text">${msg}</div>
                </div>
            `);
            $(".chat-logs").scrollTop($(".chat-logs")[0].scrollHeight);

            // Thêm hiệu ứng 3 chấm động vào chat logs
            const typingId = `typing-${Date.now()}`;
            $(".chat-logs").append(`
            <div class="chat-msg user typing" id="${typingId}">
                <span class="msg-avatar">
                    <img src="/css/images/icons/icon-chat.png">
                </span>
                <div class="cm-msg-text">
                    <div class="typing-dots">
                        <span></span><span></span><span></span>
                    </div>
                </div>
            </div>
        `);
            $(".chat-logs").scrollTop($(".chat-logs")[0].scrollHeight);


            // Gửi webhook
            const payload = [{
                id: Date.now(),
                domain: window.location.hostname,
                title: msg
            }];

            $.ajax({
                url: "https://n8n.adavigo.com/webhook/sync-client",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(payload),
                success: function (res) {

                    $(`#${typingId}`).remove();

                    if (res && res.msg) {
                        $(".chat-logs").append(`
                    <div class="chat-msg user">
                        <span class="msg-avatar">
                            <img src="/css/images/icons/icon-chat.png">
                        </span>
                        <div class="cm-msg-text">${res.msg}</div>
                    </div>
                `);
                        $(".chat-logs").scrollTop($(".chat-logs")[0].scrollHeight);
                    }


                },
                error: function (xhr, status, err) {
                    clearInterval(typingInterval);
                    $(`#${typingId}`).remove();
                    console.error("❌ Lỗi gửi:", err);
                }
            });
        }
})();


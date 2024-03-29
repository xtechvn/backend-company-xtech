
let fields = {
    STT: true,
    Token: true,
    GroupLog: true,
    GroupChatId: true,
    CreateDate: true,
    ProjectType: true,
    Status: true
}
let cookieName = 'fields_bottelegram'
var timer;
$(document).ready(function () {
    _Telegrammanagement.Loaddata();
});
var _Telegrammanagement = {

    Loaddata: function () {
        let _searchModel = {
            
        };
        var objSearch = this.SearchParam;

        objSearch = _searchModel;

        this.Search(objSearch);
    },
    Submit: function () {
        var token = $('#token').val();
        var groupChatid = $('#groupChatId').val();
        if (token.trim() == "") {
            _msgalert.error('Token được để trống.');
            return;
        }

        if (groupChatid.trim() == "") {
            _msgalert.error('Group id không được để trống.');
            return;
        }
        var data={
            token: ($('#token').val()),
            groupid: ($('#groupChatId').val()),
        }
        let dataapi = JSON.stringify(data);
        
        $.ajax({
            url: "/BotTelegram/GetGrouplogname",
            type: "post",
            data: { dataapi},
            success: function (result) {
                if (result != null) {
                    $('#groupLog').val(result).change();

                } else
                {
                    $('#groupLog').val('').change();
                    _msgalert.error('Vui lòng nhập đúng token và group id.');
                }
               
            }
        });
    },
    OnSaveBot: function () {
        //-- Validation:
        var projectType = $('#ProjectType').val();
        if (projectType.trim() == 0) {
            _msgalert.error('Vui lòng lựa chọn tên preoject Type');
            return;
        }
        var token = $('#token').val();
        var groupChatid = $('#groupChatId').val();
        var groupLog = $('#groupLog').val();

        if (token.trim() == "") {
            _msgalert.error('Token được để trống.');
            return;
        }

        if (groupChatid.trim() == "") {
            _msgalert.error('Group id không được để trống.');
            return;
        }
     
        if (groupLog.trim() == "") {
            _msgalert.error('Vui lòng nhập đúng token và group id.');
            return;
        }
        //-- Summit Data:
        var object_summit = {
            id: ($('#id').val()),
            ProjectType: ($('#ProjectType').val().trim()),
            token: ($('#token').val().trim()),
            groupChatId: ($('#groupChatId').val().trim()),
            groupLog: ($('#groupLog').val().trim()),
            createDate: ($('#createDate').val().trim()),
            status: $('input[name="status"]:checked').val(),
        }
        let data = JSON.stringify(object_summit)
     
        $.ajax({
            url: '/BotTelegram/AddBot',
            type: "post",
            data: { data },
            success: function (result) {

                if (result.stt_code === 1) {
                    _msgalert.error(result.msg);
                } else {
                    _msgalert.success(result.msg);
                    $.magnificPopup.close();
                    _Telegrammanagement.Loaddata();
                }
            }
        });
    },
   
    OnPaging: function (value) {
        let _searchModel = {
            TokenName: ($('#token_input').val()),
            currentPage: value,
        };
        var objSearch = this.SearchParam;

        this.Search(_searchModel);
    },
    
    Onchangeinput: function () {
        let _searchModel = {
            TokenName: ($('#token_input').val()),
            statusmodel: ($('input[name="status"]:checked').val()),
            Projectmodel: ($('#ProjectType').val()),
        };
        var objSearch = this.SearchParam;
       
        objSearch = _searchModel;
        
        this.Search(objSearch);
    },

    Search: function (input) {
        $.ajax({
            url: "/BotTelegram/Search",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading').hide();
                $('#grid_data').html(result);
            }
        });
    },
    Updata: function (id) {
        let title = 'Thêm mới/Cập nhật bot log telegram';
        let url = '/BotTelegram/BotDetail';
        let param = {
        };
        if (id.trim() != '') {
            param = {
                id: id,
            };
        } 
        
        _magnific.OpenSmallPopup(title, url, param);
    },
    ShowHideColumn: function () {
        $('.checkbox-tb-column').each(function () {
            let seft = $(this);
            let id = seft.data('id');
            if (seft.is(':checked')) {
                $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
            } else {
                $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
            }
        });
    },
    ChangeSetting: function (position) {
        this.ShowHideColumn();
        switch (position) {
            case 1:
                if ($('#sttDisplay').is(":checked")) {
                    fields.STT = true
                } else {
                    fields.STT = false
                }
                break;
            case 2:
                if ($('#tokenDisplay').is(":checked")) {
                    fields.STT = true
                } else {
                    fields.STT = false
                }
                break;
            case 3:
                if ($('#grouplogDisplay').is(":checked")) {
                    fields.STT = true
                } else {
                    fields.STT = false
                }
                break;
            case 4:
                if ($('#groupidDisplay').is(":checked")) {
                    fields.STT = true
                } else {
                    fields.STT = false
                }
                break;
            case 5:
                if ($('#dateDisplay').is(":checked")) {
                    fields.STT = true
                } else {
                    fields.STT = false
                }
                break;
            case 6:
                if ($('#projectDisplay').is(":checked")) {
                    fields.STT = true
                } else {
                    fields.STT = false
                }
                break;
            case 7:
                if ($('#statusDisplay').is(":checked")) {
                    fields.STT = true
                } else {
                    fields.STT = false
                }
                break;
            default:
        }
    },
}
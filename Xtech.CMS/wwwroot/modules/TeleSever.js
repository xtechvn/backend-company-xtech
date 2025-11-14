let fields = {
    STT: true,
    Name: true,
    Price: true,
    StartDate: true,
    EndDate: true,
    CreatedAt: true,
    Status: true
};
let cookieName = 'fields_bottelegram';
var timer;

$(document).ready(function () {
    _Telegrammanagement.Loaddata();
    

});

var _Telegrammanagement = {

    Loaddata: function () {
        debugger
        let _searchModel = {
            name: ($('#name_input').val() || '').trim(),
            status: $('input[name="status"]:checked').val(),
            currentPage: 1,
            pageSize: 20
        };

        this.Search(_searchModel);
    },

    OnSaveBot: function () {
        var name = $('#name').val();
        var price = $('#price').val();
        var startDate = $('#startDate').val();
        var endDate = $('#endDate').val();
        var createdAt = $('#createdAt').val();
        var status = $('input[name="status_detail"]:checked').val() || 0;

        if (!name || name.trim() === "") {
            _msgalert.error('Tên không được để trống.');
            return;
        }

        if (!price || isNaN(price)) {
            _msgalert.error('Giá không hợp lệ.');
            return;
        }

        // validate ngày bắt đầu <= ngày kết thúc
        if (startDate && endDate) {
            var sdParts = startDate.split('/');
            var edParts = endDate.split('/');

            var sd = new Date(sdParts[2], sdParts[1] - 1, sdParts[0]);
            var ed = new Date(edParts[2], edParts[1] - 1, edParts[0]);

            if (sd > ed) {
                _msgalert.error('Ngày bắt đầu không được lớn hơn ngày kết thúc.');
                return;
            }
        }

        function toIsoDate(dateString) {
            if (!dateString) return null;
            var parts = dateString.split('/');
            if (parts.length !== 3) return null;
            return parts[2] + '-' + parts[1] + '-' + parts[0]; // yyyy-MM-dd
        }

        var object_summit = {
            id: ($('#id').val() || 0),
            name: name.trim(),
            // gửi ISO date cho backend
            createdAt: toIsoDate(createdAt),
            startDate: toIsoDate(startDate),
            endDate: toIsoDate(endDate),
            price: parseFloat(price),
            status: parseInt(status)
        };

        let data = JSON.stringify(object_summit);

        $.ajax({
            url: '/SeverTelegram/AddBot',
            type: "post",
            data: { data },
            success: function (result) {
                debugger
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
            name: ($('#name_input').val() || '').trim(),
            status: $('input[name="status"]:checked').val(),
            currentPage: value,
            pageSize: 20
        };

        this.Search(_searchModel);
    },

    Onchangeinput: function () {
        let _searchModel = {
            name: ($('#name_input').val() || '').trim(),
            status: $('input[name="status"]:checked').val(),
            currentPage: 1,
            pageSize: 20
        };

        this.Search(_searchModel);
    },

    Search: function (input) {
        debugger
        $.ajax({
            url: "/SeverTelegram/Search",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading').hide();
                $('#grid_data').html(result);
                _Telegrammanagement.ShowHideColumn();
            }
        });
    },

    Updata: function (id) {
        debugger
        let title = 'Thêm mới/Cập nhật bot log telegram';
        let url = '/SeverTelegram/BotDetail';
        let param = {};

        if (id && id.trim() !== '') {
            param = { id: id };
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
                fields.STT = $('#sttDisplay').is(":checked");
                break;
            case 2:
                fields.Name = $('#nameDisplay').is(":checked");
                break;
            case 3:
                fields.Price = $('#priceDisplay').is(":checked");
                break;
            case 4:
                fields.StartDate = $('#startDateDisplay').is(":checked");
                break;
            case 5:
                fields.EndDate = $('#endDateDisplay').is(":checked");
                break;
            case 6:
                fields.CreatedAt = $('#createdAtDisplay').is(":checked");
                break;
            case 7:
                fields.Status = $('#statusDisplay').is(":checked");
                break;
        }
    }
};

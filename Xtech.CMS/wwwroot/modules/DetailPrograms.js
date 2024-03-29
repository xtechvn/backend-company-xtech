
var programs_package_html2 = {

    html_programs_package_add_packages_tr2: '<tr class="ProgramsPackage-Add-row"><td><input maxlength="12"  placeholder="Giá nhập" class="form-control currency text-right ProgramsPackage-Add-Amout" id="ProgramsPackage" /></td><td><select style="width:100%;" multiple  class="form-control ProgramsPackage-Add-WeekDay " ><option value="2">Thứ 2</option><option value="3">Thứ 3</option><option value="4">Thứ 4</option><option value="5">Thứ 5</option><option value="6">Thứ 6</option><option value="7">Thứ 7</option><option value="0">CN</option></select></td><td> <a href="javascript:;" class="blue ml-2 mb10 buttun_add" onclick="_ProgramsPackage.AddProgramsPackage($(this));"><i class="fa fa-plus-circle green"></i> </a> <a class="fa fa-trash-o" href="javascript:;" onclick="_ProgramsPackage.DeleteProgramsPackage($(this));"></a></td></tr >',
    html_programs_package_add_packages_tr3: '<tr class="ProgramsPackage-Edit-row"><td><input maxlength="12"  placeholder="Giá nhập" class="form-control currency text-right ProgramsPackage-Edit-Amout" id="ProgramsPackage" /></td><td><select style="width:100%;" multiple  class="form-control ProgramsPackage-Edit-WeekDay " >{option}</select></td><td > <a href="javascript:;" class="blue ml-2 mb10 buttun_add" onclick="_ProgramsPackage.AddProgramsPackage2($(this));"><i class="fa fa-plus-circle green"></i> </a> <a class="fa fa-trash-o" href="javascript:;" onclick="_ProgramsPackage.DeleteProgramsPackage($(this));"></a></td></tr >',
    html_option: '<option value="2">Thứ 2</option><option value="3">Thứ 3</option><option value="4">Thứ 4</option><option value="5">Thứ 5</option><option value="6">Thứ 6</option><option value="7">Thứ 7</option><option value="0">CN</option>'
}
var isPickerApprove5 = false;
var isPickerApprove = false;
var isPickerApprovePackagePrice = false;
var _DetailPrograms = {
    OpenPopup: function (id) {
        let title = 'Cập nhật chương trình';
        let url = '/Programs/AddProgramsView';
        let param = {
        };
        if (id.trim() != "") {
            param = {
                id: id,
            };
        }
        _magnific.OpenSmallPopup(title, url, param);

    },
    UpdatePrograms: function (status) {
        let FromProgramsCreate = $('#Programs_form');
        FromProgramsCreate.validate({
            rules: {


                "ProgramCode": {
                    required: true,
                },
                "ProgramName": {
                    required: true,
                },
                "Suppliersid": {
                    required: true,
                },
                "ServiceType": {
                    required: true,
                },
                "StartDate_EndDate": {
                    required: true,
                },
                "ServiceName": {
                    required: true,
                },


            },
            messages: {

                "ProgramCode": "Mã không được bỏ trống",
                "ProgramName": "Tên không được bỏ trống",
                "Suppliersid": "Nhà cung cấp không được bỏ trống",
                "ServiceType": "Dịch vụ không được bỏ trống",
                "StartDate_EndDate": "Thời gian áp dụng không được bỏ trống",
                "ServiceName": "Tên dịch vụ không được bỏ trống",
            }
        });
        if (FromProgramsCreate.valid()) {
            var StartDate;
            var EndDate;
            if ($('#StartDate_EndDate').data('daterangepicker') !== undefined && $('#StartDate_EndDate').data('daterangepicker') != null && isPickerApprove5) {
                StartDate = $('#StartDate_EndDate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
                EndDate = $('#StartDate_EndDate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
            } else {
                StartDate = null
                EndDate = null
            }
            var model = {
                Id: $('#Id').val(),
                ProgramCode: $('#ProgramCode').val(),
                ProgramName: $('#ProgramName').val(),
                SupplierId: $('#Suppliersid').select2().val(),
                ServiceName: $('#ServiceName').select2('data')[0].text,
                HotelId: $('#ServiceName').select2().val(),
                ServiceType: $('#ServiceType').val(),
                StartDateStr: StartDate,
                EndDateStr: EndDate,
                Description: $('#Description').val(),
                Status: status,
            };
            $.ajax({
                url: "/Programs/SummitPrograms",
                type: "Post",
                data: { model },
                success: function (result) {
                    if (result.status == 0) {
                        _msgalert.success(result.msg);
                        $.magnificPopup.close();
                        window.location.reload();
                    } else {
                        _msgalert.error(result.msg);

                    }
                }
            });
        }
    },
    Suppliers: function (element) {
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC ",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 1,
            ajax: {
                url: "/PaymentRequest/GetSuppliersSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        name: params.term,
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response, function (item) {
                            return {
                                text: item.fullname,
                                id: item.id,
                            }
                        })
                    };
                },
                cache: true
            }
        });
    },
    UpdateProgramsStatus: function (status, id) {
        var title = ' ';
        var title2 = ' ';
        if (status == 0) {
            title = 'Xác nhận lưu nháp';
            title2 = 'Xác nhận lưu nháp chương trình ';
        }
        if (status == 3) {
            title = 'Xác nhận từ chối';
            title2 = 'Xác nhận từ chối chương trình';
        }
        if (status == 1) {
            title = 'Xác nhận gửi duyệt';
            title2 = 'Xác nhận gửi duyệt chương trình ';
        }
        if (status == 2) {
            title = 'Xác nhận duyệt';
            title2 = 'Xác nhận duyệt chương trình';
        }
        if (status == 5) {
            title = 'Xác nhận xóa';
            title2 = 'Xác nhận xóa chương trình';
        }
        _msgconfirm.openDialog(title, title2 + ' không?', function () {
            $.ajax({
                url: "/Programs/UpdateProgramsStatus",
                type: "post",
                data: { statustype: status, id: id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        if (status != 5) {
                            setTimeout(function () {
                                $.magnificPopup.close();
                                window.location.reload();
                            }, 1000)
                        }
                        else {
                            setTimeout(function () {
                                window.location.href = '/Programs/Index';
                            }, 1000)

                        }
                    }

                }
            });
        });

    },
    OnloadHotelName: function () {
        $("#Suppliersid-error").hide()
        var id = $('#ServiceName').select2().val();
        _DetailPrograms.HotelSuggestion($('#ServiceName'));
        $("#Suppliersid").select2({
            theme: 'bootstrap4',
            placeholder: "Nhà cung cấp",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 1,
            ajax: {
                url: "/Programs/GetSupplierSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function () {
                    var query = {
                        id: id,
                    }
                    return query;
                },
                processResults: function (response) {
                    if (response == undefined) {

                        $("#Suppliersid-error").show()
                        $("#Suppliersid-error").text("Dịch vụ khách sạn này chưa có nhà cung cấp")
                        return null;
                    } else {
                        return {
                            results: $.map(response, function (item) {
                                return {
                                    text: item.FullName,
                                    id: item.SupplierId,
                                }
                            })
                        };
                    }


                },

            }
        });
    },
    loadDetailProgramsPackage: function (id, type, SupplierId, ProgramsPackageid) {
        $.magnificPopup.close();
        $('#view_btnT').hide();
        /*$('#view_summit').remove();*/
        $('#view_summit').show();
        $.ajax({
            url: "/ProgramsPackage/DetailProgramsPackage",
            type: "post",
            data: {
                id: id, type: type, SupplierId: SupplierId, ProgramsPackageid: ProgramsPackageid
            },
            success: function (result) {
                $('#view_summit').html(result);
                $('#PackageCode').removeAttr('disabled')
            }
        });


    },
    showbtnThem: function () {
        $('#view_btnT').show();
        $('#view_summit').hide();
        $('#view_summit_goi').hide();
        $('#btnthemgoi').show()
    },
    OpenviewAdd: function (id, type) {
        let title = 'Thêm mới/Cập nhật gói';

        $('#view_summit_goi').show()
        $('#btnthemgoi').hide()
        $.ajax({
            url: "/ProgramsPackage/DetailProgramsPackage",
            type: "post",
            data: {
                id: id, type: type
            },
            success: function (result) {
                $('#view_summit_goi').html(result);

            }
        });

    },
    SingleDatePickerFromNow: function (dropdown_position = 'down') {
        var date = $('#FromDateAppLy').val();
        $('#ApplyDate').daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            showDropdowns: true,
            minDate: date,
            locale: {
                format: 'DD/MM/YYYY'
            }
        }, function (start, end, label) {


        });
    },
    CheckAppLy: function (element, FromDate, ToDate, Status) {
        $('.ProgramsPackage-Edit-row').addClass("ProgramsPackage-Edit-row-2 mfp-hide");
        $('.ProgramsPackage-Edit-row').removeClass("ProgramsPackage-Edit-row");
        $('.' + element).removeClass("mfp-hide");
        $('.' + element).removeClass("ProgramsPackage-Edit-row-2");
        $('.' + element).addClass("ProgramsPackage-Edit-row");

        $('.ProgramsPackage-Edit-WeekDay').addClass("ProgramsPackage-Edit-WeekDay-2");
        $('.ProgramsPackage-Edit-WeekDay').removeClass("ProgramsPackage-Edit-WeekDay");
        $('.WeekDay-' + element).removeClass("mfp-hide");
        $('.WeekDay-' + element).removeClass("ProgramsPackage-Edit-WeekDay-2");
        $('.WeekDay-' + element).addClass("ProgramsPackage-Edit-WeekDay");

        $('input[name="datetimeApprove"]').data('daterangepicker').setStartDate(FromDate);
        $('input[name="datetimeApprove"]').data('daterangepicker').setEndDate(ToDate);
        if (Status == 0) {
            $('input[name=optradio][value=0]').prop('checked', true);
        } else {
            $('input[name=optradio][value=1]').prop('checked', true);
        }
        _ProgramsPackage.Select2WithFixedOption($('.ProgramsPackage-Edit-row .ProgramsPackage-Edit-WeekDay'))
    },
    Edit: function (id) {
        let title = 'Sửa giá';
        let url = '/ProgramsPackage/Edit';
        let param = {
            id: id,
            type: 0
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    Edit2: function (id) {
        let title = 'Sửa giá';
        let url = '/ProgramsPackage/Edit';
        let param = {
            id: id,
            type:1
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    EditProgramsPackage: function () {
        var type = $("#type").val();
        let FromAmountProgramPackage = $("#form-amount-ProgramPackage");
        FromAmountProgramPackage.validate({
            rules: {

                Amount: "required",
            },
            messages: {
                Amount: "Giá không được bỏ trống",
            }
        });
        if (FromAmountProgramPackage.valid()) {
            var id = $('#Id').val();
            var amount = $('#Price').val().replaceAll(',','');
          
     
            $.ajax({
                url: "/ProgramsPackage/EditProgramsPackage",
                type: "POST",
                data: {
                    id: id,
                    amount: amount,
                   
                },
                success: function (result) {

                    if (result.status === 1) {
                        _msgalert.error(result.msg);
                    }

                    if (result.status === 0) {
                        _msgalert.success(result.msg);
                        if (type == 1) {
                            setTimeout(function () {
                                $.magnificPopup.close();
                                _DetailPrograms.SearchDetailListProgramsPackage();
                            }, 1000)
                        } else {
                            setTimeout(function () {
                                $.magnificPopup.close();
                                location.reload();
                            }, 1000)
                        }
                       
                      

                    }
                }
            });
        }
    },
    HotelSuggestion: function (element) {
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Tên khách sạn",
            minimumInputLength: 1,
            ajax: {
                url: "/Order/HotelSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                tags: true,
                data: function (params) {
                    var query = {
                        txt_search: params.term,
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: item.name,
                                id: item.hotelid,
                            }
                        })
                    };
                },
                cache: true
            }
        });
    },
    SearchDetailListProgramsPackage: function () {
        var date = $('#filter_date_ListProgramsPackagePrice').val()
        var FromDatePrice; //Ngày bat dau
        var ToDatePrice; //Ngày hết hạn
        if ($('#filter_date_ListProgramsPackagePrice').data('daterangepicker') !== undefined && $('#filter_date_ListProgramsPackagePrice').data('daterangepicker') != null && isPickerApprovePackagePrice) {
            FromDatePrice = $('#filter_date_ListProgramsPackagePrice').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            ToDatePrice = $('#filter_date_ListProgramsPackagePrice').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            FromDatePrice = null
            ToDatePrice = null
        }
        if (date == "") {
             FromDatePrice = null
            ToDatePrice = null
        }
        let input = {
            ProgramId: $("#ProgramId").val(),
            Packageid: $("#Packageid").val(),
            FromDate: FromDatePrice,
            ToDate: ToDatePrice,
            PageIndex: -1,
            PageSize: 20,
        };

        $.ajax({
            url: "/ProgramsPackage/ListDetailProgramsPrice",
            type: "post",
            data: input,
            success: function (result) {
                $('#table-ListProgramsPackagePrice').html(result);
            },
        });
    },
};
var _ProgramsPackage = {
    Init: function () {
        var id = $('#ProgramId').val();
        objSearch = this.GetParam();
        objSearch.ProgramId = id;
        objSearch.PageIndex = 1;
        objSearch.PageSize = 20;
        _ProgramsPackage.Search(objSearch);

        objSearch2 = {
            ProgramId: id,
            PageIndex: 1,
            PageSize: 10,
        }
        _ProgramsPackage.Search2(objSearch2);
    },
    Search: function (input) {
        $.ajax({
            url: "/ProgramsPackage/ListProgramsPackage",
            type: "post",
            data: input,
            success: function (result) {
                $('#imgLoading').hide();
                $('#grid-data').html(result);
                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");
            },
        });
    },
    Search2: function (input) {
        $.ajax({
            url: "/ProgramsPackage/ListProgramsPrice",
            type: "post",
            data: input,
            success: function (result) {
                $('#imgLoading').hide();
                $('#grid-data2').html(result);
                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");
            },
        });
    },

    GetParam: function () {
        var FromDate; //Ngày bat dau
        var ToDate; //Ngày hết hạn
        if ($('#filter_date_daterangepicker').data('daterangepicker') !== undefined && $('#filter_date_daterangepicker').data('daterangepicker') != null && isPickerApprove) {
            FromDate = $('#filter_date_daterangepicker').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            ToDate = $('#filter_date_daterangepicker').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            FromDate = null
            ToDate = null
        }
        let _searchModel = {
            /*   ProgramId: $("#ProgramId").val(),*/
            ProgramId: $('#ProgramId').val(),
            FromDate: FromDate,
            ToDate: ToDate,
            PageIndex: 1,
            PageSize: $("#selectPaggingOptions").find(':selected').val(),
        };
        return _searchModel;
    },
    OnPaging: function (value) {
        objSearch = this.GetParam();
        objSearch.PageIndex = value;
        _ProgramsPackage.Search(objSearch);
    },
    onSelectPageSize: function () {
        _ProgramsPackage.SearchData();

    },
    OpenPopup: function (id, type, SupplierId) {
        let title = 'Thêm mới gói';
        let url = '/ProgramsPackage/DetailProgramsPackage';
        let param = {
        };
        if (id.trim() != "") {
            param = {
                id: id,
                type: type,
                SupplierId: SupplierId,
            };
        }
        _magnific.OpenSmallPopup(title, url, param);
        _ProgramsPackage.Select2WithFixedOption($('.ProgramsPackage-Add-WeekDay'))
    },
    AddProgramsPackage: function (element) {
        $('.buttun_add').each(function (index, item) {
            var element3 = $(item);
            element3.hide();
        });
        var table_element = element.closest('.ProgramsPackage-tbody')
        table_element.find('.ProgramsPackage-summary-row').before(programs_package_html2.html_programs_package_add_packages_tr2);

        _ProgramsPackage.Select2WithFixedOption($('.ProgramsPackage-Add-WeekDay'))


    },
    AddProgramsPackage2: function (element) {

        $('.buttun_add').each(function (index, item) {
            var element3 = $(item);
            element3.hide();
        });
        var table_element = element.closest('.ProgramsPackage-tbody')
        table_element.find('.ProgramsPackage-summary-row').before(programs_package_html2.html_programs_package_add_packages_tr3);

        _ProgramsPackage.Select2WithFixedOption($('.ProgramsPackage-Edit-WeekDay'))
    },
    DeleteProgramsPackage: function (element) {
    
        var table_element = element.closest('.ProgramsPackage-tbody')
        var data1 = [];
        var data2 = [];
        $('.ProgramsPackage-Add-WeekDay').each(function (index, item) {
            var element2 = $(item);
            var valus = element2.select2().val();
            data1.push(valus)
        });
        $('.ProgramsPackage-Edit-WeekDay').each(function (index, item) {
            var element3 = $(item);
            var valus = element3.select2().val();
            data2.push(valus)
        });
        if (data1.length > 1) {
            element.closest('.ProgramsPackage-Add-row').remove()
            _ProgramsPackage.Select2WithFixedOption($('.ProgramsPackage-Add-WeekDay'))
        } else {
            if (data1.length == 1) {
                _msgalert.error("Không thể xóa khi còn 1 dòng");
            }
          
        }
        if (data2.length > 1) {
            element.closest('.ProgramsPackage-Edit-row').remove()
            _ProgramsPackage.Select2WithFixedOption($('.ProgramsPackage-Edit-WeekDay'))
        } else {
            if (data2.length == 1) {
                _msgalert.error("Không thể xóa khi còn 1 dòng");
            }
        }

        $('.buttun_add').each(function (index, item) {
            var element3 = $(item);
            element3.show();
        });
       
    },
    loadselect2: function () {
        $('.ProgramsPackage-WeekDay').each(function (index, item) {
            var element = $(item);
            _ProgramsPackage.Select2WithFixedOption(element)
        });
    },
    Select2WithFixedOption: function (element) {
        let dataOption = [];
        var data = [];
        $('.ProgramsPackage-Add-WeekDay').each(function (index, item) {
            var element2 = $(item);
            var valus = element2.select2().val();
            data.push(valus)
        });
        $('.ProgramsPackage-Edit-row .ProgramsPackage-Edit-WeekDay').each(function (index, item) {
            var element3 = $(item);
            var valus = element3.select2().val();
            data.push(valus)
        });
        function formatSelection(state) {
            return state.text;
        }

        function formatResult(state) {
            console.log(state)
            if (!state.id) return state.text; // optgroup
            var id = 'state' + state.id.toLowerCase();
            var label = $('<label></label>', { for: id })
                .text(state.text);
            var checkbox = $('<input type="checkbox">', { id: id });

            return checkbox.add(label);
        }
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Thứ ",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 7,
            closeOnSelect: false,
            formatResult: formatResult,
            formatSelection: formatSelection,
            ajax: {
                url: "/ProgramsPackage/WeekDaySuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                   var data22= function (params) {
                      
                        $('.ProgramsPackage-Add-WeekDay').each(function (index, item) {
                            var element2 = $(item);
                            var valus = element2.select2().val();
                            dataOption.push(valus)
                        });
                        $('.ProgramsPackage-Edit-row .ProgramsPackage-Edit-WeekDay').each(function (index, item) {
                            var element3 = $(item);
                            var valus = element3.select2().val();
                            dataOption.push(valus)
                        });
                       debugger
                       return dataOption;
                    }
                    if (dataOption.length == 0) {
                        dataOption = data;
                    }
                    var query = {
                        txt_search: dataOption.toString(),
                    }
                    
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: item.name,
                                id: item.id,
                            }
                        })
                    };
                },
            }
        });

        $('.ProgramsPackage-WeekDay').each(this.options, function (i, item) {
            if (item.selected) {
                $(item).prop("disabled", true);
            }
        });
        function iformat(icon, badge,) {
            var originalOption = icon.element;
            var originalOptionBadge = $(originalOption).data('badge');

            return $('<span><i class="fa ' + $(originalOption).data('icon') + '"></i> ' + icon.text + '<span class="badge">' + originalOptionBadge + '</span></span>');
        }
    },
    Select2HotelRoomSuggest: function (element) {
        var id = $("#SupplierID").val();
        var ServiceName = $("#ServiceName").val();
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Hạng phòng ",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 1,
            ajax: {
                url: "/Programs/GetHotelRoomSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        id: id,
                        ServiceName: ServiceName,
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response, function (item) {
                            return {
                                text: item.Name,
                                id: item.Id,
                            }
                        })
                    };
                },
            }
        });
    },
    SummitProgramsPackage: function (type, type2) {
        var FromProgramsPackageCreate = $('#ProgramsPackage_form');
        //FromProgramsPackageCreate.validate({

        //    messages: {
        //        "RoomType": "Hạng  phòng không được bỏ trống",
        //        "ApplyDate": "Thời gian áp dụng không được bỏ trống",

        //        "PackageCode": {
        //            required: "Mã không được bỏ trống",
        //        },
        //    }
        //});

        if ($('#PackageCode').val() == undefined || $('#PackageCode').val() == "") {
            _msgalert.error("Mã không được bỏ trống");
            validate_failed = true
            return false;
        }
        if ($('#RoomType').val() == undefined || $('#RoomType').val() == "") {
            _msgalert.error("Hạng  phòng không được bỏ trống");
            validate_failed = true
            return false;
        }
        var validate_failed = false
        var object_summit = {

            ProgramsPackage: []
        }
        if (type == 1) {
            isPickerApproveProgramsPackage = true;
        }
        var FromDate; //Ngày bat dau
        var ToDate; //Ngày hết hạn
        if ($('#ApplyDate').data('daterangepicker') !== undefined && $('#ApplyDate').data('daterangepicker') != null ) {
            FromDate = $('#ApplyDate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            ToDate = $('#ApplyDate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");

        } else {
            FromDate = null
            ToDate = null
            _msgalert.error("Thời gian áp dụng không được bỏ trống");
            validate_failed = true
            return false;
        }

        $('.ProgramsPackage-Add-row').each(function (index, item) {
            var extra_package_element = $(item);

            var package_amount = extra_package_element.find('.ProgramsPackage-Add-Amout').val();
            var package_WeekDay = extra_package_element.find('.ProgramsPackage-Add-WeekDay').select2("val");


            if (package_amount == null || package_amount.toString() == undefined || package_amount.toString().trim() == '' || package_WeekDay == null || package_WeekDay.toString() == undefined || package_WeekDay.toString().trim() == '') {
                _msgalert.error("Nội dung của giá nhập và theo thứ không được bỏ trống");
                validate_failed = true
                return false;
            }


            var extra_package = {

                PackageCode: $('#PackageCode').val(),
                PackageName: $('#PackageName').val(),
                ProgramId: $('#ProgramId').val(),
                RoomType: $('#RoomType').select2('data')[0].text,
                RoomTypeId: $('#RoomType').val(),
                Price: package_amount,
                FromDateStr: FromDate,
                ToDateStr: ToDate,
                WeekDay: package_WeekDay.toString(),
                OpenStatus: $('.optradio:checked').val(),
            }

            object_summit.ProgramsPackage.push(extra_package);
        });
        if (validate_failed != true) {
            _global_function.AddLoading()
            $.ajax({
                url: "/ProgramsPackage/SummitProgramsPackage",
                type: "post",
                data: { data: object_summit.ProgramsPackage, type: type, type2: type2 },
                success: function (result) {
                    _global_function.RemoveLoading()
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            if (type == 0) {

                                window.location.href = '/ProgramsPackage/DetailListProgramsPackage/' + object_summit.ProgramsPackage[0].ProgramId + '/' + result.id + '';
                            } else {
                                $.magnificPopup.close();
                                window.location.reload();
                            }
                        }, 1000)

                    } else {
                        _msgalert.error(result.msg);
                    }

                }
            });
        }

    },
    SummitupdateProgramsPackage: function (type, type2) {


        if ($('#PackageCode').val() == undefined || $('#PackageCode').val() == "") {
            _msgalert.error("Mã không được bỏ trống");
            validate_failed = true
        }
        if ($('#RoomType').val() == undefined || $('#RoomType').val() == "") {
            _msgalert.error("Hạng  phòng không được bỏ trống");
            validate_failed = true
        }
        var validate_failed = false
        var object_summit = {

            ProgramsPackage: []
        }
        if (type == 1) {
            isPickerApproveProgramsPackage = true;
        }
        var FromDate; //Ngày bat dau
        var ToDate; //Ngày hết hạn
        if ($('#ApplyDate').data('daterangepicker') !== undefined && $('#ApplyDate').data('daterangepicker') != null && isPickerApproveProgramsPackage) {
            FromDate = $('#ApplyDate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            ToDate = $('#ApplyDate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");

        } else {
            FromDate = null
            ToDate = null
            _msgalert.error("Thời gian áp dụng không được bỏ trống");
            validate_failed = true
            return false;
        }

        $('.ProgramsPackage-Edit-row').each(function (index, item) {
            var extra_package_element = $(item);

            var package_amount = extra_package_element.find('.ProgramsPackage-Edit-Amout').val();
            var id = extra_package_element.find('.ProgramsPackage-Edit-Id').val();
            var package_WeekDay = extra_package_element.find('.ProgramsPackage-Edit-WeekDay').select2("val").toString();

            if (package_amount == null || package_amount.toString() == undefined || package_amount.toString().trim() == '' || package_WeekDay == null || package_WeekDay.toString() == undefined || package_WeekDay.toString().trim() == '') {
                _msgalert.error("Nội dung của giá nhập và theo thứ không được bỏ trống");
                validate_failed = true
                return false;
            }

            var extra_package = {
                id: id,
                PackageCode: $('#PackageCode').val(),
                PackageName: $('#PackageName').val(),
                ProgramId: $('#ProgramId').val(),
                RoomType: $('#RoomType').select2('data')[0].text,
                RoomTypeId: $('#RoomType').val(),
                Price: package_amount,
                FromDateStr: FromDate,
                ToDateStr: ToDate,
                WeekDay: package_WeekDay,
                OpenStatus: $('.optradio:checked').val(),
            }

            object_summit.ProgramsPackage.push(extra_package);
        });
        if (validate_failed != true) {
            _global_function.AddLoading()
            $.ajax({
                url: "/ProgramsPackage/SummitProgramsPackage",
                type: "post",
                data: { data: object_summit.ProgramsPackage, type: type, type2: type2 },
                success: function (result) {
                    _global_function.RemoveLoading()
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            $.magnificPopup.close();
                            window.location.href = '/ProgramsPackage/DetailListProgramsPackage/' + object_summit.ProgramsPackage[0].ProgramId + '/' + result.id + '';
                        }, 1000)

                    } else {
                        _msgalert.error(result.msg);
                    }

                }
            });

        }
    },
    loadgrid: function (valus) {
        if (valus == 1) {
            $('#grid-data2').attr("style", "display: none;")
            $('#grid-data').removeAttr("style", "display: none;")

            $('#DSgoi').addClass("active")
            $('#Bgia').removeClass("active")
        }
        else {
            $('#grid-data').attr("style", "display: none;")
            $('#grid-data2').removeAttr("style", "display: none;")
            $('#Bgia').addClass("active")
            $('#DSgoi').removeClass("active")
        }
    },
    ProgramsDetail: function (id, type, SupplierId, ProgramsPackageid) {
        let title = 'Chi tiết gói chương trình';
        let url = '/ProgramsPackage/DetailProgramsPackage';
        let param = {
        };
        if (id.trim() != "") {
            param = {
                id: id,
                type: type,
                SupplierId: SupplierId,
                ProgramsPackageid: ProgramsPackageid,
            };
        }
        _magnific.OpenSmallPopup(title, url, param);

    },
    ProgramsDetail2: function (id, type, SupplierId, ProgramsPackageid) {
        let title = 'Thông tin hạng phòng';
        let url = '/ProgramsPackage/DetailProgramsPackage2';
        let param = {
        };
        if (id.trim() != "") {
            param = {
                id: id,
                type: type,
                SupplierId: SupplierId,
                ProgramsPackageid: ProgramsPackageid,
            };
        }
        _magnific.OpenSmallPopup(title, url, param);

    },
    SearchProgramsPackagePrice: function () {

        var FromDate; //Ngày bat dau
        var ToDate; //Ngày hết hạn
        if ($('#filter_date_ProgramsPackagePrice').data('daterangepicker') !== undefined && $('#filter_date_ProgramsPackagePrice').data('daterangepicker') != null && isPickerApprove) {
            FromDate = $('#filter_date_ProgramsPackagePrice').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            ToDate = $('#filter_date_ProgramsPackagePrice').data('daterangepicker').endDate._d.toLocaleDateString("en-GB");
        } else {
            FromDate = null
            ToDate = null
        }
        let input = {
            ProgramId: $("#ProgramId").val(),
            FromDate: FromDate,
            ToDate: ToDate,
            PageIndex: 1,
            PageSize: $("#selectPaggingOptions").find(':selected').val(),
        };

        _ProgramsPackage.Search2(input);
    },
    ShowEditView: function () {


        $('#editview').addClass('mfp-hide');

        $('#RoomType').prop('disabled', false);
        /* $('#PackageCode').prop('disabled', false);*/
        $('#ApplyDate').prop('disabled', false);
        $('.optradio').prop('disabled', false);

        $('.ProgramsPackage-RoomType').prop('disabled', false);
        $('.ProgramsPackage-Amout').prop('disabled', false);
        $('.ProgramsPackage-WeekDay').prop('disabled', false);

        $('.ProgramsPackage-Amout').removeClass('input-disabled-background')
        $('.ProgramsPackage-WeekDay').removeClass('input-disabled-background')
        $('.ProgramsPackage-RoomType').removeClass('input-disabled-background')
        $('.ProgramsPackage-summary-row').removeClass('mfp-hide')
        $('.action').removeClass('mfp-hide')
        $('#btnLuu').removeClass('mfp-hide')

    },
    HineAddView: function () {
        $('.buttun_add').each(function (index, item) {
            var element3 = $(item);
            element3.hide();
        });
    },
    HotelRoomSuggest: function () {
        var id = $("#SupplierID").val();
        var ServiceName = $("#ServiceName").val();
        
        $('.RoomType').select2({
            theme: 'bootstrap4',
            placeholder: "Hạng phòng ",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",

            ajax: {
                url: "/Programs/GetHotelRoomSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        id: id,
                        ServiceName: ServiceName,
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response, function (item) {
                            return {
                                text: item.Name,
                                id: item.Id,
                            }
                        })
                    };
                },
            }
        });
    },

    HotelRoomSuggest2: function () {
        
        var id = $("#SupplierID").val();
        var ServiceName = $("#ServiceName").val();
        $('#RoomType').select2({
            theme: 'bootstrap4',
            placeholder: "Hạng phòng ",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",

            ajax: {
                url: "/Programs/GetHotelRoomSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        id: id,
                        ServiceName: ServiceName,
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response, function (item) {
                            return {
                                text: item.Name,
                                id: item.Id,
                            }
                        })
                    };
                },
            }
        });
    },
    DeleteProgramsPackagedb: function (id, packagecode, roomtype, amount,date) {
        _msgconfirm.openDialog('Xác nhận xóa hạng phòng', 'Xác nhận xóa hạng phòng ' + roomtype + ' của gói ' + packagecode + ' không?', function () {
            $.ajax({
                url: "/ProgramsPackage/DeleteProgramsPackage",
                type: "post",
                data: { id: id, packagecode: packagecode, roomtype: roomtype, amount: amount, date: date },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            $.magnificPopup.close();
                            window.location.reload();

                        }, 1000)

                    } else {
                        _msgalert.error(result.msg);
                    }

                }
            });
        });

    },

};

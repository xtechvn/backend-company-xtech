var _policy_create_html = {
    html_new_policy_packages:'<tr class="servicemanual-policy-row"> <td class="servicemanual-policy-package">{order}</td><td> <input class="form-control servicemanual-Policy-Clienttype" type="text" name="servicemanual-Policy-Clienttype"></td> <td> <input class="form-control servicemanual-Policy-DebtType" type="text" name="servicemanual-Policy-DebtType"></td>  <td>  </td> <td class="text-right"> <input type="text" class="form-control text-right currency servicemanual-Policy-HotelDebtAmout" onkeyup="javascript:;" name=" servicemanual-Policy-HotelDebtAmout" value=""></td> <td class="text-right"> <a class="fa fa-trash-o" href="javascript:;" onclick="_Policy.DeletePolicyExtrapackage($(this));"></a> </td> </tr>',
}
let policyFields = {
    STT: true,
    PolicyId: true,
    PolicyNameCS: true,
    EffectiveDate: true,
    PermissionTypeCS: true,
    CreatedDate: true,
    CreatedBy: true,
    ThaoT: true,
}
let cookieName = 'policyFields_transactionsms';
$(document).ready(function () {

    _Policy.Loaddata();
    $("#UserCreate").select2({
        theme: 'bootstrap4',
        placeholder: "Người tạo",
        maximumSelectionLength: 1,
        ajax: {
            url: "/Order/UserSuggestion",
            type: "post",
            dataType: 'json',
            delay: 250,
            data: function (params) {
                var query = {
                    txt_search: params.term,
                }

                // Query parameters will be ?search=[term]&type=public
                return query;
            },
            processResults: function (response) {
                return {
                    results: $.map(response.data, function (item) {
                        return {
                            text: item.fullname + ' - ' + item.email,
                            id: item.id,
                        }
                    })
                };
            },
            cache: true
        }
    });
    if (_Policy.getCookie('policyFields_transactionsms') != null) {
        let cookie = _Policy.getCookie(cookieName)
        policyFields = JSON.parse(cookie)
    } else {
        _Policy.setCookie(cookieName, JSON.stringify(policyFields), 10)
    }
    _Policy.checkShowHide();

});
let isPickerApprove = false;
let isPickerApprove2 = false;
var _Policy = {
    Loaddata: function () {
        let _searchModel = {

            PageIndex: 1,
            PageSize: 20,
        };
        var objSearch = this.SearchParam;

        objSearch = _searchModel;


        this.Search(objSearch);


    },
    Search: function (input) {
        window.scrollTo(0, 0);
        $.ajax({
            url: "/Policy/Search",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_Search').hide();
                $('#grid_data_Search').html(result);
                $('.checkbox-tb-column').each(function () {
                    let seft = $(this);
                    let id = seft.data('id');
                    if (seft.is(':checked')) {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').removeClass('mfp-hide');
                    } else {
                        $('td:nth-child(' + id + '),th:nth-child(' + id + ')').addClass('mfp-hide');
                    }
                });
                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");
            }
        });
        
    },
    OpenPopup: function (id) {
        let title = 'Thêm mới/Cập nhật chính sách hợp tác';
        let url = '/Policy/Detail';
        let param = {
        };
        if (id.trim() != "") {
            param = {
                id: id,
            };
        }
        _magnific.OpenSmallPopup(title, url, param);
    },
    OpenPopupDetail: function (id) {
        let title = 'Chi tiết chính sách hợp tác';
        let url = '/Policy/PolicyDetail';
        let param = {
        };
        if (id.trim() != "") {
            param = {
                id: id,
            };
        }
        _magnific.OpenSmallPopup(title, url, param);
    },
    SearchData: function () {
        var input = _Policy.getPram();
        this.Search(input);
    },
    OnChange: function () {
        var Permision_Type = $('#PermisionType').val();
       
        if (Permision_Type == 1 || Permision_Type == 2) {
            if (Permision_Type == 1) {
                $('#list_Policy').show();
                $('#Loai_CN').show();
                $.ajax({
                    url: "/Policy/DetailTable",
                    type: "Post",
                    data: { id: 1 },
                    success: function (result) {
                        $("#data_body").html(result);
                    }
                });
                
            }
            if (Permision_Type == 2) {
                $('#list_Policy').show();
                $('#Loai_CN').hide();
                $.ajax({
                    url: "/Policy/DetailTable",
                    type: "Post",
                    data: { id: 2 },
                    success: function (result) {
                        $("#data_body").html(result);
                    }
                });
                
            }
        } else {
            $('#list_Policy').hide();
        }
    },
    OnCreate: function () {
        var EffectiveDate = null;
        if ($("#PolicyName").val() == "") { $("#PolicyName_err").show().text("Tên chính sách không được bỏ trống"); } else { $("#PolicyName_err").hide() }
        if ($("#EffectiveDate").val() == "") { $("#EffectiveDate_err").show().text("Ngày hiệu lực không được bỏ trống"); }
        else {
            $("#EffectiveDate_err").hide()
            var from = $("#EffectiveDate").val().split("/");
            EffectiveDate = from[1] + "/" + from[0] + "/" +  from[2]
        }
        if ($("#PermisionType").val() == -1) { $("#PermisionType_err").show().text("Nhóm khách hàng không được bỏ trống"); return } else { $("#PermisionType_err").hide() }
        
        var object_summit = {
            PolicyId: $('#PolicyId').val(),
            PolicyName: $("#PolicyName").val(),
            PermissionType: $("#PermisionType").val(),
            EffectiveDate: EffectiveDate,
            extra_policy: [],
        }
        
        let FromCreate = $('#form_add_Policy');
        FromCreate.validate({
            rules: {
                "PolicyName": {
                    required: true,
                },
                "EffectiveDate": {
                    required: true,
                },
                "PermisionType": {
                    required: true,
                },
            },
            messages: {
                "PolicyName": {
                    required: "Vui lòng không bỏ trống tên",
                },
                "EffectiveDate": {
                    required: "Vui lòng không bỏ trống ngày hiệu lực",
                },
                "PermisionType": {
                    required: "Vui lòng chọn nhóm khách hàng",
                },
            }
        });
        if (FromCreate.valid()) {
            if (object_summit.PermissionType == 1) {
                if ($(".servicemanual-policy-row")[0]) {
                    var error = false;
                    $('.servicemanual-policy-row').each(function (index, item) {
                        var element = $(item)
                        if (element.find('.servicemanual-Policy-Clienttype').val() == undefined || element.find('.servicemanual-Policy-Clienttype').val() == '') {
                            _msgalert.error('Vui lòng chọn loại khách hàng cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-DebtType').val() == undefined || element.find('.servicemanual-Policy-DebtType').val() == '') {
                            _msgalert.error('Vui lòng chọn loại công nợ cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-ProductFlyTicketDebtAmount').val() == undefined || element.find('.servicemanual-Policy-ProductFlyTicketDebtAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập hạn mức công nợ VMB cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-TourDebtAmount').val() == undefined || element.find('.servicemanual-Policy-TourDebtAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập hạn mức công nợ KS cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-TourDebtAmount').val() == undefined || element.find('.servicemanual-Policy-TourDebtAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập hạn mức công nợ Tour cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-TouringCarDebtAmount').val() == undefined || element.find('.servicemanual-Policy-TouringCarDebtAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập hạn mức công nợ thuê xe du lịch cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-VinWonderDebtAmount').val() == undefined || element.find('.servicemanual-Policy-VinWonderDebtAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập hạn mức công nợ VinWonder cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                    });
                    if (error) return;
                }
                else {
                    _msgalert.error('Vui lòng nhập vào ít nhất 01 thành viên thuộc danh sách đoàn khi tạo mới dịch vụ khách sạn');
                    return;
                }
                $('.servicemanual-policy-row').each(function (index, item) {
                    var element = $(item);
                    var obj_package = {
                        ClientType: element.find('.servicemanual-Policy-Clienttype').val(),
                        DebtType: element.find('.servicemanual-Policy-DebtType').val(),
                        ProductFlyTicketDebtAmount: element.find('.servicemanual-Policy-ProductFlyTicketDebtAmount').val().replaceAll(',', ''),
                        HotelDebtAmout: element.find('.servicemanual-Policy-HotelDebtAmout').val().replaceAll(',', '') ,
                        TourDebtAmount: element.find('.servicemanual-Policy-TourDebtAmount').val().replaceAll(',', '') ,
                        TouringCarDebtAmount: element.find('.servicemanual-Policy-TouringCarDebtAmount').val().replaceAll(',', '') ,
                        VinWonderDebtAmount: element.find('.servicemanual-Policy-VinWonderDebtAmount').val().replaceAll(',', '') ,
                       
                    };
                    object_summit.extra_policy.push(obj_package);
                });
            }
            if (object_summit.PermissionType == 2) {
                if ($(".servicemanual-policy-row")[0]) {
                    var error = false;
                    $('.servicemanual-policy-row').each(function (index, item) {
                        var element = $(item)
                        if (element.find('.servicemanual-Policy-Clienttype').val() == undefined || element.find('.servicemanual-Policy-Clienttype').val() == '') {
                            _msgalert.error('Vui lòng chọn loại khách hàng cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-ProductFlyTicketDepositAmount').val() == undefined || element.find('.servicemanual-Policy-ProductFlyTicketDepositAmount').val() == '') {
                            _msgalert.error('Vui lòng Số dư ký quỹ VMB tối thiểu cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-HotelDepositAmout').val() == undefined || element.find('.servicemanual-Policy-HotelDepositAmout').val() == '') {
                            _msgalert.error('Vui lòng Số dư ký quỹ KS tối thiểu cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-TourDepositAmount').val() == undefined || element.find('.servicemanual-Policy-TourDepositAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập Số dư ký quỹ Tour cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-TouringCarDepositAmount').val() == undefined || element.find('.servicemanual-Policy-TouringCarDepositAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập Số dư ký quỹ thuê xe du lịch cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-VinWonderDepositAmount').val() == undefined || element.find('.servicemanual-Policy-VinWonderDepositAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập Số dư ký quỹ VinWonder cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                    });
                    if (error) return;
                }
                else {
                    _msgalert.error('Vui lòng nhập vào ít nhất 01 thành viên thuộc danh sách đoàn khi tạo mới dịch vụ khách sạn');
                    return;
                }
                $('.servicemanual-policy-row').each(function (index, item) {
                    var element = $(item);
                    var obj_package = {
                        ClientType: element.find('.servicemanual-Policy-Clienttype').val(),
                        ProductFlyTicketDepositAmount: element.find('.servicemanual-Policy-ProductFlyTicketDepositAmount').val().replaceAll(',', ''),
                        HotelDepositAmout: element.find('.servicemanual-Policy-HotelDepositAmout').val().replaceAll(',', ''),
                        TourDepositAmount: element.find('.servicemanual-Policy-TourDepositAmount').val().replaceAll(',', ''),
                        TouringCarDepositAmount: element.find('.servicemanual-Policy-TouringCarDepositAmount').val().replaceAll(',', ''),
                        VinWonderDepositAmount: element.find('.servicemanual-Policy-VinWonderDepositAmount').val().replaceAll(',', ''),
                    };
                    object_summit.extra_policy.push(obj_package);
                });
            }
            $.ajax({
                url: '/Policy/Setup',
                type: "post",
                data: { data: object_summit },
                success: function (result) {
                    if (result.stt_code === 0) {
                        _msgalert.success(result.msg);
                        $.magnificPopup.close();
                        _Policy.Loaddata();

                    }
                    else {
                        _msgalert.error(result.msg);

                    }

                }
            });
        }
    
    },

    GetLastestPolicyPackageTable: function () {
        var last_order = 1;
        if (!$('.servicemanual-policy-package')[0]) return 0;

        //-- Get Lastest Order
        $('.servicemanual-policy-package').each(function (index, item) {
            if (parseInt($(item).text()) != undefined && last_order < parseInt($(item).text())) {
                last_order = parseInt($(item).html())
            }
        });
        return last_order;
    },
    AddPolicyPackage: function () {
        var lastest_policy = _Policy.GetLastestPolicyPackageTable();
        lastest_policy = lastest_policy + 1;
        var html = _policy_create_html.html_new_policy_packages.replaceAll('{order}', lastest_policy)
        $(".servicemanual-policypackage-total-summary").before(html);
        _common_function.Select2FixedOptionWithAddNew($('.service-fly-extrapackage-packagename-select'))
    },
    DeletePolicyExtrapackage: function (element) {
        element.closest('.servicemanual-policy-row').remove();
        var count = 0;
        $('.servicemanual-policy-package').each(function (index, item) {
            count = count + 1;
            $(item).html(count)
        });
        _order_detail_hotel.CalucateTotalAmountOfExtraPackages();

    },
    getPram: function () {
        firstTime = false
        var EffectiveDateFrom; //Ngày tạo từ--
        var EffectiveDateTo; //Ngày tạo từ--
        var CreateDateFrom; //Ngày tạo từ--
        var CreateDateTo; //Ngày tạo từ--
   
        if ($('#EffectiveDateFrom').data('daterangepicker') !== undefined && $('#EffectiveDateFrom').data('daterangepicker') != null && isPickerApprove) {
            EffectiveDateFrom = $('#EffectiveDateFrom').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            EffectiveDateTo = $('#EffectiveDateFrom').data('daterangepicker').endDate._d.toLocaleDateString("en-GB") +' 23:59:59';
        } else {
            EffectiveDateFrom = null
            EffectiveDateTo = null
        }

        if ($('#CreateDate').data('daterangepicker') !== undefined && $('#CreateDate').data('daterangepicker') != null && isPickerApprove2) {
            CreateDateFrom = $('#CreateDate').data('daterangepicker').startDate._d.toLocaleDateString("en-GB");
            CreateDateTo = $('#CreateDate').data('daterangepicker').endDate._d.toLocaleDateString("en-GB") + ' 23:59:59';
        } else {
            CreateDateFrom = null
            CreateDateTo = null
        }
        var objSearch = {
            PolicyName : $('#PolicyName').val(),
            EffectiveDateFrom: EffectiveDateFrom,
            EffectiveDateTo: EffectiveDateTo,
            PermissionType: $('#PermissionType').val(),
            CreateDateFrom: CreateDateFrom,
            CreateDateTo: CreateDateTo,
            UserCreate: $('#UserCreate').val(),
            PageIndex: 1,
            PageSize: $("#selectPaggingOptions").find(':selected').val(),
        }
        return objSearch;
    },
    OnUpdatePolicy: function () {
        if ($("#PolicyName").val() == "") { $("#PolicyName_err").show().text("Tên chính sách không được bỏ trống"); } else { $("#PolicyName_err").hide() }
        if ($("#EffectiveDate").val() == "") { $("#EffectiveDate_err").show().text("Ngày hiệu lực không được bỏ trống"); } else { $("#EffectiveDate_err").hide() }
        if ($("#PermisionType").val() == -1) { $("#PermisionType_err").show().text("Nhóm khách hàng không được bỏ trống"); return } else { $("#PermisionType_err").hide() }
        var object_summit = {
            PolicyId: $('#PolicyId').val(),
            PolicyName: $("#PolicyName").val(),
            PermissionType: $("#PermisionType").val(),
            EffectiveDate: $("#EffectiveDate").val(),
            extra_policy: [],
        }

        let FromCreate = $('#form_add_Policy');
        FromCreate.validate({
            rules: {
                "PolicyName": {
                    required: true,
                },
                "EffectiveDate": {
                    required: true,
                },
                "PermisionType": {
                    required: true,
                },
            },
            messages: {
                "PolicyName": {
                    required: "Vui lòng không bỏ trống tên",
                },
                "EffectiveDate": {
                    required: "Vui lòng không bỏ trống ngày hiệu lực",
                },
                "PermisionType": {
                    required: "Vui lòng chọn nhóm khách hàng",
                },
            }
        });
        if (FromCreate.valid()) {
            if (object_summit.PermissionType == 1) {
                if ($(".servicemanual-policy-row-Edit")) {
                    var error = false;
                    $(".servicemanual-policy-row-Edit").each(function (index, item) {
                        var element = $(item)
                        if (element.find('.servicemanual-Policy-Clienttype').val() == undefined || element.find('.servicemanual-Policy-Clienttype').val() == '') {
                            _msgalert.error('Vui lòng chọn loại khách hàng cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-DebtType').val() == undefined || element.find('.servicemanual-Policy-DebtType').val() == '') {
                            _msgalert.error('Vui lòng chọn loại công nợ cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-ProductFlyTicketDebtAmount').val() == undefined || element.find('.servicemanual-Policy-ProductFlyTicketDebtAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập hạn mức công nợ VMB cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-HotelDebtAmout').val() == undefined || element.find('.servicemanual-Policy-HotelDebtAmout').val() == '') {
                            _msgalert.error('Vui lòng nhập hạn mức công nợ KS cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-TourDebtAmount').val() == undefined || element.find('.servicemanual-Policy-TourDebtAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập hạn mức công nợ Tour cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-TouringCarDebtAmount').val() == undefined || element.find('.servicemanual-Policy-TouringCarDebtAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập hạn mức công nợ thuê xe du lịch cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-VinWonderDebtAmount').val() == undefined || element.find('.servicemanual-Policy-VinWonderDebtAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập hạn mức công nợ VinWonder cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                    });
                    if (error) return;
                }
                else {
                    _msgalert.error('Vui lòng nhập đầy đủ thông tin chính sách hợp tác');
                    return;
                }
                $(".servicemanual-policy-row-Edit").each(function (index, item) {
                    var element = $(item);
                    var obj_package = {
                        Id: element.find('.servicemanual-Policy-Id').val(),
                        ClientType: element.find('.servicemanual-Policy-Clienttype').val(),
                        DebtType: element.find('.servicemanual-Policy-DebtType').val(),
                        ProductFlyTicketDebtAmount: element.find('.servicemanual-Policy-ProductFlyTicketDebtAmount').val().replaceAll(',', ''),
                        HotelDebtAmout: element.find('.servicemanual-Policy-HotelDebtAmout').val().replaceAll(',', ''),
                        TourDebtAmount: element.find('.servicemanual-Policy-TourDebtAmount').val().replaceAll(',', ''),
                        TouringCarDebtAmount: element.find('.servicemanual-Policy-TouringCarDebtAmount').val().replaceAll(',', ''),
                        VinWonderDebtAmount: element.find('.servicemanual-Policy-VinWonderDebtAmount').val().replaceAll(',', ''),
                    };
                    object_summit.extra_policy.push(obj_package);
                });
            }
            if (object_summit.PermissionType == 2) {
                if ($(".servicemanual-policy-row-Edit")) {
                    var error = false;
                    $(".servicemanual-policy-row Edit").each(function (index, item) {
                        var element = $(item)
                        if (element.find('.servicemanual-Policy-Clienttype').val() == undefined || element.find('.servicemanual-Policy-Clienttype').val() == '') {
                            _msgalert.error('Vui lòng chọn loại khách hàng cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-ProductFlyTicketDepositAmount').val() == undefined || element.find('.servicemanual-Policy-ProductFlyTicketDepositAmount').val() == '') {
                            _msgalert.error('Vui lòng Số dư ký quỹ VMB tối thiểu cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-HotelDepositAmout').val() == undefined || element.find('.servicemanual-Policy-HotelDepositAmout').val() == '') {
                            _msgalert.error('Vui lòng Số dư ký quỹ KS tối thiểu cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-TourDepositAmount').val() == undefined || element.find('.servicemanual-Policy-TourDepositAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập Số dư ký quỹ Tour cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-TouringCarDepositAmount').val() == undefined || element.find('.servicemanual-Policy-TouringCarDepositAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập Số dư ký quỹ thuê xe du lịch cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                        if (element.find('.servicemanual-Policy-VinWonderDepositAmount').val() == undefined || element.find('.servicemanual-Policy-VinWonderDepositAmount').val() == '') {
                            _msgalert.error('Vui lòng nhập Số dư ký quỹ VinWonder cho STT:' + element.find('.servicemanual-policy-package').text());
                            error = true;
                            return false;
                        }
                    });
                    if (error) return;
                }
                else {
                    _msgalert.error('Vui lòng nhập đầy đủ thông tin chính sách hợp tác');
                    return;
                }
                $(".servicemanual-policy-row-Edit").each(function (index, item) {
                    var element = $(item);
                    var obj_package = {
                        Id: element.find('.servicemanual-Policy-Id').val(),
                        ClientType: element.find('.servicemanual-Policy-Clienttype').val(),
                        ProductFlyTicketDepositAmount: element.find('.servicemanual-Policy-ProductFlyTicketDepositAmount').val().replaceAll(',', ''),
                        HotelDepositAmout: element.find('.servicemanual-Policy-HotelDepositAmout').val().replaceAll(',', ''),
                        TourDepositAmount: element.find('.servicemanual-Policy-TourDepositAmount').val().replaceAll(',', ''),
                        TouringCarDepositAmount: element.find('.servicemanual-Policy-TouringCarDepositAmount').val().replaceAll(',', ''),
                        VinWonderDepositAmount: element.find('.servicemanual-Policy-VinWonderDepositAmount').val().replaceAll(',', ''),
                    };
                    object_summit.extra_policy.push(obj_package);
                });
            }
            $.ajax({
                url: '/Policy/Setup',
                type: "post",
                data: { data: object_summit },
                success: function (result) {
                    if (result.stt_code === 0) {
                        _msgalert.success(result.msg);
                        $.magnificPopup.close();
                        _Policy.Loaddata();

                    }
                    else {
                        _msgalert.error(result.msg);

                    }

                }
            });
        }

    },
    onEdit: function (value) {
        $(".servicemanual-policy-row").eq(value).addClass('servicemanual-policy-row-Edit');;
    },
    ChangeSetting: function (position) {
        this.showHideColumn();
        switch (position) {
            case 1:
                if ($('#STT').is(":checked")) {
                    policyFields.STT = true
                } else {
                    policyFields.STT = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(policyFields), 10);
                break;
             
            case 2:
                if ($('#PolicyId').is(":checked")) {
                    policyFields.PolicyId = true
                } else {
                    policyFields.PolicyId = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(policyFields), 10);
                break;
            case 3:
                if ($('#PolicyName').is(":checked")) {
                    policyFields.PolicyName = true
                } else {
                    policyFields.PolicyName = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(policyFields), 10);
                break;
            case 4:
                if ($('#EffectiveDate').is(":checked")) {
                    policyFields.EffectiveDate = true
                } else {
                    policyFields.EffectiveDate = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(policyFields), 10);
                break;
            case 5:
                if ($('#PermissionType').is(":checked")) {
                    policyFields.PermissionType = true
                } else {
                    policyFields.PermissionType = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(policyFields), 10);
                break;
            case 6:
                if ($('#CreatedDate').is(":checked")) {
                    policyFields.CreatedDate = true
                } else {
                    policyFields.CreatedDate = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(policyFields), 10);
                break;
            case 7:
                if ($('#CreatedBy').is(":checked")) {
                    policyFields.CreatedBy = true
                } else {
                    policyFields.CreatedBy = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(policyFields), 10);
                break;
            case 8:
                if ($('#ThaoT').is(":checked")) {
                    policyFields.ThaoT = true
                } else {
                    policyFields.ThaoT = false
                }
                this.eraseCookie(cookieName);
                this.setCookie(cookieName, JSON.stringify(policyFields), 10);
                break;
            default:
        }
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
    checkShowHide: function () {
        if (policyFields.STT === true) {
            $('#STT').prop('checked', true);
        } else {
            $('#STT').prop('checked', false);
        }
        if (policyFields.PolicyId === true) {
            $('#PolicyId').prop('checked', true);
        } else {
            $('#PolicyId').prop('checked', false);
        }
        if (policyFields.PolicyNameCS === true) {
            $('#PolicyNameCS').prop('checked', true);
        } else {
            $('#PolicyNameCS').prop('checked', false);
        }
        if (policyFields.EffectiveDate === true) {
            $('#EffectiveDate').prop('checked', true);
        } else {
            $('#EffectiveDate').prop('checked', false);
        }
        if (policyFields.PermissionTypeCS === true) {
            $('#PermissionTypeCS').prop('checked', true);
        } else {
            $('#PermissionTypeCS').prop('checked', false);
        }
        if (policyFields.CreatedDate === true) {
            $('#CreatedDate').prop('checked', true);
        } else {
            $('#CreatedDate').prop('checked', false);
        }
        if (policyFields.CreatedBy === true) {
            $('#CreatedBy').prop('checked', true);
        } else {
            $('#CreatedBy').prop('checked', false);
        }
        if (policyFields.ThaoT === true) {
            $('#ThaoT').prop('checked', true);
        } else {
            $('#ThaoT').prop('checked', false);
        }

    },
    eraseCookie: function (name) {
        document.cookie = name + '=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    },
    getCookie: function (name) {
        var nameEQ = name + "=";
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ') c = c.substring(1, c.length);
            if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
        }
        return null;
    },
    setCookie: function (name, value, days) {
        var expires = "";
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + (value || "") + expires + "; path=/";
    },
    ontype: function () {
        $('#bt-saver').show();
        $('#bt-Edit').hide();
        $('#PolicyName').removeAttr("disabled");
        $('#EffectiveDate').removeAttr("disabled");
        $('#PermisionType').removeAttr("disabled");
        $('.servicemanual-Policy-DebtType').removeAttr("disabled");
        $('.servicemanual-Policy-ProductFlyTicketDebtAmount').removeAttr("disabled");
        $('.servicemanual-Policy-HotelDebtAmout').removeAttr("disabled");
        $('.servicemanual-Policy-ProductFlyTicketDepositAmount').removeAttr("disabled");
        $('.servicemanual-Policy-HotelDepositAmout').removeAttr("disabled");

        $('.servicemanual-Policy-TourDebtAmount').removeAttr("disabled");
        $('.servicemanual-Policy-TouringCarDebtAmount').removeAttr("disabled");
        $('.servicemanual-Policy-VinWonderDebtAmount').removeAttr("disabled");
        $('.servicemanual-Policy-TourDepositAmount').removeAttr("disabled");
        $('.servicemanual-Policy-TouringCarDepositAmount').removeAttr("disabled");
        $('.servicemanual-Policy-VinWonderDepositAmount').removeAttr("disabled");
    },
    OnPaging: function (value) {
       
        var objSearch = this.SearchParam
        objSearch = this.getPram(true)
        objSearch.PageIndex = value
        objSearch.currentPage = value
        this.Search(objSearch);
    },
    onSelectPageSize: function () {
        var objSearch = this.SearchParam;
        objSearch = this.getPram(true);
        this.Search(objSearch);
    },
    DeletePolicy: function (id) {
        $.ajax({
            url: "/Policy/PolicyDelete",
            type: "Post",
            data: { id: id },
            success: function (result) {
                if (result.stt_code === 0) {
                    _msgalert.success(result.msg);
                    _Policy.Loaddata();
                }
                else {
                    _msgalert.error(result.msg);

                }
            }
        });
    }
};
var _common_function = {
    Select2FixedOptionWithAddNew: function (element) {
        element.select2({
            tags: true,

            createTag: function (params) {
                var term = $.trim(params.term);

                if (term === '') {
                    return null;
                }

                return {
                    id: term,
                    text: term,
                }
            }
        });
    },
};
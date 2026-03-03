var _set_service_other_html = {
    html_user_option: '<option class="select2-results__option" value="{user_id}">{user_name} - {user_email}{user_phone}</option>',
    confirm_box_description_grant_order: 'Bạn có chắc muốn nhận đặt dịch vụ {Mã dịch vụ} không?',
    confirm_box_description_send_service_code: 'Bạn có chắc muốn trả code dịch vụ {Mã dịch vụ} không?',
    confirm_box_description_change_to_payment: 'Bạn có chắc muốn quyết toán dịch vụ {Mã dịch vụ} không?',
    confirm_box_description_operator_order_price: 'Thông tin giá đặt dịch vụ sẽ được lưu lại, bạn có chắc chắn không?',
    confirm_box_description_decline: 'Bạn có chắc muốn từ chối dịch vụ {Mã dịch vụ} không?',
    confirm_box_title_grant_order: 'Confirm nhận đặt dịch vụ',
    confirm_box_title_send_service_code: 'Confirm trả Code dịch vụ',
    confirm_box_title_change_to_payment: 'Confirm quyết toán',
    confirm_box_title_operator_order_price: 'Thay đổi giá đặt dịch vụ',
    confirm_box_title_decline: 'Từ chối đặt dịch vụ',
    html_ordered_new_packages: `
                        <tr class="service-other-ordered-row" data-id="0">
                        <td class="service-other-ordered-order">@(++index)</td>
                        <td><p class="form-control service-other-ordered-package-name input-disabled-background" disabled>@item.PackageName</p></td>
                        <td>
                            <select class="select2 service-other-ordered-suplier service-other-ordered-suplier-new" style="width:100% !important">

                            </select>
                        </td>
                        <td>
                               <select class="form-control supplier-select  service-other-ordered-status" name="service-other-ordered-status">
                                       <option value="0">Sử dụng </option>
                                       <option value="1">Hủy </option>
                               </select>
                        </td>
                        <td> <input class="form-control text-right currency service-other-packages-baseprice-operator"  type="text" name="service-other-packages-baseprice-operator" value=""></td>
                        <td> <input class="form-control text-right currency service-other-packages-quantity"  type="text" name="service-other-packages-quantity" value=""></td>
                        <td class="text-right"> <input class="form-control text-right currency service-other-packages-unit-price input-disabled-background" disabled style="background-color: lightgray;" value=""></td>
                        <td> <textarea class="form-control style-width2 textarea service-other-ordered-note " ></textarea> </td>
                        <td> <a class="fa fa-trash-o service-other-ordered-delete-row-disabled service-other-ordered-delete-row" href="javascript:;"></a></td>
                    </tr>`
}

var _set_service_other = {
    Initialization: function () {
        _common_function_other.SingleDateRangePicker($('.set-service-other-search-startdate'))
        _common_function_other.SingleDateRangePicker($('.set-service-other-search-enddate'))
        _common_function_other.SingleDateRangePicker($('.set-service-other-search-createddate'))

        _common_function_other.UserSuggesstionMultiple($('.set-service-other-search-saler'))
        _common_function_other.UserSuggesstionMultiple($('.set-service-other-search-main-staff'))
        _common_function_other.UserSuggesstionMultiple($('.set-service-other-search-usercreate'))

        _set_service_other.DynamicBindInput()
        _set_service_other.SearchData()
        setTimeout(function () {

            $('.set-service-other-search-status').select2({
                placeholder: "Tất cả trạng thái dịch vụ"
            })
        }, 500)
    },
    DynamicBindInput: function () {
        $("body").on('apply.daterangepicker', ".set-service-other-search-startdate", function (ev, picker) {
            _common_function_other.OnApplyStartDateTimeOfBookingRange($(this), $('.set-service-other-search-enddate'))

        });
        $("body").on('apply.daterangepicker', ".set-service-other-search-daterange", function (ev, picker) {
            $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
        });
        $("body").on('focusout', ".set-service-other-search-startdate", function (ev, picker) {
            if ($(this).val() == undefined || $(this).val().trim() == '') {
                _common_function_other.SingleDateRangePicker($('.set-service-other-search-enddate'))
            }
        });
        $("body").on('click', ".service-other-filter-by-status", function (ev, picker) {
            var element = $(this)

            $('.service-other-filter-by-status').removeClass('active')
            element.addClass('active')

            var status = [];
            if (element.attr('data-status').trim() != '-1') status = [element.attr('data-status')]
            $('.set-service-other-search-status').val(status).change();
            _set_service_other.SearchData()


        });



    },
    Search: function (input) {
        window.scrollTo(0, 0);
        $.ajax({
            url: "/SetService/OtherSearch",
            type: "Post",
            data: input,
            success: function (result) {
                $('#search_data_grid').html(result);
                _common_function_other.OrderNoSuggesstion($('.set-service-other-search-orderno'))
                _common_function_other.ServiceCodeSuggesstion($('.set-service-other-search-servicecode'))
                _common_function_other.Select2BookingCode($("#BookingCode"))
            }
        });
    },
    OnPaging: function (value) {
        if (value > 0) {
            var objSearch = _set_service_other.GetParam(value);
            _set_service_other.Search(objSearch);
        }
    },
    onSelectPageSize: function () {
        _set_service_other.SearchData();

    },
    SearchData: function () {
        var objSearch = _set_service_other.GetParam(1);
        _set_service_other.Search(objSearch);
    },
    GetParam: function (PageIndex) {
        var object_search_summit = {
            searchModel: {
                ServiceCode: $('.set-service-other-search-servicecode').val() == undefined || $('.set-service-other-search-servicecode').val().trim() == '' ? '' : $('.set-service-other-search-servicecode').val().trim(),
                OrderCode: $('.set-service-other-search-orderno').find(':selected').val() == undefined || $('.set-service-other-search-orderno').find(':selected').val().trim() == '' ? '' : $('.set-service-other-search-orderno').find(':selected').val().trim(),
            }

        }
        var status = '';
        $('.set-service-other-search-status').find(':selected').each(function (index, item) {
            var element = $(item);
            if (element.val().includes(',')) {
                status = '' + element.val()
                return false;
            }
            if (status == '') status = '' + element.val()
            else status = status + ',' + element.val()
        });
        object_search_summit.searchModel.StatusBooking = status
        object_search_summit.searchModel.BookingCode = $("#BookingCode").val(),

            object_search_summit.searchModel.UserCreate = $('.set-service-other-search-usercreate').find(':selected').val() == undefined || $('.set-service-other-search-usercreate').find(':selected').val().trim() == '' ? null : $('.set-service-other-search-usercreate').find(':selected').val()
        object_search_summit.searchModel.SalerId = $('.set-service-other-search-saler').find(':selected').val() == undefined || $('.set-service-other-search-saler').find(':selected').val().trim() == '' ? null : $('.set-service-other-search-saler').find(':selected').val()
        object_search_summit.searchModel.OperatorId = $('.set-service-other-search-main-staff').find(':selected').val() == undefined || $('.set-service-other-search-main-staff').find(':selected').val().trim() == '' ? null : $('.set-service-other-search-main-staff').find(':selected').val()
        object_search_summit.searchModel.PageIndex = PageIndex
        object_search_summit.searchModel.pageSize = isNaN(parseInt($('#selectPaggingOptions').find(':selected').val())) || parseInt($('#selectPaggingOptions').find(':selected').val()) == undefined ? 30 : parseInt($('#selectPaggingOptions').find(':selected').val())
        var compare_from = _global_function.GetDayText($('.set-service-other-search-startdate').data('daterangepicker').startDate._d, true).trim().split(' ')[0]
        var compare_to = _global_function.GetDayText($('.set-service-other-search-startdate').data('daterangepicker').endDate._d, true).trim().split(' ')[0]

        if ($('.set-service-other-search-startdate').val() == undefined || $('.set-service-other-search-startdate').val().trim() == '') {
            object_search_summit.searchModel.StartDateFrom = undefined;
            object_search_summit.searchModel.StartDateTo = undefined;

        }
        else {
            object_search_summit.searchModel.StartDateFrom = _global_function.GetDayText($('.set-service-other-search-startdate').data('daterangepicker').startDate._d, true);
            object_search_summit.searchModel.StartDateTo = _global_function.GetDayText($('.set-service-other-search-startdate').data('daterangepicker').endDate._d, true);
        }
        compare_from = _global_function.GetDayText($('.set-service-other-search-enddate').data('daterangepicker').startDate._d, true).trim().split(' ')[0]
        compare_to = _global_function.GetDayText($('.set-service-other-search-enddate').data('daterangepicker').endDate._d, true).split(' ')[0]
        if ($('.set-service-other-search-enddate').val() == undefined || $('.set-service-other-search-enddate').val().trim() == '') {
            object_search_summit.searchModel.EndDateFrom = undefined;
            object_search_summit.searchModel.EndDateTo = undefined;
        }
        else {
            object_search_summit.searchModel.EndDateFrom = _global_function.GetDayText($('.set-service-other-search-enddate').data('daterangepicker').startDate._d, true);
            object_search_summit.searchModel.EndDateTo = _global_function.GetDayText($('.set-service-other-search-enddate').data('daterangepicker').endDate._d, true);
        }
        compare_from = _global_function.GetDayText($('.set-service-other-search-createddate').data('daterangepicker').startDate._d, true).trim().split(' ')[0]
        compare_to = _global_function.GetDayText($('.set-service-other-search-createddate').data('daterangepicker').endDate._d, true).split(' ')[0]
        if ($('.set-service-other-search-createddate').val() == undefined || $('.set-service-other-search-createddate').val().trim() == '') {
            object_search_summit.searchModel.CreateDateFrom = undefined;
            object_search_summit.searchModel.CreateDateTo = undefined;

        }
        else {
            object_search_summit.searchModel.CreateDateFrom = _global_function.GetDayText($('.set-service-other-search-createddate').data('daterangepicker').startDate._d, true);
            object_search_summit.searchModel.CreateDateTo = _global_function.GetDayText($('.set-service-other-search-createddate').data('daterangepicker').endDate._d, true);
        }

        return object_search_summit;
    },
    Export: function () {
        $('#btnExport').prop('disabled', true);
        $('#icon-export').removeClass('fa-file-excel-o');
        $('#icon-export').addClass('fa-spinner fa-pulse');
        var objSearch = _set_service_other.GetParam(1);
        objSearch.PageIndex = 1;
        objSearch.searchModel.pageSize = $('#countOther').val();
        this.searchModel = objSearch
        _global_function.AddLoading()
        $.ajax({
            url: "/SetService/OtherExportExcel",
            type: "Post",
            data: this.searchModel,
            success: function (result) {
                _global_function.RemoveLoading()
                $('#btnExport').prop('disabled', false);
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    window.location.href = result.path;
                } else {
                    _msgalert.error(result.message);
                }
                $('#icon-export').removeClass('fa-spinner fa-pulse');
                $('#icon-export').addClass('fa-file-excel-o');
            }
        });
    },
}

var _set_service_other_detail = {
    ServiceType: 21,
    ServiceTypeSale: 20,
    Initialization: function () {
        _set_service_other_detail.DynamicBindInput()
        $('.service-other-detail-change-tab-button').click()
        switch ($('#service-other-detail-data').attr('data-status')) {
            case '1':
            case '2': {
                _set_service_other_detail.ShowNeedToOrderTab()

            } break;
            case '3':
            case '4': {
                _set_service_other_detail.ShowPaymentTab()

            } break;
            default: {
                _set_service_other_detail.ShowOrderTab()

            } break;
        }
        $("body").on('click', ".service-code-send-email2", function (ev, picker) {
            _set_service_other_detail.PopupSendEmailSupplier()

        });
    },
    DynamicBindInput: function () {
        $('body').on('click', '.service-other-detail-change-tab-button', function (event) {
            var element = $(this)
            switch (element.attr('data-tab-id')) {
                case '0': {
                    _set_service_other_detail.ShowOrderTab()

                } break;
                case '1': {
                    _set_service_other_detail.ShowNeedToOrderTab()

                } break;
                case '2': {
                    _set_service_other_detail.ShowOrderedTab()

                } break;
                case '3': {
                    _set_service_other_detail.ShowPaymentTab()

                } break;
                case '4': {
                    _set_service_other_detail.ShowServiceCodeTab()

                } break;
                case '5': {
                    _set_service_other_detail.ShowRefundTab()

                } break;
                case '6': {
                    _set_service_other_detail.MarketingTab()

                } break;
            }

        });
    },
    Back: function () {
        window.location.href = '/SetService/others';
    },
    PopupSendEmail: function () {
        let title = 'Gửi Code dịch vụ vé khác';
        let url = '/SetService/SendEmail';
        let param = {
            id: $('#service-other-detail-data').attr('data-booking-id').trim(),
            Orderid: $('#service-other-detail-data').attr('data-order-id').trim(),
            type: 9,
        };
        _magnific.OpenSmallPopup(title, url, param);

    },
    ShowOrderTab: function () {
        $('.service-other-detail-change-tab-button').removeClass('active')
        $('.service-other-detail-button-order').addClass('active')
        $('.service-other-detail-tab').hide()
        $('.service-other-detail-tab-order').show()
        if ($('.service-other-detail-button-order').attr('data-loaded').trim() == '0') {
            $.ajax({
                url: "/SetService/ListOrder",
                type: "Post",
                data: { id: $('#service-other-detail-data').attr('data-order-id') },
                success: function (result) {
                    debugger
                    $('.service-other-detail-tab-order').html(result);
                    $('.service-other-detail-button-order').attr('data-loaded', '1')

                }
            });
        }
    },
    ShowNeedToOrderTab: function () {
        $('.service-other-detail-change-tab-button').removeClass('active')
        $('.service-other-detail-button-need-to-order').addClass('active')
        $('.service-other-detail-tab').hide()
        $('.service-other-detail-tab-need-to-order').show()
        if ($('.service-other-detail-button-need-to-order').attr('data-loaded').trim() == '0') {
            $.ajax({
                url: "/SetService/OtherDetailBookingSale",
                type: "Post",
                data: {
                    id: $('#service-other-detail-data').attr('data-booking-id')
                },
                success: function (result) {
                    $('.service-other-detail-tab-need-to-order').html(result);
                    $('.service-other-detail-button-need-to-order').attr('data-loaded', '1')
                    $(".add-service-other-readonly-select-service").select2();
                    $(".add-service-other-readonly-select-service").prop("disabled", true);
                    _global_function.RenderFileAttachment($('.attachment-sale'), $('#service-other-detail-data').attr('data-booking-id'), _set_service_other_detail.ServiceTypeSale, false, false)

                }
            });
        }
    },
    ShowOrderedTab: function () {
        $('.service-other-detail-change-tab-button').removeClass('active')
        $('.service-other-detail-button-ordered').addClass('active')
        $('.service-other-detail-tab').hide()
        $('.service-other-detail-tab-ordered').show()
        if ($('.service-other-detail-button-ordered').attr('data-loaded').trim() == '0') {
            $.ajax({
                url: "/SetService/OtherDetailBookingOperator",
                type: "Post",
                data: {
                    id: $('#service-other-detail-data').attr('data-booking-id')
                },
                success: function (result) {
                    $('.service-other-detail-tab-ordered').html(result);
                    $('.service-other-detail-button-ordered').attr('data-loaded', '1')
                    $('.service-other-ordered-suplier').each(function (index, item) {
                        var element = $(this);
                        _common_function_other.Select2SupplierWithSearch(element)
                    });
                    _set_service_other_detail.TabReCalucateTotalAmount()
                    _common_function_other.Select2WithFixedOptionAndNoSearch($('.add-service-other-select-service'))
                    $(".add-service-other-select-service").prop("disabled", true);
                    _global_function.RenderFileAttachment($('.attachment-operator'), $('#service-other-detail-data').attr('data-booking-id'), _set_service_other_detail.ServiceType, true, false, true)
                    $('.service-other-ordered-note').attr('disabled', 'disabled')
                    $('.service-other-ordered-note').addClass('input-disabled-background')
                }
            });
        }


    },
    ShowPaymentTab: function () {
        $('.service-other-detail-change-tab-button').removeClass('active')
        $('.service-other-detail-button-payment').addClass('active')
        $('.service-other-detail-tab').hide()
        $('.service-other-detail-tab-payment').show()
        var serviceCode = $('#service-other-detail-data').attr('data-servicecode')

        $.ajax({
            url: "/SetService/ListHotelBookingpayment",
            type: "Post",
            data: {
                HotelBookingID: $('#service-other-detail-data').attr('data-booking-id'),
                serviceType: 9,
                supplierId: $('#suplier-detail').attr('data-suplier-id'),
                supplierName: $('#suplier-detail').attr('data-suplier-name'),
                orderId: $('#service-other-detail-data').attr('data-order-id'),
                amount: $('#operator-order-amount').attr('data-amount'),
                HotelBookingstatus: $('#service-other-detail-data').attr('data-status'),
                serviceCode: $('#service-other-detail-data').attr('data-servicecode')

            },
            success: function (result) {
                $('.service-other-detail-tab-payment').html(result);
            }
        });
    },
    ReloadPaymentTab: function () {
        $.ajax({
            url: "/SetService/ListHotelBookingpayment",
            type: "Post",
            data: {
                HotelBookingID: $('#service-other-detail-data').attr('data-booking-id'),
                serviceType: 9,
                supplierId: $('#suplier-detail').attr('data-suplier-id'),
                supplierName: $('#suplier-detail').attr('data-suplier-name'),
                orderId: $('#service-other-detail-data').attr('data-order-id'),
                amount: $('#operator-order-amount').attr('data-amount'),
                HotelBookingstatus: $('#service-other-detail-data').attr('data-status'),
                serviceCode: $('#service-other-detail-data').attr('data-servicecode')

            },
            success: function (result) {
                $('.service-other-detail-tab-payment').html(result);
            }
        });
    },
    ShowServiceCodeTab: function () {
        $('.service-other-detail-change-tab-button').removeClass('active')
        $('.service-other-detail-button-servicecode').addClass('active')
        $('.service-other-detail-tab').hide()
        $('.service-other-detail-tab-servicecode').show()

        $.ajax({
            url: "/SetService/ListHotelBookingCode",
            type: "Post",
            data: {
                HotelBookingstatus: $('#service-other-detail-data').attr('data-status'),
                HotelBookingID: $('#service-other-detail-data').attr('data-booking-id'),
                Type: 9
            },
            success: function (result) {
                $('.service-other-detail-tab-servicecode').html(result);
                $("body").on('click', ".service-code-send-email", function (ev, picker) {
                    _set_service_other_detail.PopupSendEmail()

                });

            }
        });
    },
    ShowRefundTab: function () {
        $('.service-other-detail-change-tab-button').removeClass('active')
        $('.service-other-detail-button-refund').addClass('active')
        $('.service-other-detail-tab').hide()
        $('.service-other-detail-tab-refund').show()

        $.ajax({
            url: "/SetService/ListPaymentRequestByClient",
            type: "Post",
            data: {
                clientId: $('#service-other-detail-data').attr('data-clientid'),
                bookingId: $('#service-other-detail-data').attr('data-booking-id'),
                amount: $('#service-other-detail-data').attr('data-amount'),
                orderId: $('#service-other-detail-data').attr('data-orderid'),
                bookingstatus: $('#service-other-detail-data').attr('data-status'),
                serviceType: 9
            },
            success: function (result) {
                $('.service-other-detail-tab-refund').html(result);

            }
        });
    },
    MarketingTab: function () {
        $('.service-other-detail-change-tab-button').removeClass('active')
        $('.service-other-detail-button-marketing').addClass('active')
        $('.service-other-detail-tab').hide()
        $('.service-other-detail-tab-marketing').show()

        $.ajax({
            url: "/SetService/MarketingPaymentRequest",
            type: "Post",
            data: {
                clientId: $('#service-other-detail-data').attr('data-clientid'),
                bookingId: $('#service-other-detail-data').attr('data-booking-id'),
                amount: $('#service-other-detail-data').attr('data-amount'),
                orderId: $('#service-other-detail-data').attr('data-orderid'),
                bookingstatus: $('#service-other-detail-data').attr('data-status'),
                serviceType: 9,
                serviceId: $('#service-other-detail-data').attr('data-booking-id'),
                serviceCode: $('#service-other-detail-data').attr('data-servicecode'),
            },
            success: function (result) {
                $('.service-other-detail-tab-marketing').html(result);

            }
        });
    },
    otherDetailBookingOrderedInitialization: function () {

        _set_service_other_detail.TabReCalucateTotalAmount()
        _set_service_other_detail.TabServiceOrderedDynamicBind()
        _common_function_other.Select2SupplierWithSearch($('.service-other-ordered-suplier'))
        $('.service-other-ordered-summary-row').find('td').eq(1).html('<a href="javascript:;" class="blue ml-2 mb10 service-other-ordered-add-new-disabled service-other-ordered-add-new" style="display:none;"><i class="fa fa-plus-circle green"></i> Thêm dòng</a>')
        $('.service-other-ordered-summary-row').find('td').eq(6).css('text-align', 'right')
        $('.service-other-ordered-summary-row').find('td').eq(6).addClass('service-other-ordered-total-amount')
    },
    TabServiceOrderedDynamicBind: function () {
        /*
        $("body").on('keyup', ".service-other-packages-unit-price", function () {
            _set_service_other_detail.TabReCalucateTotalAmount()

        });*/
        $("body").on('keyup', ".service-other-packages-baseprice-operator, .service-other-packages-quantity ", function () {
            var element = $(this)
            var row_element = element.closest('.service-other-ordered-row')
            _set_service_other_detail.CalucateRowAmount(row_element)
            _set_service_other_detail.TabReCalucateTotalAmount()

        });
        $("body").on('click', ".service-other-ordered-delete-row", function () {
            var element = $(this)
            var row_element = element.closest('.service-other-ordered-row')
            row_element.remove()
            _set_service_other_detail.TabReCalucateTotalAmount()
            _set_service_other_detail.ReIndexPackages()

        });
        $("body").on('click', ".service-other-ordered-add-new", function () {
            _set_service_other_detail.AddNewOrderedExtraPackage()
        });
        $("body").on('click', ".update-operator-order-amount", function () {
            $('.service-other-ordered-package-name').removeAttr('disabled')
            $('.service-other-ordered-package-name').removeClass('input-disabled-background')
            $('.service-other-ordered-suplier').removeAttr('disabled')
            $('.service-other-ordered-suplier').removeClass('input-disabled-background')

            $('.service-other-ordered-status').removeAttr('disabled')
            $('.service-other-ordered-status').removeClass('input-disabled-background')

            $('.service-other-packages-baseprice-operator').removeAttr('disabled')
            $('.service-other-packages-baseprice-operator').removeClass('input-disabled-background')
            $('.service-other-packages-quantity').removeAttr('disabled')
            $('.service-other-packages-quantity').removeClass('input-disabled-background')

            $('.service-other-ordered-note').removeAttr('disabled')
            $('.service-other-ordered-note').removeClass('input-disabled-background')
            $('.service-other-ordered-note').css('background', '')
            $('.service-other-ordered-delete-row').show()
            $('.service-other-ordered-add-new').show()
            $('.update-operator-order-amount-save').show()
            $('.update-operator-order-amount-cancel').show()
            $('.update-operator-order-amount').hide()
        });
        $("body").on('click', ".update-operator-order-amount-save", function () {

            _set_service_other_detail.UpdateOperatorPrice();


        });
        $("body").on('click', ".update-operator-order-amount-cancel", function () {
            $('.service-other-detail-button-ordered').attr('data-loaded', '0')
            _set_service_other_detail.ReloadOrderedTab();

        });
        $("body").on('change', ".service-other-ordered-status", function () {
            var element = $(this)
            var row_element = element.closest('.service-other-ordered-row')
            _set_service_other_detail.CalucateRowAmount(row_element)
            _set_service_other_detail.TabReCalucateTotalAmount()

        });
    },
    ReloadOrderedTab: function () {
        $("body").off('keyup', ".service-other-extrapackage-amount", null);
        $("body").off('keyup', ".service-other-ordered-delete-add-new", null);
        $("body").off('keyup', ".service-other-ordered-delete-row", null);
        $("body").off('click', ".service-other-ordered-add-new", null);
        $("body").off('click', ".update-operator-order-amount", null);
        $("body").off('click', ".update-operator-order-amount-save", null);
        $("body").off('click', ".update-operator-order-amount-cancel", null);
        $("body").on('keyup', ".service-other-packages-baseprice-operator, .service-other-packages-quantity ", null);
        _set_service_other_detail.ShowOrderedTab()
        _set_service_other_detail.ReloadPaymentTab()

    },
    ReIndexPackages: function () {
        var index = 0
        $('.service-other-ordered-order').each(function (index, item) {
            var element = $(item);
            element.html(_global_function.Comma(++index))
        });
    },
    GetLastestSetServiceOrderedTable: function () {
        var last_order = 0;
        if (!$('.service-other-ordered-order')[0]) return 0;
        //-- Get Lastest Order
        $('.service-other-ordered-order').each(function (index, item) {
            last_order++
        });
        return last_order;
    },
    CalucateRowAmount: function (row_element) {
        var base_price_val = row_element.find('.service-other-packages-baseprice-operator').val().replaceAll(',', '')
        var quantity_val = row_element.find('.service-other-packages-quantity').val().replaceAll(',', '')
        var base_price = (base_price_val == undefined ? 0 : parseFloat(base_price_val))
        var quantity = (quantity_val == undefined ? 0 : parseInt(quantity_val))
        var total = base_price * quantity
        row_element.find('.service-other-packages-unit-price').val(_global_function.Comma(total));
        row_element.find('.service-other-packages-unit-price').attr('data-amount', total);

    },
    TabReCalucateTotalAmount: function () {
        var total = 0;
        $('.service-other-packages-unit-price').each(function (index, item) {
            var element = $(this);
            var element2 = element.closest('.service-other-ordered-row').find('.service-other-ordered-status');
            if (element.val() != undefined && element.val().trim() != '') {
                if (parseFloat(element2.val()) != 1)
                    total += parseFloat(element.val().replaceAll(',', ''))
            }

        });
        $('.service-other-ordered-total-amount').html('' + _global_function.Comma(total));
        $('.service-other-ordered-total-amount').attr('data-amount', total);

    },
    AddNewOrderedExtraPackage: function () {
        var lastest_order = _set_service_other_detail.GetLastestSetServiceOrderedTable()
        var code = _set_service_other_detail.GenerateNewPackageOptionalCode()
        var html_append = _set_service_other_html.html_ordered_new_packages.replaceAll('@(++index)', ('' + (lastest_order + 1))).replaceAll('@item.PackageName', code)
        $('.service-other-ordered-summary-row').before(html_append)
        _set_service_other_detail.ReIndexPackages()
        _common_function_other.Select2SupplierWithSearch($('.service-other-ordered-suplier-new'))
        $('.service-other-ordered-suplier-new').removeClass('service-other-ordered-suplier-new')
    },
    GenerateNewPackageOptionalCode: function () {
        var code = ''
        var list_exists = []
        var count = 0
        $('.service-other-ordered-package-name').each(function (index, item) {
            var element = $(item)
            count++
            list_exists.push(element.text().trim())
        });
        var service_code = $('.set-service-other-packages-optional-tbody').attr('data-code')
        var no_duplicate = true
        for (var i = (count + 1); i < 300; i++) {
            code = service_code + '-' + i
            no_duplicate = true
            $('.service-other-ordered-package-name').each(function (index, item) {
                var element = $(item)
                if (element.text().trim() == code.trim()) {
                    no_duplicate = false
                    return false
                }
            });
            if (no_duplicate) break;
        }
        if (no_duplicate) return code
        else {
            return ''
        }
    },
    PopupChangeOperator: function (operator_name) {
        if ($('#change_operator_popup').length) {
            $('#change_operator_popup').removeClass('show')
            setTimeout(function () {
                $('#change_operator_popup').remove();
            }, 300);

        }
        $.ajax({
            url: "/SetService/OtherDetailBookingChangeOperator",
            type: "post",
            data: {
                operator_name: operator_name,
                booking_id: $('#service-other-detail-data').attr('data-booking-id')
            },
            success: function (result) {
                $('body').append(result);
                setTimeout(function () {
                    $('#change_operator_popup').addClass('show')
                }, 300);

            }
        });
    },
    PopupChangeOperatorInit: function () {
        _set_service_other_detail.UserSuggesstionMultiple($('.service-other-change-operator'))
    },
    PopupChangeOperatorClose: function () {
        $('#change_operator_popup').removeClass('show')
        setTimeout(function () {
            $('#change_operator_popup').remove();
        }, 300);
    },
    ChangeServiceOperator: function () {
        var operator_id = $('.service-other-change-operator option:selected').val()
        if (operator_id == undefined || operator_id.trim() == '') {
            _msgalert.error('Vui lòng chọn điều hành viên mới')
            return
        }
        $.ajax({
            url: "/SetService/SummitOtherDetailChangeOperator",
            type: "Post",
            data: {
                booking_id: $('.form-operator-change').attr('data-id'),
                user_id: operator_id
            },
            success: function (result) {
                if (result.status === 0) {
                    _msgalert.success(result.msg);
                    _set_service_other_detail.PopupChangeOperatorClose()
                    setTimeout(function () {
                        window.location.reload();
                    }, 500)
                }
                else {
                    _msgalert.error(result.smg);

                }

            }
        });
    },
    UpdateOperatorPrice: function () {
        debugger
        var obj_summit = []
        if (!$('.service-other-ordered-row')[0]) {
            _msgalert.error('Vui lòng nhập ít nhất bản ghi dịch vụ tại bảng kê dịch vụ')
            return
        }
        var is_success = true
        $('.service-other-ordered-row').each(function (index, item) {
            var element = $(item)
            var id = element.attr('data-id')
            var package_name = element.find('.service-other-ordered-package-name').html()
            var suplier_id = element.find('.service-other-ordered-suplier').find(':selected').val()
            var amount = element.find('.service-other-packages-unit-price').val().replaceAll(',', '')
            var base_price = element.find('.service-other-packages-baseprice-operator').val().replaceAll(',', '')
            var quantity = element.find('.service-other-packages-quantity').val().replaceAll(',', '')
            var status = element.find('.service-other-ordered-status').val()
            if (isNaN(parseFloat(element.find('.service-other-packages-unit-price').val().replaceAll(',', '')))) {
                _msgalert.error('Tổng thành tiền cho nhà cung cấp phải là số')
                is_success = false
                return false
            }
            if (suplier_id == undefined || suplier_id.trim() == '') {
                _msgalert.error('Vui lòng nhập & chọn thông tin nhà cung cấp')
                is_success = false

                return false
            }
            var note = element.find('.service-other-ordered-note').val()
            var go_id = $('.set-service-other-packages-optional-tbody').attr('data-go-id')
            var item_push = {
                Id: id,
                BookingId: go_id,
                SuplierId: suplier_id,
                Amount: amount,
                Note: note,
                PackageName: package_name,
                BasePrice: base_price == undefined ? amount : base_price,
                Quantity: quantity == undefined ? '1' : quantity,
                Status: status
            }
            obj_summit.push(item_push)
        });
        if (!is_success) return;
        $.ajax({
            url: "/SetService/UpdateOtherOperatorOrderPrice",
            type: "post",
            data: { data: obj_summit },
            success: function (result) {
                if (result != undefined && result.status == 0) {
                    _msgalert.success(result.msg)
                    $('#operator-order-amount').html(':  ' + _global_function.Comma(result.amount))
                    $('#operator-order-amount').attr('data-amount', result.amount)
                    $('#operator-total-profit').html(':  ' + _global_function.Comma(result.profit))

                    $('.service-other-ordered-package-name').attr('disabled', 'disabled')
                    $('.service-other-ordered-package-name').addClass('input-disabled-background')
                    $('.service-other-ordered-suplier').attr('disabled', 'disabled')
                    $('.service-other-ordered-suplier').addClass('input-disabled-background')
                    $('.service-other-ordered-status').attr('disabled', 'disabled')
                    $('.service-other-ordered-status').addClass('input-disabled-background')
                    $('.service-other-packages-unit-price').attr('disabled', 'disabled')
                    $('.service-other-packages-unit-price').addClass('input-disabled-background')
                    $('.service-other-packages-baseprice-operator').attr('disabled', 'disabled')
                    $('.service-other-packages-baseprice-operator').addClass('input-disabled-background')
                    $('.service-other-packages-quantity').attr('disabled', 'disabled')
                    $('.service-other-packages-quantity').addClass('input-disabled-background')
                    $('.service-other-ordered-note').attr('disabled', 'disabled')
                    $('.service-other-ordered-note').addClass('input-disabled-background')
                    $('.service-other-ordered-delete-row').hide()
                    $('.service-other-ordered-add-new').hide()
                    $('.update-operator-order-amount-save').hide()
                    $('.update-operator-order-amount-cancel').hide()
                    $('.update-operator-order-amount').show()
                    _set_service_other_detail.ReloadOrderedTab();
                    _set_service_other_detail.otherDetailBookingOrderedInitialization();

                }
                else {
                    _msgalert.error(result.msg)

                }

            }
        });
    },
    UserSuggesstionMultiple: function (element) {
        var selected = element.find(':selected').val()
        var placeholder = element.attr('placeholder')
        $.ajax({
            url: "/Order/UserSuggestion",
            type: "post",
            data: { txt_search: "" },
            success: function (result) {
                if (result != undefined && result.data != undefined && result.data.length > 0) {
                    result.data.forEach(function (item) {
                        element.append(_set_service_other_html.html_user_option.replaceAll('{user_id}', item.id).replace('{user_email}', item.email).replace('{user_name}', item.username).replace('{user_phone}', item.phone == undefined ? "" : ' - ' + item.phone))

                    });
                    element.val(selected).trigger('change');
                }
                else {
                    element.trigger('change');

                }
                element.select2({
                    placeholder: placeholder,
                    allowClear: true
                });
            }
        });
    },
    GrantOrderPermission: function (element) {
        element.attr('disabled', true)
        var booking_id = $('#service-other-detail-data').attr('data-booking-id')
        var service_code = $('#service-other-detail-data').attr('data-servicecode')
        _msgconfirm.openDialog(_set_service_other_html.confirm_box_title_grant_order, _set_service_other_html.confirm_box_description_grant_order.replace('{Mã dịch vụ}', service_code), function () {
            $.ajax({
                url: "/SetService/GrantOtherServiceOrderPermission",
                type: "post",
                data: { booking_id: booking_id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () { window.location.reload() }, 500)
                    }
                    else {
                        _msgalert.error(result.msg);
                    }

                }
            });

        });
        element.attr('disabled', false)
    },
    SendServiceCode: function (element) {
        element.attr('disabled', true)
        var booking_id = $('#service-other-detail-data').attr('data-booking-id')
        var service_code = $('#service-other-detail-data').attr('data-servicecode')
        _msgconfirm.openDialog(_set_service_other_html.confirm_box_title_send_service_code, _set_service_other_html.confirm_box_description_send_service_code.replace('{Mã dịch vụ}', service_code), function () {
            $.ajax({
                url: "/SetService/OtherServiceSendServiceCode",
                type: "post",
                data: { booking_id: booking_id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () { window.location.reload() }, 500)

                    }
                    else {
                        _msgalert.error(result.msg);
                        element.removeAttr('disabled');
                    }

                }
            });
        });
        element.removeAttr('disabled');

    },
    ChangeToPayment: function (element) {
        element.attr('disabled', true)
        var booking_id = $('#service-other-detail-data').attr('data-booking-id')
        var service_code = $('#service-other-detail-data').attr('data-servicecode')
        _msgconfirm.openDialog(_set_service_other_html.confirm_box_title_change_to_payment, _set_service_other_html.confirm_box_description_change_to_payment.replace('{Mã dịch vụ}', service_code), function () {
            $.ajax({
                url: "/SetService/OtherServiceChangeToConfirmPaymentStatus",
                type: "post",
                data: { booking_id: booking_id },
                success: function (result) {
                    if (result != undefined && result.status == 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () { window.location.reload() }, 500)

                    }
                    else {
                        _msgalert.error(result.msg);
                        element.removeAttr('disabled');

                    }
                }
            });
        });
        element.removeAttr('disabled');

    },
    PopupSendEmailSupplier: function () {
        let title = 'Gửi Email nhà cung cấp';
        let url = '/SetService/SendEmailSupplier';
        let param = {
            id: $('#service-other-detail-data').attr('data-booking-id').trim(),
            Orderid: $('#service-other-detail-data').attr('data-order-id').trim(),
            type: 8,
            SupplierId: 0,
            ServiceType: 9,
        };
        _magnific.OpenSmallPopup(title, url, param);

    },
}
var _common_function_other = {


    OrderNoSuggesstion: function (element) {

        element.select2({
            theme: 'bootstrap4',
            placeholder: "Mã đơn hàng",
            minimumInputLength: 1,
            allowClear: true,
            tags: true,
            ajax: {
                url: "/SetService/OrderSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
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
                                text: item.orderno,
                                id: item.orderno,
                            }
                        })
                    };
                },
                createTag: function (params) {
                    let term = $.trim(params.term);
                    return {
                        id: term,
                        text: term,
                        newTag: true,
                    }
                },
                cache: true
            }
        });
    },

    ServiceCodeSuggesstion: function (element) {
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Mã dịch vụ",
            minimumInputLength: 1,
            allowClear: true,
            ajax: {
                url: "/SetService/OtherServiceCodeSuggestion",
                type: "post",
                dataType: 'json',
                delay: 1000,
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
                                text: item.serviceCode,
                                id: item.serviceCode,
                            }
                        })
                    };
                },

                createTag: function (params) {
                    let term = $.trim(params.term);
                    return {
                        id: term,
                        text: term,
                        newTag: true,
                    }
                },
                cache: true
            }
        });

    },
    UserSuggesstionMultiple: function (element) {
        var selected = element.find(':selected').val()
        var placeholder = element.attr('placeholder')
        $.ajax({
            url: "/Order/UserSuggestion",
            type: "post",
            data: { txt_search: "" },
            success: function (result) {
                if (result != undefined && result.data != undefined && result.data.length > 0) {
                    result.data.forEach(function (item) {
                        element.append(_set_service_other_html.html_user_option.replaceAll('{user_id}', item.id).replace('{user_email}', item.email).replace('{user_name}', item.username).replace('{user_phone}', item.phone == undefined ? "" : ' - ' + item.phone))

                    });
                    element.val(selected).trigger('change');
                }
                else {
                    element.trigger('change');

                }
                element.select2({
                    placeholder: placeholder,
                    allowClear: true
                });
            }
        });
    },
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
    Select2WithFixedOptionAndNoSearch: function (element) {
        var placeholder = element.attr('placeholder')
        element.select2({
            placeholder: placeholder,
            allowClear: true,
            minimumResultsForSearch: Infinity
        });
    },
    SingleDateRangePicker: function (element) {

        var today = new Date();
        var yyyy = today.getFullYear();
        var yyyy_max = yyyy + 10;
        var current_date = element.val();
        var min_range = '01/01/2020';
        var max_range = '31/12/' + yyyy_max;

        element.daterangepicker({
            autoApply: true,
            autoUpdateInput: false,
            showDropdowns: true,
            drops: 'down',
            minDate: min_range,
            maxDate: max_range,
            locale: {
                format: 'DD/MM/YYYY',
                cancelLabel: 'Clear'

            }
        }, function (start, end, label) {


        });

        //.data('daterangepicker').setStartDate(current_date);
    },
    OnApplyStartDateTimeOfBookingRange: function (start_date_element, end_date_element) {
        var today = new Date();

        var min_range = _global_function.GetDayTextDateRangePicker(start_date_element.data('daterangepicker').startDate._d);
        var yyyy = today.getFullYear();
        var yyyy_max = yyyy + 10;
        var max_range = '31/12/' + yyyy_max;
        end_date_element.daterangepicker({
            autoApply: true,
            autoUpdateInput: false,
            showDropdowns: true,
            drops: 'down',
            minDate: min_range,
            maxDate: max_range,
            locale: {
                format: 'DD/MM/YYYY',
                cancelLabel: 'Clear'

            }
        }, function (start, end, label) {


        });
    },
    Select2SupplierWithSearch: function (element) {
        var selected = element.find(':selected').val()
        var selected_text = element.find('option').text()
        if (selected != null && selected != undefined) {
            var s_ele = element.find('option');
            selected = s_ele.val()
            selected_text = s_ele.html()
        }
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC ",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            minimumInputLength: 0,
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
                                text: item.id + ' - ' + item.fullname,
                                id: item.id,
                            }
                        })
                    };
                },
                cache: true
            }
        });
        if (selected != null && selected != undefined) {
            element.select2('data', { id: selected, text: selected_text });
        }
        else {
            element.val(selected).change()
        }

    },
    Select2BookingCode: function (element) {
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Mã Code",

            allowClear: true,
            tags: true,
            ajax: {
                url: "/SetService/BookingCodeSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        txt_search: params.term,
                        type: 9,
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: item.bookingcode,
                                id: item.bookingcode,
                            }
                        })
                    };
                },
                createTag: function (params) {
                    let term = $.trim(params.term);
                    return {
                        id: term,
                        text: term,
                        newTag: true,
                    }
                },
                cache: true
            }
        });
    },
}

let isPickerApprove = false;
let isPickerApprove2 = false;
let isPickerApprove3 = false;
let Isstatus = 1;
var is_dynamic_bind = false;
var loading = {
    EditMode: false
}
var _SetService_Detail = {
    HTML: {
        html_loading_export: '<a class="loading_export_vinwonder" href="javascript:;"><img src="/images/icons/loading.gif" style="width: 30px;height: 30px; margin-left:15px;" class="img_loading_summit coll"><nw id="loading_exportvw" style="color:red;">Đang xuất vé dịch vụ, vui lòng không đóng cửa sổ này </nw> </a>',
        html_loading_export_text: 'Đang xuất vé dịch vụ tự động, vui lòng không đóng cửa sổ này',
        Loading: '',
        LoadingSuccess: false
    },
    ServiceType: 13,
    ServiceTypeSale: 1,
    loadDataDetail: function () {
        let _searchModel = {
            id: $('#Orderid').val(),
            HotelBookingID: $('#HotelBookingID').val(),
            ContactClientId: $('#ContactClientId').val(),
            serviceType: 1,
            HotelBookingstatus: $('#HotelBookingstatus').val(),
            type: 1,
            supplierId: $('#suplier-detail').attr('data-suplier-id'),
            supplierName: $('#suplier-detail').attr('data-suplier-name'),
            orderId: $('#Orderid').val(),
            amount: $('#total_amount').val(),
            serviceCode: $('#ServiceCode').val(),
            clientId: $('#client_id').val(),
            bookingId: $('#HotelBookingID').val(),
            bookingstatus: $('#StatusBooking').val(),
            serviceId: $('#HotelBookingID').val(),
        };

        _SetService_Detail.hotelBookingDetail(_searchModel);
        _SetService_Detail.SearchOrder(_searchModel);
        _SetService_Detail.ListHotelServicesOrder(_searchModel);
        _SetService_Detail.ListHotelServicesbooked(_searchModel);
        _SetService_Detail.ListHotelBookingCode(_searchModel);
        _SetService_Detail.ListHotelBookingRefund(_searchModel);
        _SetService_Detail.ListMarketingPaymentRequest(_searchModel);

        $("body").on('click', ".service-code-send-email2", function (ev, picker) {
            _SetService_Detail.PopupSendEmail2()

        });
        $("body").on('click', ".service-code-get-vin-code", function (ev, picker) {
            _SetService_Detail.ExportVinHotelCode($('.service-code-get-vin-code').attr('data-booking-id'))
        });
        switch ($('#HotelBookingstatus').val()) {
            case '1':
            case '2': {
                if (!$('.service-fly-detail-button-ordered').hasClass('active')) {
                    _SetService_Detail.OnStatuse(2); _SetService_Detail.SetActive(2)
                }
            } break;
            case '3':
            case '4': {
                if (!$('.service-fly-detail-button-payment').hasClass('active')) {
                    _SetService_Detail.OnStatuse(4); _SetService_Detail.SetActive(4)
                }
            } break;
            default: {
                if (!$('.service-fly-detail-button-order').hasClass('active')) {
                    _SetService_Detail.OnStatuse(1); _SetService_Detail.SetActive(1)
                }
            } break;
        }
    },
    ExportVinHotelCode: function (booking_id) {
        var title = 'Xác nhận xuất code khách sạn Vinpearl'
        var description = 'Dịch vụ khách sạn Vinpearl sẽ được xuất code tự động, bạn có chắc chắn không?'


        _msgconfirm.openDialog(title, description, function () {
            $('.service-code-get-vin-code').after(_SetService_Detail.HTML.html_loading_export)
            _SetService_Detail.LoopDisplayLoading()
            $('#add-service-code').hide()
            $('.service-code-send-email2').hide()
            $('.service-code-send-email').hide()
            $('.service-code-get-vin-code').hide()
            $.ajax({
                url: "/SetService/ExportVinHotelCode",
                type: "post",
                data: {
                    booking_id: booking_id
                },
                success: function (result) {
                    if (result.status === 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            _SetService_Detail.ListHotelBookingCode(_searchModel);
                        }, 1000)
                    }
                    else {
                        _msgalert.error(result.msg);
                    }
                    _SetService_Detail.HTML.LoadingSuccess = true
                    $('.loading_export_vinwonder').remove()
                    $('#add-service-code').show()
                    $('.service-code-send-email2').show()
                    $('.service-code-send-email').show()
                    $('.service-code-get-vin-code').show()
                }
            });
        })
    },
    LoopDisplayLoading: function () {
        setTimeout(function () {
            _SetService_Detail.Loading = _SetService_Detail.Loading + '.'
            $('#loading_exportvw').html(_SetService_Detail.HTML.html_loading_export_text + _SetService_Detail.Loading)
            if (_SetService_Detail.Loading.trim() == '....') _SetService_Detail.Loading = ''
            if (!_SetService_Detail.HTML.LoadingSuccess) _SetService_Detail.LoopDisplayLoading()
        }, 1000)
    },
    loadTourDetail: function () {
        let _searchModel = {
            id: $('#Orderid').val(),
            HotelBookingID: $('#HotelBookingID').val(),
            ContactClientId: $('#ContactClientId').val(),
            serviceType: 1,
            /* HotelBookingstatus: $('#HotelBookingstatus').val(),*/
            type: 1,
            supplierId: $('#suplier-detail').attr('data-suplier-id'),
            supplierName: $('#suplier-detail').attr('data-suplier-name'),
            orderId: $('#Orderid').val(),
            serviceCode: $('#ServiceCode').val(),
            amount: $('#operator-order-amount').attr('data-amount'),
            HotelBookingstatus: $('#service-fly-detail-data').attr('data-status'),

        };
        _SetService_Detail.SearchOrder(_searchModel);
        _SetService_Detail.ListHotelBookingpayment(_searchModel);
    },
    loadListHotelBookingRefund: function () {
        let _searchModel = {
            id: $('#Orderid').val(),
            HotelBookingID: $('#HotelBookingID').val(),
            ContactClientId: $('#ContactClientId').val(),
            serviceType: 1,
            HotelBookingstatus: $('#HotelBookingstatus').val(),
            type: 1,
            supplierId: $('#suplier-detail').attr('data-suplier-id'),
            supplierName: $('#suplier-detail').attr('data-suplier-name'),
            orderId: $('#Orderid').val(),
            amount: $('#total_amount').val(),
            serviceCode: $('#ServiceCode').val(),
            clientId: $('#client_id').val(),
            bookingId: $('#HotelBookingID').val(),
            bookingstatus: $('#StatusBooking').val(),
        };

        _SetService_Detail.ListHotelBookingRefund(_searchModel, 1);

    },
    SearchOrder: function (input) {
        $.ajax({
            url: "/SetService/ListOrder",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_order').hide();
                $('#grid_data_order').html(result);
            }
        });
    },
    ListHotelServicesOrder: function (input) {
        $.ajax({
            url: "/SetService/ListHotelServicesOrder",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_ListHotelServicesOrder').hide();
                $('#grid_data_ListHotelServicesOrder').html(result);
                _global_function.RenderFileAttachment($('.attachment-sale'), input.HotelBookingID, _SetService_Detail.ServiceTypeSale, false, false)

            }
        });
    },
    ListHotelServicesbooked: function (input) {
        input.status = 0;
        $.ajax({
            url: "/SetService/ListHotelServicesbooked",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_ListHotelServicesbooked').hide();
                $('#grid_data_ListHotelServicesbooked').html(result);
                if (!is_dynamic_bind) {
                    _SetService_Detail.DynamicBindHotelServiceBooked()
                    is_dynamic_bind = true
                }
                //$('.text-center').each(function (index, item) {
                //    var element = $(item);
                //    if (element.text().trim() == 'Trạng thái') {
                //        element.hide()
                //    }
                //});

                _set_service_hotel_booked_function.RecorrectServiceBookedPrice()
                _global_function.RenderFileAttachment($('.attachment-operator'), input.HotelBookingID, _SetService_Detail.ServiceType, true, false, true)

            }
        });
    },
    hotelBookingDetail: function (input) {
        $.ajax({
            url: "/SetService/HotelServiceDetai",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_hotelBookingDetail').hide();
                $('#grid_data_hotelBookingDetail').html(result);
                input.amount = $('#operator-order-amount').attr('data-amount');
                _SetService_Detail.ListHotelBookingpayment(input);
            }
        });
    },
    ListHotelBookingCode: function (input) {

        $.ajax({
            url: "/SetService/ListHotelBookingCode",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_HotelBookingCode').hide();
                $('#grid_data_HotelBookingCode').html(result);
                $("body").on('click', ".service-code-send-email", function (ev, picker) {
                    _SetService_Detail.PopupSendEmail()

                });
            }
        });
    },
    ListHotelBookingpayment: function (input) {

        $.ajax({
            url: "/SetService/ListHotelBookingpayment",
            type: "Post",
            data: input,
            success: function (result) {
                $('#imgLoading_HotelBookingpayment').hide();
                $('#grid_data_HotelBookingpayment').html(result);
            }
        });
    },
    ListHotelBookingRefund: function (input, type) {
        $.ajax({
            url: "/SetService/ListPaymentRequestByClient",
            type: "Post",
            data: input,
            success: function (result) {
                $('#grid_data_HotelBookingRefund').hide();
                $('#grid_data_HotelBookingRefund').html(result);
                if (type == 1) {
                    $('#grid_data_HotelBookingRefund').show();
                }
            }
        });

    },
    LoadingMarketingPaymentRequest: function () {
        let _searchModel = {
            id: $('#Orderid').val(),
            HotelBookingID: $('#HotelBookingID').val(),
            ContactClientId: $('#ContactClientId').val(),
            serviceType: 1,
            HotelBookingstatus: $('#HotelBookingstatus').val(),
            type: 1,
            supplierId: $('#suplier-detail').attr('data-suplier-id'),
            supplierName: $('#suplier-detail').attr('data-suplier-name'),
            orderId: $('#Orderid').val(),
            amount: $('#total_amount').val(),
            serviceCode: $('#ServiceCode').val(),
            clientId: $('#client_id').val(),
            bookingId: $('#HotelBookingID').val(),
            bookingstatus: $('#StatusBooking').val(),
            serviceId: $('#HotelBookingID').val(),
        };

        _SetService_Detail.ListMarketingPaymentRequest(_searchModel, 1);
    },
    ListMarketingPaymentRequest: function (input, type) {
        $.ajax({
            url: "/SetService/MarketingPaymentRequest",
            type: "Post",
            data: input,
            success: function (result) {
                $('#grid_data_Marketing').hide();
                $('#grid_data_Marketing').html(result);
                if (type == 1) {
                    $('#grid_data_Marketing').show();
                }
            }
        });

    },
    SetActive: function (status) {
        Isstatus = status;
        $('#data_order').removeClass('active')
        $('#dataListHotelServicesOrder').removeClass('active')
        $('#data_ListHotelServicesbooked').removeClass('active')
        $('#data_HotelBookingCode').removeClass('active')
        $('#data_payment_account').removeClass('active')
        $('#data_HotelRefund').removeClass('active')
        $('#data_Marketing').removeClass('active')

        if (status == 1)
            $('#data_order').addClass('active')
        if (status == 2)
            $('#dataListHotelServicesOrder').addClass('active');
        if (status == 3)
            $('#data_ListHotelServicesbooked').addClass('active')
        if (status == 4)
            $('#data_payment_account').addClass('active')
        if (status == 5)
            $('#data_HotelBookingCode').addClass('active')
        if (status == 6)
            $('#data_HotelRefund').addClass('active')
        if (status == 7)
            $('#data_Marketing').addClass('active')
    },
    OnStatuse: function (value) {
        if (value == 1) {
            $('#grid_data_order').show();
            $('#grid_data_ListHotelServicesOrder').hide();
            $('#grid_data_ListHotelServicesbooked').hide();
            $('#grid_data_HotelBookingCode').hide();
            $('#grid_data_HotelBookingpayment').hide();
            $('#grid_data_HotelBookingRefund').hide();
            $('#grid_data_Marketing').hide();
        }
        if (value == 2) {
            $('#grid_data_order').hide();
            $('#grid_data_ListHotelServicesOrder').show();
            $('#grid_data_ListHotelServicesbooked').hide();
            $('#grid_data_HotelBookingCode').hide();
            $('#grid_data_HotelBookingpayment').hide();
            $('#grid_data_HotelBookingRefund').hide();
            $('#grid_data_Marketing').hide();

        }
        if (value == 3) {
            $('#grid_data_order').hide();
            $('#grid_data_ListHotelServicesOrder').hide();
            $('#grid_data_ListHotelServicesbooked').show();
            $('#grid_data_HotelBookingCode').hide();
            $('#grid_data_HotelBookingpayment').hide();
            $('#grid_data_HotelBookingRefund').hide();
            $('#grid_data_Marketing').hide();
        }
        if (value == 4) {
            $('#grid_data_order').hide();
            $('#grid_data_ListHotelServicesOrder').hide();
            $('#grid_data_ListHotelServicesbooked').hide();
            $('#grid_data_HotelBookingCode').hide();
            $('#grid_data_HotelBookingpayment').show();
            $('#grid_data_HotelBookingRefund').hide();
            $('#grid_data_Marketing').hide();
        }
        if (value == 5) {
            $('#grid_data_order').hide();
            $('#grid_data_ListHotelServicesOrder').hide();
            $('#grid_data_ListHotelServicesbooked').hide();
            $('#grid_data_HotelBookingpayment').hide();
            $('#grid_data_HotelBookingCode').show();
            $('#grid_data_HotelBookingRefund').hide();
            $('#grid_data_Marketing').hide();
        }
        if (value == 6) {
            $('#grid_data_order').hide();
            $('#grid_data_ListHotelServicesOrder').hide();
            $('#grid_data_ListHotelServicesbooked').hide();
            $('#grid_data_HotelBookingpayment').hide();
            $('#grid_data_HotelBookingCode').hide();
            $('#grid_data_HotelBookingRefund').show();
            $('#grid_data_Marketing').hide();
        }
        if (value == 7) {
            $('#grid_data_order').hide();
            $('#grid_data_ListHotelServicesOrder').hide();
            $('#grid_data_ListHotelServicesbooked').hide();
            $('#grid_data_HotelBookingpayment').hide();
            $('#grid_data_HotelBookingCode').hide();
            $('#grid_data_HotelBookingRefund').hide();
            $('#grid_data_Marketing').show();
        }

    },
    UpdateOrderStatus: function (id, status, Orderid) {
        var ServiceCode = $('#ServiceCode').val();
        var amount = $('#operator-order-amount').attr('data-amount')
        var Saler = $('#Saler-Name').attr('data-sale')

        var user = $('#user').val()
        var title = 'Nhận đặt dịch vụ';
        var title2 = 'Nhận đặt dịch vụ ';
        if (status == 4) {
            title = 'Quyết toán';
            title2 = 'Quyết toán dịch vụ khách sạn ';
        }
        if (status == 3) {
            title = 'Trả code dịch vụ khách sạn';
            title2 = 'Trả code dịch vụ khách sạn ';
        }
        if (status == 5) {
            title = 'Từ chối';
            title2 = 'Từ chối dịch vụ khách sạn ';
        }
        _msgconfirm.openDialog(title, title2 + ServiceCode + ' không?', function () {
            $.ajax({
                url: "/SetService/UpdateOrderStatus",
                type: "Post",
                data: { id: id, status: status, OrderId: Orderid, amount: amount },
                success: function (result) {
                    if (result.sst_status === 0) {
                        if (Saler == "") {
                            $.ajax({
                                url: "/SetService/UpdateHotelBooking",
                                type: "Post",
                                data: { id: id, OrderId: Orderid, salerId: user, type: 1 },
                                success: function (result) {
                                }
                            });
                        }
                        _msgalert.success(result.smg);
                        setTimeout(function () { window.location.reload() }, 2000)
                    }
                    else {
                        _msgalert.error(result.smg);

                    }
                }
            });
        });

    },
    TTStatus: function () {
        $('#CCthanhtien').hide();
        $('#GuiEmail').hide();
        $('#Boqua').show();
        $('#luu').show();
        $('.servicemanual-hotel-room-rates-operator-price').removeClass("input-disabled-background");
        $('.servicemanual-hotel-room-rates-operator-price').removeAttr("disabled");
        $('.servicemanual-hotel-room-rates-operator-price').removeClass("input-disabled-background");
        $('.servicemanual-hotel-room-rates-operator-price').removeAttr("disabled");

        $('.servicemanual-hotel-extrapackage-operator-price').removeClass("input-disabled-background");
        $('.servicemanual-hotel-extrapackage-operator-price').removeAttr("disabled");


        $('.servicemanual-hotel-room-suplier-id').removeAttr("disabled");
        $('.servicemanual-hotel-room-number-of-rooms').removeAttr("disabled");

        $('.servicemanual-hotel-extrapackage-suplier-id').removeAttr("disabled");
        $('.servicemanual-hotel-room-isroomfund').removeAttr("disabled");


        _set_service_hotel_booked_function.SupplierSuggesstion($('.servicemanual-hotel-room-suplier-id'))
        $('.servicemanual-hotel-extrapackage-suplier-id').each(function (index, item) {
            var element = $(item);
            _set_service_hotel_booked_function.SupplierSuggesstion(element)
        });
        $("body").on('click', ".setservice-hotel-room-clone-room", function () {
            var element = $(this);
            _SetService_Detail.CloneHotelRoom(element);
            _set_service_hotel_booked_function.CalucateTotalAmountOfHotelRoom();
            _set_service_hotel_booked_function.CalucateTotalProfitOfHotelRoom();
            _set_service_hotel_booked_function.CalucateTotalpriceOfHotelRoom();
            _set_service_hotel_booked_function.CalucateServiceAmount();
            _set_service_hotel_booked_function.CalucateServiceProfit();
        });
        $("body").on('click', ".setservice-hotel-room-delete-room", function () {
            var element = $(this);
            _SetService_Detail.DeleteHotelRoom(element);
            _set_service_hotel_booked_function.CalucateTotalAmountOfHotelRoom();
            _set_service_hotel_booked_function.CalucateTotalProfitOfHotelRoom();
            _set_service_hotel_booked_function.CalucateTotalpriceOfHotelRoom();
            _set_service_hotel_booked_function.CalucateServiceAmount();
            _set_service_hotel_booked_function.CalucateServiceProfit();
        });
        loading.EditMode = true
    },
    BQStatus: function () {

        _SetService_Detail.loadDataDetail();
        _SetService_Detail.SetActive(3);
        _SetService_Detail.OnStatuse(3);
        $("body").off('click', ".setservice-hotel-room-clone-room", null);
        $("body").off('click', ".setservice-hotel-room-delete-room", null);
        loading.EditMode = false
    },
    UpdateHotelBookingRooms: function () {
        var object_summit = {
            rooms: [],
            extra_packages: [],
        }
        var validate_failed = false
        $('.servicemanual-hotel-room-tr').each(function (index, item) {
            var element = $(item);
            var number_of_room_element = element.find('.servicemanual-hotel-room-number-of-rooms')
            var number_of_room = number_of_room_element.val() == undefined || isNaN(parseFloat(number_of_room_element.val().replaceAll(',', ''))) ? 1 : parseFloat(number_of_room_element.val().replaceAll(',', ''))
            var suplier_element = element.find('.servicemanual-hotel-room-suplier-id')
            var suplier_id = suplier_element.find(':selected').val() == undefined || isNaN(parseInt(suplier_element.find(':selected').val())) ? 0 : parseInt(suplier_element.find(':selected').val())
            var package_name = element.find('.servicemanual-hotel-room-td-package-name')
            var is_roomfund = element.find('.servicemanual-hotel-room-isroomfund').is(":checked")


            var room_summit = {
                id: element.attr('data-room-optional-id'),
                room_id: element.attr('data-room-id'),
                rates: [],
                number_of_room: number_of_room,
                suplier_id: suplier_id,
                package_name: package_name.html(),
                is_room_fund: is_roomfund
            }
            element.find('.servicemanual-hotel-room-rates-code').each(function (index_2, item_2) {
                var rate_element = $(item_2);
                var row_element = element
                var rate_id = rate_element.attr('data-rate-id')
                var operator_element = _set_service_hotel_booked_function.GetHotelRatesOperatorPrice(row_element, rate_id);
                var sale_element = _set_service_hotel_booked_function.GetHotelRatesSalePrice(row_element, rate_id)
                var nights_element = _set_service_hotel_booked_function.GetHotelRatesNights(row_element, rate_id)
                var profit_element = _set_service_hotel_booked_function.GetHotelRatesProfit(row_element, rate_id)
                var operator_amount_element = _set_service_hotel_booked_function.GetHotelRatesPrice(row_element, rate_id)
                var sale_amount_element = _set_service_hotel_booked_function.GetHotelRatesAmount(row_element, rate_id)

                var operator_price = (operator_element == undefined || operator_element.val() == undefined || isNaN(parseFloat(operator_element.val().replaceAll(',', '')))) ? 0 : parseFloat(operator_element.val().replaceAll(',', ''))
                var sale_price = isNaN(parseFloat(sale_element.val().replaceAll(',', ''))) ? 0 : parseFloat(sale_element.val().replaceAll(',', ''))
                var profit = isNaN(parseFloat(profit_element.val().replaceAll(',', ''))) ? 0 : parseFloat(profit_element.val().replaceAll(',', ''))
                var operator_amount = isNaN(parseFloat(operator_amount_element.val().replaceAll(',', ''))) ? 0 : parseFloat(operator_amount_element.val().replaceAll(',', ''))
                var sale_amount = isNaN(parseFloat(sale_amount_element.val().replaceAll(',', ''))) ? 0 : parseFloat(sale_amount_element.val().replaceAll(',', ''))
                var nights_price = isNaN(parseFloat(nights_element.val().replaceAll(',', ''))) ? 0 : parseFloat(nights_element.val().replaceAll(',', ''))

                //if (operator_price < 0) {
                //    var code_element = _set_service_hotel_booked_function.GetHotelRatesCode(row_element, rate_element.attr('data-rate-id'));
                //    _msgalert.error('Giá nhập của gói ' + code_element.val() + ' trong phòng thứ ' + row_element.find('.servicemanual-hotel-room-td-order').html() + ' phải lớn hơn / bằng 0')
                //    validate_failed = true
                //    return false
                //}

                var rate_summit = {
                    id: rate_element.attr('data-rate-id'),
                    rate_id: rate_element.attr('data-rate-sale-id'),
                    operator_price: operator_price,
                    sale_price: sale_price,
                    profit: profit,
                    operator_amount: operator_amount,
                    sale_amount: sale_amount,
                    nights: nights_price
                }
                room_summit.rates.push(rate_summit)
            })
            if (validate_failed) {
                return false
            }
            else {
                object_summit.rooms.push(room_summit)
            }
        })
        if (validate_failed) return;
        validate_failed = false
        $('.servicemanual-hotel-extrapackage-tr').each(function (index, item) {
            var element = $(item);
            var operator_element = element.find('.servicemanual-hotel-extrapackage-operator-price')
            var operator_price = (operator_element == undefined || operator_element.val() == undefined || isNaN(parseFloat(operator_element.val().replaceAll(',', '')))) ? 0 : parseFloat(operator_element.val().replaceAll(',', ''))
            //if (operator_price < 0) {
            //    _msgalert.error('Giá nhập của dịch vụ phụ thứ ' + element.find('.servicemanual-hotel-extrapackage-td-order').html() + ' phải lớn hơn / bằng 0')
            //    validate_failed = true
            //    return false
            //}

            var supplier_element = element.find('.servicemanual-hotel-extrapackage-suplier-id')
            var extra_summit = {
                id: element.attr('data-extrapackage-id'),
                operator_price: operator_price,
                unit_price: _set_service_hotel_booked_function.GetExtraPackageUnitPriceValue(element),
                supplier_id: supplier_element.val()
            }
            object_summit.extra_packages.push(extra_summit)
        })
        if (validate_failed) return;
        var hotel_booking_id = $('#servicemanual-hotel-rooms').attr('data-hotel-booking-id')
        $.ajax({
            url: '/SetService/UpdateHotelBookingUnitPrice',
            type: "post",
            data: { data: object_summit, hotel_booking_id: hotel_booking_id },
            success: function (result) {
                if (result.status === 0) {
                    _msgalert.success(result.msg);
                    _SetService_Detail.BQStatus()
                    _set_service_hotel_booked_function.RefillUpperDetailProfit()
                    _SetService_Detail.ReloadPaymentTab()
                }
                else {
                    _msgalert.error(result.msg);

                }

            }
        });
    },
    ReloadPaymentTab: function () {
        let _searchModel = {
            id: $('#Orderid').val(),
            HotelBookingID: $('#HotelBookingID').val(),
            ContactClientId: $('#ContactClientId').val(),
            serviceType: 1,
            HotelBookingstatus: $('#HotelBookingstatus').val(),
            type: 1,
            supplierId: $('#suplier-detail').attr('data-suplier-id'),
            supplierName: $('#suplier-detail').attr('data-suplier-name'),
            orderId: $('#Orderid').val(),
            amount: $('#operator-order-amount').attr('data-amount'),
            serviceCode: $('#ServiceCode').val(),

        };
        _SetService_Detail.ListHotelBookingpayment(_searchModel);

    },
    PopupAddCode: function (id, HotelBookingId, type) {
        let title = 'Thêm mới Code';
        if (id.trim() != "") {
            title = 'Sửa Code dịch vụ'
        }
        let url = '/SetService/AddPopupCode';
        let param = {
            id: 0,
            hotelbookingid: HotelBookingId,
            type: type
        };
        if (id.trim() != "") {
            param = {
                id: id,
                hotelbookingid: HotelBookingId,
                type: type
            };
        }
        _magnific.OpenSmallPopup(title, url, param);


    },
    SetUpHotelBookingCode: function () {

        let FromCreate = $('#Add-Code');
        FromCreate.validate({
            rules: {

                "BookingCode": "required",
                "Description": "required",

            },
            messages: {

                "BookingCode": "Mã không được bỏ trống",
                "Description": "Nội dung không được bỏ trống",
            }
        });
        if (FromCreate.valid()) {
            var model = {
                Id: $('#Id').val(),
                HotelBookingId: $('#HotelBookingId').val(),
                Note: $('#Note').val(),
                BookingCode: $('#BookingCode').val(),
                Description: $('#Description').val(),
                AttactFile: $('#Path').val(),
                Type: $('#Type').val(),
                OrderId: 0,
            }
            switch (model.Type) {
                case "0":
                case "1": {
                    model.OrderId = $('#Orderid').val()
                } break;
                case "3": {
                    model.OrderId = $('#service-fly-detail-data').attr('data-order-id').trim()
                } break;
                case "5": {
                    model.OrderId = $('#Orderid').val()
                } break;
                case "6": {
                    model.OrderId = $('#service-vinwonder-detail-data').attr('data-order-id').trim()
                } break;
                case "9": {
                    model.OrderId = $('#service-other-detail-data').attr('data-order-id').trim()
                } break;
            }
            $('#AddPopupCode').attr("disabled", false);
            $.ajax({
                url: "/SetService/SetUpHotelBookingCode",
                type: "Post",
                data: { model: model },
                success: function (result) {
                    if (result.sst_status === 0) {
                        _global_function.ConfirmFileUpload($('.attachment-file-block-3'), result.id)
                        _msgalert.success(result.smg);
                        $.magnificPopup.close();
                        /* _attachment_widget.ConfirmUploadFile(result.id)*/
                        switch (result.type) {
                            case 0:
                            case 1: {
                                _SetService_Detail.loadDataDetail();
                                _SetService_Detail.SetActive(5);
                                _SetService_Detail.OnStatuse(5);
                            } break;
                            case 3: {
                                _set_service_fly_detail.ShowServiceCodeTab();
                            } break;
                            case 5: {
                                _SetService_Tour_Detail.ShowTourCodeTab();
                            }
                            case 6: {
                                _set_service_vinwonder_detail.ReloadServiceCodeTab();
                            }
                            case 9: {
                                _set_service_other_detail.ShowServiceCodeTab();
                            }
                        }
                    }
                    else {
                        _msgalert.error(result.smg);
                        $('#AddPopupCode').attr("disabled", true);
                    }
                }
            });
        }

    },
    DeleteHotelBookingCode: function (id, type) {
        _msgconfirm.openDialog("Xóa code dịch vụ", 'Xác nhận xóa code dịch vụ này không?', function () {
            $.ajax({
                url: '/SetService/DeleteHotelBookingCode',
                type: "post",
                data: { id: id, type: type },
                success: function (result) {
                    if (result.sst_status === 0) {
                        _msgalert.success(result.smg);
                        switch (result.type) {
                            case 0:
                            case 1: {
                                _SetService_Detail.loadDataDetail();
                                _SetService_Detail.SetActive(5);
                                _SetService_Detail.OnStatuse(5);
                            } break;
                            case 3: {
                                _set_service_fly_detail.ShowServiceCodeTab();
                            } break;
                            case 5: {
                                _SetService_Tour_Detail.ShowTourCodeTab();
                            }
                            case 9: {
                                _set_service_other_detail.ShowServiceCodeTab();
                            }
                            case 6: {
                                _set_service_vinwonder_detail.ReloadServiceCodeTab();
                            }
                        }

                    }
                    else {
                        _msgalert.error(result.smg);

                    }

                }
            });
        });

    },
    PopupSendEmail: function () {
        let title = 'Gửi Code dịch vụ khách sạn';
        let url = '/SetService/SendEmail';
        let param = {
            id: $('.service-code-send-email').attr('data-booking-id').trim(),
            Orderid: $('#Orderid').val(),
            type: 1,
        };
        _magnific.OpenSmallPopup(title, url, param);

    },
    PopupSendEmail2: function () {
        let title = 'Gửi Email nhà cung cấp';
        let url = '/SetService/SendEmailSupplier';
        let param = {
            id: $('#HotelBookingID').val(),
            Orderid: $('#Orderid').val(),
            type: 8,
            SupplierId: 0,
            ServiceType: 1,
        };
        _magnific.OpenSmallPopup(title, url, param);

    },
    TemplateSupplier: function () {
        var id = $('#SupplierId').val();
        _SetService_Detail.LoadTTLienHe(id);
        $.ajax({
            url: "/SetService/TemplateSupplier",
            type: "Post",
            data: {
                id: $('#ServiceId').val(),
                Orderid: $('#Orderid').val(),
                type: 8,
                SupplierId: $('#SupplierId').val(),
                ServiceType: $('#ServiceType').val(),
            },
            success: function (result) {
                $('.data-supplier').html(result);

            }
        });
    },
    SendEmail: function () {
        
        let FromCreate = $('#form-send-email');
        FromCreate.validate({
            rules: {

                "Subject": "required",
                "SalerId": "required",

            },
            messages: {

                "Subject": "Tiêu đề không được bỏ trống",
                "SalerId": "Người nhận không được bỏ trống",
            }
        });
        if (FromCreate.valid()) {
            let pattern = /@/i;
            var Subject_email = $('#Subject').val();
            if (Subject_email.match(pattern)) {
                _msgalert.error("Tiêu đề email chứa ký tự @ không hợp lệ");
                return false;
            }
            var id = $("#CC_Email").val()

            var model = {
                Subject: $('#Subject').val(),
                ServiceId: $('#ServiceId').val(),
                Orderid: $('#Orderid').val(),
                To_Email: $('#To_Email').find(':selected').val(),
                CC_Email: $("#CC_Email").val() != undefined && $("#CC_Email").val().length > 0 ? $("#CC_Email").val().toString() : '',
                BCC_Email: $("#BCC_Email").val() != undefined && $("#BCC_Email").val().length > 0 ? $("#BCC_Email").val().toString() : '',
                OrderNote: $('#order_note').val(),
                PaymentNotification: $('#payment_notification').val(),
                ServiceType: $('#Type').val(),
                Type: $('#ServiceType').val(),
                Email: $('#Email').val() != undefined && $("#Email").val().length > 0 ? $("#Email").val().toString() : '',
                group_booking_id: $('#Type').attr('data-group-booking'),

                OrderAmount: $('#OrderAmount').val() != undefined ? $('#OrderAmount').val().replaceAll(',', '') : 0,
                totalAmount: $('#totalAmount').val() != undefined ? $('#totalAmount').val().replaceAll(',', '') : 0,
                OrderNo: $('#orderNo').val(),
                TTChuyenKhoan: $('#TTChuyenKhoan').val(),
                NDChuyenKhoan: $('#NDChuyenKhoan').val(),
                saler_Email: $('#saler_Email').val(),
                saler_Name: $('#saler_Name').val(),
                saler_Phone: $('#saler_Phone').val(),
                user_Email: $('#user_Email').val(),
                user_Phone: $('#user_Phone').val(),
                user_Name: $('#user_Name').val(),
                TileEmail: $('#TileEmail').val(),
                go_numberOfChild: $('#go_numberOfChild').val(),
                go_numberOfInfant: $('#go_numberOfInfant').val(),
                go_numberOfAdult: $('#go_numberOfAdult').val(),
                go_startpoint: $('#go_startpoint').val(),
                go_endpoint: $('#go_endpoint').val(),
                DKHuy: $('#DKHuy').val(),
                datatableCode: $('#datatableCode').val(),
                datatable: $('#datatable').val(),
                go_startdate: $('#go_startdate').val(),
                go_enddate: $('#go_enddate').val(),
                go_airline: $('#go_airline').val(),
                totalToday: $('#totalToday').val(),

                code_1_code: $('#code_1_code').val(),
                code_1_description: $('#code_1_description').val(),
                code_2_code: $('#code_2_code').val(),
                code_2_description: $('#code_2_description').val(),
                NumberOfRoom: $('#NumberOfRoom').val(),
                back_airline: $('#back_airline').val(),
                back_endpoint: $('#back_endpoint').val(),
                back_startpoint: $('#back_startpoint').val(),
                back_enddate: $('#back_enddate').val(),
                back_startdate: $('#back_startdate').val(),
                back_numberOfInfant: $('#back_numberOfInfant').val(),
                back_numberOfAdult: $('#back_numberOfAdult').val(),
                back_numberOfChild: $('#back_numberOfChild').val(),

                SupplierId: $('#SupplierId').val(),
            }
            $('#SendEmailDv').attr("disabled", false);

            var list_attach_file = _global_function.GetAttachmentFiles($('.attachment-email'))

            _global_function.AddLoading()
            $.ajax({
                url: "/SetService/ConfirmSendEmail",
                type: "Post",
                data: { model: model, attach_file: list_attach_file },
                success: function (result) {
                    if (result.status === 0) {
                        _global_function.RemoveLoading()
                        _msgalert.success(result.msg);
                        $.magnificPopup.close();
                        switch (model.Type == 1) {
                            case 0:
                            case 1: {
                                _SetService_Detail.loadDataDetail();
                                _SetService_Detail.SetActive(5);
                                _SetService_Detail.OnStatuse(5);
                            } break;
                            case 3: {
                                _set_service_fly_detail.ShowServiceCodeTab();
                            } break;
                            case 5: {
                                _SetService_Tour_Detail.ShowTourCodeTab();
                            }
                            case 9: {
                                _set_service_other_detail.ShowServiceCodeTab();
                            }

                        }

                    }
                    else {
                        _global_function.RemoveLoading()
                        _msgalert.error(result.msg);
                        $('#SendEmailDv').removeAttr("disabled", true);
                    }
                }
            });
        }

    },
    OpenPopupUserAgent: function (id, orderid, type) {

        let title = 'Đổi điều hành viên';
        let url = '/SetService/DetailUserHotel';
        let param = {
            id: id,
            orderid: orderid,
            type: type
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    OnUpdataUserHotel: function () {
        var id = $('#id').val()
        var type = $('#type').val()
        var salerId = $('#UserId_new').val()
        var Orderid = $('#Orderid').val()
        let FromUserAgent = $('#UserAgent_Detail');
        FromUserAgent.validate({
            rules: {

                "UserId_new": "required",


            },
            messages: {

                "UserId_new": "Vui lòng chọn nhân viên mới",
            }
        });
        if (FromUserAgent.valid()) {
            $.ajax({
                url: "/SetService/UpdateHotelBooking",
                type: "Post",
                data: { id: id, OrderId: Orderid, salerId: salerId[0], type: type },
                success: function (result) {
                    if (result.sst_status === 0) {
                        _msgalert.success(result.smg);
                        $.magnificPopup.close();
                        window.location.reload();
                    }
                    else {
                        _msgalert.error(result.smg);

                    }
                }
            });
        }

    },
    OnResetStatusPopup: function (id, type, orderid, group_booking_id) {
        debugger
        let url = '/SetService/HotelPopupDetail';
        let param = {
            id: id,
            type: type,
            orderid: orderid,
            groupbookingid: group_booking_id
        };
        var title = 'Từ chối nhận đặt dịch vụ';
        _magnific.OpenSmallPopup(title, url, param);


    },
    OnUpdatetStatus: function (id) {
        debugger
        let title = 'Xác nhận từ chối dịch vụ';
        let description = 'Bạn xác nhận muốn từ chối dịch vụ này?';
        var type = $('#type').val();
        if (type == 3) {
            id = $('#group_booking_id').val();
        }
        if (id != 0) {
            let FromCreate = $('#form_Note_TC');
            FromCreate.validate({
                rules: {

                    "Note_txt": "required",
                },
                messages: {

                    "Note_txt": "Lý do từ chối không được bỏ trống",
                }
            });
            if (FromCreate.valid()) {


                title = 'Xác nhận từ chối dịch vụ';
                description = 'Bạn xác nhận muốn từ chối dịch vụ này?';

            } else {
                return;
            }
        }
        var Note = $('#Note_txt').val();
        var orderid = $('#orderid').val();
        var type = $('#type').val();
        var groupbookingid = $('#group_booking_id').val();
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/SetService/UpdateServiceStatus",
                type: "post",
                data: { id: id, orderid: orderid, type: type, Note: Note, groupbookingid: groupbookingid },
                success: function (result) {
                    if (result.status == 0) {
                        _msgalert.success(result.msg);
                        $.magnificPopup.close();
                        setTimeout(function () {
                            window.location.reload();
                        }, 300);
                    } else {
                        _msgalert.error(result.msg);
                    }
                }
            });

        });
    },
    LoadFile: function (data_id, type) {

    },

    DynamicBindHotelServiceBooked: function () {
        if ($('.servicemanual-hotel-room-tr').length <= 1) {
            $('.setservice-hotel-room-delete-room').hide()
        }

        $("body").on('keyup', ".servicemanual-hotel-room-rates-operator-price, .servicemanual-hotel-room-number-of-rooms", function () {
            var element = $(this)
            var row_element = element.closest('.servicemanual-hotel-room-tr')
            _set_service_hotel_booked_function.RecorrectServiceBookedPrice()
            _set_service_hotel_booked_function.CalucateRowTotalAmountAndProfit(row_element)
            _set_service_hotel_booked_function.CalucateTotalAmountOfHotelRoom();
            _set_service_hotel_booked_function.CalucateTotalProfitOfHotelRoom();
            _set_service_hotel_booked_function.CalucateTotalpriceOfHotelRoom();
            _set_service_hotel_booked_function.CalucateServiceAmount();
            _set_service_hotel_booked_function.CalucateServiceProfit();


        });
        $("body").on('keyup', ".servicemanual-hotel-extrapackage-operator-price", function () {
            var element = $(this)
            var row_element = element.closest('.servicemanual-hotel-extrapackage-tr')
            _set_service_hotel_booked_function.CalucateExtraPackageProfit(row_element)
            _set_service_hotel_booked_function.CalucateTotalProfitOfExtraPacakge();
            _set_service_hotel_booked_function.CalucateTotalAmountOfExtraPackage();
            _set_service_hotel_booked_function.CalucateUnitPricetOfExtraPackage();
            _set_service_hotel_booked_function.CalucateServiceProfit();

        });


        $('.servicemanual-hotel-room-rates-daterange').each(function (index, item) {
            var element = $(item);
            element.daterangepicker({
                showDropdowns: true,
                drops: 'down',
                autoApply: true,
                minDate: '01/01/2000',
                maxDate: '01/01/3000',
                locale: {
                    format: 'DD/MM/YYYY'
                }
            }, function (start, end, label) {


            });
        });
        $('.servicemanual-hotel-extrapackage-daterange').each(function (index, item) {
            var element = $(item);
            element.daterangepicker({
                showDropdowns: true,
                drops: 'down',
                autoApply: true,
                minDate: '01/01/2000',
                maxDate: '01/01/3000',
                locale: {
                    format: 'DD/MM/YYYY'
                }
            }, function (start, end, label) {


            });
        });
    },
    CloneHotelRoom: function (element) {
        var last_order = _SetService_Detail.GetLastestHotelRoomOrder();
        last_order++;
        //-- Create and Append Div:
        var new_div_id = 'hotel-room-new-room';
        var element_clone = element.closest('.servicemanual-hotel-room-tr').clone().prop('id', new_div_id);
        $('.servicemanual-hotel-room-total-summary').before(element_clone)
        //-- Get new div
        var new_element = $('#' + new_div_id);

        //-- Change Order Text:
        new_element.attr('data-room-optional-id', '0');
        //-- Change Package & daterange-indentifer:

        var html = ` <div class="d-flex align-center servicemanual-hotel-room-div-code">
                        <select class="form-control servicemanual-hotel-room-suplier-id" disabled placeholder="Nhập tên nhà cung cấp" >

                        </select>
                    </div>`
        var element_suggesstion = new_element.find('.servicemanual-hotel-room-suplier-id')
        var td_suplier = new_element.find('.servicemanual-hotel-room-td-rates-suplier-id')
        td_suplier.html(html)
        td_suplier.removeAttr('data-select2-id')
        _set_service_hotel_booked_function.SupplierSuggesstion($('.servicemanual-hotel-room-suplier-id'))
        if (loading.EditMode) {
            $('.servicemanual-hotel-room-suplier-id').removeAttr("disabled");
        }

        var code = _set_service_hotel_booked_function.GenerateNewPackageOptionalCode()
        new_element.find('.servicemanual-hotel-room-td-package-name').html(code)

        new_element.attr('id', '')

        _SetService_Detail.ReIndexRoomOrder()
        _set_service_hotel_booked_function.CalucateTotalAmountOfHotelRoom();
        _set_service_hotel_booked_function.CalucateTotalProfitOfHotelRoom();
        _set_service_hotel_booked_function.CalucateTotalpriceOfHotelRoom();
        _set_service_hotel_booked_function.CalucateServiceAmount();
        _set_service_hotel_booked_function.CalucateServiceProfit();
        $('.setservice-hotel-room-delete-room').show()



    },
    DeleteHotelRoom: function (element) {
        element.closest('.servicemanual-hotel-room-tr').remove()
        _SetService_Detail.ReIndexRoomOrder()
        _set_service_hotel_booked_function.CalucateTotalAmountOfHotelRoom();
        _set_service_hotel_booked_function.CalucateTotalProfitOfHotelRoom();
        _set_service_hotel_booked_function.CalucateTotalpriceOfHotelRoom();
        _set_service_hotel_booked_function.CalucateServiceAmount();
        _set_service_hotel_booked_function.CalucateServiceProfit();
        if ($('.servicemanual-hotel-room-tr').length <= 1) {
            $('.setservice-hotel-room-delete-room').hide()
        }
    },
    GetLastestHotelRoomOrder: function () {
        var last_order = 0;
        if (!$('.servicemanual-hotel-room-td-order')[0]) return last_order;
        //-- Get Lastest Order
        $('.servicemanual-hotel-room-td-order').each(function (index, item) {
            last_order++;
        });
        return last_order;
    },
    ReIndexRoomOrder: function () {
        var index = 0
        $('.servicemanual-hotel-room-td-order').each(function (index, item) {
            var element = $(item);
            element.html(_global_function.Comma(++index))
        });
    },
    OpenPopupSupplier: function (id) {

        let title = 'Đổi nhà cung cấp';
        let url = '/SetService/DetailSupplier';
        let param = {
            SupplierId: id,
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    OnUpdataSupplier: function () {
        var _searchModel = {
            id: $('#Orderid').val(),
            HotelBookingID: $('#HotelBookingID').val(),
            ContactClientId: $('#ContactClientId').val(),
            serviceType: 1,
            HotelBookingstatus: $('#HotelBookingstatus').val(),
            type: 1,
            supplierId: $('#suplier-detail').attr('data-suplier-id'),
            supplierName: $('#suplier-detail').attr('data-suplier-name'),
            orderId: $('#Orderid').val(),
            amount: $('#operator-order-amount').attr('data-amount'),

        };
        var supplierid = $('#supplier-select').val().toString();
        var id = $('#HotelBookingID').val();
        _global_function.AddLoading()
        $.ajax({
            url: "/SetService/Updatesupplier",
            type: "Post",
            data: { id: id, supplierid: supplierid },
            success: function (result) {
                if (result.status == 0) {
                    _global_function.RemoveLoading()
                    _msgalert.success(result.smg);
                    $.magnificPopup.close();
                    _SetService_Detail.hotelBookingDetail(_searchModel);
                } else {
                    _global_function.RemoveLoading()
                    _msgalert.error(result.smg);
                }
            }
        });
    },
    PopupYCChi: function (id, type) {
        let title = 'Yêu cầu chi';
        if (type == 1) {
            title = 'Yêu cầu chi hoàn trả khách hàng'
        }
        let url = '/SetService/SendYCChi';
        var profit = $('#operator-order-profit').attr('data-profit');
        if (profit == undefined) {
            profit = $('#operator-total-profit').attr('data-profit');

        }

        let param = {
            id: id,
            profit: profit,
            type: type,
        };
        _magnific.OpenSmallPopup(title, url, param);
    },
    ConfirmYCChi: function (id, type) {

        var profit = $('#operator-order-profit').attr('data-profit');
        if (profit == undefined) {
            profit = $('#operator-total-profit').attr('data-profit');

        }

        var model = {
            GhiChu: $('#GhiChu').val(),
            Swiftcode: $('#Swiftcode').val(),
        }
        _global_function.AddLoading()
        $.ajax({
            url: "/SetService/ConfirmSendYCChi",
            type: "Post",
            data: { id: id, profit: profit, type: type, model: model },
            success: function (result) {
                _global_function.RemoveLoading()
                if (result.status === 0) {
                    $.magnificPopup.close();
                    let text = window.open();
                    text.document.body.innerHTML = result.html;
                    text.print();

                }
                else {
                    _global_function.RemoveLoading()
                    _msgalert.error(result.msg);

                }
            }
        });
    },
    printYCChi: function () {

    },
    LoadTTLienHe: function (id) {
        if (id == undefined) {
            id = 0;
        }
        $("#Email").select2({
            theme: 'bootstrap4',
            placeholder: "Email nhà cung cấp",
            /*maximumSelectionLength: 1,*/
            allowClear: true,
            tags: true,
            ajax: {
                url: "/SetService/SupplierContactSuggestion",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        txt_search: params.term,
                        Supplierid: id.toString(),
                    }
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: item.email,
                                id: item.email,
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
var _set_service_hotel_booked_function = {
    RecorrectServiceBookedPrice: function () {
        $('.servicemanual-hotel-room-tr').find('.servicemanual-hotel-room-rates-code').each(function (index, item) {
            var element = $(item);
            var row_element = element.closest('.servicemanual-hotel-room-tr')
            var number_of_room_element = row_element.find('.servicemanual-hotel-room-number-of-rooms')
            var number_of_room = number_of_room_element.val() == undefined || isNaN(parseFloat(number_of_room_element.val().replaceAll(',', ''))) ? 1 : parseFloat(number_of_room_element.val().replaceAll(',', ''))
            var rate_id = element.attr('data-rate-id')
            if (rate_id && rate_id != undefined && rate_id.trim() != '') {
                var sale_element = _set_service_hotel_booked_function.GetHotelRatesSalePrice(row_element, rate_id)
                var nights_element = _set_service_hotel_booked_function.GetHotelRatesNights(row_element, rate_id)
                var sale_amount_element = _set_service_hotel_booked_function.GetHotelRatesAmount(row_element, rate_id)

                var sale_price = isNaN(parseFloat(sale_element.val().replaceAll(',', ''))) ? 0 : parseFloat(sale_element.val().replaceAll(',', ''))
                var nights_price = isNaN(parseFloat(nights_element.val().replaceAll(',', ''))) ? 0 : parseFloat(nights_element.val().replaceAll(',', ''))
                var sale_amount = sale_price * nights_price * number_of_room

                //sale_amount_element.val(_global_function.Comma(sale_amount))
            }

        });
    },
    GetHotelRatesOperatorPrice: function (row_element, rate_id) {
        var operator_price_element = undefined
        row_element.find('.servicemanual-hotel-room-rates-operator-price').each(function (index_3, item_3) {
            var date_range_element = $(item_3);
            if (date_range_element.attr('data-rate-id').trim() == rate_id.trim()) {
                operator_price_element = date_range_element
                return false
            }
        })
        return operator_price_element
    },
    GetHotelRatesSalePrice: function (row_element, rate_id) {
        var saler_price_element = undefined
        row_element.find('.servicemanual-hotel-room-rates-sale-price').each(function (index_3, item_3) {
            var date_range_element = $(item_3);
            if (date_range_element.attr('data-rate-id').trim() == rate_id.trim()) {
                saler_price_element = date_range_element
                return false
            }
        })
        return saler_price_element
    },
    GetHotelRatesAmount: function (row_element, rate_id) {
        var amount_element = undefined
        row_element.find('.servicemanual-hotel-room-rates-total-amount').each(function (index_3, item_3) {
            var date_range_element = $(item_3);
            if (date_range_element.attr('data-rate-id').trim() == rate_id.trim()) {
                amount_element = date_range_element
                return false
            }
        })
        return amount_element
    },
    GetHotelRatesPrice: function (row_element, rate_id) {
        var price_element = undefined
        row_element.find('.servicemanual-hotel-room-rates-price').each(function (index_3, item_3) {
            var date_price_element = $(item_3);
            if (date_price_element.attr('data-rate-id').trim() == rate_id.trim()) {
                price_element = date_price_element
                return false
            }
        })
        return price_element
    },
    GetHotelRatesCode: function (row_element, rate_id) {
        var rate_element = undefined
        row_element.find('.servicemanual-hotel-room-rates-code').each(function (index_3, item_3) {
            var date_range_element = $(item_3);
            if (date_range_element.attr('data-rate-id').trim() == rate_id.trim()) {
                rate_element = date_range_element
                return false
            }
        })
        return rate_element
    },
    GetHotelRatesProfit: function (row_element, rate_id) {
        var profit_element = undefined
        row_element.find('.servicemanual-hotel-room-rates-profit').each(function (index_3, item_3) {
            var date_range_element = $(item_3);
            if (date_range_element.attr('data-rate-id').trim() == rate_id.trim()) {
                profit_element = date_range_element
                return false
            }
        })
        return profit_element
    },

    GetHotelRatesNights: function (row_element, rate_id) {
        var nights_element = undefined
        row_element.find('.servicemanual-hotel-room-rates-nights').each(function (index_3, item_3) {
            var date_range_element = $(item_3);
            if (date_range_element.attr('data-rate-id').trim() == rate_id.trim()) {
                nights_element = date_range_element
                return false
            }
        })
        return nights_element
    },
    GetHotelRoomNumberOfRoom: function (row_element) {
        var room_nums_element = undefined
        room_nums_element = row_element.find('.servicemanual-hotel-room-number-of-rooms')
        return room_nums_element
    },
    CalucateRowTotalAmountAndProfit: function (row_element, rate_id) {
        var number_of_room_element = row_element.find('.servicemanual-hotel-room-number-of-rooms')
        var number_of_room = number_of_room_element.val() == undefined || isNaN(parseFloat(number_of_room_element.val().replaceAll(',', ''))) ? 1 : parseFloat(number_of_room_element.val().replaceAll(',', ''))
        row_element.find('.servicemanual-hotel-room-rates-code').each(function (index, item) {
            var element = $(item);
            var rate_id = element.attr('data-rate-id')
            if (rate_id && rate_id != undefined && rate_id.trim() != '') {
                var operator_element = _set_service_hotel_booked_function.GetHotelRatesOperatorPrice(row_element, rate_id)
                var sale_element = _set_service_hotel_booked_function.GetHotelRatesSalePrice(row_element, rate_id)
                var nights_element = _set_service_hotel_booked_function.GetHotelRatesNights(row_element, rate_id)
                var profit_element = _set_service_hotel_booked_function.GetHotelRatesProfit(row_element, rate_id)
                var operator_amount_element = _set_service_hotel_booked_function.GetHotelRatesPrice(row_element, rate_id)
                var sale_amount_element = _set_service_hotel_booked_function.GetHotelRatesAmount(row_element, rate_id)


                var operator_price = isNaN(parseFloat(operator_element.val().replaceAll(',', ''))) ? 0 : parseFloat(operator_element.val().replaceAll(',', ''))
                var sale_price = isNaN(parseFloat(sale_element.val().replaceAll(',', ''))) ? 0 : parseFloat(sale_element.val().replaceAll(',', ''))
                var nights_price = isNaN(parseFloat(nights_element.val().replaceAll(',', ''))) ? 0 : parseFloat(nights_element.val().replaceAll(',', ''))

                var sale_amount = sale_price * nights_price * number_of_room
                var operator_amount = operator_price * nights_price * number_of_room
                var profit_amount = sale_amount - operator_amount


                profit_element.val((profit_amount >= 0 ? '' : '-') + _global_function.Comma(profit_amount))
                operator_amount_element.val(_global_function.Comma(operator_amount))
                sale_amount_element.val(_global_function.Comma(sale_amount))
            }

        });

    },
    GetHotelRatesOtherAmount: function (row_element, rate_id) {
        var nights_element = undefined
        row_element.find('.servicemanual-hotel-room-rates-others-amount').each(function (index_3, item_3) {
            var date_range_element = $(item_3);
            if (date_range_element.attr('data-rate-id').trim() == rate_id.trim()) {
                nights_element = date_range_element
                return false
            }
        })
        return nights_element
    },
    GetHotelRatesDiscount: function (row_element, rate_id) {
        var nights_element = undefined
        row_element.find('.servicemanual-hotel-room-rates-discount').each(function (index_3, item_3) {
            var date_range_element = $(item_3);
            if (date_range_element.attr('data-rate-id').trim() == rate_id.trim()) {
                nights_element = date_range_element
                return false
            }
        })
        return nights_element
    },
    CalucateTotalAmountOfHotelRoom: function () {
        var total = 0;
        $('#grid_data_ListHotelServicesbooked').find('.servicemanual-hotel-room-rates-total-amount').each(function (index, item) {
            var element = $(item);
            if (element.val() != undefined && element.val().trim() != '') {
                total += parseFloat(element.val().replaceAll(',', ''))
            }

        });
        $('#grid_data_ListHotelServicesbooked').find('#servicemanual-hotel-room-total-amount-final').html('' + _global_function.Comma(total)).change();
    },
    CalucateTotalProfitOfHotelRoom: function () {
        var total = 0;
        $('.servicemanual-hotel-room-rates-profit').each(function (index, item) {
            var element = $(item);
            if (element.val() != undefined && element.val().trim() != '') {
                total += parseFloat(element.val().replaceAll(',', ''))
            }

        });
        $('#servicemanual-hotel-room-total-profit-final').html((total >= 0 ? '' : '-') + _global_function.Comma(total)).change();
    },
    CalucateTotalpriceOfHotelRoom: function () {
        var total = 0;
        $('.servicemanual-hotel-room-rates-price').each(function (index, item) {
            var element = $(item);
            if (element.val() != undefined && element.val().trim() != '') {
                total += parseFloat(element.val().replaceAll(',', ''))
            }

        });
        $('#servicemanual-hotel-room-total-price-final').html('' + _global_function.Comma(total)).change();
    },
    CalucateServiceAmount: function () {
        var total = 0;
        $('#grid_data_ListHotelServicesbooked').find('.servicemanual-hotel-room-rates-price').each(function (index, item) {
            var element = $(item);
            if (element.val() != undefined && element.val().trim() != '') {
                total += parseFloat(element.val().replaceAll(',', ''))
            }

        });
        $('#grid_data_ListHotelServicesbooked').find('.servicemanual-hotel-extrapackage-unit-price').each(function (index, item) {
            var element = $(item);
            if (element.val() != undefined && element.val().trim() != '') {
                total += parseFloat(element.val().replaceAll(',', ''))
            }

        });
        $('#grid_data_ListHotelServicesbooked').find('.servicemanual-hotel-room-rates-others-amount').each(function (index, item) {
            var element = $(item);
            if (element.val() != undefined && element.val().trim() != '') {
                total += parseFloat(element.val().replaceAll(',', ''))
            }

        });
        $('#grid_data_ListHotelServicesbooked').find('.servicemanual-hotel-room-rates-discount').each(function (index, item) {
            var element = $(item);
            if (element.val() != undefined && element.val().trim() != '') {
                total += parseFloat(element.val().replaceAll(',', ''))
            }

        });



        // total += parseFloat($('#grid_data_ListHotelServicesbooked').find('#servicemanual-hotel-room-total-amount-final').html().replaceAll(',', ''))
        // total += parseFloat($('#grid_data_ListHotelServicesbooked').find('#servicemanual-hotel-extrapackage-total-amount-final').html().replaceAll(',', ''))

        $('.total_amount_service').html('' + _global_function.Comma(total));
    },

    CalucateServiceProfit: function () {

        var sale_amount_element = $('#sale-order-amount').attr('data-amount')
        var sale_amount = (sale_amount_element == undefined || isNaN(parseFloat(sale_amount_element.replaceAll(',', '')))) ? 0 : parseFloat(sale_amount_element.replaceAll(',', ''))
        var operator_amount = parseFloat($('.total_amount_service').html().replaceAll(',', ''))
        var total_profit = sale_amount - operator_amount

        var discount_element = $('#servicemanual-hotel-discount')
        var other_amount_element = $('#servicemanual-hotel-other-amount')
        var discount = isNaN(parseFloat(discount_element.val().replaceAll(',', ''))) ? 0 : parseFloat(discount_element.val().replaceAll(',', ''))
        var other_amount = isNaN(parseFloat(other_amount_element.val().replaceAll(',', ''))) ? 0 : parseFloat(other_amount_element.val().replaceAll(',', ''))
        total_profit = total_profit - discount - other_amount


        $('.total_profit_service').html((total_profit >= 0 ? '' : '-') + _global_function.Comma(total_profit));
    },
    RefillUpperDetailProfit: function () {
        var profit = parseFloat($('#grid_data_ListHotelServicesbooked').find('.total_profit_service').html().replaceAll(',', ''))
        if (profit == null || profit == NaN || profit == undefined) profit = 0
        var amount = parseFloat($('#grid_data_ListHotelServicesbooked').find('.total_amount_service').html().replaceAll(',', ''))
        if (amount == null || amount == NaN || amount == undefined) amount = 0
        $('#operator-order-amount').html(': ' + _global_function.Comma(amount - profit))
        $('#operator-order-profit').html(': ' + $('#grid_data_ListHotelServicesbooked').find('.total_profit_service').html())
    },
    GetUnitPriceValueByRateId: function (row_element, rate_id) {
        var unit_price = 0
        var element_amount = _set_service_hotel_booked_function.GetHotelRatesAmount(row_element, rate_id)
        var element_profit = _set_service_hotel_booked_function.GetHotelRatesProfit(row_element, rate_id)
        var amount = (element_amount == undefined || element_amount.val() == undefined || isNaN(parseFloat(element_amount.val().replaceAll(',', '')))) ? 0 : parseFloat(element_amount.val().replaceAll(',', ''))
        var profit = (element_profit == undefined || element_profit.val() == undefined || isNaN(parseFloat(element_profit.val().replaceAll(',', '')))) ? 0 : parseFloat(element_profit.val().replaceAll(',', ''))
        if (amount - profit > 0) unit_price = amount - profit
        return unit_price
    },
    GetExtraPackageUnitPriceValue: function (row_element) {
        var unit_price = 0
        var element_amount = row_element.find('.servicemanual-hotel-extrapackage-total-amount')
        var element_profit = row_element.find('.servicemanual-hotel-extrapackage-profit')
        var amount = (element_amount == undefined || element_amount.val() == undefined || isNaN(parseFloat(element_amount.val().replaceAll(',', '')))) ? 0 : parseFloat(element_amount.val().replaceAll(',', ''))
        var profit = (element_profit == undefined || element_profit.val() == undefined || isNaN(parseFloat(element_profit.val().replaceAll(',', '')))) ? 0 : parseFloat(element_profit.val().replaceAll(',', ''))
        if (amount - profit > 0) unit_price = amount - profit
        return unit_price
    },
    CalucateExtraPackageProfit: function (row_element) {
        var operator_element = row_element.find('.servicemanual-hotel-extrapackage-operator-price')
        var sale_element = row_element.find('.servicemanual-hotel-extrapackage-sale-price')
        var nights_element = row_element.find('.servicemanual-hotel-extrapackage-nights')
        var quanity_element = row_element.find('.servicemanual-hotel-extrapackage-number-of-extrapackages')
        var profit_element = row_element.find('.servicemanual-hotel-extrapackage-profit')
        var price_element = row_element.find('.servicemanual-hotel-extrapackage-unit-price')


        var operator_price = operator_element.val() == undefined || isNaN(parseFloat(operator_element.val().replaceAll(',', ''))) ? 0 : parseFloat(operator_element.val().replaceAll(',', ''))
        var sale_price = sale_element.val() == undefined || isNaN(parseFloat(sale_element.val().replaceAll(',', ''))) ? 0 : parseFloat(sale_element.val().replaceAll(',', ''))
        var nights_price = nights_element.val() == undefined || isNaN(parseFloat(nights_element.val().replaceAll(',', ''))) ? 0 : parseFloat(nights_element.val().replaceAll(',', ''))
        var quanity = quanity_element.val() == undefined || isNaN(parseFloat(quanity_element.val().replaceAll(',', ''))) ? 0 : parseFloat(quanity_element.val().replaceAll(',', ''))

        var amount = ((sale_price - operator_price) <= 0 ? 0 : (sale_price - operator_price)) * nights_price * quanity
        var price = operator_price * nights_price * quanity
        profit_element.val(_global_function.Comma(amount))
        price_element.val(_global_function.Comma(price))
    },
    CalucateTotalAmountOfExtraPackage: function () {
        var total = 0;
        $('.servicemanual-hotel-extrapackage-total-amount').each(function (index, item) {
            var element = $(item);
            if (element.val() != undefined && element.val().trim() != '') {
                total += parseFloat(element.val().replaceAll(',', ''))
            }

        });
        $('#servicemanual-hotel-extrapackage-total-amount-final').html('' + _global_function.Comma(total)).change();
    },
    CalucateUnitPricetOfExtraPackage: function () {
        var total = 0;
        $('.servicemanual-hotel-extrapackage-unit-price').each(function (index, item) {
            var element = $(item);
            if (element.val() != undefined && element.val().trim() != '') {
                total += parseFloat(element.val().replaceAll(',', ''))
            }

        });
        $('#servicemanual-hotel-extrapackage-unit-price-final').html('' + _global_function.Comma(total)).change();
    },
    CalucateTotalProfitOfExtraPacakge: function () {
        var total = 0;
        $('.servicemanual-hotel-extrapackage-profit').each(function (index, item) {
            var element = $(item);
            if (element.val() != undefined && element.val().trim() != '') {
                total += parseFloat(element.val().replaceAll(',', ''))
            }

        });
        $('#servicemanual-hotel-extrapackage-total-profit-final').html('' + _global_function.Comma(total)).change();
    },
    LoadServiceTotalProfitToInformationBoard: function () {
        var profit = $('#servicemanual-hotel-extrapackage-total-profit-final').html() == undefined && $('#servicemanual-hotel-extrapackage-total-profit-final').html().trim() == '' || isNaN(parseFloat($('#servicemanual-hotel-extrapackage-total-profit-final').html().replaceAll(',', ''))) ? undefined : parseFloat($('#servicemanual-hotel-extrapackage-total-profit-final').html().replaceAll(',', ''))
        var amount = parseFloat($('#servicemanual-hotel-extrapackage-total-amount-final').html().replaceAll(',', ''))
        if (profit == undefined) {
            $('#operator-order-profit').html
        }

    },
    SupplierSuggesstion: function (element) {
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC ",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
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
    },
    GenerateNewPackageOptionalCode: function () {
        var code = ''
        var list_exists = []
        var count = 0
        $('.servicemanual-hotel-room-td-package-name').each(function (index, item) {
            var element = $(item)
            count++
            list_exists.push(element.text().trim())
        });
        var service_code = $('#servicemanual-hotel-rooms').attr('data-code')
        var no_duplicate = true
        for (var i = (count + 1); i < 300; i++) {
            code = service_code + '-' + i
            no_duplicate = true
            $('.servicemanual-hotel-room-td-package-name').each(function (index, item) {
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
}
var _SetService_Sendemail = {
    SetServiceSendEmail: 22,
    SetServiceSendEmailOperator: 23,

    loadformSenmail: function () {
        _SetService_Sendemail.Select2WithUserSuggesstionEmail1($("#To_Email"))
        _SetService_Sendemail.Select2WithUserSuggesstionEmail($("#CC_Email"))
        _SetService_Sendemail.Select2WithUserSuggesstionEmail($("#BCC_Email"))
        _SetService_Sendemail.Select2SupplierSuggesstion($("#SupplierId"))
        _global_function.RenderFileAttachment($('.attachment-email'), -2, _SetService_Sendemail.SetServiceSendEmail, false, false)
    },
    Select2WithUserSuggesstion: function (element) {
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Người nhận",
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
    },
    Select2WithUserSuggesstionEmail: function (element) {
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Chọn Email",
            tags: true,
            ajax: {
                url: "/Order/UserSuggestion",
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
                                text: item.fullname + ' - ' + item.email,
                                id: item.email,
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
    Select2WithUserSuggesstionEmail1: function (element) {
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Chọn Email",
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
                    return query;
                },
                processResults: function (response) {
                    return {
                        results: $.map(response.data, function (item) {
                            return {
                                text: item.fullname + ' - ' + item.email,
                                id: item.email,
                            }
                        })
                    };
                },
                cache: true
            }
        });
    },
    Select2SupplierSuggesstion: function (element) {
        var id = $('#ServiceId').val();
        var ServiceType = $('#ServiceType').val();
        element.select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC ",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 1,
            ajax: {
                url: "/ReportDepartment/GetSuppliersSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        id: id,
                        type: ServiceType,
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
    },
}
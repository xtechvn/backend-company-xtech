var listPaymentRequestDetail = []
var listTourPackageOption = []
var listDetail = []
var amountEdit = 0
var serviceId = 0
var serviceType = 0
var serviceCode = 0
var supplierId = 0
var supplierName = ""
var clientId_name = ""
var amount_service = 0
var amount_supplier_refund = 0
var totalPayment_service = 0
var orderId = 0
var isServiceInclude = 0
var isEditAmountReject = 0
var isEdit = false
var isEditView = false
var listBankAccount = []
var bankingAccountId = 0
var clientId_service = 0
var totalRemain = 0
$().ready(function () {
    isServiceInclude = parseInt($('#isServiceIncluded').val())
    isEditAmountReject = parseInt($('#isEditAmountReject').val())
})
var _add_payment_request = {
    Initialization: function (isEditView = false) {
        $("#client-select").select2({
            theme: 'bootstrap4',
            placeholder: "Tên KH, Điện Thoại, Email",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 1,
            ajax: {
                url: "/Contract/ClientSuggestion",
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
                                text: item.clientname + ' - ' + item.email + ' - ' + item.phone,
                                id: item.id,
                            }
                        })
                    };
                },
                cache: true
            }
        });
        $("#supplier-select").select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC, Điện Thoại, Email",
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
                                text: item.name,
                                id: item.id,
                            }
                        })
                    };
                },
                cache: true
            }
        });
        $('#lblBankAccountRequired').show()
        $('#lblBankNameRequired').show()
        $('#lblBankAccount').hide()
        $('#lblBankName').hide()
        $('#lblSupplier').show()
        $('#divSupplier').show()
        $('#divIsSupplierDebt').hide()
        $('#lblCustomer').hide()
        $('#divCustomer').hide()
        $('input').attr('autocomplete', 'off');
        setTimeout(function () {
            serviceType = $('#serviceType').val()
            if (!isEditView)
                _add_payment_request.SetServiceData()
        }, 800)
    },
    SetServiceData: function () {
        serviceId = parseInt($('#serviceId').val())
        serviceType = parseInt($('#serviceType').val())
        payment_request_type = parseInt($('#payment_request_type').val())    
        serviceCode = $('#serviceCode').val()
        supplierId = parseInt($('#supplierId_service').val())
        amount_service = parseFloat($('#amount_service').val().replaceAll('.', '').replaceAll(',', ''))
        amount_supplier_refund = parseFloat($('#amount_supplier_refund').val().replaceAll('.', '').replaceAll(',', ''))
        clientId_service = parseInt($('#clientId_service').val())
        totalPayment_service = 0
        if ($('#totalPayment_service').val() !== undefined && $('#totalPayment_service').val() !== null && $('#totalPayment_service').val() !== '') {
            totalPayment_service = parseFloat($('#totalPayment_service').val().replaceAll(',', ''))
        }
        orderId = parseInt($('#orderId').val())
        supplierName = $('#supplierName').val()
        clientId_name = $('#clientId_name').val()
        if (serviceId !== undefined && serviceId !== null && parseInt(serviceId) !== 0) {
            $('#divisPaymentBefore').hide()
            $('#lblSupplier').show()
            $('#divSupplier').show()
            $('#divIsSupplierDebt').show()
            $('#lblCustomer').hide()
            $('#divCustomer').hide()
            $('#service-relate').hide()
            $('#order-relate').hide()
            $('#payment-request-type').val(payment_request_type)
            totalRemain = amount_service - totalPayment_service
            if (payment_request_type == 1 && (serviceType == 5 || serviceType == 9 || serviceType == 1 || serviceType == 3 || serviceType==6)) // dvu Tour hoặc dvu Khác hoặc dvu khách sạn hoặc dv vé máy bay
            {
                //$('#payment-request-pay-type').val(2)
                //$('#payment-request-pay-type').attr('disabled', true)
                //$('#payment-request-pay-type').addClass('background-disabled')
                if (serviceType == 5)// dvu Tour
                    _add_payment_request.GetTourPackageOption(serviceId, orderId);
                if (serviceType == 9)// dvu Khác
                    _add_payment_request.GetOtherPackageOption(serviceId, orderId);
                if (serviceType == 1)// dvu khách sạn 
                    _add_payment_request.GetHotelPackageOption(serviceId, orderId);
                if (serviceType == 3)// dvu vé máy bay
                    _add_payment_request.GetFlyBookingPackagesOptional(serviceId, orderId);
                if (serviceType == 6)// dvu vé máy bay
                    _add_payment_request.GetVinwonderPackagesOptional(serviceId, orderId);
                $('#amount').val(0)
            }
            if (payment_request_type == 4) {
                _add_payment_request.GetAllSupplier();
            }
         
            $('#payment-request-type').attr('disabled', true)
            $('#payment-request-type').addClass('background-disabled')
            if (serviceType != 5 && serviceType != 9 && serviceType != 1 && serviceType != 3 && serviceType != 6) // khac dvu Tour va dvu Khac va dvu khách sạn
            {
                $('#amount').val(_add_payment_request.FormatNumberStr(totalRemain < 0 ? 0 : totalRemain))
                setTimeout(function () {
                    var newOption = new Option(supplierName, supplierId, true, true);
                    $('#supplier-select').append(newOption).trigger('change');
                }, 500)
            }
            _add_payment_request.GetListBankAccountBySupplierID(supplierId)
        }
        if (clientId_service !== undefined && clientId_service !== null && parseInt(clientId_service) !== 0) {
            $('#divisPaymentBefore').hide()
            $('#lblSupplier').hide()
            $('#divSupplier').hide()
            $('#divIsSupplierDebt').hide()
            $('#lblCustomer').show()
            $('#divCustomer').show()
            $('#service-relate').hide()
            $('#order-relate').hide()
            totalRemain = amount_service - totalPayment_service
            //$('#amount').val(0)
            $('#client-select').attr('disabled', true)
            $('#client-select').addClass('background-disabled')
            $('#payment-request-type').val(3)
            $('#payment-request-type').attr('disabled', true)
            $('#payment-request-type').addClass('background-disabled')
            $('#amount').attr('disabled', false)
            $('#amount').removeClass('background-disabled')
            $('#amount').val(_add_payment_request.FormatNumberStr(totalRemain < 0 ? 0 : totalRemain))
            setTimeout(function () {
                var newOption = new Option(clientId_name, clientId_service, true, true);
                $('#client-select').append(newOption).trigger('change');
            }, 500)
            _add_payment_request.GetListBankAccountByClientID(clientId_service)
        }
        if (payment_request_type == 5) {
            $('#divisPaymentBefore').hide()
            $('#payment-request-type').val(5)
            $('#payment-request-type').attr('disabled', true)
            $('#payment-request-type').addClass('background-disabled')
            $('#service-relate').hide()
            $('#lblSupplier').hide()
            $('#divSupplier').hide()
            $('#lblBankName').hide()
            $('#divIsSupplierDebt').hide()
            $('#lblCustomer').show()
            $('#divCustomer').show()
            $('#amount').attr('disabled', false)
            $('#amount').removeClass('background-disabled')
            var client_id = $('#client-select').val()
            if (client_id !== null && client_id !== undefined && client_id !== '' && client_id != 0) {
                this.GetDataByClientOrSupplier(client_id[0], 0)
            }
        }
    },
    GetAllSupplier: function (serviceId, orderId) {

        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/GetAllSupplier",
            type: "Post",
            data: {},
            success: function (result) {
                _global_function.RemoveLoading()
                listTourPackageOption = result.data
            }
        });
        $("#supplier-select").empty()
        $("#supplier-select").select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC, Điện Thoại, Email",
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
                        name: params.term
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
    GetTourPackageOption: function (serviceId, orderId) {

        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/GetTourPackageOption",
            type: "Post",
            data: { 'serviceId': serviceId, 'serviceType': serviceType, 'orderId': orderId },
            success: function (result) {
                _global_function.RemoveLoading()
                listTourPackageOption = result.data
            }
        });
        $("#supplier-select").empty()
        $("#supplier-select").select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC, Điện Thoại, Email",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 1,
            ajax: {
                url: "/PaymentRequest/GetTourPackageOptionSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        name: params.term,
                        'serviceId': serviceId
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
    GetOtherPackageOption: function (serviceId, orderId) {

        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/GetOtherPackageOption",
            type: "Post",
            data: { 'serviceId': serviceId, 'serviceType': serviceType, 'orderId': orderId },
            success: function (result) {
                _global_function.RemoveLoading()
                listTourPackageOption = result.data
            }
        });
        $("#supplier-select").empty()
        $("#supplier-select").select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC, Điện Thoại, Email",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 1,
            ajax: {
                url: "/PaymentRequest/GetOtherPackageOptionSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        name: params.term,
                        'serviceId': serviceId
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
    GetHotelPackageOption: function (serviceId, orderId) {
        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/GetHotelPackageOption",
            type: "Post",
            data: { 'serviceId': serviceId, 'serviceType': serviceType, 'orderId': orderId },
            success: function (result) {
                _global_function.RemoveLoading()
                listTourPackageOption = result.data
            }
        });
        $("#supplier-select").empty()
        $("#supplier-select").select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC, Điện Thoại, Email",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 1,
            ajax: {
                url: "/PaymentRequest/GetHotelPackageOptionSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        name: params.term,
                        'serviceId': serviceId
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
    GetFlyBookingPackagesOptional: function (serviceId, orderId) {
        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/GetFlyBookingPackagesOptional",
            type: "Post",
            data: { 'serviceId': serviceId, 'serviceType': serviceType, 'orderId': orderId },
            success: function (result) {
                _global_function.RemoveLoading()
                listTourPackageOption = result.data
            }
        });
        $("#supplier-select").empty()
        $("#supplier-select").select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC, Điện Thoại, Email",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 1,
            ajax: {
                url: "/PaymentRequest/GetFlyBookingPackagesOptionalSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        name: params.term,
                        'serviceId': serviceId
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
    GetVinwonderPackagesOptional: function (serviceId, orderId) {
        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/GetVinwonderPackageOption",
            type: "Post",
            data: { 'serviceId': serviceId, 'serviceType': serviceType, 'orderId': orderId },
            success: function (result) {
                _global_function.RemoveLoading()
                listTourPackageOption = result.data
            }
        });
        $("#supplier-select").empty()
        $("#supplier-select").select2({
            theme: 'bootstrap4',
            placeholder: "Tên NCC, Điện Thoại, Email",
            hintText: "Nhập từ khóa tìm kiếm",
            searchingText: "Đang tìm kiếm...",
            maximumSelectionLength: 1,
            ajax: {
                url: "/PaymentRequest/GetVinwonderPackagesOptionalSuggest",
                type: "post",
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    var query = {
                        name: params.term,
                        'serviceId': serviceId
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
    FormatNumber: function () {
        var amount = $('#amount').val()
        $('#amount').val(amount.replaceAll(',', '.').replaceAll(',', ''))
        var n = parseFloat($('#amount').val().replace(/\D/g, ''), 10);
        $('#amount').val(isNaN(n) === true ? '' : n.toLocaleString().replaceAll('.', ','));
        var type = parseInt($('#payment_request_type').val());
           if (type == 5) {
               var client_id = $('#client-select').val()
               $.ajax({
                   url: "/FundCustomerCare/GetTotalCustomerCareFundByClientid",
                   type: "Post",
                   data: {
                       'id': client_id,
                   },
                   success: function (result) {
      
                       if (result.status == 0) {
                           if (result.data >= 0 && result.data < parseFloat(amount.replaceAll(',', ''))) {
                               $('.validate-amount').html('<p style="color: red;">Số tiền bạn nhập đã vượt qua quỹ của khách hàng</p>');
                               $('.validate-amount').show();
                           } else {
                               $('.validate-amount').hide();
                           }
                       }
                       else
                       {
                           $('.validate-amount').html('<p style="color: red;">Số tiền bạn nhập đã vượt qua quỹ của khách hàng</p>');
                           $('.validate-amount').show();
                       }
                      
                   }
               });
        }
    },
    FormatNumberStr: function (amount) {
        var n = parseFloat(amount, 10);
        return isNaN(n) === true ? '' : n.toLocaleString().replaceAll('.', ',')
    },
    FormatNumberOrder: function (i) {
        var amount = $('#amount_order_' + i).val()
        $('#amount_order_' + i).val(amount.replaceAll(',', '.').replaceAll(',', ''))
        var n = parseFloat($('#amount_order_' + i).val().replace(/\D/g, ''), 10);
        $('#amount_order_' + i).val(isNaN(n) === true ? '' : n.toLocaleString().replaceAll('.', ','));
    },
    UpdateAmount: function (index, orderId) {
        var timer = 0;
        clearTimeout(timer);
        timer = setTimeout(function () {
            var totalAmount = 0
            for (var i = 0; i < listPaymentRequestDetail.length; i++) {
                if (listPaymentRequestDetail[i].orderId == orderId) {
                    var amount_input = parseFloat($('#amount_order_' + index).val().replaceAll('.', '').replaceAll(',', ''))
                    listPaymentRequestDetail[i].amountPayment = amount_input
                    if (amount_input > listPaymentRequestDetail[i].totalNeedPayment)
                        listPaymentRequestDetail[i].amountPayment = listPaymentRequestDetail[i].totalNeedPayment
                }
                if (listPaymentRequestDetail[i].amountPayment !== undefined && listPaymentRequestDetail[i].amountPayment !== null
                    && listPaymentRequestDetail[i].amountPayment) {
                    var checked = $('#order_ckb_' + i).is(":checked")
                    if (checked) {
                        totalAmount += listPaymentRequestDetail[i].amountPayment
                    }
                    if (i == index) {
                        $('#amount_order_' + index).val(_add_payment_request.FormatNumberStr(listPaymentRequestDetail[i].amountPayment))
                    }
                }
            }
            $('#total_amount_need_pay').html(_add_payment_request.FormatNumberStr(totalAmount))
            $('#amount').val(_add_payment_request.FormatNumberStr(totalAmount))
        }, 1500);
    },
    GetDataByClientOrSupplier: function (clientId, supplier_id, isEdit = false) {
        var isServiceIncluded = $('#isServiceIncluded').val()
        if (isServiceIncluded !== undefined && isServiceIncluded !== null && isServiceIncluded !== '' && parseInt(isServiceIncluded) == 1)
            return
        if (clientId_service !== undefined && clientId_service !== null && clientId_service !== '' && parseInt(clientId_service) > 0)
            return
/*        $('#bankName').val("")*/
        $("#bankingAccount").empty();
        let payment_request_type = $('#payment-request-type').val()
        if (payment_request_type === '1') {
            this.GetServiceListBySupplierId(supplier_id, isEdit);
        }
        if (payment_request_type === '4') {
            this.GetAllSupplier()
        }
        if (payment_request_type === '2') {
            _add_payment_request.GetListBankAccountBySupplierID(supplier_id)
        }
        if (payment_request_type === '3') {
            this.GetOrderListByClientId(clientId);
            _add_payment_request.GetListBankAccountByClientID(clientId)
        }
        if (payment_request_type === '5') {
            _add_payment_request.GetListBankAccountByClientID(clientId)
        }
    },
    GetServiceListBySupplierId: function (supplierId, isEdit = false) {
        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/GetServiceListBySupplierId",
            type: "Post",
            data: {
                'supplierId': supplierId,
                'requestId': $('#payId').val(),
                'serviceId': serviceId,
            },
            success: function (result) {
                _global_function.RemoveLoading()
                listPaymentRequestDetail = result.data
                var totalAmount = 0
                var totalPayment = 0
                var totalDisarmed = 0
                var totalNeedPayment = 0
                $("#body_service_list").empty();

                for (var i = 0; i < result.data.length; i++) {
                    var urlService = ''
                    if (result.data[i].serviceType == 1) { //Khách sạn
                        urlService = "/SetService/VerifyHotelServiceDetai/" + result.data[i].serviceId
                    }
                    if (result.data[i].serviceType == 3) { // vé máy bay
                        urlService = "/SetService/fly/detail/" + result.data[i].groupBookingId
                    }
                    if (result.data[i].serviceType == 5) { //tour
                        urlService = "/SetService/Tour/Detail/" + result.data[i].serviceId
                    }
                    if (result.data[i].serviceType == 9) { //other
                        urlService = "/SetService/Others/Detail/" + result.data[i].serviceId
                    }
                    $('#service-relate-table').find('tbody').append(
                        "<tr id='order_" + i + "'>" +
                        "<td>" +
                        "<label class='check-list number'>" +
                        " <input type='checkbox' name='order_ckb' id='order_ckb_" + i + "' onclick='_add_payment_request.OnCheckBox(" + i + ");_add_payment_request.AddToListDetail(" + i + ")'>" +
                        " <span class='checkmark'></span>" + (i + 1) +
                        "  </label>" +
                        "</td>" +
                        "<td>" +
                        " <a class='blue' href='" + urlService + "'> " + result.data[i].serviceCode + " </a>"
                        + "</td>" +
                        "<td>" + result.data[i].startDateStr + " - " + result.data[i].endDateStr + "</td>" +
                        "<td>" +
                        " <a class='blue' href='/Order/Orderdetails?id=" + result.data[i].orderId + "'> " + result.data[i].orderNo + " </a>"
                        + "</td>" +
                        "<td>" + result.data[i].salerName + "</td>" +
                        "<td class='text-right'>" + _add_payment_request.FormatNumberStr(result.data[i].totalAmount) + "</td>" +
                        "<td class='text-right'>" + _add_payment_request.FormatNumberStr(result.data[i].totalDisarmed) + "</td>" +
                        "<td class='text-right'>" + _add_payment_request.FormatNumberStr(result.data[i].totalNeedPayment) + "</td>" +
                        "<td class='text-right'>" + "<input class='background-disabled text-right form-control' type='text' id='amount_order_" + i + "' maxlength='15'  autocomplete='off' style='min-width: 100px;' disabled  onkeyup='_add_payment_request.FormatNumberOrder(" + i + ");' oninput='_add_payment_request.UpdateAmount(" + i + "," + result.data[i].orderId + ")' >" + "</td>" +
                        "</tr>"
                    );
                    totalAmount += result.data[i].totalAmount
                    totalDisarmed += result.data[i].totalDisarmed
                    totalNeedPayment += result.data[i].totalNeedPayment
                    if (isEditView) {
                        totalPayment += result.data[i].amountPayment
                        let index = i
                        let payment = result.data[i].amountPayment
                        setTimeout(function () {
                            $('#amount_order_' + index).val(_add_payment_request.FormatNumberStr(payment))
                        }, 1000)
                    }
                    if (result.data[i].isChecked) {
                        let index = i
                        setTimeout(function () {
                            $('#order_ckb_' + index).prop('checked', true)
                            $('#amount_order_' + index).attr('disabled', false)
                            $('#amount_order_' + index).removeClass('background-disabled')
                        }, 800)
                    }
                    if (result.data[i].isDisabled) {
                        let index = i
                        setTimeout(function () {
                            $('#order_ckb_' + index).attr('disabled', true)
                            $('#amount_order_' + index).attr('disabled', true)
                            $('#amount_order_' + index).addClass('background-disabled')
                            $('#order_' + index).addClass('background-disabled')
                        }, 1000)
                    }
                }
                $('#service-relate-table').find('tbody').append(
                    "<tr style='font-weight:bold !important;'>" +
                    "<td class='text-right' colspan='5'> Tổng </td>" +
                    "<td class='text-right'>" + _add_payment_request.FormatNumberStr(totalAmount) + "</td>" +
                    "<td class='text-right'>" + _add_payment_request.FormatNumberStr(totalDisarmed) + "</td>" +
                    "<td class='text-right'>" + _add_payment_request.FormatNumberStr(totalNeedPayment) + "</td>" +
                    "<td class='text-right' id='total_amount_need_pay'>0</td>" +
                    "</tr>"
                );
                var isPaymentBefore = $('#isPaymentBefore').is(":checked")
                if (isPaymentBefore != true) {
                    setTimeout(function () {
                        $('#total_amount_need_pay').html(_add_payment_request.FormatNumberStr(totalPayment))
                        $('#amount').val(_add_payment_request.FormatNumberStr(totalPayment))
                    }, 1000)
                }
            }
        });
        _add_payment_request.GetListBankAccountBySupplierID(supplierId)
    },
    GetOrderListByClientId: function (clientId) {
        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/GetServiceListByClientId",
            type: "Post",
            data: { 'clientId': clientId, 'requestId': $('#payId').val() },
            success: function (result) {
                _global_function.RemoveLoading()
                listPaymentRequestDetail = result.data
                var totalAmount = 0
                var totalDisarmed = 0
                var totalNeedPayment = 0
                var totalPayment = 0
                $("#body_order_list").empty();
                for (var i = 0; i < result.data.length; i++) {
                    $('#order-relate-table').find('tbody').append(
                        "<tr id='order_" + i + "'>" +
                        "<td>" +
                        "<label class='check-list number'>" +
                        " <input type='checkbox' id='order_ckb_" + i + "' name='order_ckb' onclick='_add_payment_request.OnCheckBox(" + i + ");_add_payment_request.AddToListDetail(" + i + ")'>" +
                        " <span class='checkmark'></span>" + (i + 1) +
                        "  </label>"
                        + "</td>" +
                        "<td>" +
                        " <a class='blue' href='/Order/Orderdetails?id=" + result.data[i].orderId + "'> " + result.data[i].orderNo + " </a>"
                        + "</td>" +
                        "<td>" + result.data[i].startDateStr + " - " + result.data[i].endDateStr + "</td>" +
                        "<td>" + result.data[i].salerName + "</td>" +
                        "<td class='text-right'>" + _add_payment_request.FormatNumberStr(result.data[i].totalAmount) + "</td>" +
                        "<td class='text-right'>" + _add_payment_request.FormatNumberStr(result.data[i].totalDisarmed) + "</td>" +
                        "<td class='text-right'>" + _add_payment_request.FormatNumberStr(result.data[i].totalNeedPayment) + "</td>" +
                        "<td class='text-right'>" + "<input class='background-disabled text-right' type='text' id='amount_order_" + i + "' maxlength='15'  autocomplete='off' style='min-width: 100px;' disabled  onkeyup='_add_payment_request.FormatNumberOrder(" + i + ");' oninput='_add_payment_request.UpdateAmount(" + i + "," + result.data[i].orderId + ")' >" + "</td>" +
                        "</tr>"
                    );
                    totalAmount += result.data[i].totalAmount
                    totalDisarmed += result.data[i].totalDisarmed
                    totalNeedPayment += result.data[i].totalNeedPayment
                    if (isEdit) {
                        totalPayment += result.data[i].amountPayment
                        let index = i
                        let payment = result.data[i].amountPayment
                        setTimeout(function () {
                            $('#amount_order_' + index).val(_add_payment_request.FormatNumberStr(payment))
                        }, 1000)
                    }
                    if (result.data[i].isChecked) {
                        let index = i
                        setTimeout(function () {
                            $('#order_ckb_' + index).prop('checked', true)
                        }, 800)
                    }
                    if (result.data[i].isDisabled) {
                        let index = i
                        setTimeout(function () {
                            $('#order_ckb_' + index).attr('disabled', true)
                            $('#amount_order_' + index).attr('disabled', true)
                            $('#amount_order_' + index).addClass('background-disabled')
                            $('#order_' + index).addClass('background-disabled')
                        }, 1000)
                    }
                }
                $('#order-relate-table').find('tbody').append(
                    "<tr style='font-weight:bold !important;'>" +
                    "<td class='text-right' colspan='4'> Tổng </td>" +
                    "<td class='text-right'>" + _add_payment_request.FormatNumberStr(totalAmount) + "</td>" +
                    "<td class='text-right'>" + _add_payment_request.FormatNumberStr(totalDisarmed) + "</td>" +
                    "<td class='text-right'>" + _add_payment_request.FormatNumberStr(totalNeedPayment) + "</td>" +
                    "<td class='text-right' id='total_amount_need_pay'>0</td>" +
                    "</tr>"
                );
                if (isEdit) {
                    setTimeout(function () {
                        $('#total_amount_need_pay').html(_add_payment_request.FormatNumberStr(totalPayment))
                        _add_payment_request.OnCheckBox();
                    }, 1000)
                }
            }
        });
    },
    GetListBankAccountBySupplierID: function (supplierId) {
        debugger
        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/GetListBankAccountBySupplierId",
            type: "Post",
            data: {
                'supplierId': supplierId,
            },
            success: function (result) {
                _global_function.RemoveLoading()
                $("#bankingAccount").empty();
                $('#bankingAccount').append(
                    "<option value='0'>Chọn</option>"
                );
                listBankAccount = result.data
                for (var i = 0; i < result.data.length; i++) {
                    $('#bankingAccount').append(
                        "<option value='" + result.data[i].id + "'> " + result.data[i].bankId + " - " + result.data[i].accountNumber + "</option>"
                    );
                }
                if (bankingAccountId !== undefined && bankingAccountId !== null && bankingAccountId !== 0) {
                    $("#bankingAccount").val(bankingAccountId);
                    for (var i = 0; i < listBankAccount.length; i++) {
                        if (listBankAccount[i].id === parseInt(bankingAccountId))
                            $('#bankName').val(listBankAccount[i].accountName)
                    }
                    _add_payment_request.GetAccountName()
                }
            }
        });
    },
    GetListBankAccountByClientID: function (clientId) {
        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/GetListBankAccountByClientID",
            type: "Post",
            data: {
                'clientId': clientId,
            },
            success: function (result) {
                _global_function.RemoveLoading()
                $("#bankingAccount").empty();
                $('#bankingAccount').append(
                    "<option value='0'>Chọn</option>"
                );
                listBankAccount = result.data
                for (var i = 0; i < result.data.length; i++) {
                    $('#bankingAccount').append(
                        "<option value='" + result.data[i].id + "'> " + result.data[i].bankId + " - " + result.data[i].accountNumber + "</option>"
                    );
                }
                if (bankingAccountId !== undefined && bankingAccountId !== null && bankingAccountId !== 0) {
                    $("#bankingAccount").val(bankingAccountId);
                    for (var i = 0; i < listBankAccount.length; i++) {
                        if (listBankAccount[i].id === parseInt(bankingAccountId))
                            $('#bankName').val(listBankAccount[i].accountName)
                    }
                }
            }
        });
    },
    GetAccountName: function () {
        var bankingAccountId = $("#bankingAccount").val();
        if (bankingAccountId !== null && bankingAccountId !== undefined && bankingAccountId !== '') {
            for (var i = 0; i < listBankAccount.length; i++) {
                if (listBankAccount[i].id === parseInt(bankingAccountId))
                    $('#bankName').val(listBankAccount[i].accountName)
            }
        }
    },
    Validate: function () {
        
        _add_payment_request.ClearError()
        let result = true
        if ($('#payment-request-type').val() == undefined || $('#payment-request-type').val() == null || $('#payment-request-type').val() == '') {
            _add_payment_request.DisplayError('validate-payment-request-type', 'Vui lòng chọn loại nghiệp vụ')
            result = false;
        }
        if ($('#payment-request-type').val() === '3' && ($('#client-select').val() == undefined || $('#client-select').val() == null || $('#client-select').val() == '')) {
            _add_payment_request.DisplayError('validate-client-select', 'Vui lòng chọn khách hàng')
            result = false;
        }
        if (($('#payment-request-type').val() === '1' || $('#payment-request-type').val() === '2' || $('#payment-request-type').val() === '4')
            && ($('#supplier-select').val() == undefined || $('#supplier-select').val() == null || $('#supplier-select').val() == '')) {
            _add_payment_request.DisplayError('validate-supplier-select', 'Vui lòng chọn nhà cung cấp')
            result = false;
        }
        if ($('#payment-request-pay-type').val() == undefined || $('#payment-request-pay-type').val() == null || $('#payment-request-pay-type').val() == '') {
            _add_payment_request.DisplayError('validate-payment-request-pay-type', 'Vui lòng chọn loại nghiệp vụ')
            result = false;
        }
        //if (parseInt($('#payment-request-pay-type').val()) == 2 && ($('#bankName').val() == undefined
        //    || $('#bankName').val() == null || $('#bankName').val() == '')) {
        //    _add_payment_request.DisplayError('validate-bankingName', 'Vui lòng nhập tên người thụ hưởng')
        //    result = false;
        //}
        if ((parseInt($('#payment-request-pay-type').val()) == 2 && ($('#bankingAccount').val() == undefined
            || $('#bankingAccount').val() == null || $('#bankingAccount').val() == '' || $('#bankingAccount').val() == 0 || $('#bankingAccount').val() == '0' )&& $('#payment-request-type').val() != 5)) {
            _add_payment_request.DisplayError('validate-bankingAccount', 'Vui lòng nhập số tài khoản nhận')
            result = false;
        }
        if ($('#amount').val() == undefined || $('#amount').val() == null || $('#amount').val() == '') {
            _add_payment_request.DisplayError('validate-amount', 'Vui lòng nhập số tiền')
            result = false;
        }
        var payment_request_type = $('#payment-request-type').val()
        if (!isEdit && serviceId !== undefined && serviceId !== null && parseInt(serviceId) !== 0 && parseInt(payment_request_type) === 1) {
            var amount = parseFloat($('#amount').val().replaceAll('.', '').replaceAll(',', ''))
            //if (amount === 0) {
            //    _add_payment_request.DisplayError('validate-amount', 'Vui lòng nhập số tiền')
            //    result = false;
            //}
            if ((serviceType == 5 || serviceType == 9 || serviceType == 1 || serviceType == 3) &&
                (supplierId !== null && supplierId !== undefined && supplierId !== '' && supplierId !== '0')) {
                var amount_supplier_remain = 0
                var amount_supplier = 0
                for (var i = 0; i < listTourPackageOption.length; i++) {
                    if (listTourPackageOption[i].supplierId == parseInt(($('#supplier-select').val())) || listTourPackageOption[i].suplierId == parseInt(($('#supplier-select').val()))) {
                        amount_supplier_remain = listTourPackageOption[i].totalAmountPay
                        amount_supplier = listTourPackageOption[i].amount
                        break
                    }
                }
                if (amount > (amount_supplier - amount_supplier_remain) + amount_supplier_refund) {
                    _add_payment_request.DisplayError('validate-amount',
                        'Vui lòng nhập số tiền không quá số tiền còn lại của dịch vụ tương ứng với nhà cung cấp')
                    result = false;
                }
            } else {
                if (amount > (amount_service - totalPayment_service) + amount_supplier_refund) {
                    _add_payment_request.DisplayError('validate-amount', 'Vui lòng nhập số tiền không quá số tiền còn lại của dịch vụ')
                    result = false;
                }
            }
        }

        if ($('#paymentDate').val() == undefined || $('#paymentDate').val() == null || $('#paymentDate').val() == '') {
            _add_payment_request.DisplayError('validate-paymentDate', 'Vui lòng chọn thời hạn thanh toán')
            result = false;
        }
        let fromDateVal = $('#paymentDate').val()
        if (fromDateVal !== undefined && fromDateVal !== null && fromDateVal !== "" && fromDateVal.length > 16) {
            _add_payment_request.DisplayError('validate-paymentDate', 'Thời hạn thanh toán không đúng định dạng')
            result = false;
        }
        var [fromDateComponents, fromTimeComponents] = fromDateVal.split(' ');
        var [fromday, frommonth, fromyear] = fromDateComponents.split('/');
        if (fromTimeComponents === undefined || fromTimeComponents === null) fromTimeComponents = ''
        var [fromhours, fromminutes] = fromTimeComponents.split(':');
        if (fromhours === undefined || fromhours === '' || fromhours === null) fromhours = 0
        if (fromminutes === undefined || fromminutes === '' || fromminutes === null) fromminutes = 0
        if (fromhours < 0 || fromhours > 24 || fromminutes < 0 || fromminutes > 60) {
            _add_payment_request.DisplayError('validate-paymentDate', 'Thời hạn thanh toán không đúng định dạng')
            result = false;
        }
        var fromDate = new Date(+fromyear, frommonth - 1, +fromday, +fromhours, +fromminutes);
        if (typeof fromDate.getMonth !== 'function') {
            _add_payment_request.DisplayError('validate-paymentDate', 'Thời hạn thanh toán không đúng định dạng')
            result = false;
        }

        if (!isEdit) {
            let paymentDate = $('#paymentDate').val()
            let now = new Date()
            if (paymentDate != null && paymentDate != '' && now != null && now != '') {
                const [fromday, frommonth, fromyear] = paymentDate.split('/');
                const fromDate = new Date(+fromyear, frommonth - 1, +fromday, 0, 0);
                const toDate = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0);
                if (fromDate < toDate) {
                    _add_payment_request.DisplayError('validate-paymentDate', 'Thời hạn thanh toán không được nhỏ hơn ngày hiện tại')
                    result = false
                }
            }

        }
        if ($('#content').val() == undefined || $('#content').val() == null || $('#content').val() == '') {
            _add_payment_request.DisplayError('validate-content', 'Vui lòng nhập nội dung')
            result = false;
        }
        if (($('#content').val()).length > 500) {
            _add_payment_request.DisplayError('validate-content', 'Vui lòng nhập nội dung không quá 500 kí tự')
            result = false;
        }
        if ($('#description').val() !== undefined && $('#description').val() !== null
            && $('#description').val() !== '' && ($('#description').val()).length > 3000) {
            _add_payment_request.DisplayError('validate-description', 'Vui lòng nhập ghi chú không quá 3000 kí tự')
            result = false;
        }
        if (!result) return false

        var isPaymentBefore = $('#isPaymentBefore').is(":checked")
        if (isPaymentBefore)
            return true

        var flag = false
        if (payment_request_type !== null && payment_request_type !== '' &&
            (parseInt(payment_request_type) == 1 || parseInt(payment_request_type) == 4)) { // thanh toán dịch vụ or chi phí marketing
            var totalInputAmount = 0
            for (var i = 0; i < listPaymentRequestDetail.length; i++) {
                var checked = $('#order_ckb_' + i).is(":checked")
                if (checked) {
                    totalInputAmount += listPaymentRequestDetail[i].amountPayment
                    if (listPaymentRequestDetail[i].amountPayment === 0) {
                        _msgalert.error('Vui lòng nhập số tiền giải trừ lớn hơn 0');
                        return false;
                    }
                    flag = true
                }
            }
            if (!flag && (serviceId === undefined || serviceId === null || parseInt(serviceId) === 0)
                && (isServiceInclude === undefined || isServiceInclude === null || parseInt(isServiceInclude) === 0)) {
                if (parseInt(payment_request_type) == 1)
                    _msgalert.error('Vui lòng tích chọn dịch vụ cần giải trừ');
                return false;
            }
            if (!flag && (serviceId === undefined || serviceId === null || parseInt(serviceId) === 0)
                && (isServiceInclude === undefined || isServiceInclude === null || parseInt(isServiceInclude) === 0)) {
                if (parseInt(payment_request_type) == 4)
                    _msgalert.error('Vui lòng tích chọn dịch vụ cần tạo chi phí');
                return false;
            }
            if (totalInputAmount === 0 && (serviceId === undefined || serviceId === null || parseInt(serviceId) === 0)
                && (isServiceInclude === undefined || isServiceInclude === null || parseInt(isServiceInclude) === 0)) {
                _msgalert.error('Vui lòng nhập số tiền giải trừ lớn hơn 0');
                return false;
            }
        }
        if (!isEdit && clientId_service !== undefined && clientId_service !== null && parseInt(clientId_service) !== 0) {
            if (payment_request_type !== null && payment_request_type !== '' && parseInt(payment_request_type) == 3
                && (clientId_service === undefined || clientId_service === null || parseInt(clientId_service) === 0)) { // hoàn trả khách hàng
                var totalInputAmount = 0
                for (var i = 0; i < listPaymentRequestDetail.length; i++) {
                    var checked = $('#order_ckb_' + i).is(":checked")
                    if (checked) {
                        totalInputAmount += listPaymentRequestDetail[i].amountPayment
                        if (listPaymentRequestDetail[i].amountPayment === 0) {
                            _msgalert.error('Vui lòng nhập số tiền hoàn trả lớn hơn 0');
                            return false;
                        }
                        flag = true
                    }
                }
                if (!flag) {
                    _msgalert.error('Vui lòng tích chọn đơn hàng cần hoàn trả');
                    return false;
                }
                if (totalInputAmount === 0) {
                    _msgalert.error('Vui lòng nhập số tiền hoàn trả lớn hơn 0');
                    return false;
                }
            }
        }

        return result
    },
    AddRequest: function (isSend = 0) {
        $('.btn-send-payment-request').attr('disabled', true)
        let validate = _add_payment_request.Validate()
        if (!validate) {
            $('.btn-send-payment-request').removeAttr('disabled')
            return;
        }
        let paymentRequestDetails = []
        var payment_request_type = $('#payment-request-type').val()
        if (payment_request_type !== null && payment_request_type !== '' && (parseInt(payment_request_type) == 1 || parseInt(payment_request_type) == 4)) { // thanh toán dịch vụ
            for (var i = 0; i < listPaymentRequestDetail.length; i++) {
                var checked = $('#order_ckb_' + i).is(":checked")
                if (checked) {
                    paymentRequestDetails.push(listPaymentRequestDetail[i])
                }
            }
        }
        if (payment_request_type !== null && payment_request_type !== '' && parseInt(payment_request_type) == 3) { // hoàn trả khách hàng
            for (var i = 0; i < listPaymentRequestDetail.length; i++) {
                var checked = $('#order_ckb_' + i).is(":checked")
                if (checked) {
                    paymentRequestDetails.push(listPaymentRequestDetail[i])
                }
            }
        }
        if (serviceId !== undefined && serviceId !== null && parseInt(serviceId) !== 0) {
            paymentRequestDetails = []
            paymentRequestDetails.push({
                'orderId': orderId,
                'serviceId': serviceId,
                'serviceType': serviceType,
                'serviceCode': serviceCode,
                'amountPayment': parseFloat($('#amount').val().replaceAll('.', '').replaceAll(',', '')),
            })
        }
        if (clientId_service !== undefined && clientId_service !== null && parseInt(clientId_service) !== 0) {
            var amount = parseFloat($('#amount').val().replaceAll('.', '').replaceAll(',', ''))
            console.log('amount:'+ amount)
            console.log('totalRemain:' + totalRemain)
            if (amount > totalRemain) {
                $('.btn-send-payment-request').attr('disabled', false)
                _msgalert.error('Vui lòng nhập số tiền không được lớn hơn số tiền còn lại của dịch vụ');
                return
            }
            paymentRequestDetails = []
            paymentRequestDetails.push({
                'orderId': orderId,
                'serviceId': serviceId,
                'serviceType': serviceType,
                'serviceCode': serviceCode,
                'amountPayment': parseFloat($('#amount').val().replaceAll('.', '').replaceAll(',', '')),
            })
        }
        var isSupplierDebt = $('#isSupplierDebt').is(":checked")
        if (isSupplierDebt === undefined || isSupplierDebt === null)
            isSupplierDebt = false
        var isPaymentBefore = $('#isPaymentBefore').is(":checked")
        if (isPaymentBefore === undefined || isPaymentBefore === null)
            isPaymentBefore = false
        let obj = {
            'type': parseInt($('#payment-request-type').val()),
            'paymentType': parseInt($('#payment-request-pay-type').val()),
            'bankingAccountId': parseInt($('#bankingAccount').val()),
            'bankName': $('#bankName').val(),
            'bankAccount': $('#bankAccount').val(),
            'isServiceIncluded': serviceId > 0 || clientId_service > 0 ? true : false,
            'description': $('#description').val(),
            'paymentDateStr': $('#paymentDate').val(),
            'isSupplierDebt': isSupplierDebt,
            'isPaymentBefore': isPaymentBefore,
            'isSend': isSend,
            'note': $('#content').val(),
            'clientId': parseInt(($('#client-select').val())),
            'supplierId': parseInt(($('#supplier-select').val())),
            'amount': parseFloat($('#amount').val().replaceAll('.', '').replaceAll(',', '')),
            'totalAmountService': amount_service,
            'totalSupplierRefund': amount_supplier_refund,
            'paymentRequestDetails': paymentRequestDetails,
        }
        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/AddJson",
            type: "Post",
            data: obj,
            success: function (result) {
                _global_function.RemoveLoading()
                if (result.isSuccess) {
                    var serviceTypeBK = serviceType
                    var payment_request_typeBK = payment_request_type
                    _msgalert.success(result.message);
                    $.magnificPopup.close();
                    setTimeout(function () {
                        if (serviceTypeBK !== 0 && serviceTypeBK == 3) { // vé máy bay
                            if (serviceId !== null && serviceId !== undefined && serviceId !== '' && serviceId !== '0') {
                                if (payment_request_typeBK == '4')
                                    _set_service_fly_detail.MarketingTab()
                                else
                                    _set_service_fly_detail.ShowPaymentTab()
                            }
                            if (clientId_service !== null && clientId_service !== undefined && clientId_service !== '' && clientId_service !== '0' && clientId_service !== 0)
                                _set_service_fly_detail.ShowRefundTab()
                        } else if (serviceTypeBK !== 0 && serviceTypeBK == 1) { // Khách sạn
                            if (serviceId !== null && serviceId !== undefined && serviceId !== '' && serviceId !== '0') {
                                if (payment_request_typeBK == '4')
                                    _SetService_Detail.LoadingMarketingPaymentRequest()
                                else
                                    _SetService_Detail.loadTourDetail()
                            }
                            if (clientId_service !== null && clientId_service !== undefined && clientId_service !== '' && clientId_service !== '0' && clientId_service !== 0)
                                _SetService_Detail.loadListHotelBookingRefund()
                        }
                        else if (serviceTypeBK !== 0 && serviceTypeBK == 5) { // Tour
                            if (serviceId !== null && serviceId !== undefined && serviceId !== '' && serviceId !== '0') {
                                if (payment_request_typeBK == '4')
                                    _SetService_Tour_Detail.MarketingPaymentRequest(1);
                                else
                                    _SetService_Tour_Detail.ShowTourPaymentTab();
                            }
                            if (clientId_service !== null && clientId_service !== undefined && clientId_service !== '' && clientId_service !== '0' && clientId_service !== 0)
                                _SetService_Tour_Detail.TourServiceRefund(1);
                        }
                        else if (serviceTypeBK !== 0 && serviceTypeBK == 9) { // Others
                            if (serviceId !== null && serviceId !== undefined && serviceId !== '' && serviceId !== '0') {
                                if (typeof _set_service_other_detail != "undefined") {
                                    if (payment_request_typeBK == '4')
                                        _set_service_other_detail.MarketingTab();
                                    else
                                        _set_service_other_detail.ShowPaymentTab();
                                } else {
                                    if (payment_request_typeBK == '4')
                                        _set_service_ws_detail.ShowPaymentTab();
                                    else
                                        _set_service_ws_detail.ShowPaymentTab();
                                }
                            }
                            if (clientId_service !== null && clientId_service !== undefined && clientId_service !== '' && clientId_service !== '0' && clientId_service !== 0)
                                _set_service_other_detail.ShowRefundTab()
                        }
                        else {
                            window.location.reload()
                        }
                    }, 1000)
                } else {
                    _msgalert.error(result.message);
                }
                $('.btn-send-payment-request').removeAttr('disabled')

            }
        });
    },
    OnChooseRequestType: function () {

        if (serviceId !== undefined && serviceId !== null && parseInt(serviceId) !== 0)
            return
        if (clientId_service !== undefined && clientId_service !== null && parseInt(clientId_service) !== 0)
            return
        var payment_request_type = $('#payment-request-type').val()
        if (payment_request_type === undefined || payment_request_type === null || payment_request_type === '')
            return
        $('#body_order_list').empty()
        $('#body_service_list').empty()
        $('#service-relate').hide()
        $('#order-relate').hide()
        if (!isEdit) {
            $('#amount').val('')
            $('#amount').attr('disabled', true)
            $('#amount').addClass('background-disabled')
        }
        if (!isEditView) {
            $('#bankName').val("")
            $("#bankingAccount").empty();
        }
        if (payment_request_type !== null && payment_request_type !== '' && (parseInt(payment_request_type) == 1 || parseInt(payment_request_type) == 4)) { // thanh toán dịch vụ
            $('#service-relate').show()
            $('#order-relate').hide()
            $('#lblSupplier').show()
            $('#divSupplier').show()
            $('#divIsSupplierDebt').show()
            $('#lblCustomer').hide()
            $('#divCustomer').hide()
            var supplier_id = $('#supplier-select').val()
            if (supplier_id !== null && supplier_id !== undefined && supplier_id !== '') {
                this.GetDataByClientOrSupplier(0, supplier_id[0])
            }
        }
        if (payment_request_type !== null && payment_request_type !== '' && parseInt(payment_request_type) == 2) { // thanh toán khác
            $('#service-relate').hide()
            $('#order-relate').hide()
            $('#lblSupplier').show()
            $('#divSupplier').show()
            $('#divIsSupplierDebt').hide()
            $('#lblCustomer').hide()
            $('#divCustomer').hide()
            $('#amount').attr('disabled', false)
            $('#amount').removeClass('background-disabled')
            var supplier_id = $('#supplier-select').val()
            if (supplier_id !== null && supplier_id !== undefined && supplier_id !== '' && supplier_id != 0) {
                this.GetDataByClientOrSupplier(0, supplier_id[0])
            }
        }
        if (payment_request_type !== null && payment_request_type !== '' && parseInt(payment_request_type) == 3) {//hoàn trả khách hàng
            $('#service-relate').hide()
            $('#order-relate').show()
            $('#lblSupplier').hide()
            $('#divSupplier').hide()
            $('#divIsSupplierDebt').hide()
            $('#lblCustomer').show()
            $('#divCustomer').show()
            var client_id = $('#client-select').val()
            if (client_id !== null && client_id !== undefined && client_id !== '' && client_id != 0) {
                this.GetDataByClientOrSupplier(client_id[0], 0)
                //_add_payment_request.GetListBankAccountByClientID(client_id[0])
            }
        }
        if (payment_request_type !== null && payment_request_type !== '' && parseInt(payment_request_type) == 5) {//Quỹ chăm sóc khách hàng
            $('#BankingAccountId').hide()
            $('#bankAccount').hide()
            $('#lblBankNameRequired').hide()
            $('#lblBankAccountRequired').hide()
            $('#service-relate').hide()
            $('#lblSupplier').hide()
            $('#divSupplier').hide()
            $('#lblBankName').hide()
            $('#divIsSupplierDebt').hide()
            $('#lblCustomer').show()
            $('#divCustomer').show()
         
            $('#amount').attr('disabled', false)
            $('#amount').removeClass('background-disabled')
            $('#bankName').attr('disabled', false)
            $('#bankName').removeClass('background-disabled')
            var client_id = $('#client-select').val()
            
            if (client_id !== null && client_id !== undefined && client_id !== '' && client_id != 0) {
                this.GetDataByClientOrSupplier(client_id[0], 0)
                //_add_payment_request.GetListBankAccountByClientID(client_id[0])
            }
        }
    },
    OnChooseSupllier: function () {
        if (serviceId !== undefined && serviceId !== null && parseInt(serviceId) !== 0 && (parseInt(serviceType) === 5 || parseInt(serviceType) === 9 || parseInt(serviceType) === 1 || parseInt(serviceType) === 3 || parseInt(serviceType) === 6)) {
            var supplierId = parseInt(($('#supplier-select').val()))
            for (var i = 0; i < listTourPackageOption.length; i++) {
                debugger
                if (listTourPackageOption[i].supplierId == supplierId || listTourPackageOption[i].suplierId == supplierId) {
                    var amountRemain = listTourPackageOption[i].amount - listTourPackageOption[i].totalAmountPay + listTourPackageOption[i].totalContractPay
                    $('#amount').val(_add_payment_request.FormatNumberStr(amountRemain < 0 ? 0 : amountRemain))
                    break
                }
            }
            _add_payment_request.GetListBankAccountBySupplierID(supplierId)
        }
    },
    OnChooseRequestTypeEdit: function (client_id, supplier_id, bankAccountId, isAdminEdit = false) {
        isEditView = true
        bankingAccountId = bankAccountId
        //var isServiceIncluded = $('#isServiceIncluded').val()
        //if (isServiceIncluded !== undefined && isServiceIncluded !== null && isServiceIncluded !== '' && parseInt(isServiceIncluded) == 1)
        //    return
        if ((client_id == undefined || client_id == null || client_id == 0 || client_id == '')
            && (supplier_id == undefined || supplier_id == null || supplier_id == 0 || supplier_id == '')) {
            return
        }
        amountEdit = parseFloat($('#amount').val().replaceAll('.', '').replaceAll(',', ''))
        isEdit = true
        if (client_id !== undefined && client_id !== null && client_id !== 0 && client_id !== '') {
            setTimeout(function () {
                var newOption = new Option($('#client_name_hide').val(), client_id, true, true);
                $('#client-select').append(newOption).trigger('change');
            }, 500)
        }
        if (supplier_id !== undefined && supplier_id !== null && supplier_id !== 0 && supplier_id !== '') {
            setTimeout(function () {
                var newOption = new Option($('#supplier_name_hide').val(), supplier_id, true, true);
                $('#supplier-select').append(newOption).trigger('change');
            }, 500)
        }

        var payment_request_type = $('#payment-request-type').val()
        if (payment_request_type !== null && payment_request_type !== '' && (parseInt(payment_request_type) == 1 || parseInt(payment_request_type) == 4)) { // thanh toán dịch vụ
            if (isEditAmountReject === undefined || isEditAmountReject == null || isEditAmountReject == 0) {
                $('#service-relate').show()
                $('#order-relate').hide()
                $('#amount').attr('disabled', true)
                $('#amount').addClass('background-disabled')
            }
        }
        if (payment_request_type !== null && payment_request_type !== '' && parseInt(payment_request_type) == 2) { // thanh toán dịch vụ
            $('#service-relate').hide()
            $('#order-relate').hide()
            $('#amount').attr('disabled', false)
            $('#amount').removeClass('background-disabled')
        }
        if (payment_request_type !== null && payment_request_type !== '' && parseInt(payment_request_type) == 3) {//hoàn trả khách hàng
            $('#service-relate').hide()
            $('#order-relate').show()
            $('#amount').attr('disabled', true)
            $('#amount').addClass('background-disabled')
        }
        if (payment_request_type !== null && payment_request_type !== '' && parseInt(payment_request_type) == 5) {//Quỹ chăm sóc khách hàng
            $('#service-relate').hide()
            $('#lblBankName').hide()
            $('#amount').attr('disabled', true)
            $('#amount').addClass('background-disabled')
        }
        if (client_id !== null && client_id !== undefined && client_id !== '' && client_id !== 0) {
            this.GetDataByClientOrSupplier(client_id, 0, true)
        }
        if (supplier_id !== null && supplier_id !== undefined && supplier_id !== '' && supplier_id !== 0) {
            this.GetDataByClientOrSupplier(0, supplier_id, true)
        }
        var isSupplierDebt = $('#isSupplierDebtHide').is(":checked")
        if (isSupplierDebt)
            $('#isSupplierDebt').prop('checked', true)
        var isPaymentBeforeHide = $('#isPaymentBeforeHide').is(":checked")
        if (isPaymentBeforeHide)
            $('#isPaymentBefore').prop('checked', true)
        var serviceType = $('#serviceType').val()
        if (serviceType !== undefined && serviceType !== null && parseInt(serviceType) !== 0 && serviceType !== '' && (parseInt(serviceType) == 5 || parseInt(serviceType) == 9
            || parseInt(serviceType) == 1 || parseInt(serviceType) == 3)) {
            $('#payment-request-pay-type').attr('disabled', true)
            $('#payment-request-pay-type').addClass('background-disabled')
        }
        _add_payment_request.OnChoosePaymentType()
        if (supplier_id !== undefined && supplier_id !== null && supplier_id !== 0 && supplier_id !== '') {
            _add_payment_request.GetListBankAccountBySupplierID(supplier_id)
        }
        if (isAdminEdit) {
            $('#amount').attr('disabled', false)
            $('#amount').removeClass('background-disabled')
            $('#payment-request-pay-type').attr('disabled', false)
            $('#payment-request-pay-type').removeClass('background-disabled')
        }
        _add_payment_request.OnChangePaymentBefore()
    },
    Close: function () {
        $('#create_contract_pay').removeClass('show')
        setTimeout(function () {
            $('#create_contract_pay').remove();
        }, 300);
    },
    EditRequest: function (isSend = 0) {

        let validate = _add_payment_request.Validate()
        if (!validate)
            return;
        let paymentRequestDetails = []
        var payment_request_type = $('#payment-request-type').val()
        if (payment_request_type !== null && payment_request_type !== '' && (parseInt(payment_request_type) == 1
            || parseInt(payment_request_type) == 4)) { // thanh toán dịch vụ
            for (var i = 0; i < listPaymentRequestDetail.length; i++) {
                var checked = $('#order_ckb_' + i).is(":checked")
                if (checked) {
                    paymentRequestDetails.push(listPaymentRequestDetail[i])
                }
            }
        }
        if (payment_request_type !== null && payment_request_type !== '' && parseInt(payment_request_type) == 3) { // hoàn trả khách hàng
            for (var i = 0; i < listPaymentRequestDetail.length; i++) {
                var checked = $('#order_ckb_' + i).is(":checked")
                if (checked) {
                    paymentRequestDetails.push(listPaymentRequestDetail[i])
                }
            }
        }
        var isSupplierDebt = $('#isSupplierDebt').is(":checked")
        if (isSupplierDebt === undefined || isSupplierDebt === null)
            isSupplierDebt = false
        var isPaymentBefore = $('#isPaymentBefore').is(":checked")
        if (isPaymentBefore === undefined || isPaymentBefore === null)
            isPaymentBefore = false
        var amount_supplier_refund_2=0
        if ($('#amount_supplier_refund').val() != undefined && $('#amount_supplier_refund').val() != null && $('#amount_supplier_refund').val() != '') {
           amount_supplier_refund_2= parseFloat($('#amount_supplier_refund').val().replaceAll('.', '').replaceAll(',', '')) 
        }
        let obj = {
            'id': parseInt($('#payId').val()),
            'status': parseInt($('#payStatus').val()),
            'paymentCode': $('#paymentCode').val(),
            'type': parseInt($('#payment-request-type').val()),
            'paymentType': parseInt($('#payment-request-pay-type').val()),
            'bankingAccountId': parseInt($('#bankingAccount').val()),
            'bankName': $('#bankName').val(),
            'bankAccount': $('#bankAccount').val(),
            'IsServiceIncluded': $('#isServiceIncluded').val() == 1 ? true : false,
            'IsEditAmountReject': $('#isEditAmountReject').val() == 1 ? true : false,
            'isAdminEdit': isSend == 5 ? true : false,
            'description': $('#description').val(),
            'isPaymentBefore': isPaymentBefore,
            'isSupplierDebt': isSupplierDebt,
            'isSend': isSend,
            'paymentDateStr': $('#paymentDate').val(),
            'note': $('#content').val(),
            'clientId': parseInt(($('#client-select').val())),
            'supplierId': parseInt(($('#supplier-select').val())),
            'amount': parseFloat($('#amount').val().replaceAll('.', '').replaceAll(',', '')),
            'totalAmountService': amount_service == 0 ? parseFloat($('#amount_service').val().replaceAll('.', '').replaceAll(',', '')) : amount_service,
            'totalSupplierRefund': amount_supplier_refund == 0 ? amount_supplier_refund_2 : amount_supplier_refund,
            'paymentRequestDetails': paymentRequestDetails,
            'ServiceId': parseInt($('#serviceId').val()),
            'ServiceType': parseInt($('#serviceType').val()),
        }
        _global_function.AddLoading()
        $.ajax({
            url: "/PaymentRequest/Update",
            type: "Post",
            data: obj,
            success: function (result) {
                _global_function.RemoveLoading()
                if (result.isSuccess) {
                    var serviceTypeBK = serviceType
                    var payment_request_typeBK = payment_request_type
                    _msgalert.success(result.message);
                    $.magnificPopup.close();
                    setTimeout(function () {
                        if (serviceTypeBK !== 0 && serviceTypeBK == 3) { // vé máy bay
                            if (serviceId !== null && serviceId !== undefined && serviceId !== '' && serviceId !== '0') {
                                if (payment_request_typeBK == '4')
                                    _set_service_fly_detail.MarketingTab()
                                else
                                    _set_service_fly_detail.ShowPaymentTab()
                            }
                            if (clientId_service !== null && clientId_service !== undefined && clientId_service !== '' && clientId_service !== '0' && clientId_service !== 0)
                                _set_service_fly_detail.ShowRefundTab()
                        } else if (serviceTypeBK !== 0 && serviceTypeBK == 1) { // Khách sạn
                            if (serviceId !== null && serviceId !== undefined && serviceId !== '' && serviceId !== '0') {
                                if (payment_request_typeBK == '4')
                                    _SetService_Detail.LoadingMarketingPaymentRequest()
                                else
                                    _SetService_Detail.loadTourDetail()
                            }
                            if (clientId_service !== null && clientId_service !== undefined && clientId_service !== '' && clientId_service !== '0' && clientId_service !== 0)
                                _SetService_Detail.loadListHotelBookingRefund()
                        }
                        else if (serviceTypeBK !== 0 && serviceTypeBK == 5) { // Tour
                            if (serviceId !== null && serviceId !== undefined && serviceId !== '' && serviceId !== '0') {
                                if (payment_request_typeBK == '4')
                                    _SetService_Tour_Detail.MarketingPaymentRequest(1);
                                else
                                    _SetService_Tour_Detail.ShowTourPaymentTab();
                            }
                            if (clientId_service !== null && clientId_service !== undefined && clientId_service !== '' && clientId_service !== '0' && clientId_service !== 0)
                                _SetService_Tour_Detail.TourServiceRefund(1);
                        }
                        else if (serviceTypeBK !== 0 && serviceTypeBK == 9) { // Others
                            if (serviceId !== null && serviceId !== undefined && serviceId !== '' && serviceId !== '0') {
                                if (typeof _set_service_other_detail != "undefined") {
                                    if (payment_request_typeBK == '4')
                                        _set_service_other_detail.MarketingTab();
                                    else
                                        _set_service_other_detail.ShowPaymentTab();
                                } else {
                                    if (payment_request_typeBK == '4')
                                        _set_service_ws_detail.ShowPaymentTab();
                                    else
                                        _set_service_ws_detail.ShowPaymentTab();
                                }
                            }
                            if (clientId_service !== null && clientId_service !== undefined && clientId_service !== '' && clientId_service !== '0' && clientId_service !== 0)
                                _set_service_other_detail.ShowRefundTab()
                        }
                        else {
                            window.location.reload()
                        }
                    }, 1000)
                } else {
                    _msgalert.error(result.message);

                }
            }
        });
    },
    OnChoosePaymentType: function () {
        var pay_type = $('#payment-request-pay-type').val()
        if (parseInt(pay_type) != 2) { //thanh toán tiền mặt
            $('#bankingAccount').attr('disabled', true)
            $('#bankName').attr('disabled', true)
            $('#bankAccount').attr('disabled', true)
            $('#lblBankAccountRequired').hide()
            $('#lblBankNameRequired').hide()
            $('#lblBankAccount').show()
            $('#lblBankName').show()
            $('#bankingAccount').val(-1)
        } else {
            $('#bankingAccount').attr('disabled', false)
            $('#bankName').attr('disabled', true)
            $('#bankAccount').attr('disabled', false)
            $('#lblBankAccountRequired').show()
            $('#lblBankNameRequired').show()
            $('#lblBankAccount').hide()
            $('#lblBankName').hide()
        }
        if ($('#payment-request-type').val() == 5) {
            $('#lblBankAccountRequired').hide()
            $('#lblBankNameRequired').hide()
            $('#lblBankName').hide()
            $('#lblBankAccount').hide()
            $('#bankingAccount').hide()
            $('#bankAccount').hide()
            $('#bankingAccount').val(-1)
            $('#bankName').attr('disabled', false)
            $('#bankName').removeClass('background-disabled')
        }
        if ($('#supplier-select').val() !== undefined && $('#supplier-select').val() !== null && $('#supplier-select').val() !== '' && $('#supplier-select').val() !== 0) {
            _add_payment_request.GetListBankAccountBySupplierID($('#supplier-select').val())
        }
    },
    OnCheckBox: function (index) {
        if (isServiceInclude !== null && isServiceInclude !== undefined && isServiceInclude === 1)
            return

        var totalAmount = 0
        var payment_request_type = $('#payment-request-type').val()
        for (var i = 0; i < listPaymentRequestDetail.length; i++) {
            if (listPaymentRequestDetail[i].amountPayment !== undefined && listPaymentRequestDetail[i].amountPayment !== null
                && listPaymentRequestDetail[i].amountPayment) {
                var checked = $('#order_ckb_' + i).is(":checked")
                if ((checked || listPaymentRequestDetail[i].isChecked) && $('#amount_order_' + i).val() !== undefined && $('#amount_order_' + i).val() !== null
                    && $('#amount_order_' + i).val() !== '') {
                    totalAmount += parseFloat($('#amount_order_' + i).val().replaceAll('.', '').replaceAll(',', ''))
                }
            }
        }
        if (isNaN(totalAmount)) totalAmount = 0

        for (var i = 0; i < listPaymentRequestDetail.length; i++) {
            if (listPaymentRequestDetail[i].isDisabled)
                continue
            var checked = $('#order_ckb_' + i).is(":checked")
            if (checked || listPaymentRequestDetail[i].isChecked) {
                if (payment_request_type !== null && payment_request_type !== '' && parseInt(payment_request_type) != 1) {
                    $('#amount_order_' + i).attr('disabled', false)
                    $('#amount_order_' + i).removeClass('background-disabled')
                }
                var amount = $('#amount_order_' + i).val()
                if (amount === null || amount === undefined || amount === '' || parseInt(amount) === 0) {
                    $('#amount_order_' + i).val(_add_payment_request.FormatNumberStr(listPaymentRequestDetail[i].totalNeedPayment))
                }
                _add_payment_request.UpdateAmount(i, listPaymentRequestDetail[i].orderId)
            } else {
                $('#amount_order_' + i).attr('disabled', true)
                $('#amount_order_' + i).addClass('background-disabled')
                $('#amount_order_' + i).val('')
            }
        }
        $('#amount').val(this.FormatNumberStr(totalAmount))
        _add_payment_request.GetTotalAmount(index)
    },
    GetTotalAmount: function (index) {
        var totalAmount = 0
        for (var i = 0; i < listPaymentRequestDetail.length; i++) {
            if (listPaymentRequestDetail[i].amountPayment !== undefined && listPaymentRequestDetail[i].amountPayment !== null
                && listPaymentRequestDetail[i].amountPayment) {
                var checked = $('#order_ckb_' + i).is(":checked")
                if (checked) {
                    totalAmount += parseFloat($('#amount_order_' + i).val().replaceAll('.', '').replaceAll(',', ''))
                }
            }
        }
        $('#total_amount_need_pay').html(_add_payment_request.FormatNumberStr(totalAmount))
        $('#amount').html(_add_payment_request.FormatNumberStr(totalAmount))
    },
    OnChangeAmount: function () {

        var payment_request_type = $('#payment-request-type').val()
        if (payment_request_type !== null && payment_request_type !== ''
            && (parseInt(payment_request_type) == 1 || parseInt(payment_request_type) == 3 || parseInt(payment_request_type) == 4)) { // thanh toán dịch vụ
            var currentAmomunt = parseFloat($('#amount').val().replaceAll('.', '').replaceAll(',', ''))
            if (currentAmomunt < amountEdit) {
                _msgalert.error('Vui lòng nhập số tiền lớn hơn hoặc bằng số tiền ban đầu');
                $('#amount').val(_add_payment_request.FormatNumberStr(amountEdit))
                return false;
            }
        }
    },
    AddToListDetail: function (index) {
        var payment_request_type = $('#payment-request-type').val()
        if (payment_request_type !== null && payment_request_type !== '' && (parseInt(payment_request_type) == 1 || parseInt(payment_request_type) == 3)) { // thanh toán dịch vụ
            for (var i = 0; i < listPaymentRequestDetail.length; i++) {
                if (i === index) {
                    listDetail.push(listPaymentRequestDetail[i])
                }
            }
        }
    },
    ClearError: function () {
        $(".validate-payment-request-type").find('p').remove();
        $(".validate-supplier-select").find('p').remove();
        $(".validate-client-select").find('p').remove();
        $(".validate-amount").find('p').remove();
        $(".validate-payment-request-pay-type").find('p').remove();
        $(".validate-bankingAccount").find('p').remove();
        $(".validate-bankingName").find('p').remove();
        $(".validate-paymentDate").find('p').remove();
        $(".validate-content").find('p').remove();
        $(".validate-description").find('p').remove();
    },
    DisplayError: function (className, message) {
        $("." + className).find('p').remove();
        $("." + className).append("<p>" + message + "</p>");
        $("." + className).css("color", "red");
        $("." + className).css("font-size", "13px");
        $("." + className).css("margin-top", "3px");
    },
    OnChangePaymentBefore: function () {
        var isPaymentBefore = $('#isPaymentBefore').is(":checked")
        if (isPaymentBefore) {
            $('#amount').attr('disabled', true)
            $('#amount').removeClass('background-disabled')
        }
    },
}
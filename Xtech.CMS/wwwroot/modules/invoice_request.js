

let countCreate = -1;
let lstRemove = [];
let lstUpSert = [];
var InvoiceRequest;

let invoiceSearchModel = {
    invoiceRequestNo: null,
    planDateFrom: null,
    planDateTo: null,
    invoiceNo: null,
    invoiceCode: null,
    exportDateFrom: null,
    exportDateTo: null,
    invoiceRequestStatus: null,
    isHasBill: null,
    clientId: null,
    userCreate: null,
    createDateFrom: null,
    createDateTo: null,
    userVerify: null,
    verifyDateFrom: null,
    verifyDateTo: null,
    pageIndex: 1,
    pageSize: 10
};

$(document).ready(function ()
{
    _invoiceRequest.Search(invoiceSearchModel);

    $('input[name="PlainDate"]').daterangepicker({
        autoUpdateInput: false,
        locale: {
            cancelLabel: 'Clear'
        }
    });
    $('input[name="PlainDate"]').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
        isPicker = true;
    });
    $('input[name="PlainDate"]').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        searchModel.CreateDateFrom = null;
        searchModel.CreateDateTo = null;
        isPicker = false;
    }); 


    $('input[name="datetimeOrder"]').daterangepicker({
        maxDate: new Date(),
        autoUpdateInput: false,
        locale: {
            cancelLabel: 'Clear'
        }
    });
    $('input[name="datetimeOrder"]').on('apply.daterangepicker', function (ev, picker) {
        $(this).val(picker.startDate.format('DD/MM/YYYY') + ' - ' + picker.endDate.format('DD/MM/YYYY'));
        isPicker = true;
    });
    $('input[name="datetimeOrder"]').on('cancel.daterangepicker', function (ev, picker) {
        $(this).val('');
        searchModel.CreateDateFrom = null;
        searchModel.CreateDateTo = null;
        isPicker = false;
    }); 

    $("#InvoiceRequestNo").select2({
        theme: 'bootstrap4',
        placeholder: "Mã phiếu",
        maximumSelectionLength: 1,
        ajax: {
            url: "/InvoiceRequest/InvoiceRequestSuggestion",
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
                            text: item.invoicerequestno,
                            id: item.invoicerequestno,
                        }
                    })
                };
            },
            cache: true
        }
    }).on('select2:opening', function (e) {
        $('#InvoiceRequestNo').val([]).trigger('change');
    });

    $("#client").select2({
        theme: 'bootstrap4',
        placeholder: "Tên KH, Điện Thoại, Email",
        maximumSelectionLength: 1,
        ajax: {
            url: "/CustomerManager/ClientSuggestion",
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
                            text: item.clientname + ' - ' + item.email + ' - ' + item.phone,
                            id: item.id,
                        }
                    })
                };
            },
            cache: true
        }
    });

    $("#txtNguoiTao").select2({
        theme: 'bootstrap4',
        placeholder: "Người tạo",
        maximumSelectionLength: 1,
        ajax: {
            url: "/OrderManual/UserSuggestion",
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

    $("#txtNguoiDuyet").select2({
        theme: 'bootstrap4',
        placeholder: "Người duyệt",
        maximumSelectionLength: 1,
        ajax: {
            url: "/OrderManual/UserSuggestion",
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
})

var _invoiceRequest = {

    GetParams: function ()
    {
        var rangeCreatedTime = $("#filter_Created_Date").val().split("- "); 
        var rangePlainTime = $("#filter_PlainDate").val().split("- ");
        var rangeExportTime = $("#filter_Export_Date").val().split("- ");
        var rangeVerifyTime = $("#filter_Verify_Date").val().split("- ");

        invoiceSearchModel.createDateFrom = rangeCreatedTime != '' ? _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate(rangeCreatedTime[0])) : null;
        invoiceSearchModel.createDateTo = rangeCreatedTime != '' ? _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate(rangeCreatedTime[1])) : null;
        invoiceSearchModel.planDateFrom = rangePlainTime != '' ? _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate(rangePlainTime[0])) : null;
        invoiceSearchModel.planDateTo = rangePlainTime != '' ? _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate(rangePlainTime[1])) : null;
        invoiceSearchModel.exportDateFrom = rangeExportTime != '' ? _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate(rangeExportTime[0])) : null;
        invoiceSearchModel.exportDateTo = rangeExportTime != '' ? _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate(rangeExportTime[1])) : null;
        invoiceSearchModel.verifyDateFrom = rangeVerifyTime != '' ? _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate(rangeVerifyTime[0])) : null;
        invoiceSearchModel.verifyDateTo = rangeVerifyTime != '' ? _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate(rangeVerifyTime[1])) : null;
        invoiceSearchModel.invoiceRequestNo = $("#InvoiceRequestNo").val();
        invoiceSearchModel.invoiceRequestStatus = $("#PaymentRequestStatus").find(':selected').val();
        invoiceSearchModel.clientId = $("#client").val();
        invoiceSearchModel.userCreate = $("#txtNguoiTao").val();
        invoiceSearchModel.userVerify = $("#txtNguoiDuyet").val();
        invoiceSearchModel.pageIndex = 1;
        invoiceSearchModel.pageSize = 10;
        _invoiceRequest.Search(invoiceSearchModel);
    },

    Search: function ()
    {
        $.ajax({
            url: "/InvoiceRequest/Search",
            type: "post",
            data: invoiceSearchModel ,
            success: function (result) {
                $("#grid_data_invoice").html(result);
            }
        });
    },

    OnchangeExTra: function (id) {
        var ExTrapriceEle = $('input#price_extra_' + id);
        ExTrapriceEle.prop('disabled', 'true')
        var ExTraprice = ExTrapriceEle.val().replaceAll(',', '');
        if (/^\d+$/.test(ExTraprice)) {
            var formattedExtraprice = parseFloat(ExTraprice).toLocaleString('vi-VN');
            formattedExtraprice = formattedExtraprice.replaceAll('.', ',');
            ExTrapriceEle.val(formattedExtraprice);

            _invoiceRequest.CalculatingTotalExtra();

            ExTrapriceEle.prop('disabled', false)
            ExTrapriceEle.focus();
        }
        else {
            ExTraprice = ExTrapriceEle.val().replace(/[^\d]/g, "")
            var formattedExtraprice = parseFloat(ExTraprice).toLocaleString('vi-VN');
            formattedExtraprice = formattedExtraprice.replaceAll('.', ',')
            ExTrapriceEle.val(formattedExtraprice)
            ExTrapriceEle.prop('disabled', false)
            ExTrapriceEle.focus();
        }
    },

    CalculatingTotalExtra: function () {
        var inputs = $(".invoice_request_detail_tbody tr td input.invoice_rq_detail_extra");
        var totalExTra = 0;
        inputs.each(function () {
            var inputVal = $(this).val().replaceAll(',', '');
            totalExTra = totalExTra + parseInt(inputVal);
        });
        var formattedtotalExTra = totalExTra.toLocaleString('vi-VN');
        formattedtotalExTra = formattedtotalExTra.replaceAll('.', ',');
        $(".invoice_request_extra").text(formattedtotalExTra)
    },

    CalculatingTotalExtraExport: function () {
        var inputs = $(".invoice_request_detail_tbody tr td input.invoice_rq_detail_extra_export");
        var totalExTraExport = 0;
        inputs.each(function () {
            var inputVal = $(this).val().replaceAll(',', '');
            totalExTraExport = totalExTraExport + parseInt(inputVal);
        });
        var formattedtotalExTraExport = totalExTraExport.toLocaleString('vi-VN');
        formattedtotalExTraExport = formattedtotalExTraExport.replaceAll('.', ',');
        $(".invoice_request_extraExport").text(formattedtotalExTraExport)
    },

    OnchangeExTraExport: function (id) {
        var ExTraExportEle = $('input#price_extra_export_' + id);
        ExTraExportEle.prop('disabled', 'true')
        var ExTraExport = ExTraExportEle.val().replaceAll(',', '');
        if (/^\d+$/.test(ExTraExport)) {
            var formattedExtraExport = parseFloat(ExTraExport).toLocaleString('vi-VN');
            formattedExtraExport = formattedExtraExport.replaceAll('.', ',');
            ExTraExportEle.val(formattedExtraExport);

            _invoiceRequest.CalculatingTotalExtraExport();

            ExTraExportEle.prop('disabled', false)
            ExTraExportEle.focus();
        }
        else {
            ExTraExport = ExTraExportEle.val().replace(/[^\d]/g, "")
            var formattedExtraExport = parseFloat(ExTraExport).toLocaleString('vi-VN');
            formattedExtraExport = formattedExtraExport.replaceAll('.', ',')
            ExTraExportEle.val(formattedExtraExport)
            ExTraExportEle.prop('disabled', false)
            ExTraExportEle.focus();
        }
    },
    ShowForm: function (input, ClientId,OrderId) {
        $(".show_form_invoice").addClass('disabled_td');
        $.ajax({
            url: "/InvoiceRequest/InvoiceRequestForm",
            type: "post",
            data: { invoiceId: input, client_Id: ClientId, Client_Name: $(".name").text() },
            success: function (result) {
                $('body').append(result)
                _invoiceRequest.CalculatingTotalExtra();
                _invoiceRequest.CalculatingTotalExtraExport();
                var Total_IR_Amount = 0;
                var inputs = $(".invoice_request_detail_tbody tr:not(:last)");
                _invoiceRequest.LoadAddCustomer(ClientId);
                inputs.each(function () {
                    var input_items = $(this).find('td input.invoice_rq_detail_amount');
                    var inputVal = input_items.val().replaceAll(',', '');
                    Total_IR_Amount = Total_IR_Amount + parseInt(inputVal);
                });
                var formattedTotalAmount = Total_IR_Amount.toLocaleString('vi-VN');
                formattedTotalAmount = formattedTotalAmount.replaceAll('.', ',');
                $("#invoice_request_totalAmount").text(formattedTotalAmount);
                $(".show_form_invoice").removeClass('disabled_td');
                _invoiceRequest.LoadRelatedOrders(ClientId, OrderId);
                _global_function.RenderFileAttachment($("#invoice_rq_files"), input, 15);
            }
        });
    },
    LoadAddCustomer: function (ClientId)
    {
        if (!ClientId)
        {
            $("#Client_select").select2({
                theme: 'bootstrap4',
                placeholder: "Tên KH, Điện Thoại, Email",
                maximumSelectionLength: 1,
                ajax: {
                    url: "/CustomerManager/ClientSuggestion",
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
                                    text: item.clientname + ' - ' + item.email + ' - ' + item.phone,
                                    id: item.id,
                                }
                            })
                        };
                    },
                    cache: true
                }
            }).on('select2:opening', function (e) {
                $('#Client_select').val([]).trigger('change');
                _invoiceRequest.LoadRelatedOrders()
            });
            $("#Client_select").on("select2:select", function (e) {
                var data = e.params.data;
                _invoiceRequest.LoadRelatedOrders(data.id)
            });
        }
    },
    LoadRelatedOrders: function (client_ID, OrderId) {
       /* let Order_Id = $("#order_Id").val();*/
        let _searchModel2 = {
            ClientId: client_ID,
            CreateDateFrom: null,
            OrderId: OrderId,
            CreateDateTo: null,
            Status: null,
            PaymentStatus: '1,2',
            StatusTab: 99,
            PageIndex: 1,
            pageSize: 20,
            currentPage: 1,
        };
        $('.list_related_order').html(``);
        if (_searchModel2.ClientId != null)
        {
            $.ajax({
                url: "/InvoiceRequest/ListOrderRelated",
                type: "post",
                data: _searchModel2,
                success: function (result) {
                    if (result != null) {
                        if (result.listData) {
                            var STT = 1;
                            result.listData.forEach(function (item) {
                                $('.list_related_order').append(`
                            <tr>
                               <td>
                                  <label class="check-list number">
                                  <input onchange="_invoiceRequest.OnchangeOrder(this)" class="order_related checkbox" type="radio" name="order_related" value="${item.orderId}">
                                  <span class="checkmark"></span>
                                  ${STT}
                                  </label>
                               </td>
                               <td class="blue">${item.orderNo}</td>
                               <td>${new Date(item.startDate).toLocaleDateString('vi-VN')} - ${new Date(item.endDate).toLocaleDateString('vi-VN')}</td>
                               <td>
                                  <div>${item.salerName}</div>
                                  <div>${item.salerGroupName != null ? item.salerGroupName : ""}</div>
                               </td>
                               <td class="text-right">${item.amount.toLocaleString("vi-VN").replaceAll('.', ',')}</td>
                            </tr>`)
                                if (item.orderId == OrderId) {
                                    var currentOrder = $('input.order_related[value = "' + OrderId + '"]');
                                    currentOrder.prop('checked', true);
                                }
                                STT += 1;
                            })
                        }
                    }
                }
            });
        }
    },

    CloseForm: function () {
        $("#invoice-form").remove();
    },

    CreateInvoiceRequestDetail: function () {
        var lastRow = $(".invoice_request_detail_sumary_row");
        $('<tr id="' + countCreate + '">' +
            '<td class="invoice_request_detail_number"></td >' +
            '<td><input type="text" class="form-control invoice_rq_detail_pruductname" value="" id="product_name_' + countCreate + '"></td>' +
            '<td>' +
            '<input class="form-control invoice_rq_detail_unit" id="unit_' + countCreate + '">' +
            '</td>' +
            '<td><input type="number" class="form-control text-right invoice_rq_detail_quantity" min="0" oninput="_invoiceRequest.OnchangeQuantity(' + countCreate + ')" value="" id="quantity_' + countCreate + '"></td>' +
            '<td><input type="text" class="form-control text-right invoice_rq_detail_price" min="0" oninput="_invoiceRequest.OnchangePrice(' + countCreate + ')" id="price_' + countCreate + '" value=""></td>' +
            '<td><input type="text" class="form-control text-right invoice_rq_detail_amount" disabled value="" id="total_price_' + countCreate + '" /></td>' +
            '<td><input type="text" class="form-control text-right invoice_rq_detail_extra_export" min="0" oninput="_invoiceRequest.OnchangeExTraExport(' + countCreate + ')" id="price_extra_export_' + countCreate + '" value=""></td>' +
            '<td><input type="text" class="form-control text-right invoice_rq_detail_extra" min="0" oninput="_invoiceRequest.OnchangeExTra(' + countCreate + ')" id="price_extra_' + countCreate + '" value=""></td>' +
            '<td>' +
            '<a class="delete" onclick="_invoiceRequest.DeleteInvoiceRequestDetail(' + countCreate + ')"><i class="fa fa-trash-o"></i></a>' +
            '</td>' +
            '</tr >').insertBefore(lastRow);
        countCreate -= 1;
        var rows = $(".invoice_request_detail_tbody tr");
        var index = 1;
        rows.each(function () {
            $(this).find(".invoice_request_detail_number").text(index);
            index++;
        })
    },

    DeleteInvoiceRequestDetail: function (id) {
        var row = $(".invoice_request_detail_tbody tr#" + id + "");
        row.remove();
        this.CalculatingTotalExtra();
        this.CalculatingTotalExtraExport()

        var Total_IR_Amount = 0;
        var inputs = $(".invoice_request_detail_tbody tr:not(:last)");

        inputs.each(function () {
            var input_items = $(this).find('td input.invoice_rq_detail_amount');
            var inputVal = input_items.val().replaceAll(',', '');
            Total_IR_Amount = Total_IR_Amount + parseInt(inputVal);
        });
        var formattedTotalAmount = Total_IR_Amount.toLocaleString('vi-VN');
        formattedTotalAmount = formattedTotalAmount.replaceAll('.', ',');
        $("#invoice_request_totalAmount").text(formattedTotalAmount);
        if (id > 0) {
            lstRemove.push(id);
        }
    },
    GetListObjUpSert: function (type) {
        InvoiceRequest = {
            Id: $("#InvoiceRequest_Id").val(),
            ClientId: $("#Client_select").val(),
            PlanDate: _global_function.ParseJSDate($("#plan_date").val()) != undefined ? _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate($("#plan_date").val())) : null,
            TaxNo: $("#tax_no").val(),
            CompanyName: $("#company_name").val(),
            Status: 0,
            Address: $("#address").val(),
            OrderId: $("#order_Id").val() != undefined ? $("#order_Id").val() : $('input[name="order_related"]:checked').val(),
            Note: $("#Note_Invoice_rq").val()
        }
        if (type == 3)
        {
            InvoiceRequest.Status = 3 // luu nhap
        }

        var table = $(".invoice_request_detail_tbody tr:not(:last)");
        table.each(function () {
            var row_item = $(this);
            var obj =
            {
                Id: row_item.attr('id'),
                ProductName: row_item.find('td input.invoice_rq_detail_pruductname').val(),
                Unit: row_item.find('td input.invoice_rq_detail_unit').val(),
                Quantity: row_item.find('td input.invoice_rq_detail_quantity').val(),
                Price: row_item.find('td input.invoice_rq_detail_price').val().replaceAll(',',''),
                PriceExtra: row_item.find('td input.invoice_rq_detail_extra').val().replaceAll(',', ''),
                PriceExtraExport: row_item.find('td input.invoice_rq_detail_extra_export').val().replaceAll(',', '')
            }
            if (obj.Price >= 0 &&
                obj.PriceExtra >= 0 &&
                obj.PriceExtraExport >= 0 &&
                obj.ProductName != null &&
                obj.Price != '' &&
                obj.PriceExtra != '' &&
                obj.PriceExtraExport != '' &&
                obj.ProductName != '')
            {
                lstUpSert.push(obj);
            }
        })
    },
    Reject: function ()
    {
        var rejectObj =
        {
            Id: $("#InvoiceRequest_Id").val(),
            DeclineReason: $("#Decline_reason").val(),
            Status : 2 // Bi tu choi
        }
        $.ajax({
            url: "/InvoiceRequest/ApproveOrReject",
            type: "post",
            data: rejectObj,
            success: function (result) {
                $('#img_loading_summit').hide();
                if (result < 0) {
                    _msgalert.error(result.msg);
                    $('#btn_summit_order').show();
                    return;
                }
                _msgalert.success(result.msg);
                $('.toast-success').text('Cập nhật thành công')
                $('.img_loading_summit').hide();

                setTimeout(function () {
                    window.location.reload();
                }, 2000);
                return;
            }
        });
    },
    Approve: function (id)
    {
        var approveObj =
        {
            Id: $("#InvoiceRequest_Id").val(),
            Status: 1 // Da duyet 
        }
        $.ajax({
            url: "/InvoiceRequest/ApproveOrReject",
            type: "post",
            data: approveObj,
            success: function (result) {
                $('#img_loading_summit').hide();
                if (result < 0) {
                    _msgalert.error(result.msg);
                    $('#btn_summit_order').show();
                    return;
                }
                _msgalert.success(result.msg);
                $('.toast-success').text('Cập nhật thành công')
                $('.img_loading_summit').hide();

                setTimeout(function () {
                    window.location.reload();
                }, 2000);
                return;
            }
        });
    },

    Onsubmit: function (type) {
        _invoiceRequest.GetListObjUpSert(type);
        if (lstUpSert.length > 0) {
            var rsvalid = true;
            if (InvoiceRequest.PlanDate == null) {
                _msgalert.error("Ngày dự kiến rỗng hoặc không đúng định dạng");
                rsvalid = false;
            }
            if (InvoiceRequest.TaxNo == '')
            {
                _msgalert.error("Mã số thuế đang rỗng");
                rsvalid = false;
            }
            if (InvoiceRequest.CompanyName == '') {
                _msgalert.error("Tên công ty đang rỗng");
                rsvalid = false;
            }
            if (InvoiceRequest.Address == '') {
                _msgalert.error("Địa chỉ đang rỗng");
                rsvalid = false;
            }
            if (InvoiceRequest.OrderId == undefined) {
                _msgalert.error("Vui lòng chọn hóa đơn liên quan");
                rsvalid = false;
            }
            if (!rsvalid) {
                InvoiceRequest.PlanDate = null;
                InvoiceRequest.TaxNo = '';
                InvoiceRequest.CompanyName = '';
                InvoiceRequest.Address = '';
                InvoiceRequest.Note = '';
                lstUpSert = [];
            }
            else
            {
/*                if ($("#InvoiceRequest_Id").val()) {
                    _global_function.ConfirmFileUpload($("#invoice_rq_files"), $("#InvoiceRequest_Id").val())
                }*/
                $.ajax({
                    url: "/InvoiceRequest/SubmitChange",
                    type: "post",
                    data: { lstInvoiceRQDetail: lstUpSert, lstRemove: lstRemove, invoiceRequest: InvoiceRequest },
                    success: function (result) {
                        $('#img_loading_summit').hide();
                        if (result < 0) {
                            _msgalert.error(result.msg);
                            $('#btn_summit_order').show();
                            return;
                        }
                        _msgalert.success(result.msg);
                        $('.toast-success').text('Cập nhật thành công')
                        $('.img_loading_summit').hide();
                        _global_function.ConfirmFileUpload($("#invoice_rq_files"), result)
                        setTimeout(function () {
                            window.location.reload();
                        }, 2000);
                        return;
                    }
                });
            }
        }
        else
        {
            _msgalert.error("Cập nhật thất bại");
            InvoiceRequest.PlanDate = null;
            InvoiceRequest.TaxNo = '';
            InvoiceRequest.CompanyName = '';
            InvoiceRequest.Address = '';
            InvoiceRequest.Note = '';
            lstUpSert = [];
        }
    },
    OnchangeQuantity: function (id) {
        this.CalculatingAmount(id)
    },

    OnchangePrice: function (id) {
        var priceEle = $('input#price_' + id);
        priceEle.prop('disabled','true')
        var price = priceEle.val().replaceAll(',','');
        if (/^\d+$/.test(price)) {
            var formattedprice = parseFloat(price).toLocaleString('vi-VN');
            formattedprice = formattedprice.replaceAll('.', ',');
            priceEle.val(formattedprice);

            this.CalculatingAmount(id)
            priceEle.prop('disabled', false)
            priceEle.focus();
        }
        else
        {
            price = priceEle.val().replace(/[^\d]/g, "")
            var formattedprice = parseFloat(price).toLocaleString('vi-VN');
            formattedprice = formattedprice.replaceAll('.', ',')
            priceEle.val(formattedprice)
            priceEle.prop('disabled', false)
            priceEle.focus();
        }
    },
    OnchangeOrder: function (Ele)
    {
        InvoiceRequest.OrderId = Ele.value;
    },

    CalculatingAmount: function (Id) {
        if (Id) {
            var quantity = $("#quantity_" + Id).val();
            var price = $("#price_" + Id).val().replaceAll(',','');
            var total = $("#total_price_" + Id);
            var Total_IR_Amount = 0;
            var Amount = quantity * price;
            // Định dạng số theo chuẩn Việt Nam (sử dụng dấu chấm)
            var formattedAmount = Amount.toLocaleString('vi-VN');

            // Thay thế dấu chấm bằng dấu phẩy
            formattedAmount = formattedAmount.replaceAll('.', ',');

            total.val(formattedAmount);

            var inputs = $(".invoice_request_detail_tbody tr:not(:last)");

            inputs.each(function () {
                var input_items = $(this).find('td input.invoice_rq_detail_amount');
                var inputVal = input_items.val().replaceAll(',','');
                Total_IR_Amount = Total_IR_Amount + parseInt(inputVal);
            });
            var formattedTotalAmount = Total_IR_Amount.toLocaleString('vi-VN');
            formattedTotalAmount = formattedTotalAmount.replaceAll('.', ',');
            $("#invoice_request_totalAmount").text(formattedTotalAmount);
        }
    },

    OnPaging: function (value)
    {
        invoiceSearchModel.pageIndex = value
        _invoiceRequest.Search(invoiceSearchModel);
    },
    onSelectPageSize: function () {
        invoiceSearchModel.pageSize = parseInt($("#selectPaggingOptions").find(':selected').val());
        invoiceSearchModel.pageIndex = 1;
        this.Search(invoiceSearchModel);
    }
}
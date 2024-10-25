var _order_detail_create = {
    //No MSG
    Initialization: function () {
        _order_detail_create.ClientSuggesstion()
        _order_detail_common.Select2WithFixedOptionAndNoSearch($("#branch"))
        $('#main-staff').select2();
        $('#sub-staff').select2();

        _order_detail_create.DynamicBindClientInput();
        _order_detail_create.UserSuggesstion();
    },
    ClientSuggesstion: function () {
        $("#client-select").select2({
            theme: 'bootstrap4',
            placeholder: "Khách hàng",
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
            $('#client-select').val([]).trigger('change');
        });

    },
    
    Summit: function () {
        $("#btn_summit_order").prop("disabled", true);
        let Form = $("#form-create-order-manual")
        Form.validate({
            rules: {
                "client": {
                    required: true,
                },
                "order_label": {
                    required: true,
                }
            },
            messages: {
                "client": {
                    required: "Thông tin khách hàng không được để trống",
                },
                "order_label": {
                    required: "Nhãn đơn không được để trống",
                }
            },

        });

        if ($('#client-select').find(':selected').val() == undefined || parseInt($('#client-select').find(':selected').val()) <= 0) {
            $("#btn_summit_order").prop("disabled", false);
            _msgalert.error("Vui lòng nhập / chọn đúng khách hàng cho đơn hàng này");
            return;
        }
        var order_label = $('#order_label').val()
        if (order_label == undefined || order_label.trim() == '') {
            _msgalert.error("Vui lòng nhập đơn nhãn cho đơn hàng này");
            return;
        }
        if (order_label.trim().length > 60) {
            _msgalert.error("Nhãn đơn của đơn hàng không được vượt quá 60 ký tự");
            $("#btn_summit_order").prop("disabled", false);
            debugger;
            return;
        }
        var selected_branch = $('#branch').find(":selected").val();
        if (selected_branch == undefined || parseInt(selected_branch) <= 0) {
            $("#btn_summit_order").prop("disabled", false);
            _msgalert.error("Vui lòng chọn đúng chi nhánh cho đơn hàng");
            return;
        }
        $('#btn_summit_order').hide();
        $('.img_loading_summit').show();
        var lstSubId = $("#sub-staff").select2("val")
        var summit_model = {
            ClientId: $('#client-select').val(),
            SalerId: $("#main-staff").select2("val"),
            SalerGroupId: "",
            BranchCode: $('#branch').find(":selected").val(),
            order_source: "CMS",
            Note: $('#note').val(),
            Label: $('#order_label').val()
        };

        if (lstSubId != null && lstSubId.length > 0)
        {
            lstSubId.forEach(item => {
                summit_model.SalerGroupId = summit_model.SalerGroupId + item + ","
            })
            summit_model.SalerGroupId = summit_model.SalerGroupId.substring(0, summit_model.SalerGroupId.length - 1)
        }

        $.ajax({
            url: "/OrderManual/CreateOrder",
            type: "post",
            data: summit_model,
            success: function (result) {
                $('#img_loading_summit').hide();
                if (result.status != 0) {
                    _msgalert.error(result.msg);
                    $('#btn_summit_order').show();
                    return;
                }
                _msgalert.success("Tạo đơn hàng thành công","thông báo");
                $('#btn_summit_order').prop("onclick", null).off("click");;
                $('#btn_summit_order').text('Tạo đơn hàng thành công');
                $('#btn_summit_order').show();
                $('.img_loading_summit').hide();

                setTimeout(function () {
                    window.location.href = "/OrderDetail/" + result.order_id;
                }, 1000);
                return;

            }
        });

    },
    DynamicBindClientInput: function () {
        $('body').on('click', '.modal-order', function (event) {
            if (!$(event.target).hasClass('modal-dialog')) {
                $(event.target).closest('.modal').removeClass('show');
                setTimeout(function () {
                    $(event.target).closest('.modal').remove();
                }, 300);
            }

        });


    },
    UserSuggesstion: function () {
        $('#main-staff').select2();
        $('#sub-staff').select2();

        $.ajax({
            url: "/CustomerManager/UserSuggestion",
            type: "post",
            data: { txt_search: "" },
            success: function (result) {
                if (result != undefined && result.data != undefined && result.data.length > 0) {
                    result.data.forEach(function (item) {
                        $('#main-staff').append(_order_detail_html.html_user_option.replaceAll('{user_id}', item.id).replace('{user_email}', item.email).replace('{user_name}', item.username).replace('{user_phone}', item.phone == undefined ? "" : ' - ' + item.phone))
                        $('#sub-staff').append(_order_detail_html.html_user_option.replaceAll('{user_id}', item.id).replace('{user_email}', item.email).replace('{user_name}', item.username).replace('{user_phone}', item.phone == undefined ? "" : ' - ' + item.phone))

                    });
                    $("#main-staff").trigger('change');
                    $("#sub-staff").trigger('change');
                    $('#main-staff').val(result.selected).trigger('change');
                }
                else {
                    $("#main-staff").trigger('change');
                    $("#sub-staff").trigger('change');
                }

            }
        });
    }
}

let lstDelete = [];
let lstUpdate = [];
let lstCreate = [];
let BookingId;
let countNew = -1;
var OtherBookingUpdate;


var _order_detail_create_service = {
    ShowOtherServiceForm: function (Id) {
        var orderId = $("#order_Id").val();
        $.ajax({
            url: "/Order/AddOtherService",
            type: "post",
            data: { BookingId: Id, OrderId :orderId},
            success: function (result) {
                BookingId = Id;
                $('body').append(result);
                var table = $(".service-other-packages-tbody tr:not(:last)");
                var TotalQuantity = 0;
                table.each(function () {
                    var row_item = $(this);
                    TotalQuantity = TotalQuantity + parseFloat(row_item.find('.service-other-packages-quantity').val());
                });
                $(".service-other-packages-total-amount").text(TotalQuantity);
                _global_function.RenderFileAttachment($("#attachment-file-block"), Id, 20);
            }
        });
    },

    Close: function () {
        $('#otherbooking-service').remove();
    },

    DeleteOtherBookingPackage: function (Elet) {
        let OtherBookingId = $("#BookingId")
        var IdOtherBookingPackage = Elet[0].id.replace('OtherBooingPackageID_', '');
        if (OtherBookingId != null && IdOtherBookingPackage) {
            lstDelete.push(IdOtherBookingPackage);
        }
        Elet.remove();
        var rows = $(".service-other-packages-tbody tr:not(:last)");
        var index = 1;
        if (rows.length > 0) {
            rows.each(function () {
                $(this).find(".service-other-packages-order").text(index);
                index++;
            })
        }
        else
        {
            $(".service-other-packages-total-amount").text(0);
            $(".service-other-packages-total-profit").text($("#servicemanual-other-other-amount").val().replaceAll(',', ''));
        }
    },
    CalculatingAmount: function (Id) {
        if (Id)
        {
            var baseprice = $("#baseprice_" + Id).val().replaceAll(',', '');
            var profit = $("#profit_" + Id).val();
            var quantity = $("#quantity_" + Id).val();
            var Amount;
            if (baseprice != 0 && profit != 0 && quantity != 0) {
                Amount = (baseprice * quantity);
                var totalprofit = (profit / 100) * Amount;
                Amount += totalprofit;
                $("#amount_" + Id).val(Amount.toFixed(2));
            }
            else if (baseprice != 0 && quantity != 0) {
                Amount = baseprice * quantity;
                $("#amount_" + Id).val(Amount.toFixed(2));
            }
            else {
                $("#amount_" + Id).val(0);
            }
        }


        //cập nhật giá chung
        var table = $(".service-other-packages-tbody tr:not(:last)");
        var TotalQuantity = 0;
        var TotalAmount = 0;
        var OtherAmount = $("#servicemanual-other-other-amount").val().replaceAll(',', '');
        table.each(function () {
            var row_item = $(this);
            TotalQuantity = TotalQuantity + parseFloat(row_item.find('.service-other-packages-quantity').val());
            TotalAmount = TotalAmount +  parseFloat(row_item.find('.service-other-packages-amount').val());
        });
        TotalAmount = TotalAmount + parseFloat(OtherAmount);
        $(".service-other-packages-total-amount").text(TotalQuantity);
        $(".service-other-packages-total-profit").text(TotalAmount.toLocaleString());
    },
    OnchangeVAT: function (input)
    {
        var Id = input[0].id.replace('profit_','');
        this.CalculatingAmount(Id);
    },
    OnchangeOthersAmount: function () {
        this.CalculatingAmount();
    },
    OnchangeBaseprice: function (input)
    {
        var Id = input[0].id.replace('baseprice_', '');
        this.CalculatingAmount(Id);
    },
    OnchangeQuantity: function (input) {
        var Id = input[0].id.replace('quantity_', '');
        this.CalculatingAmount(Id);
    },
    GetListObjSumbit: function ()
    {
        var table = $(".service-other-packages-tbody tr:not(:last)");
        if (table.length > 0) {
            table.each(function () {
                var row_item = $(this)
                var obj =
                {
                    Id: row_item[0].id.replace('OtherBooingPackageID_', ''),
                    BookingId: BookingId,
                    Name: row_item.find('.service-other-packages-packagename').val(),
                    BasePrice: row_item.find('.service-other-packages-baseprice').val(),
                    Quantity: row_item.find('.service-other-packages-quantity').val(),
                    Profit: row_item.find('.service-other-packages-profit').val().replaceAll(',', ''),
                    Amount: row_item.find('.service-other-packages-amount').val().replaceAll(',', ''),
                    BasePrice: row_item.find('.service-other-packages-baseprice').val().replaceAll(',', ''),
                }
                lstUpdate.push(obj);
            })

            OtherBookingUpdate =
            {
                Id: BookingId,
                OrderId: $("#OrderId").val(),
                StartDate: _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate($('#toDate').val())),
                EndDate: _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate($('#fromDate').val())),
                Quantity: parseInt($(".service-other-packages-total-amount").text()),
                OperatorID: $("#add-service-other-main-staff").val(),
                Amount: parseFloat($(".service-other-packages-total-profit").text().replaceAll(',', '')),
                OthersAmount: $("#servicemanual-other-other-amount").val().replaceAll(',', ''),
                Note: $(".service-other-note").val()
            };
        }
        else
        {
            OtherBookingUpdate =
            {
                Id: BookingId,
                OrderId: $("#OrderId").val(),
                StartDate: _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate($('#toDate').val())),
                EndDate: _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate($('#fromDate').val())),
                Quantity: 0,
                OperatorID: $("#add-service-other-main-staff").val(),
                Amount: $("#servicemanual-other-other-amount").val().replaceAll(',', ''),
                OthersAmount: $("#servicemanual-other-other-amount").val().replaceAll(',', ''),
                Note: $(".service-other-note").val()
            };
        }
    },
    CreateOtherBookingPackage: function () {
        var lastRow = $(".service-other-packages-summary-row");
        $(
            '<tr class="service-other-packages-row" data-extra-package-id="0">' +
                '<td class="service-other-packages-order">0</td>' +
                '<td><input type="text" class="form-control service-other-packages-packagename" style="width:100% !important" value=""></td>' +
            '<td> <input class="form-control text-right currency service-other-packages-baseprice"' + 'id = "baseprice_' + countNew + '"' + 'name="service-other-packages-baseprice" oninput="_order_detail_create_service.OnchangeBaseprice($(this))" value=""></td>' +
            '<td> <input class="form-control text-right currency service-other-packages-quantity"' + 'id = "quantity_' + countNew + '"' + 'name="service-other-packages-quantity" oninput="_order_detail_create_service.OnchangeQuantity($(this))" value=""></td>' +
            '<td class="text-right service-other-packages-profit-row"><input class="form-control text-right currency service-other-packages-profit"' + 'id="profit_' + countNew + '"' + 'style="background-color: lightgray;" oninput="_order_detail_create_service.OnchangeVAT($(this))" value=""></td>'+
            '<td class="text-right"> <input class="form-control text-right currency service-other-packages-amount" ' + 'id = "amount_' + countNew + '"' + ' style="background-color: lightgray;" disabled="" value=""></td>'+
                '<td class="text-right">'+
            '<a class="fa fa-trash-o" href="javascript:;" onclick="_order_detail_create_service.DeleteOtherBookingPackage($(this).parent().parent());"></a>'+
            '</td>' +
            '</tr>').insertBefore(lastRow);
        countNew - 1;
        var rows = $(".service-other-packages-tbody tr");
        var index = 1;
        rows.each(function ()
        {
            $(this).find(".service-other-packages-order").text(index);
            index++;
        })
    },

    Submit: function ()
    {
        var OrderId = $("#OrderId").val();
        this.GetListObjSumbit();
        if (OtherBookingUpdate.StartDate < OtherBookingUpdate.EndDate) {
            $('.img_loading_summit').show();
            _global_function.ConfirmFileUpload($("#attachment-file-block"), BookingId)//
            this.Close();
            $.ajax({
                url: "/Order/SubmitChange",
                type: "post",
                data: {
                    lstUpdate: lstUpdate,
                    lstDelete: lstDelete,
                    OtherBooking: OtherBookingUpdate
                },
                success: function (result) {
                    $('#img_loading_summit').hide();
                    if (result.status != 0) {
                        _msgalert.error(result.msg);
                        $('#btn_summit_order').show();
                        return;
                    }
                    _msgalert.success(result.msg);
                    $('.toast-success').text('Cập nhật thành công')
                    $('.img_loading_summit').hide();

                    setTimeout(function () {
                        window.location.href = '/OrderDetail/' + OrderId;
                    }, 2000);
                    return;
                }
            });
        }
        else
        {
            
            lstUpdate = [];
            lstDelete = [];
            _msgalert.error("Ngày bắt đầu và ngày kết thúc không hợp lệ");
        }
    },
    
    OtherService: function (order_id, booking_id) {
        _global_function.AddLoading()

        if ($('#add-service-other').length) {
            $('#add-service-other').removeClass('show')
            setTimeout(function () {
                $('#add-service-other').remove();
            }, 300);

        }
        $.ajax({
            url: "AddOtherService",
            type: "post",
            data: {
                order_id: order_id,
                other_booking_id: booking_id
            },
            success: function (result) {
                $('body').append(result);
                setTimeout(function () {
                    _order_detail_create_service.StopScrollingBody();
                    _order_detail_other.Initialization(order_id, booking_id);
                    _global_function.RemoveLoading()

                }, 300);

            }
        });
    },
}



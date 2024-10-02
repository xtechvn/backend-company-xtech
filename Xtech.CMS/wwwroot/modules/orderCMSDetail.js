let input = 0;
let type = 7;
let SalerGroupId = "";
let SalerId = 0;
$(document).ready(function () {
    input=$('#order_Id').val();
    SalerGroupId = $('#Saler_GroupId').val();
    SalerId = $('#Saler_Id').val();
    _orderDetail.LoadPackages(input);
    _orderDetail.LoadPersonInCharge(SalerId, SalerGroupId);
    _orderDetail.LoadFile(input, type);
    _orderDetail.LoadContractPay(input);
    _orderDetail.LoadBillVAT(input);
});
var _orderDetail = {
    UpdateOrder: function ()
    {
        var lstSalerGroup = $("#SalerGroup").val();
        var SalerGroup =''; 
        if (lstSalerGroup)
        {
            lstSalerGroup.forEach(item =>
            {
                SalerGroup = SalerGroup + item + ","
            })
        }
        SalerGroup = SalerGroup.substring(0, SalerGroup.length - 1)
        var Order =
        {
            OrderId : input,
            SalerId : $("#SalerId").val(),
            SalerGroupId: SalerGroup,
            Label: $("#Order_Label").val(),
            Note: $("#Order_Note").val()
        }
        $('.img_loading_summit').show();
        $.ajax({
            url: "/Order/UpdateOrder",
            type: "Post",
            data: {model : Order },
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
                    window.location.href = '/OrderDetail/' + input;
                }, 2000);
                return;
            }
        });
    },
    LoadOeederDetail: function () {
        _orderDetail.LoadPackages(input);
        _orderDetail.LoadContractPay(input);
        _orderDetail.LoadBillVAT(input);
        _orderDetail.LoadFile(input, type);
        _orderDetail.LoadPersonInCharge(input);
    },

    LoadPackages: function (input) {
        $.ajax({
            url: "/Order/Packages",
            type: "Post",
            data: { orderId: input },
            success: function (result) {
                $('#imgLoading_Packages').hide();
                $('#grid_data_Packages').html(result);
            }
        });
    },
    LoadContractPay: function (input) {
        $.ajax({
            url: "/Order/ContractPay",
            type: "Post",
            data: { orderId: input },
            success: function (result) {
                $('#imgLoading_ContractPay').hide();
                $('#grid_data_ContractPay').html(result);
            }
        });
    },
    LoadBillVAT: function (input) {
        $.ajax({
            url: "/Order/BillVAT",
            type: "Post",
            data: { orderId: input },
            success: function (result) {
                $('#imgLoading_BillVAT').hide();
                $('#grid_data_BillVAT').html(result);
            }
        });
    },
    LoadListPassenger: function (input) {
        $.ajax({
            url: "/Order/ListPassenger",
            type: "Post",
            data: { orderId: input },
            success: function (result) {
                $('#imgLoading_ListPassenger').hide();
                $('#grid_data_ListPassenger').html(result);
            }
        });
    },
    LoadFile: function (input, type) {
        _global_function.RenderFileAttachment($('#grid_data_File'), input, type)
        $('#imgLoading_File').hide();
        /*
        $.ajax({
            url: "/Order/File",
            type: "Post",
            data: { orderId: input, type: type },
            success: function (result) {
                $('#imgLoading_File').hide();
                $('#grid_data_File').html(result);
            }
        });
        */
    },
    LoadPersonInCharge: function (input) {
        $.ajax({
            url: "/Order/PersonInCharge",
            type: "Post",
            data: { SalerId: SalerId, SalerGroupId: SalerGroupId },
            success: function (result) {
                $('#imgLoading_PersonInCharge').hide();
                $('#grid_data_PersonInCharge').html(result);
            }
        });
    },
    ChangeOrderSaler: function (order_id, order_no) {

        var title = 'Nhận xử lý đơn hàng';
        var description = 'Bạn có chắc chắn muốn nhận xử lý đơn hàng này không?';
        _msgconfirm.openDialog(title, description, function () {
            $.ajax({
                url: "/Order/ChangeOrderSaler",
                type: "Post",
                data: { order_id: order_id, saleid: 0, OrderNo: order_no },
                success: function (result) {
                    if (result.status === 0) {
                        _msgalert.success(result.msg);
                        setTimeout(function () {
                            window.location.reload();
                        }, 1000);
                    }
                    else {
                        _msgalert.error(result.msg);

                    }
                }
            });
        });
    },

}
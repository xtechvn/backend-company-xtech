let searchModel =
{
    Status: null,
    CreateDateFrom: null,
    CreateDateTo: null,
    StartDate: null,
    EndDate: null,
    StatusTab: null,
    ClientId: null,
    ServiceType:null,
    SalerPermission: null,
    SalerId : null,
    PaymentStatus: null,
    PaymentMethod:null,
    OrderId: null,
    OrderNo: null,
    PageIndex: 1,
    PageSize: 10
}
let lstStatus = [];
let lstPaymentMethod = [];
let lstServiceType = [];
let lstPaymentStatus = [];
let isPicker = false;
$(document).ready(function ()
{
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

    $("#OrderNo").select2({
/*        theme: 'bootstrap4',*/
        placeholder: "Mã đơn hàng",
        /* tags: true,*/
        ajax: {
            url: "/OrderManual/OrderNoSuggestion",
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
                            id: item.orderno.toUpperCase(),
                        }
                    })
                };
            },
            cache: true
        }
    }).on('select2:opening', function (e) {
        $('#OrderNo').val([]).trigger('change');
    });

    $(document).click(function (event) {
        var $target = $(event.target);
        if (!$target.closest('#Status').length) {
            if ($('#list-item-status').is(":visible") && !$target[0].id.includes('status_data') && !$target[0].id.includes('checkbox_status')
                && !$target[0].id.includes('list-item-status') && !$target[0].id.includes('status_text')) {
                selectBtnStatus.classList.toggle("open");
            }
        }
        if (!$target.closest('#HINHTHUCTT').length) {
            if ($('#list-item-HINHTHUCTT').is(":visible") && !$target[0].id.includes('HINHTHUCTT_data') && !$target[0].id.includes('checkbox_HINHTHUCTT')
                && !$target[0].id.includes('list-item-HINHTHUCTT') && !$target[0].id.includes('HINHTHUCTT_text')) {
                selectBtnHINHTHUCTT.classList.toggle("open");
            }
        } 

        if (!$target.closest('#PaymentSTT').length) {
            if ($('#list-item-PaymentSTT').is(":visible") && !$target[0].id.includes('PaymentSTT_data') && !$target[0].id.includes('checkbox_PaymentSTT')
                && !$target[0].id.includes('list-item-PaymentSTT') && !$target[0].id.includes('PaymentSTT_text')) {
                selectBtnPaymentSTT.classList.toggle("open");
            }
        } 

        if (!$target.closest('#serviceType').length) {
            if ($('#list-item-serviceType').is(":visible") && !$target[0].id.includes('serviceType_data') && !$target[0].id.includes('checkbox_serviceType')
                && !$target[0].id.includes('list-item-serviceType') && !$target[0].id.includes('serviceType_text')) {
                selectBtnserviceType.classList.toggle("open");
            }
        } 

/*        if (!$target.closest('#filter-body').length)
        {
            _ordersCMS.closeForm();
        }*/

    });
    


    $("#ClientId").select2({
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
        $('#ClientId').val([]).trigger('change');
    });

    $("#txtNguoiPhuTrach").select2({
        theme: 'bootstrap4',
        placeholder: "Người phụ trách",
        maximumSelectionLength: 1,
        ajax: {
            url: "/CustomerManager/UserSuggestion",
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
                            text: item.fullname + ' - ' + item.email + ' - ' + item.phone,
                            id: item.id,
                        }
                    })
                };
            },
            cache: true
        }
    }).on('select2:opening', function (e) {
        $('#txtNguoiPhuTrach').val([]).trigger('change');
    });


    _ordersCMS.Init();

    const selectBtnStatus = document.querySelector(".select-btn-status");
    const itemsStatus = document.querySelectorAll(".item-status");

    const selectBtnHINHTHUCTT = document.querySelector(".select-btn-HINHTHUCTT");
    const itemsHINHTHUCTT = document.querySelectorAll(".item-HINHTHUCTT"); 

    const selectBtnPaymentSTT = document.querySelector(".select-btn-PaymentSTT");
    const itemsPaymentSTT = document.querySelectorAll(".item-PaymentSTT");

    const selectBtnserviceType = document.querySelector(".select-btn-serviceType");
    const itemsserviceType = document.querySelectorAll(".item-serviceType");


    selectBtnserviceType.addEventListener("click", (e) => {
        e.preventDefault();
        selectBtnserviceType.classList.toggle("open");
    });
    selectBtnStatus.addEventListener("click", (e) => {
        e.preventDefault();
        selectBtnStatus.classList.toggle("open");
    });
    selectBtnHINHTHUCTT.addEventListener("click", (e) => {
        e.preventDefault();
        selectBtnHINHTHUCTT.classList.toggle("open");
    }); 
    selectBtnPaymentSTT.addEventListener("click", (e) => {
        e.preventDefault();
        selectBtnPaymentSTT.classList.toggle("open");
    });


    itemsStatus.forEach(item => {
        item.addEventListener("click", () => {
            item.classList.toggle("checked");
            var id_chosen = null;
            if (item.classList.contains('checked')) {
                id_chosen =  item.getAttribute('id');
                lstStatus.push(parseInt(id_chosen.replace('status_data_', '')));
            }
            else
            {
                id_chosen = item.getAttribute('id');
                var DetetedMethod = parseInt(id_chosen.replace('status_data_', ''))
                lstStatus = lstStatus.filter(x => x !== DetetedMethod);
            }
            

            let checked = document.querySelectorAll("#list-item-status .checked"),
                btnText = document.querySelector(".btn-text-status");
            let checked_list = []
            listStatus = []
            for (var i = 0; i < checked.length; i++) {
                id = checked[i].getAttribute('id')
                if (id.includes('status_data_')) {
                    checked_list.push(checked[i]);
                }
                listStatus.push(parseInt(id.replace('status_data_', '')));
            }
            if (listStatus && listStatus.length > 0) {
                btnText.innerText = `${listStatus.length} Selected`;
            } else {
                btnText.innerText = "Tất cả trạng thái";
            }
        })
    })
    itemsHINHTHUCTT.forEach(item => {
        item.addEventListener("click", () => {
            item.classList.toggle("checked");
            var id_chosen = null;
            if (item.classList.contains('checked')) {
                id_chosen = item.getAttribute('id');
                lstPaymentMethod.push((id_chosen.replace('HINHTHUCTT_data_', '')));
            } else {
                // Tìm vị trí của phần tử trong mảng lstPaymentMethod
                id_chosen = item.getAttribute('id').replace('HINHTHUCTT_data_', '');
                lstPaymentMethod = lstPaymentMethod.filter(x => x !== id_chosen);
            }

            let checked = document.querySelectorAll("#list-item-HINHTHUCTT .checked"),
                btnTextHINHTHUCTT = document.querySelector(".btn-text-HINHTHUCTT");
            let checked_list = []
            listHINHTHUCTT = []
            for (var i = 0; i < checked.length; i++) {
                id = checked[i].getAttribute('id')
                if (id.includes('HINHTHUCTT_data_')) {
                    checked_list.push(checked[i]);
                }

                listHINHTHUCTT.push((id.replace('HINHTHUCTT_data_', '')))
            }
            if (listHINHTHUCTT && listHINHTHUCTT.length > 0) {
                btnTextHINHTHUCTT.innerText = `${listHINHTHUCTT.length} Selected`;
            } else {
                btnTextHINHTHUCTT.innerText = "Tất cả trạng thái thanh toán";
            }
        })
    }) 

    itemsserviceType.forEach(item => {
        item.addEventListener("click", () => {
            item.classList.toggle("checked");
            var id_chosen = null;
            if (item.classList.contains('checked')) {
                id_chosen = item.getAttribute('id');
                lstServiceType.push((id_chosen.replace('serviceType_data_', '')));
            } else {
                // Tìm vị trí của phần tử trong mảng lstPaymentMethod
                id_chosen = item.getAttribute('id').replace('serviceType_data_', '');
                lstServiceType = lstServiceType.filter(x => x !== id_chosen);
            }

            let checked = document.querySelectorAll("#list-item-serviceType .checked"),
                btnTextserviceType = document.querySelector(".btn-text-serviceType");
            let checked_list = []
            listserviceType = []
            for (var i = 0; i < checked.length; i++) {
                id = checked[i].getAttribute('id')
                if (id.includes('serviceType_data_')) {
                    checked_list.push(checked[i]);
                }

                listserviceType.push((id.replace('serviceType_data_', '')))
            }
            if (listserviceType && listserviceType.length > 0) {
                btnTextserviceType.innerText = `${listserviceType.length} Selected`;
            } else {
                btnTextserviceType.innerText = "Tất cả dịch vụ";
            }
        })
    }) 

    itemsPaymentSTT.forEach(item => {
        item.addEventListener("click", () => {
            item.classList.toggle("checked");
            var id_chosen = null;
            if (item.classList.contains('checked')) {
                id_chosen = item.getAttribute('id');
                lstPaymentStatus.push((id_chosen.replace('PaymentSTT_data_', '')));
            } else {
                // Tìm vị trí của phần tử trong mảng lstPaymentMethod
                id_chosen = item.getAttribute('id').replace('PaymentSTT_data_', '');
                lstPaymentStatus = lstPaymentStatus.filter(x => x !== id_chosen);
            }

            let checked = document.querySelectorAll("#list-item-PaymentSTT .checked"),
                btnTextPaymentSTT = document.querySelector(".btn-text-PaymentSTT");
            let checked_list = []
            listPaymentSTT = []
            for (var i = 0; i < checked.length; i++) {
                id = checked[i].getAttribute('id')
                if (id.includes('PaymentSTT_data_')) {
                    checked_list.push(checked[i]);
                }

                listPaymentSTT.push((id.replace('PaymentSTT_data_', '')))
            }
            if (listPaymentSTT && listPaymentSTT.length > 0) {
                btnTextPaymentSTT.innerText = `${listPaymentSTT.length} Selected`;
            } else {
                btnTextPaymentSTT.innerText = "Tất cả trạng thái thanh toán";
            }
        })
    }) 

    
})

var _ordersCMS = {
    FirstLoading: false,
    Init: function () {
        var today = new Date();
        var yyyy = today.getFullYear();
        var mm = today.getMonth() + 1; // Months start at 0!
        var dd = today.getDate();
        if (dd < 10) dd = '0' + dd;
        if (mm < 10) mm = '0' + mm;
        var min_range = '01/01/2020';
        var max_range = dd + '/' + mm + '/' + (yyyy + 5);

        $('.date-range-filter').each(function (index, item) {
            var element = $(item)
            element.daterangepicker({
                autoApply: true,
                autoUpdateInput: false,
                showDropdowns: true,
                drops: 'down',
                minDate: min_range,
                maxDate: max_range,
                locale: {
                    format: 'DD/MM/YYYY'
                }
            });
            element.data('daterangepicker').setStartDate(min_range);
            element.data('daterangepicker').setEndDate(max_range);
        })
        $('#fromDate').daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            showDropdowns: true,
            autoUpdateInput: false,
            drops: 'down',
            minDate: '01/01/2020',
            maxDate: max_range,
            locale: {
                format: 'DD/MM/YYYY',
                cancelLabel: 'Clear'
            }
        });
        $('#toDate').daterangepicker({
            singleDatePicker: true,
            autoApply: true,
            autoUpdateInput: false,
            showDropdowns: true,
            drops: 'down',
            minDate: '01/01/2020',
            maxDate: max_range,
            locale: {
                format: 'DD/MM/YYYY',
                cancelLabel: 'Clear'
            }
        });

        $('input[name="toDate"]').on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            isPicker = false;
        });

        $('input[name="fromDate"]').on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            isPicker = false;
        });
        $("body").on('apply.daterangepicker', ".date-range-filter", function (ev, picker) {
            var element = $(this)
            element.val(_global_function.GetDayText(element.data('daterangepicker').startDate._d).split(' ')[0])
        });
        this.Search(searchModel);
    },
    Search: function (input) {
        $('#imgLoading').show();
        $.ajax({
            url: "/Order/Search",
            type: "post",
            data: { searchModel: input },
            success: function (result) {
                $('#imgLoading').hide();
                $('#grid-data').html(``);
                $('#grid-data').append(result);
                var total = $('#data-total-record').val();
                $('#total-article-filter').text(total);
                $('#selectPaggingOptions').val(input.PageSize).attr("selected", "selected");
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },

    GetParams: function () {
        var rangeTime = $("#filter_date_daterangepicker").val().split("- ");
        searchModel.OrderNo = $("#OrderNo").val();
        searchModel.SalerId = $("#txtNguoiPhuTrach").val();
        if (rangeTime.length > 1) {
            searchModel.CreateDateFrom = _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate(rangeTime[0]));
            searchModel.CreateDateTo = _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate(rangeTime[1]));
        }
        searchModel.ClientId = $("#ClientId").val();
        if ($('#fromDate').val() && $('#toDate').val()) {
            searchModel.StartDate = _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate($('#fromDate').val()));
            searchModel.EndDate = _global_function.ParseJSDateToCSharpDateTime(_global_function.ParseJSDate($('#toDate').val()));
        }
        searchModel.Status = '';
        lstStatus.forEach(item => {
            searchModel.Status = searchModel.Status + item + ',';
        })
        searchModel.Status = searchModel.Status.substring(0, searchModel.Status.length - 1)

        searchModel.PaymentMethod = '';
        lstPaymentMethod.forEach(item => {
            searchModel.PaymentMethod = searchModel.PaymentMethod + item + ',';
        })
        searchModel.PaymentMethod = searchModel.PaymentMethod.substring(0, searchModel.PaymentMethod.length - 1)

        searchModel.PaymentStatus = '';
        lstPaymentStatus.forEach(item => {
            searchModel.PaymentStatus = searchModel.PaymentStatus + item + ',';
        })
        searchModel.PaymentStatus = searchModel.PaymentStatus.substring(0, searchModel.PaymentStatus.length - 1) 

        searchModel.ServiceType = '';
        lstServiceType.forEach(item => {
            searchModel.ServiceType = searchModel.ServiceType + item + ',';
        })
        searchModel.ServiceType = searchModel.ServiceType.substring(0, searchModel.ServiceType.length - 1) 

        searchModel.PageIndex = 1;
    },

    ResetParams: function () {
        $("#fromDate").val('');
        $("#toDate").val('');
        $("#filter_date_daterangepicker").val('');
        $("#ClientId").val([]).trigger('change');

        var StatusItems = document.querySelectorAll("#list-item-status .checked");
        StatusItems.forEach(item => {
            item.classList.remove('checked');
            $(".btn-text-status").html('Tất cả trạng thái đơn');
        })
        lstStatus = [];

        var PaymentStatusItems = document.querySelectorAll("#list-item-PaymentSTT .checked");
        PaymentStatusItems.forEach(item => {
            item.classList.remove('checked');
            $(".btn-text-PaymentSTT").html('Tất cả trạng thanh toán');
        })
        lstPaymentStatus = [];

        var PaymentTypeItems = document.querySelectorAll("#list-item-HINHTHUCTT .checked");
        PaymentTypeItems.forEach(item => {
            item.classList.remove('checked');
            $(".btn-text-HINHTHUCTT").html('Tất cả phương thức thanh toán');
        })
        lstPaymentMethod = [];

        var serviceTypeItems = document.querySelectorAll("#list-item-serviceType .checked");
        serviceTypeItems.forEach(item => {
            item.classList.remove('checked');
            $(".btn-text-serviceType").html('Tất cả dịch vụ');
        })
        lstServiceType = [];

        searchModel.StartDate = null;
        searchModel.EndDate = null;
        searchModel.CreateDateTo = null;
        searchModel.CreateDateFrom = null;
    },
    OnPaging: function (pageIndex)
    {
        searchModel.PageIndex = pageIndex;
        this.Search(searchModel);
    },
    onSelectPageSize: function ()
    {
        searchModel.PageSize = parseInt($("#selectPaggingOptions").find(':selected').val());
        searchModel.PageIndex = 1;
        this.Search(searchModel);
    },

    displayForm: function ()
    {
        $("#filter-body").css("display", "none");

    },
    Export: function ()
    {

    },
    SearchData: function ()
    {
        this.GetParams();
        this.displayForm();
        $('#imgLoading').show();
        this.Search(searchModel);
    }
} 

var _order_manual = {
    CreateOrderManual: function () {
        if ($('#create_order_manual').length) {
            $('#create_order_manual').removeClass('show')
            setTimeout(function () {
                $('#create_order_manual').remove();
            }, 300);

        }
        $.ajax({
            url: "/OrderManual/CreateOrderManual",
            type: "post",
            data: {},
            success: function (result) {
                $('body').append(result);
                setTimeout(function () {
                    $('#create_order_manual').addClass('show')
                }, 300);

            }
        });
    },
    Close: function () {
        $('#create_order_manual').removeClass('show')
        setTimeout(function () {
            $('#create_order_manual').remove();
        }, 300);
    },

}
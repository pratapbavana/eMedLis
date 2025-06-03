
$(document).ready(function () {
   
    var i = $("#invtable").DataTable(
        {  
            "paging": false,
            "searching": false,
            "sorting": false,
            "bInfo": false,
            "scrollY": '30vh',
        }
    )
  

    var j = $("#paymentGrid").DataTable(
        {
            "paging": false,
            "searching": false,
            "sorting": false,
            "bInfo": false
        }
    )
    $('#invtable tbody').on('click', '.delete', function () {
        i
            .row($(this).parents('tr'))
            .remove()
            .draw();
        var ratearray = i.columns(4).data()[0]
        var subtotal = ratearray.reduce(function (pv, cv) { return pv + cv; }, 0);
        $('#subtotal').text(subtotal);
    });
    $('#paymentGrid tbody').on('click', '.btnDelete', function () {
        i
            .row($(this).parents('tr'))
            .remove()
            .draw();
        var ratearray = i.columns(4).data()[0]
        var subtotal = ratearray.reduce(function (pv, cv) { return pv + cv; }, 0);
        $('#subtotal').text(subtotal);
    });
})

function ToggleForm() {
    $("#PatList").show();
    $("#Billing").hide();
}

// #region Save Bill
function SaveBill() {
    var res = validate();
    if (res == false) {
        return false;
    }
    var empObj = {
        InvName: $('#InvName').val(),
        Rate: $('#Rate').val(),
        InvCode: $('#Code').val(),
        ReportHdr: $('#RepHdr').val(),
        SubDeptId: $('#SubDeptId').text(),
        SpecimenId: $('#SpecId').text(),
        VacutainerId: $('#VacId').text(),
        ReportTime: $('#RptTime').val(),
        GuideLines: $('#GLines').val(),
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/Investigation/Add",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                M.toast({ html: result.Item2, classes: 'green rounded' });
                $('#modal_aside_left').modal('hide');
                //$("#alertbox").fadeIn();
                //closeSnoAlertBox();
                $('#depttable').DataTable().ajax.reload();
            }
            else
                M.toast({ html: result.Item2, classes: 'red rounded' });

        },
        error: function (errormessage) {
            M.toast({ html: 'Something Wrong!', classes: 'red rounded' });
        }
    });
}
// #endregion


// #region Clear Form Fields
function clearfields() {
    $("#PatList").hide();
    $("#Billing").show();
    $('#header').html("Patient Billing");
    $('#MobileNo').val("");
    $('#PatName').val("");
    $('#Age').val("");
    $('#AgeType').val("");
    $('#Gender').val("");
    $('#Ref').val("");
    $('#Area').val("");
    $('#City').val("");
    $('#Email').val("");
    $('#Inv').val("");
    $('#MobileNo').removeClass("valid is-invalid");
    $('#PatName').removeClass("valid is-invalid");
    $('#Age').removeClass("valid is-invalid");
    $('#AgeType').removeClass("valid is-invalid");
    $('#Gender').removeClass("valid is-invalid");
    $('#Ref').removeClass("valid is-invalid");
    $('#Area').removeClass("valid is-invalid");
    $('#City').removeClass("valid is-invalid");
    $('#Email').removeClass("valid is-invalid");
    $('#Inv').removeClass("valid is-invalid");
    $('#Inv').val(null).trigger("change");
    $("#invtable").DataTable().columns.adjust().draw();
}
// #endregion

// #region Load Bills

function loaddatatable() {

    var a = $("#depttable").DataTable({
        "order": [[1, 'asc']],
        ajax: {
            url: '/Investigation/List',
            method: "GET",
            dataSrc: function (json) {
                return json;
            }
        },

        scrollX: true,
        columns: [
            {
                "title": "Serial",
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { data: 'InvName' },
            { data: 'SubDeptName' },
            { data: 'Rate' },

            {
                data: 'Active',
                render: function (data, type, row) {
                    if (data == true) {
                        return '<span class="cbadge cbadge-pill cbadge-outline-success">ACTIVE</span>'
                    }
                    else {
                        return '<span class="cbadge cbadge-pill cbadge-outline-danger">INACTIVE</span>'
                    }

                }
            },
            {
                data: 'Id',
                render: function (data, type, row) {

                    return '<a href="#" data-toggle="modal" data-target="#modal_aside_left" onclick="return GetInvbyId(' + data + ');">Edit</a>'
                }
            }
        ],
        "columnDefs": [{
            "searchable": false,
            "orderable": false,
            "targets": 0
        }],

    });

    $('#global_filter').keyup(function () {
        a.search($(this).val()).draw();
    })
    a.on('order.dt search.dt', function () {
        a.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();
}
//#endregion

// #region Add inv to List

function addinvtolist(Id) {
    if (!Id) {
        return;
    }
        $.ajax({
            url: "/Investigation/getbyID/" + Id,
            type: "GET",
            contentType: "application/json;charset=UTF-8",
            dataType: "json",
            success: function (result) {
                var i = $("#invtable").DataTable()
                var idx = i
                    .columns(0)
                    .data()
                    .eq(0) // Reduce the 2D array into a 1D array of data
                    .indexOf(result[0].InvCode);

                if (idx === -1) {
                    var i = $("#invtable").DataTable()
                    var rowNode = i.row.add([
                        result[0].InvCode,
                        '<div class="text-wrap">' + result[0].InvName + '</div>',
                        result[0].Rate,
                        '<div style="display: flex;"> <input class="form-control form-control-sm" type="number" id="DiscountAmount" style="flex: 1;" name="DiscountAmount" placeholder="Rs 0.00" style="width:50%;" > <input class="form-control form-control-sm" type="number" id="DiscountPercent" style="flex: 1;" name="DiscountPercent" placeholder="0%" style="width:50%;"> </div>',
                        result[0].Rate,
                        '<a href="#"><i class="fa fa-trash fa-sm delete" style="color: #ad0000;"></i></a>'
                    ]).draw()
                        .node();
                    $(rowNode).find('td').eq(4).addClass('indigo-text right-align');
                    var ratearray = i.columns(4).data()[0]
                    var subtotal = ratearray.reduce(function (pv, cv) { return pv + cv; }, 0);
                    //$('#subtotal').text(subtotal);
                    //console.log(sum);
                    //$('#invtable').DataTable().columns.adjust();
                    $('#Inv').val(null).trigger("change");
                }
                else {
                    toastr.error('Test already added!');
                    $('#Inv').val(null).trigger("change");
                }
            },
            error: function (errormessage) {
                alert(errormessage.responseText);
            }
    });
}
//#endregion

// #region Form Valdidation
function validate() {
    var isValid = true;
    if ($('#InvName').val().trim() == "") {
        $('#InvName').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#InvName').css('border-color', 'lightgrey');
    }
    if ($('#Rate').val().trim() == "") {
        $('#Rate').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Rate').css('border-color', 'lightgrey');
    }

    if ($('#Code').val().trim() == "") {
        $('#Code').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#Code').css('border-color', 'lightgrey');
    }

    if ($('#RepHdr').val().trim() == "") {
        $('#RepHdr').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#RepHdr').css('border-color', 'lightgrey');
    }

    if ($('#deptname').val().trim() == "") {
        $('#deptname').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#deptname').css('border-color', 'lightgrey');
    }

    if ($('#spec').val().trim() == "") {
        $('#spec').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#spec').css('border-color', 'lightgrey');
    }

    if ($('#vac').val().trim() == "") {
        $('#vac').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#vac').css('border-color', 'lightgrey');
    }

    if ($('#subdeptname').val().trim() == "") {
        $('#subdeptname').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#RptTime').css('border-color', 'lightgrey');
    }
    if ($('#RptTime').val().trim() == "") {
        $('#RptTime').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#RptTime').css('border-color', 'lightgrey');
    }
    if ($('#GLines').val().trim() == "") {
        $('#GLines').css('border-color', 'Red');
        isValid = false;
    }
    else {
        $('#GLines').css('border-color', 'lightgrey');
    }
    return isValid;
}
// #endregion

// #region Investigation Dropdown
$(document).ready(function () {
        $.ajax({
            url: '/Investigation/List',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                const formattedData = data.map(item => ({
                    id: item.Id,
                    text: item.InvName,
                }));
                $(".invsearch").select2({
                    data: formattedData,
                    width: "100%",
                    placeholder: 'Select Investigation'
                });
                $('.invsearch').on('change', function () {
                    const SelectedId = $(this).val();
                    addinvtolist(SelectedId);
                });
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.error('Error fetching data:', textStatus, errorThrown);
            },
        });
});
// #endregion
//$(document).ready(function () {
//    $("#invtable").on("keyup", "input[name='DiscountAmount']", function () {
//        var row = $(this).closest("tr");
//        var rate = parseFloat(row.find("td:nth-child(3)").text()); // Get Rate from 3rd column
//        var discount = parseFloat($(this).val()) || 0;

//        // Validate Discount (should not be more than Rate)
//        if (discount > rate) {
//            $(this).val(rate); // Set Discount to Rate if exceeds
//            discount = rate;
//        }
//        else if (discount < 0) {
//            $(this).val('');
//            discount = 0;
//        }
//        var netValue = rate - discount;
//        row.find("td:nth-child(5)").text(netValue.toFixed(2)); // Update Net Value text
//    });
//});

$(document).ready(function () {
    var isUpdatingPercent = false;
    var isUpdatingAmount = false;

    $("#invtable").on("keyup", "input[name='DiscountPercent'], input[name='DiscountAmount']", function () {
        const input2 = $(this).is("input[name = 'DiscountPercent']");
        var row = $(this).closest("tr");
        var discountAmountInput = row.find("input[name='DiscountAmount']");
        var discountPercentInput = row.find("input[name='DiscountPercent']");
        var rate = parseFloat(row.find("td:nth-child(3)").text()); // Get Rate from 3rd column
        var discountPercent = parseFloat(row.find("input[name='DiscountPercent']").val()) || 0;
        var discountAmount = parseFloat(row.find("input[name='DiscountAmount']").val()) || 0;

        // Ensure minimum 0 for both Discount (%) and Discount ($)
        discountPercent = Math.max(0, discountPercent);
        discountAmount = Math.max(0, discountAmount);

        if ($(this).is("input[name='DiscountPercent']")) {
            isUpdatingPercent = true;
            if (discountPercent > 0) {
                discountAmountInput.prop("disabled", true);
                discountAmount = rate * (discountPercent / 100);
            } else {
                discountAmount = 0;
                discountAmountInput.prop("disabled", false);
            }
            isUpdatingAmount = false;
        } else {
            isUpdatingAmount = true;
            if (discountAmount > 0) {
                discountPercentInput.prop("disabled", true);
                discountPercent = (discountAmount / rate) * 100;
            } else {
                discountPercent = 0;
                discountPercentInput.prop("disabled", false);
            }
            isUpdatingPercent = false;
        }

        // Update the other field only if the change wasn't triggered by itself
        if (!isUpdatingPercent) {
            row.find("input[name='DiscountPercent']").val(discountPercent.toFixed(2));
        }
        if (!isUpdatingAmount) {
            row.find("input[name='DiscountAmount']").val(discountAmount.toFixed(2));
        }

        // Validate Discount (should not be more than Rate) - Consider both percentage and amount
        var totalDiscount = Math.max(discountAmount, rate * (discountPercent / 100));
        if (totalDiscount > rate) {
            // Set both Discount (%) and Discount (Rs) to enforce maximum discount
            row.find("input[name='DiscountPercent']").val((100).toFixed(2));
            row.find("input[name='DiscountAmount']").val(rate.toFixed(2));
            discountAmount=rate
        }

        var netValue = rate - discountAmount;
        row.find("td:nth-child(5)").text(netValue.toFixed(2)); // Update Net Value text
    });
});

// #region Pay Mode to List
$(document).ready(function () {
    $("#btnAddPay").click(function (event) {
        event.preventDefault();
        var paymentMode = $("#paymentMode").val();
        var amount = $("#pamount").val();
        if (!isNaN(paymentMode) &&!isNaN(amount) && amount > 0) { // Basic validation for positive amount
            // Add a new row to the grid
            var j = $("#paymentGrid").DataTable()
            var rowNode = j.row.add([
                paymentMode,
                amount,
                '<a href="#"><i class="fa fa-trash fa-sm btnDelete" style="color: #ad0000;"></i></a>'
            ]).draw()
                .node();
            // Clear the input fields
            $("#pamount").val("");
            //$(".btnDelete").click(function () {
            //    $(this).parent().parent().parent().remove(); // Remove the entire row
            //});
        } else {
            alert("Please enter a valid amount.");
        }
    });
});
//#endregion
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
        // Recalculate and update labels after row deletion
        calculateAndSetSummaryAmounts();
    });
    $('#paymentGrid tbody').on('click', '.btnDelete', function () {
        j
            .row($(this).parents('tr'))
            .remove()
            .draw();
        // !!! IMPORTANT: Recalculate Paid and Due Amounts after deleting payment !!!
        updatePaymentSummary();
    });
    $('form').on('keydown', function (event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            return false;
        }
    });
    $('#pamount').on('keydown', function (event) {
        if (event.keyCode === 13) { // 13 is the keycode for the Enter key
            event.preventDefault(); // Prevent the default action (e.g., form submission)
            $('#btnAddPay').click(); // Trigger the click event on the Add Payment button
        }
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
    // Reset summary amounts on clear
    $('#lblTotalBill').text('0.00');
    $('#lblNetAmount').text('0.00');
    $('#lblDiscountAmount').text('0.00');
    $('#lblPaidAmount').text('0.00');
    $('#lblDueAmount').text('0.00');
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
                    result[0].Rate.toFixed(2), // Ensure rate is formatted as number
                    '<div style="display: flex;"> <input class="form-control form-control-sm" type="number" id="DiscountAmount" style="flex: 1;" name="DiscountAmount" placeholder="Rs 0.00"> <input class="form-control form-control-sm" type="number" id="DiscountPercent" style="flex: 1;" name="DiscountPercent" placeholder="0%"> </div>',
                    result[0].Rate.toFixed(2), // Net Amount initially same as Rate
                    '<a href="#"><i class="fa fa-trash fa-sm delete" style="color: #ad0000;"></i></a>'
                ]).draw()
                    .node();
                $(rowNode).find('td').eq(4).addClass('indigo-text right-align'); // No change here, this is for visual alignment

                // Recalculate and update labels after adding a new row
                calculateAndSetSummaryAmounts();

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

$(document).ready(function () {
    var isUpdatingPercent = false;
    var isUpdatingAmount = false;

    $("#invtable").on("keyup", "input[name='DiscountPercent'], input[name='DiscountAmount']", function () {
        const input2 = $(this).is("input[name = 'DiscountPercent']"); // This variable is not used
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
                // Ensure discount percent does not exceed 100
                if (discountPercent > 100) {
                    discountPercent = 100;
                    $(this).val(100);
                }
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
                // Ensure discount amount does not exceed rate
                if (discountAmount > rate) {
                    discountAmount = rate;
                    $(this).val(rate);
                }
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
            discountPercentInput.val(discountPercent.toFixed(2));
        }
        if (!isUpdatingAmount) {
            discountAmountInput.val(discountAmount.toFixed(2));
        }

        var netValue = rate - discountAmount;
        row.find("td:nth-child(5)").text(netValue.toFixed(2)); // Update Net Value text in table

        // !!! IMPORTANT: Call the function to recalculate and update summary amounts !!!
        calculateAndSetSummaryAmounts();
    });
});

// New function to encapsulate calculation of summary amounts
// New function to encapsulate calculation of summary amounts
function calculateAndSetSummaryAmounts() {
    var i = $("#invtable").DataTable();
    var totalBill = 0;
    var totalNetAmount = 0;
    var totalDiscountAmount = 0;

    // Iterate over each row in the DataTable
    i.rows().every(function () {
        var rowNode = this.node(); // Get the DOM node for the current row
        var rate = parseFloat($(rowNode).find("td:nth-child(3)").text()); // Rate from 3rd column
        // Get Net Amount directly from the visible table cell (5th column)
        var netAmount = parseFloat($(rowNode).find("td:nth-child(5)").text());

        if (isNaN(rate)) rate = 0; // Handle potential NaN
        if (isNaN(netAmount)) netAmount = 0; // Handle potential NaN

        totalBill += rate;
        totalNetAmount += netAmount;
        totalDiscountAmount += (rate - netAmount); // Discount is Rate - Net Amount
    });

    $('#lblTotalBill').text(totalBill.toFixed(2));
    $('#lblNetAmount').text(totalNetAmount.toFixed(2));
    $('#lblDiscountAmount').text(totalDiscountAmount.toFixed(2));

    // After updating NetAmount, recalculate Due Amount as well
    updatePaymentSummary(); // // Call this to update paid/due based on new net total
}

// #region Pay Mode to List
$(document).ready(function () {
    $("#btnAddPay").click(function (event) {
        event.preventDefault();
        var paymentModeValue = $("#paymentMode").val();
        var paymentModeText = $("#paymentMode option:selected").text();
        var amount = parseFloat($("#pamount").val()) || 0; // Parse as float
        var refno = $("#refno").val();

        var currentDueAmount = parseFloat($('#lblDueAmount').text()) || 0; // Get the current due amount

        if (paymentModeValue && !isNaN(amount) && amount > 0) { // Basic validation for positive amount
            // NEW VALIDATION: Prevent payment from exceeding due amount
            if (amount > currentDueAmount) {
                toastr.info('Payment amount cannot be more than the due amount!');
                return; // Stop the function if amount exceeds due
            }

            // Add a new row to the grid
            var j = $("#paymentGrid").DataTable()
            var rowNode = j.row.add([
                paymentModeText,
                amount.toFixed(2), // Format amount for display
                refno,
                '<a href="#"><i class="fa fa-trash fa-sm btnDelete" style="color: #ad0000;"></i></a>'
            ]).draw()
                .node();
            // Clear the input fields
            $("#pamount").val("");
            $("#paymentMode").val("");
            $("#refno").val("");

            // !!! IMPORTANT: Recalculate Paid and Due Amounts after adding payment !!!
            updatePaymentSummary();

        } else {
            toastr.info("Please enter a valid amount and select a payment mode.");
        }
    });
});
//#endregion

// New function to encapsulate calculation of payment summary amounts
function updatePaymentSummary() {
    var j = $("#paymentGrid").DataTable();
    var totalPaidAmount = 0;

    // Iterate over each row in the paymentGrid DataTable
    j.rows().every(function () {
        var data = this.data();
        totalPaidAmount += parseFloat(data[1]); // Amount is in the 2nd column (index 1)
    });

    var netAmount = parseFloat($('#lblNetAmount').text()) || 0;
    var dueAmount = netAmount - totalPaidAmount;

    // NEW LOGIC: If due amount becomes negative, clear the payment grid
    if (dueAmount < 0) {
        toastr.info("Due amount cannot be negative. Payment details have been reset.");
        j.clear().draw(); // Clear all rows from the payment grid
        totalPaidAmount = 0; // Reset total paid amount since grid is cleared
        dueAmount = netAmount; // Due amount becomes net amount again
    }

    $('#lblPaidAmount').text(totalPaidAmount.toFixed(2));
    $('#lblDueAmount').text(dueAmount.toFixed(2));

    // Also clear payment input fields if the grid was reset
    if (dueAmount < 0) { // This condition will be true if the grid was just cleared
        $("#pamount").val("");
        $("#paymentMode").val("");
        $("#refno").val("");
    }
}


$(document).ready(function () {
    $('#paymentMode').on('change', function () {
        var mode = $(this).val();
        if (mode === "1") { // Cash
            $('#refno').prop('disabled', true).val('');
        } else {
            $('#refno').prop('disabled', false);
        }
    });
});
$(document).ready(function () {
    var i = $("#invtable").DataTable(
        {
            "paging": false,
            "searching": false,
            "sorting": false,
            "bInfo": false,
            "scrollY": '30vh',
            "columnDefs": [
                { "visible": false, "targets": [0] } // NEW: Hide the first column (where InvId will be)
            ]
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
    // --- Client-side validation ---
    if (!$("#PatName").val() || !$("#MobileNo").val()) {
        alert("Patient Name and Mobile Number are mandatory.");
        return false;
    }
    if ($("#invtable").DataTable().rows().count() === 0) {
        alert("Please add at least one investigation.");
        return false;
    }
    if (parseFloat($('#lblDueAmount').text()) > 0 && !$('#Remarks').val()) {
        alert("Remarks are mandatory as due amount is greater than 0.");
        $('#Remarks').focus(); // Focus on the Remarks field
        return false;
    }

    // --- 1. Collect Patient Details ---
    var patientDetails = {
        MobileNo: $("#MobileNo").val(),
        PatName: $("#PatName").val(),
        Age: parseInt($("#Age").val()) || 0,
        AgeType: $("#AgeType").val(),
        Gender: $("#Gender").val(),
        Ref: $("#Ref").val(),
        Area: $("#Area").val(),
        City: $("#City").val(),
        Email: $("#Email").val()
    };

    // --- 2. Collect Bill Summary Details ---
    var billSummary = {
        TotalBill: parseFloat($('#lblTotalBill').text()) || 0,
        TotalDiscountAmount: parseFloat($('#lblDiscountAmount').text()) || 0,
        NetAmount: parseFloat($('#lblNetAmount').text()) || 0,
        PaidAmount: parseFloat($('#lblPaidAmount').text()) || 0,
        DueAmount: parseFloat($('#lblDueAmount').text()) || 0,
        Remarks: $('#Remarks').val()
    };

    // --- 3. Collect Bill Details (Investigations from invtable) ---
    var billDetails = [];
    var invTable = $("#invtable").DataTable();
    invTable.rows().every(function () {
        var data = this.data();
        var rowNode = this.node();
        billDetails.push({
            // IMPORTANT: Adjust these indices based on the actual order of columns
            // in your 'invtable' DataTable when rows are added.
            // My assumption is: [InvCode (hidden), InvName, Rate, DiscountPercent, DiscountAmount, NetAmount, Action]
            InvId: data[0], // Assuming InvCode is the first (possibly hidden) column
            InvName: $(rowNode).find("td:nth-child(2)").text(), // Assuming InvName is the second column
            Rate: parseFloat($(rowNode).find("td:nth-child(3)").text()) || 0,
            DiscountPercent: parseFloat($(rowNode).find("td:nth-child(4) input[name='DiscountPercent']").val()) || 0,
            DiscountAmount: parseFloat($(rowNode).find("td:nth-child(4) input[name='DiscountAmount']").val()) || 0,
            NetAmount: parseFloat($(rowNode).find("td:nth-child(5)").text()) || 0
        });
    });

    // --- 4. Collect Payment Details (from paymentGrid) ---
    var paymentDetails = [];
    var paymentTable = $("#paymentGrid").DataTable();
    paymentTable.rows().every(function () {
        var data = this.data();
        paymentDetails.push({
            // IMPORTANT: Adjust these indices based on the actual order of columns
            // in your 'paymentGrid' DataTable when rows are added.
            // My assumption is: [PaymentMode, Amount, Ref.No, Action]
            PaymentMode: data[0],
            Amount: parseFloat(data[1]),
            RefNo: data[2]
        });
    });

    var existingPatientId = $('#hiddenPatientId').val();
    if (existingPatientId) {
        existingPatientId = parseInt(existingPatientId, 10);
    } else {
        existingPatientId = null;
    }

    // --- Bundle all collected data into the ViewModel structure ---
    var patientBillData = {
        PatientInfoId: existingPatientId,  
        PatientDetails: patientDetails,
        SummaryDetails: billSummary,
        BillDetails: billDetails,
        PaymentDetails: paymentDetails
    };
    // --- AJAX Call to save the bill ---
    $.ajax({
        url: '/PatientBilling/SaveBill', // Matches your PatientBillingController and SaveBill action
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify(patientBillData), // Convert JavaScript object to JSON string
        success: function (response) {
            if (response.success === true) { // Assuming result.Item1 is status (1 for success)
                toastr.success(response.message + ' with Bill No: ' + response.billId);
                clearfields(); // Clear the form on successful save
                $("#invtable").DataTable().clear().draw();
                $("#paymentGrid").DataTable().clear().draw();
                // You can add logic here to redirect, print the bill, etc.
                setTimeout(function () {
                    var printUrl = '/PatientBilling/PrintBill/' + response.billId;
                    var printWindow = window.open(printUrl, '_blank', 'width=800,height=600,scrollbars=yes,resizable=yes');

                    // Focus the print window
                    if (printWindow) {
                        printWindow.focus();
                    }
                }, 1000);
            } else {
                toastr.error(response.message);
            }
        },
        error: function (errormessage) {
            toastr.error('Something Wrong! ' + errormessage.responseText);
            console.error("Error saving bill:", errormessage);
        }
    });

    return false; // Prevent default form submission (important for buttons inside a form)
}
// #endregion


// #region Clear Form Fields
function clearfields() {
    $("#PatList").hide();
    $("#Billing").show();
    $('#header').html("Patient Billing");
    clearPatientForm(true);
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
    $('.form-control').removeClass("valid is-invalid");
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
                var SelectedInvId = Id
                var i = $("#invtable").DataTable()
                var rowNode = i.row.add([
                    SelectedInvId,
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

$(document).ready(function () {
    // Auto-search when mobile number is entered (10 digits)
    $('#MobileNo').on('input', function () {
        var mobile = $(this).val();
        if (mobile.length === 10) {
            searchPatients(mobile);
        }
    });

    // Manual search button
    $('#btnSearchPatient').click(function () {
        var mobile = $('#MobileNo').val();
        if (mobile.length >= 10) {
            searchPatients(mobile);
        } else {
            toastr.warning('Please enter a valid 10-digit mobile number.');
        }
    });

    // New patient button in modal
    $('#btnNewPatient').click(function () {
        $('#patientSearchModal').modal('hide');
        clearPatientForm(false);
        $('#hiddenPatientId').val('');   // reset to force new insert
        $('#PatName').focus();
    });
});

function searchPatients(mobileNo) {
    $.ajax({
        url: '/PatientBilling/SearchPatients',
        type: 'POST',
        dataType: 'json',
        data: { mobileNo: mobileNo },
        success: function (response) {
            if (response.success && response.patients && response.patients.length > 0) {
                populatePatientSearchModal(response.patients);
                $('#patientSearchModal').modal('show');
            } else {
                // No existing patients found - user can enter new details
                clearPatientForm(false);
                $('#PatName').focus();
                toastr.info('No existing patients found with this mobile number. Please enter patient details.');
            }
        },
        error: function (xhr, status, error) {
            toastr.error('Error searching patients: ' + error);
        }
    });
}

function populatePatientSearchModal(patients) {
    var tbody = $('#patientSearchTable tbody');
    tbody.empty();

    patients.forEach(function (patient) {
        var row = `
            <tr>
                <td>${patient.uhid || 'N/A'}</td>
                <td>${patient.patName}</td>
                <td>${patient.age} ${patient.ageType || 'Yrs'} / ${patient.gender || 'N/A'}</td>
                <td>${patient.lastVisit}</td>
                <td>
                    <button class="btn btn-sm btn-success select-patient" 
                            data-patient='${JSON.stringify(patient)}'>
                        Select
                    </button>
                </td>
            </tr>
        `;
        tbody.append(row);
    });

    // Handle patient selection
    $('.select-patient').click(function () {
        var patientData = JSON.parse($(this).attr('data-patient'));
        fillPatientForm(patientData);
        $('#patientSearchModal').modal('hide');
        toastr.success('Patient details loaded successfully!');
    });
}

function fillPatientForm(patient) {
    $('#MobileNo').val(patient.mobileNo);
    $('#PatName').val(patient.patName);
    $('#Age').val(patient.age);
    $('#AgeType').val(patient.ageType || 'Years');
    $('#Gender').val(patient.gender || '');
    $('#Ref').val(patient.ref || '');
    $('#Area').val(patient.area || '');
    $('#City').val(patient.city || '');
    $('#Email').val(patient.email || '');

    // Store patient ID for potential updates
    $('#hiddenPatientId').val(patient.patientInfoId);
}
function clearPatientForm(includeMobile = true) {
    if (includeMobile) {
        $('#MobileNo').val('');
    }
    $('#PatName').val('');
    $('#Age').val('');
    $('#AgeType').val('Years');
    $('#Gender').val('');
    $('#Ref').val('');
    $('#Area').val('');
    $('#City').val('');
    $('#Email').val('');
    $('#hiddenPatientId').val('');
}
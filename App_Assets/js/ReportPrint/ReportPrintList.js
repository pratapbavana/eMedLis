$(document).ready(function () {
    bindReportPrintListEvents();
    searchAuthorizedBills();
});

function bindReportPrintListEvents() {
    $('#btnRpSearch').on('click', function () {
        searchAuthorizedBills();
    });

    $('#btnRpClear').on('click', function () {
        $('#rpBillNo,#rpSampleBarcode,#rpPatientName,#rpMobileNo,#rpDateFrom,#rpDateTo,#rpSubDepartment').val('');
        searchAuthorizedBills();
    });
}

function searchAuthorizedBills() {
    $.ajax({
        url: '/ReportPrint/SearchBills',
        type: 'GET',
        dataType: 'json',
        data: {
            billNo: $('#rpBillNo').val().trim(),
            sampleBarcode: $('#rpSampleBarcode').val().trim(),
            patientName: $('#rpPatientName').val().trim(),
            mobileNo: $('#rpMobileNo').val().trim(),
            dateFrom: $('#rpDateFrom').val(),
            dateTo: $('#rpDateTo').val(),
            subDepartment: $('#rpSubDepartment').val().trim()
        },
        success: function (rows) {
            renderBillRows(rows || []);
        },
        error: function () {
            toastr.error('Failed to load authorized bills');
        }
    });
}

function renderBillRows(rows) {
    var tbody = $('#rpBillTable tbody');
    tbody.empty();

    if (!rows || rows.length === 0) {
        tbody.append('<tr><td colspan="8" class="text-center">No records found</td></tr>');
        return;
    }

    rows.forEach(function (row) {
        var ageGender = (row.PatientAge || 0) + ' ' + normalizeAgeType(row.PatientAgeType) + ' / ' + normalizeGender(row.PatientGender);
        var tr = '<tr>' +
            '<td>' + htmlEncode(row.BillNo || '-') + '</td>' +
            '<td>' + htmlEncode(row.PatientName || '-') + '</td>' +
            '<td>' + htmlEncode(ageGender) + '</td>' +
            '<td>' + htmlEncode(row.MobileNo || '-') + '</td>' +
            '<td>' + htmlEncode(row.CollectionBarcode || '-') + '</td>' +
            '<td>' + htmlEncode(formatDate(row.CollectionDate)) + '</td>' +
            '<td>' + (row.InvestigationCount || 0) + '</td>' +
            '<td><a class="btn btn-sm btn-primary" href="/ReportPrint/Entry?billSummaryId=' + row.BillSummaryId + '">Select</a></td>' +
            '</tr>';
        tbody.append(tr);
    });
}

function normalizeAgeType(ageType) {
    var value = (ageType || '').toString().toLowerCase();
    if (value === '1' || value.indexOf('year') >= 0) return 'Year(s)';
    if (value === '2' || value.indexOf('month') >= 0) return 'Month(s)';
    if (value === '3' || value.indexOf('day') >= 0) return 'Day(s)';
    return 'Year(s)';
}

function normalizeGender(gender) {
    var value = (gender || '').toString().trim().toLowerCase();
    if (value === '1' || value === 'male' || value === 'm') return 'Male';
    if (value === '2' || value === 'female' || value === 'f') return 'Female';
    if (value === '3' || value === 'others' || value === 'other' || value === 'o') return 'Others';
    return gender || '-';
}

function formatDate(input) {
    if (!input) return '-';
    var d = new Date(input);
    if (isNaN(d.getTime())) return input;
    var dd = String(d.getDate()).padStart(2, '0');
    var mm = String(d.getMonth() + 1).padStart(2, '0');
    var yy = d.getFullYear();
    return dd + '/' + mm + '/' + yy;
}

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}

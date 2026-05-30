$(document).ready(function () {
    bindEvents();
    searchOrders();
});

function bindEvents() {
    $('#btnSearchOrders').on('click', function () {
        searchOrders();
    });

    $('#btnClearSearch').on('click', function () {
        $('#searchBillNo').val('');
        $('#searchSampleBarcode').val('');
        $('#searchPatientName').val('');
        $('#searchMobileNo').val('');
        $('#searchDateFrom').val('');
        $('#searchDateTo').val('');
        searchOrders();
    });
}

function searchOrders() {
    $.ajax({
        url: '/ReportEntry/SearchOrders',
        type: 'GET',
        dataType: 'json',
        data: {
            billNo: $('#searchBillNo').val().trim(),
            sampleBarcode: $('#searchSampleBarcode').val().trim(),
            patientName: $('#searchPatientName').val().trim(),
            mobileNo: $('#searchMobileNo').val().trim(),
            dateFrom: $('#searchDateFrom').val(),
            dateTo: $('#searchDateTo').val()
        },
        success: function (data) {
            renderOrders(data || []);
        },
        error: function () {
            toastr.error('Failed to load patient test orders.');
        }
    });
}

function renderOrders(rows) {
    var tbody = $('#ordersTable tbody');
    tbody.empty();

    if (!rows || rows.length === 0) {
        tbody.append('<tr><td colspan="8" class="text-center">No records found</td></tr>');
        return;
    }

    rows.forEach(function (row) {
        var orderDate = row.CollectionDate ? formatDateTime(row.CollectionDate) : '-';
        var ageGender = (row.PatientAge || 0) + ' ' + normalizeAgeType(row.PatientAgeType) + ' / ' + (row.PatientGender || '-');
        var entryUrl = '/ReportEntry/Entry?sampleCollectionId=' + row.SampleCollectionId;

        var tr = '<tr>' +
            '<td>' + htmlEncode(row.BillNo || '-') + '</td>' +
            '<td>' + htmlEncode(row.PatientName || '-') + '</td>' +
            '<td>' + htmlEncode(ageGender) + '</td>' +
            '<td>' + htmlEncode(row.MobileNo || '-') + '</td>' +
            '<td>' + htmlEncode(row.CollectionBarcode || '-') + '</td>' +
            '<td>' + htmlEncode(orderDate) + '</td>' +
            '<td>' + (row.InvestigationCount || 0) + '</td>' +
            '<td><a class="btn btn-sm btn-primary" href="' + entryUrl + '">Select</a></td>' +
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

function formatDateTime(input) {
    var d = parseJsonDate(input);
    if (isNaN(d.getTime())) {
        return input;
    }

    var day = String(d.getDate()).padStart(2, '0');
    var month = String(d.getMonth() + 1).padStart(2, '0');
    var year = d.getFullYear();
    var hour = String(d.getHours()).padStart(2, '0');
    var minute = String(d.getMinutes()).padStart(2, '0');

    return day + '/' + month + '/' + year + ' ' + hour + ':' + minute;
}

function parseJsonDate(input) {
    if (typeof input === 'string') {
        var match = /\/Date\((-?\d+)(?:[+-]\d+)?\)\//.exec(input);
        if (match) {
            return new Date(parseInt(match[1], 10));
        }
    }

    return new Date(input);
}

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}

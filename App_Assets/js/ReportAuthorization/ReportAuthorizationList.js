$(document).ready(function () {
    bindAuthEvents();
    searchAuthReports();
});

function bindAuthEvents() {
    $('#btnAuthSearch').on('click', function () {
        searchAuthReports();
    });

    $('#btnAuthClear').on('click', function () {
        $('#authDateFrom').val('');
        $('#authDateTo').val('');
        $('#authPatientName').val('');
        $('#authSampleId').val('');
        $('#authInvestigation').val('');
        $('#authCriticalOnly').prop('checked', false);
        searchAuthReports();
    });
}

function searchAuthReports() {
    $.ajax({
        url: '/ReportAuthorization/Search',
        type: 'GET',
        dataType: 'json',
        data: {
            dateFrom: $('#authDateFrom').val(),
            dateTo: $('#authDateTo').val(),
            patientName: $('#authPatientName').val().trim(),
            sampleBarcode: $('#authSampleId').val().trim(),
            investigation: $('#authInvestigation').val().trim(),
            criticalOnly: $('#authCriticalOnly').prop('checked')
        },
        success: function (data) {
            renderAuthRows(data || []);
        },
        error: function () {
            toastr.error('Failed to load pending reports.');
        }
    });
}

function renderAuthRows(rows) {
    var tbody = $('#authTable tbody');
    tbody.empty();

    if (!rows || rows.length === 0) {
        tbody.append('<tr><td colspan="9" class="text-center">No pending reports found</td></tr>');
        return;
    }

    rows.forEach(function (row) {
        var ageGender = (row.PatientAge || 0) + ' ' + normalizeAgeType(row.PatientAgeType) + ' / ' + normalizeGender(row.PatientGender);
        var statusHtml = getStatusBadge(row.ResultStatus);
        var criticalHtml = row.HasCritical
            ? '<span class="badge badge-danger">YES</span>'
            : '<span class="badge badge-secondary">NO</span>';

        var tr = '<tr>' +
            '<td>' + htmlEncode(row.SampleBarcode || '-') + '</td>' +
            '<td>' + htmlEncode(row.PatientName || '-') + '</td>' +
            '<td>' + htmlEncode(ageGender) + '</td>' +
            '<td>' + htmlEncode(row.InvestigationName || '-') + '</td>' +
            '<td>' + htmlEncode(row.DepartmentName || '-') + '</td>' +
            '<td>' + htmlEncode(formatDateTime(row.CollectionDate)) + '</td>' +
            '<td>' + statusHtml + '</td>' +
            '<td>' + criticalHtml + '</td>' +
            '<td><a class="btn btn-sm btn-primary" href="/ReportAuthorization/Review?sampleDetailId=' + row.SampleDetailId + '">Review</a></td>' +
            '</tr>';
        tbody.append(tr);
    });
}

function getStatusBadge(status) {
    var text = status || '-';
    if (text === 'Pending Authorization') {
        return '<span class="badge badge-primary">' + htmlEncode(text) + '</span>';
    }
    if (text === 'Rejected') {
        return '<span class="badge badge-danger">' + htmlEncode(text) + '</span>';
    }
    if (text === 'Authorized') {
        return '<span class="badge badge-success">' + htmlEncode(text) + '</span>';
    }
    return '<span class="badge badge-secondary">' + htmlEncode(text) + '</span>';
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

function formatDateTime(input) {
    if (!input) return '-';
    var d = new Date(input);
    if (isNaN(d.getTime())) return input;
    var day = String(d.getDate()).padStart(2, '0');
    var month = String(d.getMonth() + 1).padStart(2, '0');
    var year = d.getFullYear();
    return day + '/' + month + '/' + year;
}

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}

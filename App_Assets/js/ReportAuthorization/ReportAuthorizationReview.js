var reviewData = null;

$(document).ready(function () {
    bindReviewEvents();
    loadReview();
});

function bindReviewEvents() {
    $('#btnRvSave').on('click', function () {
        submitReviewAction('/ReportAuthorization/SaveReview', false);
    });

    $('#btnRvAuthorize').on('click', function () {
        if (!confirm('Authorize this report? After authorization, results will be locked.')) {
            return;
        }
        submitReviewAction('/ReportAuthorization/Authorize', false);
    });

    $('#btnRvReject').on('click', function () {
        if (!$('#rvRejectReason').val().trim()) {
            toastr.error('Reject reason is required.');
            return;
        }
        if (!confirm('Reject this report and send back to technician?')) {
            return;
        }
        submitReviewAction('/ReportAuthorization/Reject', true);
    });
}

function loadReview() {
    $.ajax({
        url: '/ReportAuthorization/GetReview',
        type: 'GET',
        dataType: 'json',
        data: { sampleDetailId: reportAuthorizationSampleDetailId },
        success: function (res) {
            if (!res || !res.success || !res.data) {
                toastr.error(res && res.message ? res.message : 'Unable to load report');
                return;
            }
            reviewData = res.data;
            bindSummary();
            bindResults();
            bindPermissions();
        },
        error: function () {
            toastr.error('Unable to load report');
        }
    });
}

function bindSummary() {
    var p = reviewData.Patient || {};
    $('#rvBillNo').text(p.BillNo || '-');
    $('#rvPatientName').text(p.PatientName || '-');
    $('#rvAgeGender').text((p.PatientAge || 0) + ' ' + normalizeAgeType(p.PatientAgeType) + ' / ' + normalizeGender(p.PatientGender));
    $('#rvSampleId').text(p.SampleBarcode || '-');
    $('#rvInvestigation').text(p.InvestigationName || '-');
    $('#rvDepartment').text(p.DepartmentName || '-');
    $('#rvAuthorizedBy').text(reviewData.AuthorizedDoctor || '-');
    $('#rvInterpretation').val(reviewData.Interpretation || '');
    $('#rvRejectReason').val(reviewData.RejectedReason || '');

    var badge = $('#rvStatus');
    var status = reviewData.ResultStatus || '-';
    badge.text(status).removeClass('badge-primary badge-success badge-danger badge-secondary');
    if (status === 'Pending Authorization') {
        badge.addClass('badge-primary');
    } else if (status === 'Authorized') {
        badge.addClass('badge-success');
    } else if (status === 'Rejected') {
        badge.addClass('badge-danger');
    } else {
        badge.addClass('badge-secondary');
    }

    if (reviewData.HasSignature && reviewData.DoctorId) {
        $('#rvSignature').attr('src', '/DoctorMaster/Signature/' + reviewData.DoctorId + '?_=' + new Date().getTime());
        $('#rvSignatureWrap').removeClass('d-none');
    } else {
        $('#rvSignatureWrap').addClass('d-none');
    }
}

function bindResults() {
    var tbody = $('#rvTable tbody');
    tbody.empty();

    var rows = reviewData.Results || [];
    if (!rows.length) {
        tbody.append('<tr><td colspan="6" class="text-center">No results available</td></tr>');
        return;
    }

    var currentHeader = '__NONE__';
    rows.forEach(function (r) {
        var header = (r.HeaderName || '').trim();
        if (header && header !== currentHeader) {
            currentHeader = header;
            tbody.append('<tr class="bg-light"><td colspan="6" class="font-weight-bold">' + htmlEncode(header) + '</td></tr>');
        }
        if (!header) {
            currentHeader = '__NONE__';
        }

        var rowClass = '';
        var flag = r.Flag || '';
        if (r.IsCritical || flag === 'C') {
            rowClass = 'table-danger';
        } else if (flag === 'H') {
            rowClass = 'table-warning';
        } else if (flag === 'L') {
            rowClass = 'table-info';
        }

        var tr = '<tr class="' + rowClass + '">' +
            '<td>' + htmlEncode(r.ParameterName || '-') + '</td>' +
            '<td>' + htmlEncode(r.MethodName || '-') + '</td>' +
            '<td>' + htmlEncode(r.ResultValue || '-') + '</td>' +
            '<td>' + htmlEncode(r.Unit || '-') + '</td>' +
            '<td>' + htmlEncode(r.NormalRange || '-') + '</td>' +
            '<td class="text-center font-weight-bold">' + htmlEncode(flag) + '</td>' +
            '</tr>';
        tbody.append(tr);
    });
}

function bindPermissions() {
    var canAuthorize = reviewData && reviewData.CanAuthorize === true && reviewData.ResultStatus === 'Pending Authorization';
    $('#btnRvSave').prop('disabled', !canAuthorize);
    $('#btnRvAuthorize').prop('disabled', !canAuthorize);
    $('#btnRvReject').prop('disabled', !canAuthorize);
    $('#rvInterpretation').prop('readonly', !canAuthorize);
    $('#rvRejectReason').prop('readonly', !canAuthorize);
}

function submitReviewAction(url, includeRejectReason) {
    var payload = {
        SampleDetailId: reportAuthorizationSampleDetailId,
        Interpretation: $('#rvInterpretation').val().trim(),
        RejectReason: includeRejectReason ? $('#rvRejectReason').val().trim() : ''
    };

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json;charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(payload),
        success: function (res) {
            var code = res ? parseInt(res.Item1, 10) : 0;
            if (code === 1) {
                toastr.success(res.Item2 || 'Saved');
                loadReview();
            } else {
                toastr.error(res && res.Item2 ? res.Item2 : 'Action failed');
            }
        },
        error: function () {
            toastr.error('Action failed');
        }
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

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}

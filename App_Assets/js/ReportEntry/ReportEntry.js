var reportEntryState = {
    selectedSampleCollectionId: 0,
    selectedSampleDetailId: 0,
    templateItems: [],
    rangeByParameter: {}
};

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

    $('#methodSelect').on('change', function () {
        var methodId = parseInt($(this).val() || '0');
        if (methodId <= 0) {
            reportEntryState.rangeByParameter = {};
            renderTemplateGrid();
            return;
        }

        if (!reportEntryState.selectedSampleDetailId) {
            return;
        }

        loadRanges(reportEntryState.selectedSampleDetailId, methodId);
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
        clearInvestigationPanel();
        return;
    }

    rows.forEach(function (row) {
        var orderDate = row.CollectionDate ? formatDateTime(row.CollectionDate) : '-';
        var ageGender = (row.PatientAge || 0) + ' ' + normalizeAgeType(row.PatientAgeType) + ' / ' + (row.PatientGender || '-');
        var tr = '<tr>' +
            '<td>' + htmlEncode(row.BillNo || '-') + '</td>' +
            '<td>' + htmlEncode(row.PatientName || '-') + '</td>' +
            '<td>' + htmlEncode(ageGender) + '</td>' +
            '<td>' + htmlEncode(row.MobileNo || '-') + '</td>' +
            '<td>' + htmlEncode(row.CollectionBarcode || '-') + '</td>' +
            '<td>' + htmlEncode(orderDate) + '</td>' +
            '<td>' + (row.InvestigationCount || 0) + '</td>' +
            '<td><button type="button" class="btn btn-sm btn-primary" onclick="selectOrder(' + row.SampleCollectionId + ')">Select</button></td>' +
            '</tr>';
        tbody.append(tr);
    });
}

function selectOrder(sampleCollectionId) {
    reportEntryState.selectedSampleCollectionId = sampleCollectionId;
    reportEntryState.selectedSampleDetailId = 0;
    clearTemplatePanel();

    $.ajax({
        url: '/ReportEntry/GetInvestigations',
        type: 'GET',
        dataType: 'json',
        data: { sampleCollectionId: sampleCollectionId },
        success: function (data) {
            renderInvestigations(data || []);
        },
        error: function () {
            toastr.error('Failed to load investigations.');
        }
    });
}

function renderInvestigations(items) {
    var tbody = $('#investigationsTable tbody');
    tbody.empty();

    if (!items || items.length === 0) {
        tbody.append('<tr><td colspan="3" class="text-center">No investigations found</td></tr>');
        return;
    }

    items.forEach(function (item) {
        var actionText = item.HasTemplate ?
            '<button type="button" class="btn btn-sm btn-success" onclick="loadTemplate(' + item.SampleDetailId + ')">Load</button>' :
            '<span class="text-danger">Template Missing</span>';

        var tr = '<tr>' +
            '<td>' + htmlEncode(item.InvestigationName || '-') + '</td>' +
            '<td>' + htmlEncode(item.SampleBarcode || '-') + '</td>' +
            '<td>' + actionText + '</td>' +
            '</tr>';

        tbody.append(tr);
    });
}

function loadTemplate(sampleDetailId) {
    reportEntryState.selectedSampleDetailId = sampleDetailId;
    reportEntryState.rangeByParameter = {};

    $.ajax({
        url: '/ReportEntry/LoadTemplate',
        type: 'GET',
        dataType: 'json',
        data: { sampleDetailId: sampleDetailId },
        success: function (response) {
            if (!response || !response.Patient) {
                toastr.error('Template context not found.');
                clearTemplatePanel();
                return;
            }

            reportEntryState.templateItems = response.TemplateItems || [];
            bindPatientInfo(response.Patient);
            bindMethods(response.Methods || []);
            renderTemplateGrid();
        },
        error: function () {
            toastr.error('Failed to load investigation template.');
        }
    });
}

function bindPatientInfo(patient) {
    $('#piPatientName').text(patient.PatientName || '-');
    $('#piAge').text((patient.PatientAge || 0) + ' ' + normalizeAgeType(patient.PatientAgeType));
    $('#piGender').text(patient.PatientGender || '-');
    $('#piInvestigation').text(patient.InvestigationName || '-');
    $('#piSampleId').text(patient.SampleBarcode || '-');
    $('#piBillNo').text(patient.BillNo || '-');
}

function bindMethods(methods) {
    var select = $('#methodSelect');
    select.empty();
    select.append('<option value="">Select Method</option>');

    methods.forEach(function (m) {
        select.append('<option value="' + m.MethodId + '">' + htmlEncode(m.MethodName) + '</option>');
    });

    select.val('');
}

function loadRanges(sampleDetailId, methodId) {
    $.ajax({
        url: '/ReportEntry/LoadRanges',
        type: 'GET',
        dataType: 'json',
        data: {
            sampleDetailId: sampleDetailId,
            methodId: methodId
        },
        success: function (response) {
            if (!response || !response.success) {
                toastr.error((response && response.message) ? response.message : 'Unable to load ranges.');
                return;
            }

            var map = {};
            (response.ranges || []).forEach(function (r) {
                map[r.ParameterId] = r.DisplayRange;
            });

            reportEntryState.rangeByParameter = map;
            renderTemplateGrid();
        },
        error: function () {
            toastr.error('Failed to load method-wise ranges.');
        }
    });
}

function renderTemplateGrid() {
    var tbody = $('#templateGrid tbody');
    tbody.empty();

    if (!reportEntryState.templateItems || reportEntryState.templateItems.length === 0) {
        tbody.append('<tr><td colspan="4" class="text-center">Select an investigation to load template</td></tr>');
        return;
    }

    var methodSelected = !!$('#methodSelect').val();

    reportEntryState.templateItems.forEach(function (item, index) {
        if ((item.ItemType || '').toLowerCase() === 'header') {
            var headerName = item.HeaderName || '-';
            tbody.append('<tr class="bg-light"><td colspan="4" class="font-weight-bold">' + htmlEncode(headerName) + '</td></tr>');
            return;
        }

        if ((item.ItemType || '').toLowerCase() === 'parameter') {
            var parameterId = item.ParameterId || 0;
            var rangeValue = methodSelected ? (reportEntryState.rangeByParameter[parameterId] || '-') : 'Select method';
            var disabledAttr = methodSelected ? '' : 'disabled';
            var row = '<tr>' +
                '<td>' + htmlEncode(item.ParameterName || '-') + '</td>' +
                '<td><input type="text" class="form-control form-control-sm" id="result_' + parameterId + '_' + index + '" ' + disabledAttr + ' /></td>' +
                '<td>' + htmlEncode(item.Unit || '-') + '</td>' +
                '<td>' + htmlEncode(rangeValue) + '</td>' +
                '</tr>';
            tbody.append(row);
        }
    });
}

function clearInvestigationPanel() {
    $('#investigationsTable tbody').empty().append('<tr><td colspan="3" class="text-center">Search and select an order</td></tr>');
    clearTemplatePanel();
}

function clearTemplatePanel() {
    reportEntryState.selectedSampleDetailId = 0;
    reportEntryState.templateItems = [];
    reportEntryState.rangeByParameter = {};
    $('#piPatientName').text('-');
    $('#piAge').text('-');
    $('#piGender').text('-');
    $('#piInvestigation').text('-');
    $('#piSampleId').text('-');
    $('#piBillNo').text('-');
    $('#methodSelect').empty().append('<option value="">Select Method</option>');
    renderTemplateGrid();
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

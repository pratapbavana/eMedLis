var reportEntryState = {
    selectedSampleCollectionId: 0,
    selectedSampleDetailId: 0,
    templateItems: [],
    methods: [],
    noMethodId: 0,
    rangeByParameter: {},
    selectedMethodByParameter: {},
    dropdownValuesByParameter: {},
    savedResultsByParameter: {},
    resultMetaByParameter: {},
    resultStatus: 'Draft',
    isEditable: true,
    criticalNotified: {},
    flagDebounceTimers: {}
};

$(document).ready(function () {
    reportEntryState.selectedSampleCollectionId = parseInt(reportEntrySampleCollectionId || 0);
    bindEvents();

    if (!reportEntryState.selectedSampleCollectionId) {
        toastr.error('Invalid sample collection selected.');
        return;
    }

    loadOrderSummary(reportEntryState.selectedSampleCollectionId);
    loadInvestigations(reportEntryState.selectedSampleCollectionId);
    renderTemplateGrid();
    setStatusUI();
});

function bindEvents() {
    $(document).on('change', '.parameter-method-select', function () {
        var parameterId = parseInt($(this).data('parameter-id') || '0');
        var methodId = parseInt($(this).val() || '0');
        reportEntryState.selectedMethodByParameter[parameterId] = methodId;

        if (methodId > 0) {
            loadParameterRange(parameterId, methodId);
        } else {
            reportEntryState.rangeByParameter[parameterId] = null;
            $('#range_' + parameterId).text('-');
            applyFlag(parameterId, '');
        }
    });

    $(document).on('input change', '.result-input, .result-dropdown', function (e) {
        updateFlagForInput($(this), e.type);
        calculateDerivedParameters();
    });

    $(document).on('blur', '.result-input', function () {
        updateFlagForInput($(this), 'blur');
    });

    $('#btnSaveDraft').on('click', function () {
        saveResults('Draft');
    });

    $('#btnSubmitAuthorization').on('click', function () {
        saveResults('Pending Authorization');
    });
}

function loadOrderSummary(sampleCollectionId) {
    $.ajax({
        url: '/ReportEntry/GetOrderSummary',
        type: 'GET',
        dataType: 'json',
        data: { sampleCollectionId: sampleCollectionId },
        success: function (data) {
            if (!data) {
                toastr.error('Unable to load patient details.');
                return;
            }

            var ageGender = (data.PatientAge || 0) + ' ' + normalizeAgeType(data.PatientAgeType) + ' / ' + normalizeGender(data.PatientGender);
            $('#osBillNo').text(data.BillNo || '-');
            $('#osPatientName').text(data.PatientName || '-');
            $('#osUhid').text(data.UHID || '-');
            $('#osAgeGender').text(ageGender);
            $('#osMobile').text(data.MobileNo || '-');
            $('#osCollectionBarcode').text(data.CollectionBarcode || '-');
        },
        error: function () {
            toastr.error('Failed to load order summary.');
        }
    });
}

function loadInvestigations(sampleCollectionId) {
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

        var tr = '<tr id="inv_row_' + item.SampleDetailId + '">' +
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
    reportEntryState.selectedMethodByParameter = {};
    reportEntryState.dropdownValuesByParameter = {};
    reportEntryState.savedResultsByParameter = {};
    reportEntryState.resultMetaByParameter = {};
    reportEntryState.criticalNotified = {};

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
            reportEntryState.methods = response.Methods || [];
            reportEntryState.noMethodId = resolveNoMethodId();
            reportEntryState.resultStatus = response.ResultStatus || 'Draft';
            reportEntryState.isEditable = response.IsEditable !== false;

            (response.SavedResults || []).forEach(function (r) {
                reportEntryState.savedResultsByParameter[r.ParameterId] = r;
                reportEntryState.resultMetaByParameter[r.ParameterId] = {
                    flag: r.Flag || '',
                    isCritical: !!r.IsCritical
                };
            });

            reportEntryState.templateItems.forEach(function (item) {
                if ((item.ItemType || '').toLowerCase() === 'parameter' && item.ParameterId) {
                    var saved = reportEntryState.savedResultsByParameter[item.ParameterId];
                    var selected = saved && saved.MethodId != null ? saved.MethodId :
                        (item.DefaultMethodId != null ? item.DefaultMethodId : reportEntryState.noMethodId);
                    reportEntryState.selectedMethodByParameter[item.ParameterId] = selected;
                }
            });

            bindPatientInfo(response.Patient);
            markSelectedInvestigationRow(sampleDetailId);
            setStatusUI();

            loadDropdownValuesForTemplate(function () {
                renderTemplateGrid();
                refreshAllParameterRanges();
                calculateDerivedParameters();
            });
        },
        error: function () {
            toastr.error('Failed to load investigation template.');
        }
    });
}

function bindPatientInfo(patient) {
    $('#osPatientName').text(patient.PatientName || '-');
    $('#osAgeGender').text((patient.PatientAge || 0) + ' ' + normalizeAgeType(patient.PatientAgeType) + ' / ' + normalizeGender(patient.PatientGender));
    $('#osCollectionBarcode').text(patient.SampleBarcode || $('#osCollectionBarcode').text() || '-');
    $('#osBillNo').text(patient.BillNo || $('#osBillNo').text() || '-');
    $('#ciInvestigation').text(patient.InvestigationName || '-');
    $('#ciSampleId').text(patient.SampleBarcode || '-');
}

function renderTemplateGrid() {
    var tbody = $('#templateGrid tbody');
    tbody.empty();

    if (!reportEntryState.templateItems || reportEntryState.templateItems.length === 0) {
        tbody.append('<tr><td colspan="6" class="text-center">Select an investigation to load template</td></tr>');
        return;
    }

    reportEntryState.templateItems.forEach(function (item) {
        if ((item.ItemType || '').toLowerCase() === 'header') {
            var headerName = item.HeaderName || '-';
            tbody.append('<tr class="bg-light"><td colspan="6" class="font-weight-bold">' + htmlEncode(headerName) + '</td></tr>');
            return;
        }

        if ((item.ItemType || '').toLowerCase() === 'parameter') {
            var parameterId = item.ParameterId || 0;
            var selectedMethodId = reportEntryState.selectedMethodByParameter[parameterId] || 0;
            var rangeInfo = reportEntryState.rangeByParameter[parameterId] || null;
            var rangeValue = rangeInfo && rangeInfo.DisplayRange ? rangeInfo.DisplayRange : '-';
            var inputControl = buildResultControl(item);
            var flagValue = (reportEntryState.resultMetaByParameter[parameterId] || {}).flag || '';
            var rowClass = resolveRowClass(flagValue);

            var row = '<tr id="row_' + parameterId + '" class="' + rowClass + '">' +
                '<td>' + htmlEncode(item.ParameterName || '-') + '</td>' +
                '<td>' + buildMethodDropdown(parameterId, selectedMethodId) + '</td>' +
                '<td>' + inputControl + '</td>' +
                '<td>' + htmlEncode(item.Unit || '-') + '</td>' +
                '<td id="range_' + parameterId + '">' + htmlEncode(rangeValue) + '</td>' +
                '<td id="flag_' + parameterId + '" class="font-weight-bold text-center">' + htmlEncode(flagValue) + '</td>' +
                '</tr>';

            tbody.append(row);
        }
    });

    ensureSelect2ForMethodControls();
    initResultSelects();
}

function buildResultControl(item) {
    var parameterId = item.ParameterId || 0;
    var type = (item.ResultType || '').toLowerCase();
    var saved = reportEntryState.savedResultsByParameter[parameterId] || null;
    var value = saved && saved.ResultValue != null ? saved.ResultValue : '';
    var disabledAttr = reportEntryState.isEditable ? '' : ' disabled';

    if (type === 'dropdown') {
        var values = reportEntryState.dropdownValuesByParameter[parameterId] || [];
        var options = '<option value="">Select</option>';
        values.forEach(function (v) {
            var selected = (value === v.ValueText) ? ' selected' : '';
            options += '<option value="' + htmlEncode(v.ValueText) + '"' + selected + '>' + htmlEncode(v.ValueText) + '</option>';
        });
        return '<select class="form-control form-control-sm result-dropdown" id="result_' + parameterId + '" data-parameter-id="' + parameterId + '"' + disabledAttr + '>' + options + '</select>';
    }

    if (type === 'positivenegative') {
        return '<select class="form-control form-control-sm result-dropdown" id="result_' + parameterId + '" data-parameter-id="' + parameterId + '"' + disabledAttr + '>' +
            '<option value="">Select</option><option value="Positive"' + (value === 'Positive' ? ' selected' : '') + '>Positive</option><option value="Negative"' + (value === 'Negative' ? ' selected' : '') + '>Negative</option></select>';
    }

    if (type === 'calculated') {
        return '<input type="text" class="form-control form-control-sm result-input" id="result_' + parameterId + '" data-parameter-id="' + parameterId + '" value="' + htmlEncode(value) + '" readonly />';
    }

    return '<input type="text" class="form-control form-control-sm result-input" id="result_' + parameterId + '" data-parameter-id="' + parameterId + '" value="' + htmlEncode(value) + '"' + disabledAttr + ' />';
}

function buildMethodDropdown(parameterId, selectedMethodId) {
    var disabledAttr = reportEntryState.isEditable ? '' : ' disabled';
    var html = '<select class="form-control form-control-sm parameter-method-select" data-parameter-id="' + parameterId + '"' + disabledAttr + '>';
    html += '<option value="' + reportEntryState.noMethodId + '">No Method</option>';

    reportEntryState.methods.forEach(function (m) {
        if (m.MethodId === reportEntryState.noMethodId) {
            return;
        }
        var selectedAttr = (m.MethodId === selectedMethodId) ? ' selected' : '';
        html += '<option value="' + m.MethodId + '"' + selectedAttr + '>' + htmlEncode(m.MethodName) + '</option>';
    });

    html += '</select>';
    return html;
}

function loadDropdownValuesForTemplate(callback) {
    var dropdownParameterIds = reportEntryState.templateItems
        .filter(function (x) { return (x.ItemType || '').toLowerCase() === 'parameter' && (x.ResultType || '').toLowerCase() === 'dropdown' && x.ParameterId; })
        .map(function (x) { return x.ParameterId; });

    if (!dropdownParameterIds.length) {
        callback();
        return;
    }

    var pending = dropdownParameterIds.length;
    dropdownParameterIds.forEach(function (parameterId) {
        $.ajax({
            url: '/ParameterMaster/DropdownValues',
            type: 'GET',
            dataType: 'json',
            data: { parameterId: parameterId },
            success: function (data) {
                reportEntryState.dropdownValuesByParameter[parameterId] = data || [];
            },
            complete: function () {
                pending--;
                if (pending === 0) {
                    callback();
                }
            }
        });
    });
}

function refreshAllParameterRanges() {
    reportEntryState.templateItems.forEach(function (item) {
        if ((item.ItemType || '').toLowerCase() !== 'parameter' || !item.ParameterId) {
            return;
        }

        var parameterId = item.ParameterId;
        var methodId = reportEntryState.selectedMethodByParameter[parameterId] || 0;
        if (methodId > 0) {
            loadParameterRange(parameterId, methodId);
        } else {
            reportEntryState.rangeByParameter[parameterId] = null;
            $('#range_' + parameterId).text('-');
            applyFlag(parameterId, '');
        }
    });
}

function loadParameterRange(parameterId, methodId) {
    if (!reportEntryState.selectedSampleDetailId || parameterId <= 0 || methodId <= 0) {
        return;
    }

    $.ajax({
        url: '/ReportEntry/LoadParameterRange',
        type: 'GET',
        dataType: 'json',
        data: {
            sampleDetailId: reportEntryState.selectedSampleDetailId,
            parameterId: parameterId,
            methodId: methodId
        },
        success: function (response) {
            if (!response || !response.success || !response.range || response.range.Found !== true) {
                reportEntryState.rangeByParameter[parameterId] = null;
                $('#range_' + parameterId).text('-');
                applyFlag(parameterId, '');
                return;
            }

            reportEntryState.rangeByParameter[parameterId] = response.range;
            $('#range_' + parameterId).text(response.range.DisplayRange || '-');
            evaluateAndApplyFlag(parameterId);
        }
    });
}

function calculateDerivedParameters() {
    var calculated = reportEntryState.templateItems.filter(function (x) {
        return (x.ItemType || '').toLowerCase() === 'parameter' && (x.ResultType || '').toLowerCase() === 'calculated' && x.Formula;
    });

    calculated.forEach(function (item) {
        var value = evaluateFormula(item.Formula, item.DecimalPrecision || 0);
        if (value === null || value === undefined || isNaN(value)) {
            $('#result_' + item.ParameterId).val('');
        } else {
            $('#result_' + item.ParameterId).val(value);
        }
        evaluateAndApplyFlag(item.ParameterId);
    });
}

function updateFlagForInput($input, eventType) {
    var parameterId = parseInt($input.data('parameter-id') || '0');
    if (!parameterId) {
        return;
    }

    var type = (eventType || '').toLowerCase();
    if (type === 'input') {
        // Debounce while typing so transient partial values do not raise false critical alerts.
        clearTimeout(reportEntryState.flagDebounceTimers[parameterId]);
        reportEntryState.flagDebounceTimers[parameterId] = setTimeout(function () {
            evaluateAndApplyFlag(parameterId);
        }, 600);
        return;
    }

    clearTimeout(reportEntryState.flagDebounceTimers[parameterId]);
    evaluateAndApplyFlag(parameterId);
}

function evaluateAndApplyFlag(parameterId) {
    var item = getTemplateParameter(parameterId);
    if (!item) {
        return;
    }

    var type = (item.ResultType || '').toLowerCase();
    if (type !== 'numeric' && type !== 'calculated') {
        applyFlag(parameterId, '');
        return;
    }

    var raw = $('#result_' + parameterId).val();
    var value = parseFloat(raw);
    if (raw === '' || raw === null || raw === undefined || isNaN(value)) {
        applyFlag(parameterId, '');
        return;
    }

    var range = reportEntryState.rangeByParameter[parameterId] || null;
    if (!range || range.Found !== true) {
        applyFlag(parameterId, '');
        return;
    }

    var normalMin = parseFloat(range.NormalMin);
    var normalMax = parseFloat(range.NormalMax);
    if (isNaN(normalMin) || isNaN(normalMax)) {
        applyFlag(parameterId, '');
        return;
    }

    var flag = '';
    var criticalMin = range.CriticalMin;
    var criticalMax = range.CriticalMax;

    if (criticalMin !== null && criticalMin !== undefined && value < parseFloat(criticalMin)) {
        flag = 'C';
    }
    if (criticalMax !== null && criticalMax !== undefined && value > parseFloat(criticalMax)) {
        flag = 'C';
    }

    if (!flag) {
        if (value < normalMin) {
            flag = 'L';
        } else if (value > normalMax) {
            flag = 'H';
        }
    }

    applyFlag(parameterId, flag);
}

function applyFlag(parameterId, flag) {
    var normalized = flag || '';
    var isCritical = normalized === 'C';

    reportEntryState.resultMetaByParameter[parameterId] = {
        flag: normalized,
        isCritical: isCritical
    };

    $('#flag_' + parameterId).text(normalized);
    var row = $('#row_' + parameterId);
    row.removeClass('table-danger table-warning table-info');
    row.addClass(resolveRowClass(normalized));

    if (isCritical && !reportEntryState.criticalNotified[parameterId]) {
        var p = getTemplateParameter(parameterId);
        toastr.error('Critical Value Detected! ' + (p ? p.ParameterName : ('Parameter ' + parameterId)));
        reportEntryState.criticalNotified[parameterId] = true;
    }
    if (!isCritical) {
        reportEntryState.criticalNotified[parameterId] = false;
    }
}

function resolveRowClass(flag) {
    if (flag === 'C') return 'table-danger';
    if (flag === 'H') return 'table-warning';
    if (flag === 'L') return 'table-info';
    return '';
}

function getTemplateParameter(parameterId) {
    for (var i = 0; i < reportEntryState.templateItems.length; i++) {
        var item = reportEntryState.templateItems[i];
        if ((item.ItemType || '').toLowerCase() === 'parameter' && parseInt(item.ParameterId || 0) === parseInt(parameterId || 0)) {
            return item;
        }
    }
    return null;
}

function setStatusUI() {
    var status = reportEntryState.resultStatus || 'Draft';
    var badge = $('#resultStatusBadge');
    badge.text(status).removeClass('badge-secondary badge-primary badge-success badge-danger');
    if (status === 'Draft') {
        badge.addClass('badge-secondary');
    } else if (status === 'Pending Authorization') {
        badge.addClass('badge-primary');
    } else if (status === 'Rejected') {
        badge.addClass('badge-danger');
    } else {
        badge.addClass('badge-success');
    }

    var editable = reportEntryState.isEditable === true;
    $('#btnSaveDraft').prop('disabled', !editable);
    $('#btnSubmitAuthorization').prop('disabled', !editable);
}

function collectResultPayload(validateForSubmit) {
    var items = [];
    var hasError = false;

    reportEntryState.templateItems.forEach(function (item, idx) {
        if ((item.ItemType || '').toLowerCase() !== 'parameter' || !item.ParameterId) {
            return;
        }

        var parameterId = item.ParameterId;
        var resultValue = $('#result_' + parameterId).val();
        var resultType = (item.ResultType || '').toLowerCase();
        var methodId = reportEntryState.selectedMethodByParameter[parameterId];
        var range = reportEntryState.rangeByParameter[parameterId] || {};
        var flagMeta = reportEntryState.resultMetaByParameter[parameterId] || {};

        if ((resultType === 'numeric' || resultType === 'calculated') && resultValue !== '' && resultValue !== null && isNaN(parseFloat(resultValue))) {
            hasError = true;
            $('#result_' + parameterId).addClass('is-invalid');
            return;
        } else {
            $('#result_' + parameterId).removeClass('is-invalid');
        }

        if (validateForSubmit && resultType !== 'calculated' && (resultValue === '' || resultValue === null || resultValue === undefined)) {
            hasError = true;
            $('#result_' + parameterId).addClass('is-invalid');
            return;
        }

        items.push({
            ParameterId: parameterId,
            MethodId: methodId > 0 ? methodId : null,
            ResultValue: resultValue,
            ResultType: item.ResultType || '',
            Unit: item.Unit || '',
            NormalMin: range.NormalMin != null ? range.NormalMin : null,
            NormalMax: range.NormalMax != null ? range.NormalMax : null,
            CriticalMin: range.CriticalMin != null ? range.CriticalMin : null,
            CriticalMax: range.CriticalMax != null ? range.CriticalMax : null,
            RangeText: range.RangeText != null ? range.RangeText : null,
            Flag: flagMeta.flag || '',
            IsCritical: !!flagMeta.isCritical,
            DisplayOrder: idx + 1
        });
    });

    if (hasError) {
        return null;
    }

    return items;
}

function saveResults(targetStatus) {
    if (!reportEntryState.isEditable) {
        toastr.warning('Editing is blocked for current status.');
        return;
    }
    if (!reportEntryState.selectedSampleDetailId) {
        toastr.error('Load an investigation first.');
        return;
    }

    var validateForSubmit = targetStatus === 'Pending Authorization';
    var items = collectResultPayload(validateForSubmit);
    if (!items) {
        toastr.error('Please correct invalid results before saving.');
        return;
    }

    var payload = {
        SampleDetailId: reportEntryState.selectedSampleDetailId,
        TargetStatus: targetStatus,
        Items: items
    };

    var url = targetStatus === 'Pending Authorization' ? '/ReportEntry/SubmitForAuthorization' : '/ReportEntry/SaveDraft';
    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json;charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(payload),
        success: function (result) {
            if (result && result.Item1 === 1) {
                toastr.success(result.Item2 || 'Saved successfully');
                applySavedPayload(items);
                reportEntryState.resultStatus = targetStatus;
                reportEntryState.isEditable = (targetStatus === 'Draft');
                setStatusUI();
                if (targetStatus !== 'Draft') {
                    renderTemplateGrid();
                    refreshAllParameterRanges();
                }
            } else {
                toastr.error(result && result.Item2 ? result.Item2 : 'Save failed');
            }
        },
        error: function () {
            toastr.error('Save failed');
        }
    });
}

function applySavedPayload(items) {
    if (!items || !items.length) {
        return;
    }

    items.forEach(function (it) {
        reportEntryState.savedResultsByParameter[it.ParameterId] = {
            ParameterId: it.ParameterId,
            MethodId: it.MethodId,
            ResultValue: it.ResultValue,
            ResultType: it.ResultType,
            Unit: it.Unit,
            NormalMin: it.NormalMin,
            NormalMax: it.NormalMax,
            CriticalMin: it.CriticalMin,
            CriticalMax: it.CriticalMax,
            RangeText: it.RangeText,
            Flag: it.Flag || '',
            IsCritical: !!it.IsCritical,
            DisplayOrder: it.DisplayOrder
        };

        if (it.MethodId != null) {
            reportEntryState.selectedMethodByParameter[it.ParameterId] = it.MethodId;
        }

        reportEntryState.resultMetaByParameter[it.ParameterId] = {
            flag: it.Flag || '',
            isCritical: !!it.IsCritical
        };
    });
}

function evaluateFormula(formula, precision) {
    try {
        var expression = formula.replace(/\{(\d+)\}/g, function (match, parameterId) {
            var val = parseFloat($('#result_' + parameterId).val());
            return isNaN(val) ? '0' : val.toString();
        });

        if (!/^[0-9+\-*/().\s]+$/.test(expression)) {
            return null;
        }

        if (/\/\s*0+(\.0+)?(\D|$)/.test(expression)) {
            return null;
        }

        var result = Function('return (' + expression + ');')();
        if (!isFinite(result)) {
            return null;
        }

        return Number(result).toFixed(precision);
    } catch (e) {
        return null;
    }
}

function clearTemplatePanel() {
    reportEntryState.selectedSampleDetailId = 0;
    reportEntryState.templateItems = [];
    reportEntryState.methods = [];
    reportEntryState.rangeByParameter = {};
    reportEntryState.selectedMethodByParameter = {};
    reportEntryState.dropdownValuesByParameter = {};
    reportEntryState.savedResultsByParameter = {};
    reportEntryState.resultMetaByParameter = {};
    reportEntryState.resultStatus = 'Draft';
    reportEntryState.isEditable = true;
    Object.keys(reportEntryState.flagDebounceTimers).forEach(function (k) {
        clearTimeout(reportEntryState.flagDebounceTimers[k]);
    });
    reportEntryState.flagDebounceTimers = {};
    $('#ciInvestigation').text('-');
    $('#ciSampleId').text('-');
    $('#investigationsTable tbody tr').removeClass('table-active');
    setStatusUI();
    renderTemplateGrid();
}

function resolveNoMethodId() {
    var none = (reportEntryState.methods || []).find(function (m) {
        var name = (m.MethodName || '').toLowerCase().trim();
        return name === 'none' || name === 'no method' || name === 'nomethod';
    });
    return none ? none.MethodId : 0;
}

function ensureSelect2ForMethodControls() {
    if (!$.fn.select2) {
        return;
    }

    $('.parameter-method-select').each(function () {
        if ($(this).hasClass('select2-hidden-accessible')) {
            $(this).select2('destroy');
        }
        $(this).select2({
            width: '100%',
            placeholder: 'Select Method',
            allowClear: false
        });
    });
}

function initResultSelects() {
    if (!$.fn.select2) {
        return;
    }

    $('.result-dropdown').each(function () {
        if ($(this).hasClass('select2-hidden-accessible')) {
            $(this).select2('destroy');
        }
        $(this).select2({
            width: '100%',
            placeholder: 'Select',
            allowClear: true
        });
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
    if (value === '3' || value === 'other' || value === 'others' || value === 'o') return 'Others';
    return gender || '-';
}

function markSelectedInvestigationRow(sampleDetailId) {
    $('#investigationsTable tbody tr').removeClass('table-active');
    $('#inv_row_' + sampleDetailId).addClass('table-active');
}

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}

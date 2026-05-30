var parameterState = {
    allParameters: [],
    dropdownValues: []
};

function collectFormData() {
    var resultType = $('#ResultType').val();
    return {
        Id: parseInt($('#Id').val() || '0'),
        ParameterHeaderId: null,
        ParameterName: $('#ParameterName').val().trim(),
        ShortName: $('#ShortName').val().trim(),
        Unit: $('#Unit').val().trim(),
        ResultType: resultType,
        DecimalPrecision: parseInt($('#DecimalPrecision').val() || '0'),
        AllowRange: $('#AllowRange').prop('checked'),
        AllowCriticalRange: $('#AllowCriticalRange').prop('checked'),
        IsCalculated: resultType === 'Calculated',
        Formula: resultType === 'Calculated' ? $('#Formula').val().trim() : '',
        Active: $('#active1').prop('checked')
    };
}

function Add() {
    if (!validate()) {
        return false;
    }

    var payload = collectFormData();
    $.ajax({
        url: '/ParameterMaster/Add',
        data: JSON.stringify(payload),
        type: 'POST',
        contentType: 'application/json;charset=utf-8',
        dataType: 'json',
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#parameter_modal').modal('hide');
                $('#parametertable').DataTable().ajax.reload(function () {
                    if (payload.ResultType === 'Dropdown' && parameterState.dropdownValues.length > 0) {
                        autoSaveDropdownValuesByName(payload.ParameterName);
                    }
                });
            } else {
                toastr.error(result.Item2);
            }
        },
        error: function () {
            toastr.error('Something Wrong!');
        }
    });

    return false;
}

function Update() {
    if (!validate()) {
        return false;
    }

    var payload = collectFormData();
    $.ajax({
        url: '/ParameterMaster/Update',
        data: JSON.stringify(payload),
        type: 'POST',
        contentType: 'application/json;charset=utf-8',
        dataType: 'json',
        success: function (result) {
            if (result.Item1 == 1) {
                if (payload.ResultType === 'Dropdown') {
                    saveDropdownValues(payload.Id);
                }
                toastr.success(result.Item2);
                $('#parameter_modal').modal('hide');
                $('#parametertable').DataTable().ajax.reload();
            } else {
                toastr.error(result.Item2);
            }
        },
        error: function () {
            toastr.error('Something Wrong!');
        }
    });

    return false;
}

function GetParameterbyId(Id) {
    $('#header').html('Edit Parameter');
    clearValidation();
    $.ajax({
        url: '/ParameterMaster/getbyID/' + Id,
        type: 'GET',
        dataType: 'json',
        success: function (result) {
            var row = result[0];
            $('#Id').val(row.Id);
            $('#ParameterName').val(row.ParameterName);
            $('#ShortName').val(row.ShortName);
            $('#Unit').val(row.Unit);
            $('#ResultType').val(row.ResultType).trigger('change');
            $('#DecimalPrecision').val(row.DecimalPrecision);
            $('#AllowRange').prop('checked', row.AllowRange);
            $('#AllowCriticalRange').prop('checked', row.AllowCriticalRange);
            $('#Formula').val(row.Formula || '');
            $('#active1').prop('checked', row.Active);
            $('#btnUpdate').removeClass('d-none');
            $('#btnAdd').addClass('d-none');

            if (row.ResultType === 'Dropdown') {
                loadDropdownValues(row.Id);
            }
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
    return false;
}

function SetActive(Id, active) {
    $.ajax({
        url: '/ParameterMaster/SetActive',
        data: { Id: Id, Active: active },
        type: 'POST',
        dataType: 'json',
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#parametertable').DataTable().ajax.reload();
            }
            else
                toastr.error(result.Item2);
        },
        error: function () {
            toastr.error('Something Wrong!');
        }
    });
    return false;
}

function clearfields() {
    $('#header').html('Create Parameter');
    $('#Id').val('0');
    $('#ParameterName').val('');
    $('#ShortName').val('');
    $('#Unit').val('');
    $('#ResultType').val('').trigger('change');
    $('#DecimalPrecision').val('0');
    $('#AllowRange').prop('checked', false);
    $('#AllowCriticalRange').prop('checked', false);
    $('#Formula').val('');
    $('#formulaEditor').val('');
    parameterState.dropdownValues = [];
    renderDropdownValuesTable();
    $('#btnUpdate').addClass('d-none');
    $('#btnAdd').removeClass('d-none');
    clearValidation();
    $('#active1').prop('checked', true);
}

$(() => {
    loaddatatable();
    bindEvents();
});

function loaddatatable() {
    var a = $('#parametertable').DataTable({
        order: [],
        ajax: {
            url: '/ParameterMaster/List',
            method: 'GET',
            dataSrc: function (json) {
                parameterState.allParameters = json || [];
                return json;
            }
        },
        columns: [
            { data: 'Id' },
            { data: 'Id' },
            { data: 'ParameterName' },
            { data: 'Unit' },
            { data: 'ResultType' },
            {
                data: null,
                render: function (data, type, row) {
                    if (row.ResultType === 'Calculated') {
                        return row.Formula || '-';
                    }
                    if (row.ResultType === 'Dropdown') {
                        return row.DropdownDisplayValues || '-';
                    }
                    return '-';
                }
            },
            {
                data: 'Active',
                render: function (data) {
                    return data ? '<span class="badge badge-pill badge-outline-success">ACTIVE</span>' : '<span class="badge badge-pill badge-outline-danger">INACTIVE</span>';
                }
            },
            {
                data: 'Id',
                render: function (data, type, row) {
                    var toggleText = row.Active ? 'Deactivate' : 'Activate';
                    return '<a href="#" data-toggle="modal" data-target="#parameter_modal" onclick="return GetParameterbyId(' + data + ');">Edit</a> | ' +
                        '<a href="#" onclick="return SetActive(' + data + ',' + (!row.Active) + ');">' + toggleText + '</a>';
                }
            }
        ],
        responsive: true,
        columnDefs: [{ searchable: false, orderable: false, targets: 0 }, { responsivePriority: 1, targets: 2 }, { responsivePriority: 2, targets: -1 }]
    });

    a.on('order.dt search.dt', function () {
        a.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();
}

function bindEvents() {
    $('#ResultType').on('change', function () {
        handleResultTypeChange();
    });

    $('#btnAddDropdownValue').on('click', function () {
        var valueText = $('#dropdownValueText').val().trim();
        if (!valueText) {
            return;
        }

        parameterState.dropdownValues.push({
            Id: 0,
            ParameterId: parseInt($('#Id').val() || '0'),
            ValueText: valueText,
            DisplayOrder: parameterState.dropdownValues.length + 1,
            Active: true
        });
        $('#dropdownValueText').val('');
        renderDropdownValuesTable();
    });

    $('#btnSaveDropdownValues').on('click', function () {
        var parameterId = parseInt($('#Id').val() || '0');
        if (parameterId <= 0) {
            toastr.info('Save parameter first, then configure dropdown values.');
            return;
        }

        saveDropdownValues(parameterId);
        $('#dropdown_values_modal').modal('hide');
    });

    $('#btnOpenFormulaBuilder').on('click', function () {
        loadFormulaParameterButtons();
        $('#formulaEditor').val($('#Formula').val() || '');
    });

    $(document).on('click', '.formula-param-btn', function () {
        var parameterId = $(this).data('parameter-id');
        appendToFormulaEditor('{' + parameterId + '}');
    });

    $(document).on('click', '.formula-op', function () {
        appendToFormulaEditor($(this).data('op'));
    });

    $('#btnClearFormula').on('click', function () {
        $('#formulaEditor').val('');
    });

    $('#btnApplyFormula').on('click', function () {
        $('#Formula').val($('#formulaEditor').val().trim());
        $('#formula_builder_modal').modal('hide');
    });

    $('#btnValidateFormula').on('click', function () {
        validateFormula();
    });
}

function handleResultTypeChange() {
    var resultType = $('#ResultType').val();

    $('#dropdownManageGroup').toggleClass('d-none', resultType !== 'Dropdown');
    $('#formulaBuilderGroup').toggleClass('d-none', resultType !== 'Calculated');
    $('#formulaGroup').toggleClass('d-none', resultType !== 'Calculated');
    $('#decimalPrecisionGroup').toggleClass('d-none', resultType !== 'Numeric' && resultType !== 'Calculated');

    if (resultType !== 'Calculated') {
        $('#Formula').val('');
        $('#formulaEditor').val('');
    }

    if (resultType !== 'Dropdown') {
        parameterState.dropdownValues = [];
        renderDropdownValuesTable();
    }
}

function loadDropdownValues(parameterId) {
    $.ajax({
        url: '/ParameterMaster/DropdownValues',
        type: 'GET',
        data: { parameterId: parameterId },
        dataType: 'json',
        success: function (data) {
            parameterState.dropdownValues = data || [];
            renderDropdownValuesTable();
        },
        error: function () {
            parameterState.dropdownValues = [];
            renderDropdownValuesTable();
        }
    });
}

function renderDropdownValuesTable() {
    var tbody = $('#dropdownValuesTable tbody');
    tbody.empty();

    if (!parameterState.dropdownValues.length) {
        tbody.append('<tr><td colspan="3" class="text-center">No values configured</td></tr>');
        return;
    }

    parameterState.dropdownValues.sort(function (a, b) { return a.DisplayOrder - b.DisplayOrder; });

    parameterState.dropdownValues.forEach(function (item, idx) {
        var row = '<tr>' +
            '<td>' + htmlEncode(item.ValueText) + '</td>' +
            '<td>' + item.DisplayOrder + '</td>' +
            '<td><a href="#" onclick="return removeDropdownValue(' + idx + ');">Delete</a></td>' +
            '</tr>';
        tbody.append(row);
    });
}

function removeDropdownValue(index) {
    parameterState.dropdownValues.splice(index, 1);
    parameterState.dropdownValues.forEach(function (x, idx) { x.DisplayOrder = idx + 1; });
    renderDropdownValuesTable();
    return false;
}

function saveDropdownValues(parameterId) {
    var values = parameterState.dropdownValues.map(function (x, idx) {
        return {
            Id: x.Id || 0,
            ParameterId: parameterId,
            ValueText: x.ValueText,
            DisplayOrder: idx + 1,
            Active: true
        };
    });

    $.ajax({
        url: '/ParameterMaster/SaveDropdownValues',
        type: 'POST',
        contentType: 'application/json;charset=utf-8',
        dataType: 'json',
        data: JSON.stringify({
            ParameterId: parameterId,
            Values: values
        }),
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#parametertable').DataTable().ajax.reload();
            } else {
                toastr.error(result.Item2);
            }
        },
        error: function () {
            toastr.error('Failed to save dropdown values');
        }
    });
}

function autoSaveDropdownValuesByName(parameterName) {
    var rows = $('#parametertable').DataTable().rows().data().toArray();
    var match = rows.find(function (r) { return (r.ParameterName || '').toLowerCase() === (parameterName || '').toLowerCase(); });
    if (match && match.Id) {
        saveDropdownValues(match.Id);
    }
}

function loadFormulaParameterButtons() {
    var currentId = parseInt($('#Id').val() || '0');
    var container = $('#formulaParameterList');
    container.empty();

    var parameters = parameterState.allParameters.filter(function (x) { return x.Id !== currentId; });
    if (!parameters.length) {
        container.append('<div class="text-muted">No parameters available</div>');
        return;
    }

    parameters.forEach(function (p) {
        container.append('<button type="button" class="btn btn-outline-primary btn-sm m-1 formula-param-btn" data-parameter-id="' + p.Id + '">[' + htmlEncode(p.ParameterName) + ']</button>');
    });
}

function appendToFormulaEditor(text) {
    var current = $('#formulaEditor').val();
    $('#formulaEditor').val(current + text);
}

function validateFormula() {
    var formula = $('#Formula').val().trim();
    if (!formula) {
        toastr.error('Formula is required');
        return;
    }

    $.ajax({
        url: '/ParameterMaster/ValidateFormula',
        type: 'POST',
        dataType: 'json',
        data: { formula: formula },
        success: function (result) {
            if (result.valid) {
                toastr.success(result.message || 'Formula Valid');
            } else {
                toastr.error(result.message || 'Invalid Formula');
            }
        },
        error: function () {
            toastr.error('Unable to validate formula');
        }
    });
}

function validate() {
    clearValidation();
    var isValid = true;

    if ($('#ParameterName').val().trim() === '') {
        $('#ParameterName').addClass('is-invalid');
        isValid = false;
    }

    if ($('#ResultType').val().trim() === '') {
        $('#ResultType').addClass('is-invalid');
        isValid = false;
    }

    if ($('#ResultType').val() === 'Calculated' && $('#Formula').val().trim() === '') {
        $('#Formula').addClass('is-invalid');
        isValid = false;
    }

    return isValid;
}

function clearValidation() {
    $('#ParameterName').removeClass('is-invalid');
    $('#ResultType').removeClass('is-invalid');
    $('#Formula').removeClass('is-invalid');
}

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}

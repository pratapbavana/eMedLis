var rangeRows = [];
var rangeMode = "Add";
var parametersIndex = {};
var comboIndex = {};
var methodIndex = {};
var pendingSavePayload = null;
var rangeSaveInProgress = false;

function ageBoundaryToStorageDays(value, unit, isFromBoundary) {
    var v = parseFloat(value);
    if (isNaN(v)) {
        return 0;
    }

    var normalized = (unit || "").toLowerCase();
    if (normalized === "days") {
        return Math.round(v);
    }
    if (normalized === "months") {
        var monthDays = Math.round(v * 30);
        return isFromBoundary ? monthDays + 1 : monthDays;
    }
    var yearDays = Math.round(v * 365);
    return isFromBoundary ? yearDays + 1 : yearDays;
}

// #region Modal Open
function openAddModal() {
    rangeSaveInProgress = false;
    $('#btnSave').prop('disabled', false);
    rangeMode = "Add";
    $('#header').html("Create Reference Ranges");
    $('#parametername').prop('disabled', false);
    $('#MethodId').prop('disabled', false);
    $('#parametername').val(null).trigger('change');
    $('#MethodId').val(getDefaultMethodId()).trigger('change');
    clearRowInputs();
    rangeRows = [];
    renderRowsTable();
    $('#btnSave span').text("Save Ranges");
}

function openEditModal(parameterId, methodId) {
    rangeSaveInProgress = false;
    $('#btnSave').prop('disabled', false);
    rangeMode = "Edit";
    $('#header').html("Edit Reference Ranges");
    $('#parametername').val(parameterId).trigger('change');
    $('#MethodId').val(methodId).trigger('change');
    $('#parametername').prop('disabled', true);
    $('#MethodId').prop('disabled', true);
    clearRowInputs();
    rangeRows = [];
    renderRowsTable();
    $('#btnSave span').text("Update Ranges");

    $.ajax({
        url: "/ReferenceRange/ListByParameterMethod",
        type: "GET",
        data: { ParameterId: parameterId, MethodId: methodId },
        dataType: "json",
        success: function (result) {
            rangeRows = result.map(function (r) {
                return {
                    Gender: r.Gender,
                    AgeFromValue: r.AgeFromValue,
                    AgeFromUnit: r.AgeFromUnit,
                    AgeToValue: r.AgeToValue,
                    AgeToUnit: r.AgeToUnit,
                    AgeFromDays: r.AgeFromDays,
                    AgeToDays: r.AgeToDays,
                    NormalMin: r.NormalMin,
                    NormalMax: r.NormalMax,
                    CriticalMin: r.CriticalMin,
                    CriticalMax: r.CriticalMax,
                    RangeText: r.RangeText || '',
                    Active: r.Active
                };
            });
            renderRowsTable();
        },
        error: function (errormessage) {
            toastr.error('Something Wrong!');
        }
    });
}
// #endregion

// #region Add Row
function addRow() {
    var paramId = $('#parametername').val();
    if (!paramId) {
        toastr.error("Parameter is required");
        return false;
    }
    var gender = $('#Gender').val();
    if (!gender) {
        toastr.error("Gender is required");
        return false;
    }
    var ageFromValue = $('#AgeFromValue').val();
    var ageToValue = $('#AgeToValue').val();
    if (ageFromValue === "" || ageToValue === "") {
        toastr.error("Age From and Age To are required");
        return false;
    }
    var ageFromUnit = $('#AgeFromUnit').val();
    var ageToUnit = $('#AgeToUnit').val();
    var ageFromDaysValue = ageBoundaryToStorageDays(ageFromValue, ageFromUnit, true);
    var ageToDaysValue = ageBoundaryToStorageDays(ageToValue, ageToUnit, false);
    if (ageFromDaysValue > ageToDaysValue) {
        toastr.error("Age From must be less than or equal to Age To");
        return false;
    }

    var allowRange = getAllowRange();
    var allowCritical = getAllowCriticalRange();

    var normalMin = $('#NormalMin').val();
    var normalMax = $('#NormalMax').val();
    var rangeText = ($('#RangeText').val() || '').trim();
    var hasNumeric = normalMin !== "" || normalMax !== "";

    if (allowRange && hasNumeric && (normalMin === "" || normalMax === "")) {
        toastr.error("Both Normal Min and Normal Max are required when numeric range is used");
        return false;
    }
    if (allowRange && normalMin !== "" && normalMax !== "" && parseFloat(normalMin) >= parseFloat(normalMax)) {
        toastr.error("Normal Min must be less than Normal Max");
        return false;
    }
    if (!allowRange) {
        normalMin = "";
        normalMax = "";
        hasNumeric = false;
    }
    if (!hasNumeric && rangeText === "") {
        toastr.error("Provide either numeric normal range or descriptive range text");
        return false;
    }
    if (rangeText.length > 500) {
        toastr.error("Reference Range Text cannot exceed 500 characters");
        return false;
    }

    var criticalMin = $('#CriticalMin').val();
    var criticalMax = $('#CriticalMax').val();
    if (allowCritical && (criticalMin === "" || criticalMax === "")) {
        toastr.error("Critical range is required");
        return false;
    }
    if (!allowCritical) {
        criticalMin = "";
        criticalMax = "";
    }

    if (isOverlapping(gender, ageFromDaysValue, ageToDaysValue)) {
        toastr.error("Overlapping age range for the same gender is not allowed");
        return false;
    }

    rangeRows.push({
        Gender: gender,
        AgeFromValue: ageFromValue,
        AgeFromUnit: ageFromUnit,
        AgeToValue: ageToValue,
        AgeToUnit: ageToUnit,
        AgeFromDays: ageFromDaysValue,
        AgeToDays: ageToDaysValue,
        NormalMin: normalMin,
        NormalMax: normalMax,
        CriticalMin: criticalMin,
        CriticalMax: criticalMax,
        RangeText: rangeText,
        Active: $('#active1').prop('checked')
    });

    renderRowsTable();
    clearRowInputs();
    return false;
}

function isOverlapping(gender, fromDays, toDays) {
    for (var i = 0; i < rangeRows.length; i++) {
        var r = rangeRows[i];
        if (!gendersOverlap(r.Gender, gender)) {
            continue;
        }
        if (fromDays <= r.AgeToDays && r.AgeFromDays <= toDays) {
            return true;
        }
    }
    return false;
}

function gendersOverlap(existingGender, newGender) {
    if (existingGender === newGender) return true;
    if (existingGender === "Both" || newGender === "Both") return true;
    return false;
}
// #endregion

// #region Save
function SaveBatch() {
    if (rangeSaveInProgress) {
        return false;
    }

    var paramId = $('#parametername').val();
    if (!paramId) {
        toastr.error("Parameter is required");
        return false;
    }

    var methodId = $('#MethodId').val();
    if (!methodId) {
        toastr.error("Method is required");
        return false;
    }
    var comboKey = paramId + "|" + methodId;
    if (rangeMode === "Add" && comboIndex[comboKey]) {
        toastr.error("Ranges already exist for this Parameter and Method. Please edit the existing ranges.");
        return false;
    }

    if (rangeRows.length === 0) {
        toastr.error("Add at least one range");
        return false;
    }

    var payload = {
        ParameterId: parseInt(paramId),
        MethodId: parseInt(methodId),
        Mode: rangeMode,
        Ranges: rangeRows
    };

    var gapWarnings = buildGapWarnings(rangeRows);
    if (gapWarnings.length > 0) {
        pendingSavePayload = payload;
        showGapWarningModal(gapWarnings);
        return false;
    }

    executeSaveBatch(payload);
    return false;
}
// #endregion

function executeSaveBatch(payload) {
    if (!payload) {
        return false;
    }

    if (rangeSaveInProgress) {
        return false;
    }
    rangeSaveInProgress = true;
    $('#btnSave').prop('disabled', true);

    $.ajax({
        url: "/ReferenceRange/SaveBatch",
        data: JSON.stringify(payload),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#range_modal').modal('hide');
                $('#combosTable').DataTable().ajax.reload();
            }
            else {
                toastr.error(result.Item2);
            }
        },
        error: function (errormessage) {
            toastr.error('Something Wrong!');
        },
        complete: function () {
            rangeSaveInProgress = false;
            $('#btnSave').prop('disabled', false);
        }
    });
}

function proceedSaveAfterGapWarning() {
    $('#gap_warning_modal').modal('hide');
    var payload = pendingSavePayload;
    pendingSavePayload = null;
    executeSaveBatch(payload);
    return false;
}

function clearGapWarningAndClose() {
    pendingSavePayload = null;
    return false;
}

function buildGapWarnings(rows) {
    var warnings = [];
    var maleGaps = findGapsForGender(rows, "Male");
    var femaleGaps = findGapsForGender(rows, "Female");

    if (maleGaps.length > 0) {
        warnings.push("Potential age gaps found for Male: " + formatGapList(maleGaps) + ". Save will continue.");
    }
    if (femaleGaps.length > 0) {
        warnings.push("Potential age gaps found for Female: " + formatGapList(femaleGaps) + ". Save will continue.");
    }

    return warnings;
}

function findGapsForGender(rows, gender) {
    var intervals = [];
    for (var i = 0; i < rows.length; i++) {
        var r = rows[i];
        if (!shouldCheckGapForRow(r)) {
            continue;
        }
        if (r.Gender === gender || r.Gender === "Both") {
            var fromMonths = toMonthsValue(r.AgeFromValue, r.AgeFromUnit);
            var toMonths = toMonthsValue(r.AgeToValue, r.AgeToUnit);
            if (fromMonths === null || toMonths === null) {
                continue;
            }
            intervals.push({
                fromMonths: fromMonths,
                toMonths: toMonths,
                rowNo: i + 1
            });
        }
    }

    if (intervals.length <= 1) {
        return [];
    }

    intervals.sort(function (a, b) {
        if (a.fromMonths !== b.fromMonths) return a.fromMonths - b.fromMonths;
        return a.toMonths - b.toMonths;
    });

    var gaps = [];
    var currentTo = intervals[0].toMonths;
    var currentEndRowNo = intervals[0].rowNo;
    var eps = 0.000001;
    for (var j = 1; j < intervals.length; j++) {
        if (intervals[j].fromMonths > currentTo + eps) {
            gaps.push({
                fromMonths: currentTo,
                toMonths: intervals[j].fromMonths,
                leftRowNo: currentEndRowNo,
                rightRowNo: intervals[j].rowNo
            });
            currentTo = intervals[j].toMonths;
            currentEndRowNo = intervals[j].rowNo;
            continue;
        }
        if (intervals[j].toMonths > currentTo) {
            currentTo = intervals[j].toMonths;
            currentEndRowNo = intervals[j].rowNo;
        }
    }

    return gaps;
}

function formatGapList(gaps) {
    var parts = [];
    for (var i = 0; i < gaps.length; i++) {
        var part = formatMonthsLabel(gaps[i].fromMonths) + " to " + formatMonthsLabel(gaps[i].toMonths);
        if (gaps[i].leftRowNo && gaps[i].rightRowNo) {
            part += " (between rows " + gaps[i].leftRowNo + " and " + gaps[i].rightRowNo + ")";
        }
        parts.push(part);
        if (parts.length === 3 && gaps.length > 3) {
            parts.push("...");
            break;
        }
    }
    return parts.join(", ");
}

function shouldCheckGapForRow(row) {
    var fromUnit = (row.AgeFromUnit || "").toLowerCase();
    var toUnit = (row.AgeToUnit || "").toLowerCase();
    return (fromUnit === "months" || fromUnit === "years") && (toUnit === "months" || toUnit === "years");
}

function showGapWarningModal(warnings) {
    var list = $('#gapWarningList');
    list.empty();
    for (var i = 0; i < warnings.length; i++) {
        list.append('<li>' + warnings[i] + '</li>');
    }
    $('#gap_warning_modal').modal('show');
}

function toMonthsValue(value, unit) {
    var v = parseFloat(value);
    if (isNaN(v)) {
        return null;
    }

    var normalized = (unit || "").toLowerCase();
    if (normalized === "months") {
        return v;
    }
    if (normalized === "years") {
        return v * 12;
    }

    return null;
}

function formatMonthsLabel(months) {
    var rounded = Math.round(months * 100) / 100;
    var years = rounded / 12;
    if (Math.abs(years - Math.round(years)) < 0.000001) {
        var y = Math.round(years);
        return y + " year" + (y === 1 ? "" : "s");
    }

    if (Math.abs(rounded - Math.round(rounded)) < 0.000001) {
        var m = Math.round(rounded);
        return m + " month" + (m === 1 ? "" : "s");
    }

    return rounded + " months";
}

// #region Render Rows
function renderRowsTable() {
    var tbody = $('#rangeRowsTable tbody');
    tbody.empty();
    if (rangeRows.length === 0) {
        tbody.append('<tr><td colspan="10" class="text-center">No rows added</td></tr>');
        return;
    }

    rangeRows.sort(function (a, b) {
        var fromDiff = (a.AgeFromDays || 0) - (b.AgeFromDays || 0);
        if (fromDiff !== 0) {
            return fromDiff;
        }
        return (a.AgeToDays || 0) - (b.AgeToDays || 0);
    });

    rangeRows.forEach(function (r, idx) {
        var row = '<tr>' +
            '<td>' + r.AgeFromValue + ' ' + r.AgeFromUnit + '</td>' +
            '<td>' + r.AgeToValue + ' ' + r.AgeToUnit + '</td>' +
            '<td>' + r.Gender + '</td>' +
            '<td>' + (r.NormalMin == null || r.NormalMin === '' ? '-' : r.NormalMin) + '</td>' +
            '<td>' + (r.NormalMax == null || r.NormalMax === '' ? '-' : r.NormalMax) + '</td>' +
            '<td>' + (r.CriticalMin == null || r.CriticalMin === '' ? '-' : r.CriticalMin) + '</td>' +
            '<td>' + (r.CriticalMax == null || r.CriticalMax === '' ? '-' : r.CriticalMax) + '</td>' +
            '<td>' + htmlEncode(r.RangeText || '') + '</td>' +
            '<td>' + (r.Active ? 'Yes' : 'No') + '</td>' +
            '<td><a href="#" onclick="return removeRow(' + idx + ');">Remove</a></td>' +
            '</tr>';
        tbody.append(row);
    });
}

function removeRow(index) {
    rangeRows.splice(index, 1);
    renderRowsTable();
    return false;
}
// #endregion

// #region Helpers
function clearRowInputs() {
    $('#Gender').val("");
    $('#AgeFromValue').val("");
    $('#AgeFromUnit').val("Years");
    $('#AgeToValue').val("");
    $('#AgeToUnit').val("Years");
    $('#NormalMin').val("");
    $('#NormalMax').val("");
    $('#CriticalMin').val("");
    $('#CriticalMax').val("");
    $('#RangeText').val("");
    $("#active1").prop('checked', true);
}

function getAllowRange() {
    var paramId = $('#parametername').val();
    var p = parametersIndex[paramId];
    return p ? p.AllowRange : true;
}

function getAllowCriticalRange() {
    var paramId = $('#parametername').val();
    var p = parametersIndex[paramId];
    return p ? p.AllowCriticalRange : true;
}

function applyRangeFieldVisibility() {
    var allowRange = getAllowRange();
    var allowCritical = getAllowCriticalRange();
    if (allowRange) {
        $('.normal-range-group').show();
    } else {
        $('.normal-range-group').hide();
        $('#NormalMin').val("");
        $('#NormalMax').val("");
    }
    if (allowCritical) {
        $('.critical-range-group').show();
    } else {
        $('.critical-range-group').hide();
        $('#CriticalMin').val("");
        $('#CriticalMax').val("");
    }
}

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}
// #endregion

// #region Combos Table
$(() => {
    loadCombosTable();
    loadParameters();
    loadMethods();
    $('#gap_warning_modal').on('hidden.bs.modal', function () {
        $('#gapWarningList').empty();
    });
});

function loadCombosTable() {
    var a = $("#combosTable").DataTable({
        order: [],
        ajax: {
            url: '/ReferenceRange/ComboList',
            method: "GET",
            dataSrc: function (json) {
                comboIndex = {};
                json.forEach(function (c) {
                    var key = c.ParameterId + "|" + c.MethodId;
                    comboIndex[key] = true;
                });
                return json;
            }
        },
        columns: [
            { data: 'ParameterId' },
            { data: 'ParameterName' },
            {
                data: 'MethodName',
                render: function (data, type, row) {
                    return data && data.length ? data : 'Default';
                }
            },
            { data: 'RangeCount' },
            {
                data: 'ActiveCount',
                render: function (data, type, row) {
                    return data > 0 ? 'Yes' : 'No';
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    return '<a href="#" data-toggle="modal" data-target="#range_modal" onclick="return openEditModal(' + row.ParameterId + ', ' + row.MethodId + ');">Edit</a>';
                }
            }
        ],
        responsive: true,
        columnDefs: [{
            searchable: false,
            orderable: false,
            targets: 0
        },
            { responsivePriority: 1, targets: 1 },
            { responsivePriority: 2, targets: -1 }
        ],
    });
    a.on('order.dt search.dt', function () {
        a.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();
}
// #endregion

// #region Parameter Dropdowns
function loadParameters() {
    $.ajax({
        url: '/ParameterMaster/List',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            parametersIndex = {};
            const formattedData = data.map(item => {
                parametersIndex[item.Id] = item;
                return {
                    id: item.Id,
                    text: item.ParameterName,
                };
            });
            $(".parametersearch").select2({
                data: formattedData,
                width: "100%",
                placeholder: 'Select Parameter'
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error fetching data:', textStatus, errorThrown);
        },
        dropdownParent: $("#range_modal")
    });

    $('#parametername').on('change', function () {
        applyRangeFieldVisibility();
    });
}
// #endregion

// #region Method Dropdown
function loadMethods() {
    $.ajax({
        url: '/TestMethod/List',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            methodIndex = {};
            const formattedData = data.map(item => {
                methodIndex[item.Id] = item;
                return {
                    id: item.Id,
                    text: item.MethodName,
                };
            });
            $("#MethodId").select2({
                data: formattedData,
                width: "100%",
                placeholder: 'Select Method'
            });
            var defId = getDefaultMethodId();
            if (defId) {
                $("#MethodId").val(defId).trigger('change');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error fetching data:', textStatus, errorThrown);
        },
        dropdownParent: $("#range_modal")
    });
}

function getDefaultMethodId() {
    var ids = Object.keys(methodIndex);
    for (var i = 0; i < ids.length; i++) {
        var id = ids[i];
        if (methodIndex[id] && methodIndex[id].MethodName === "None") {
            return id;
        }
    }
    return "";
}
// #endregion

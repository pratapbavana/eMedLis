var templateItems = [];
var headersIndex = {};
var rangeCombos = [];
var methodOptionsByParameter = {};
var currentParameterMethodOptions = [];
var isDirty = false;
var currentInvestigationId = "";
var editingRowIndex = -1;

$(() => {
    loadInvestigations();
    loadHeaders();
    loadParameters();
    loadRangeCombos();
    bindParameterMethodLoader();
    bindInterpretationEditor();
    $('#templateEditModal').on('hidden.bs.modal', function () {
        editingRowIndex = -1;
        $('#editParameterName').val('');
        $('#editMethodSelect').empty();
    });
});

function loadInvestigations() {
    $.ajax({
        url: '/Investigation/List',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            const formattedData = data.map(item => ({
                id: item.Id,
                text: item.InvName
            }));
            $(".investigationsearch").select2({
                data: formattedData,
                width: "100%",
                placeholder: 'Select Investigation'
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error fetching data:', textStatus, errorThrown);
        }
    });

    $('#investigationSelect').on('change', function () {
        var nextId = $('#investigationSelect').val();
        if (isDirty) {
            var proceed = confirm("You have unsaved changes. Do you want to discard them?");
            if (!proceed) {
                $('#investigationSelect').val(currentInvestigationId).trigger('change.select2');
                return;
            }
        }
        currentInvestigationId = nextId;
        loadTemplate();
    });
}

function loadHeaders() {
    $.ajax({
        url: '/ParameterHeader/List',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            headersIndex = {};
            const formattedData = data.map(item => {
                headersIndex[item.Id] = item;
                return {
                    id: item.Id,
                    text: item.HeaderName
                };
            });
            $(".headersearch").select2({
                data: formattedData,
                width: "100%",
                placeholder: 'Select Header'
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error fetching data:', textStatus, errorThrown);
        }
    });
}

function loadParameters() {
    $.ajax({
        url: '/ParameterMaster/List',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            const formattedData = data.map(item => ({
                id: item.Id,
                text: item.ParameterName
            }));
            $(".parametersearch").select2({
                data: formattedData,
                width: "100%",
                placeholder: 'Select Parameter'
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error fetching data:', textStatus, errorThrown);
        }
    });
}

function loadRangeCombos() {
    $.ajax({
        url: '/ReferenceRange/ComboList',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            rangeCombos = data || [];
            methodOptionsByParameter = {};
            rangeCombos.forEach(function (item) {
                var parameterId = parseInt(item.ParameterId || 0);
                var methodId = parseInt(item.MethodId || 0);
                if (!parameterId || !methodId) return;
                if (!methodOptionsByParameter[parameterId]) {
                    methodOptionsByParameter[parameterId] = {};
                }
                methodOptionsByParameter[parameterId][methodId] = item.MethodName || "";
            });
            resetMethodSelect();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error fetching data:', textStatus, errorThrown);
        }
    });
}

function bindParameterMethodLoader() {
    $('#parameterSelect').on('change', function () {
        var parameterId = parseInt($('#parameterSelect').val() || '0');
        loadMethodsForParameter(parameterId);
    });
}

function loadMethodsForParameter(parameterId) {
    var methodMap = methodOptionsByParameter[parameterId] || {};
    currentParameterMethodOptions = Object.keys(methodMap).map(function (k) {
        return { id: parseInt(k), text: methodMap[k] };
    });

    currentParameterMethodOptions.sort(function (a, b) {
        return a.text.localeCompare(b.text);
    });

    var options = [{ id: "", text: currentParameterMethodOptions.length > 0 ? "No Method (Optional)" : "No method mapped in ranges (Optional)" }];
    currentParameterMethodOptions.forEach(function (m) {
        options.push({ id: m.id, text: m.text });
    });

    $("#methodSelect").empty().select2({
        data: options,
        width: "100%",
        placeholder: "Method (Optional)"
    });

    if (currentParameterMethodOptions.length === 0) {
        $("#methodSelect").val("").trigger('change');
    }
}

function resetMethodSelect() {
    $("#methodSelect").empty().select2({
        data: [{ id: "", text: "Select Parameter First" }],
        width: "100%",
        placeholder: "Select Parameter First"
    });
}

function loadTemplate() {
    var investigationId = $('#investigationSelect').val();
    if (!investigationId) {
        templateItems = [];
        renderTemplateTable();
        setInterpretationHtml("", false);
        isDirty = false;
        return;
    }
    $.ajax({
        url: '/InvestigationTemplate/List',
        type: 'GET',
        dataType: 'json',
        data: { InvestigationId: investigationId },
        success: function (data) {
            templateItems = data.map(function (d) {
                return {
                    ItemType: d.ItemType,
                    HeaderId: d.HeaderId,
                    HeaderName: d.HeaderName,
                    ParameterId: d.ParameterId,
                    ParameterName: d.ParameterName,
                    MethodId: d.MethodId,
                    MethodName: d.MethodName,
                    DisplayOrder: d.DisplayOrder,
                    Active: d.Active
                };
            });
            renderTemplateTable();
            loadInterpretation(investigationId, function () {
                isDirty = false;
            });
        },
        error: function () {
            toastr.error('Something Wrong!');
        }
    });
}

function addHeader() {
    var headerId = $('#headerSelect').val();
    if (!headerId) {
        toastr.error('Header is required');
        return false;
    }
    var headerText = $('#headerSelect').select2('data')[0].text;
    templateItems.push({
        ItemType: "Header",
        HeaderId: parseInt(headerId),
        HeaderName: headerText,
        ParameterId: null,
        ParameterName: "",
        DisplayOrder: templateItems.length + 1,
        Active: true
    });
    renderTemplateTable();
    isDirty = true;
    return false;
}

function addParameter() {
    var parameterId = $('#parameterSelect').val();
    if (!parameterId) {
        toastr.error('Parameter is required');
        return false;
    }
    var methodId = $('#methodSelect').val();
    var parameterText = $('#parameterSelect').select2('data')[0].text;
    var methodText = methodId ? $('#methodSelect').select2('data')[0].text : "";
    templateItems.push({
        ItemType: "Parameter",
        HeaderId: null,
        HeaderName: "",
        ParameterId: parseInt(parameterId),
        ParameterName: parameterText,
        MethodId: methodId ? parseInt(methodId) : null,
        MethodName: methodText,
        DisplayOrder: templateItems.length + 1,
        Active: true
    });
    renderTemplateTable();
    isDirty = true;
    return false;
}

function renderTemplateTable() {
    var tbody = $('#templateTable tbody');
    tbody.empty();
    if (templateItems.length === 0) {
        tbody.append('<tr><td colspan="6" class="text-center">No template items</td></tr>');
        return;
    }
    templateItems.forEach(function (item, idx) {
        var editAction = item.ItemType === "Parameter"
            ? '<a href="#" onclick="return openEditRow(' + idx + ');">Edit</a> | '
            : '';
        var row = '<tr>' +
            '<td>' + (idx + 1) + '</td>' +
            '<td>' + (item.HeaderName || '-') + '</td>' +
            '<td>' + (item.ParameterName || '-') + '</td>' +
            '<td>' + (item.MethodName || '-') + '</td>' +
            '<td>' + item.ItemType + '</td>' +
            '<td>' +
            editAction +
            '<a href="#" onclick="return moveTo(' + idx + ');">Move To</a> | ' +
            '<a href="#" onclick="return moveUp(' + idx + ');">Up</a> | ' +
            '<a href="#" onclick="return moveDown(' + idx + ');">Down</a> | ' +
            '<a href="#" onclick="return removeItem(' + idx + ');">Remove</a>' +
            '</td>' +
            '</tr>';
        tbody.append(row);
    });
}

function moveUp(index) {
    if (index <= 0) return false;
    var temp = templateItems[index - 1];
    templateItems[index - 1] = templateItems[index];
    templateItems[index] = temp;
    renderTemplateTable();
    isDirty = true;
    return false;
}

function moveDown(index) {
    if (index >= templateItems.length - 1) return false;
    var temp = templateItems[index + 1];
    templateItems[index + 1] = templateItems[index];
    templateItems[index] = temp;
    renderTemplateTable();
    isDirty = true;
    return false;
}

function removeItem(index) {
    templateItems.splice(index, 1);
    renderTemplateTable();
    isDirty = true;
    return false;
}

function openEditRow(index) {
    if (index < 0 || index >= templateItems.length) {
        return false;
    }

    var item = templateItems[index];
    if (item.ItemType !== "Parameter") {
        toastr.info('Only parameter rows are editable.');
        return false;
    }

    editingRowIndex = index;
    $('#editParameterName').val(item.ParameterName || "");

    var methods = getMethodOptionsForParameter(item.ParameterId);
    var options = [{ id: "", text: methods.length > 0 ? "No Method (Optional)" : "No method mapped in ranges (Optional)" }];
    methods.forEach(function (m) {
        options.push({ id: m.id, text: m.text });
    });

    var currentMethodId = item.MethodId ? parseInt(item.MethodId) : "";
    var currentMethodName = item.MethodName || "";
    var exists = methods.some(function (m) { return parseInt(m.id) === parseInt(currentMethodId || 0); });
    if (currentMethodId && !exists) {
        options.push({ id: currentMethodId, text: currentMethodName + " (Current)" });
    }

    $("#editMethodSelect").empty().select2({
        data: options,
        width: "100%",
        dropdownParent: $('#templateEditModal')
    });
    $("#editMethodSelect").val(currentMethodId).trigger('change');
    $('#templateEditModal').modal('show');
    return false;
}

function applyRowEdit() {
    if (editingRowIndex < 0 || editingRowIndex >= templateItems.length) {
        return false;
    }

    var item = templateItems[editingRowIndex];
    if (item.ItemType !== "Parameter") {
        return false;
    }

    var methodId = $('#editMethodSelect').val();
    var methods = getMethodOptionsForParameter(item.ParameterId);
    var selected = $('#editMethodSelect').select2('data');
    item.MethodId = methodId ? parseInt(methodId) : null;
    item.MethodName = methodId && selected && selected.length > 0 ? selected[0].text.replace(" (Current)", "") : "";

    $('#templateEditModal').modal('hide');
    renderTemplateTable();
    isDirty = true;
    return false;
}

function moveTo(index) {
    if (index < 0 || index >= templateItems.length) {
        return false;
    }

    var input = prompt("Move row " + (index + 1) + " to position (1-" + templateItems.length + "):", (index + 1).toString());
    if (input === null) {
        return false;
    }

    var target = parseInt(input, 10);
    if (isNaN(target) || target < 1 || target > templateItems.length) {
        toastr.error("Invalid position");
        return false;
    }

    var toIndex = target - 1;
    if (toIndex === index) {
        return false;
    }

    var movedItem = templateItems.splice(index, 1)[0];
    templateItems.splice(toIndex, 0, movedItem);
    renderTemplateTable();
    isDirty = true;
    return false;
}

function getMethodOptionsForParameter(parameterId) {
    var methodMap = methodOptionsByParameter[parseInt(parameterId || 0)] || {};
    var methods = Object.keys(methodMap).map(function (k) {
        return { id: parseInt(k), text: methodMap[k] };
    });
    methods.sort(function (a, b) {
        return a.text.localeCompare(b.text);
    });
    return methods;
}

function saveTemplate() {
    var investigationId = $('#investigationSelect').val();
    if (!investigationId) {
        toastr.error('Investigation is required');
        return false;
    }
    if (templateItems.length === 0) {
        toastr.error('Add at least one header or parameter');
        return false;
    }

    var payload = {
        InvestigationId: investigationId,
        InterpretationHtml: getInterpretationHtml(),
        Items: templateItems.map(function (item, idx) {
            return {
                ItemType: item.ItemType,
                HeaderId: item.HeaderId,
                ParameterId: item.ParameterId,
                MethodId: item.ItemType === "Parameter" ? item.MethodId : null,
                DisplayOrder: idx + 1,
                Active: true
            };
        })
    };

    $.ajax({
        url: '/InvestigationTemplate/Save',
        type: 'POST',
        data: JSON.stringify(payload),
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                loadTemplate();
                isDirty = false;
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

function bindInterpretationEditor() {
    $('#interpretationEditor').on('input keyup paste', function () {
        isDirty = true;
    });
}

function loadInterpretation(investigationId, done) {
    $.ajax({
        url: '/InvestigationTemplate/GetInterpretation',
        type: 'GET',
        dataType: 'json',
        data: { InvestigationId: investigationId },
        success: function (result) {
            var html = result && result.InterpretationHtml ? result.InterpretationHtml : "";
            setInterpretationHtml(html, false);
            if (typeof done === 'function') done();
        },
        error: function () {
            setInterpretationHtml("", false);
            if (typeof done === 'function') done();
        }
    });
}

function setInterpretationHtml(html, markDirty) {
    $('#interpretationEditor').html(html || "");
    if (markDirty === true) {
        isDirty = true;
    }
}

function getInterpretationHtml() {
    return $('#interpretationEditor').html() || "";
}

function interpretationCommand(command, value) {
    var editor = document.getElementById('interpretationEditor');
    if (!editor) {
        return false;
    }
    editor.focus();
    document.execCommand(command, false, value || null);
    isDirty = true;
    return false;
}

function interpretationApplyFont(fontName) {
    if (!fontName) {
        return false;
    }
    return interpretationCommand('fontName', fontName);
}

function interpretationApplySize(sizeValue) {
    if (!sizeValue) {
        return false;
    }
    return interpretationCommand('fontSize', sizeValue);
}

function interpretationApplyBlock(blockName) {
    if (!blockName) {
        return false;
    }
    return interpretationCommand('formatBlock', '<' + blockName + '>');
}

function interpretationCreateLink() {
    var url = prompt('Enter URL', 'https://');
    if (!url) {
        return false;
    }
    return interpretationCommand('createLink', url);
}

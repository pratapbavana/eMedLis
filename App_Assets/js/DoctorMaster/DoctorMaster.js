$(document).ready(function () {
    loadUserOptions();
    loadSubDepartmentOptions();
    loadDoctorTable();

    $('#SignatureFile').on('change', function (e) {
        readSignatureFile(e);
    });
});

function loadDoctorTable() {
    var table = $("#doctorTable").DataTable({
        destroy: true,
        order: [],
        ajax: {
            url: '/DoctorMaster/List',
            method: 'GET',
            dataSrc: function (json) {
                return json || [];
            }
        },
        columns: [
            { data: 'Id' },
            { data: 'Id' },
            {
                data: null,
                render: function (data, type, row) {
                    var fullName = row.FullName || '';
                    var userName = row.UserName || '';
                    return htmlEncode(fullName || userName) + (fullName && userName ? ' <small class="text-muted">(' + htmlEncode(userName) + ')</small>' : '');
                }
            },
            { data: 'Designation', defaultContent: '-' },
            { data: 'RegistrationNumber', defaultContent: '-' },
            { data: 'SubDepartments', defaultContent: '-' },
            {
                data: 'HasSignature',
                render: function (data, type, row) {
                    return data
                        ? '<a href="/DoctorMaster/Signature/' + row.Id + '" target="_blank">View</a>'
                        : '<span class="text-muted">No</span>';
                }
            },
            {
                data: 'Active',
                render: function (data) {
                    return data
                        ? '<span class="badge badge-pill badge-outline-success">ACTIVE</span>'
                        : '<span class="badge badge-pill badge-outline-danger">INACTIVE</span>';
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    var toggleText = row.Active ? 'Deactivate' : 'Activate';
                    var toggleActive = row.Active ? 'false' : 'true';
                    return '<a href="#" data-toggle="modal" data-target="#doctor_modal" onclick="return editDoctor(' + row.Id + ');">Edit</a> | ' +
                        '<a href="#" onclick="return setDoctorActive(' + row.Id + ',' + toggleActive + ');">' + toggleText + '</a>';
                }
            }
        ],
        responsive: true,
        columnDefs: [
            { searchable: false, orderable: false, targets: 0 },
            { responsivePriority: 1, targets: 2 },
            { responsivePriority: 2, targets: -1 }
        ]
    });

    table.on('order.dt search.dt', function () {
        table.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();
}

function loadUserOptions() {
    $.ajax({
        url: '/DoctorMaster/Users',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            var ddl = $('#UserId');
            ddl.empty();
            ddl.append('<option value="">Select User</option>');

            (data || []).forEach(function (item) {
                var text = (item.FullName || item.UserName || '') + ((item.FullName && item.UserName) ? ' (' + item.UserName + ')' : '');
                ddl.append('<option value="' + item.UserId + '">' + htmlEncode(text) + '</option>');
            });

            initUserSelect();
        },
        error: function () {
            toastr.error('Unable to load users');
        }
    });
}

function loadSubDepartmentOptions() {
    $.ajax({
        url: '/SubDepartment/List',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            var ddl = $('#SubDepartmentIds');
            ddl.empty();

            (data || []).forEach(function (item) {
                ddl.append('<option value="' + item.Id + '">' + htmlEncode(item.SubDeptName) + '</option>');
            });

            initSubDepartmentSelect();
        },
        error: function () {
            toastr.error('Unable to load sub departments');
        }
    });
}

function initUserSelect() {
    if (!$.fn.select2) return;

    var ddl = $('#UserId');
    if (ddl.hasClass('select2-hidden-accessible')) {
        ddl.select2('destroy');
    }

    ddl.select2({
        width: '100%',
        dropdownParent: $('#doctor_modal'),
        placeholder: 'Select User',
        allowClear: true
    });
}

function initSubDepartmentSelect() {
    if (!$.fn.select2) return;

    var ddl = $('#SubDepartmentIds');
    if (ddl.hasClass('select2-hidden-accessible')) {
        ddl.select2('destroy');
    }

    ddl.select2({
        width: '100%',
        dropdownParent: $('#doctor_modal'),
        placeholder: 'Select Sub Departments'
    });
}

function clearDoctorForm() {
    $('#doctorModalHeader').text('Add Doctor');
    $('#DoctorId').val(0);
    $('#UserId').val('').trigger('change');
    $('#Designation').val('');
    $('#RegistrationNumber').val('');
    $('#SubDepartmentIds').val([]).trigger('change');
    $('#DoctorActive').prop('checked', true);
    $('#SignatureFile').val('');
    $('#SignatureBase64').val('');
    setSignaturePreview('', false);
    $('#btnDoctorAdd').removeClass('d-none');
    $('#btnDoctorUpdate').addClass('d-none');
}

function editDoctor(id) {
    clearDoctorForm();
    $('#doctorModalHeader').text('Edit Doctor');

    $.ajax({
        url: '/DoctorMaster/GetbyID/' + id,
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            if (!data || !data.length) {
                toastr.error('Doctor details not found');
                return;
            }

            var row = data[0];
            $('#DoctorId').val(row.Id || 0);
            $('#UserId').val((row.UserId || '').toString()).trigger('change');
            $('#Designation').val(row.Designation || '');
            $('#RegistrationNumber').val(row.RegistrationNumber || '');
            $('#DoctorActive').prop('checked', row.Active === true);

            var subIds = [];
            if (row.SubDepartmentIds) {
                subIds = row.SubDepartmentIds.split(',').map(function (x) { return x.trim(); });
            }
            $('#SubDepartmentIds').val(subIds).trigger('change');

            if (row.HasSignature) {
                setSignaturePreview('/DoctorMaster/Signature/' + row.Id + '?_=' + new Date().getTime(), true);
            } else {
                setSignaturePreview('', false);
            }

            $('#btnDoctorAdd').addClass('d-none');
            $('#btnDoctorUpdate').removeClass('d-none');
        },
        error: function () {
            toastr.error('Failed to load doctor details');
        }
    });
    return false;
}

function saveDoctor(mode) {
    var payload = buildDoctorPayload();
    if (!payload) {
        return false;
    }

    var url = mode === 'Update' ? '/DoctorMaster/Update' : '/DoctorMaster/Add';
    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json;charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(payload),
        success: function (result) {
            var statusCode = result ? parseInt(result.Item1, 10) : 0;
            if (statusCode === 1) {
                toastr.success(result.Item2 || 'Saved');
                $('#doctor_modal').modal('hide');
                $('#doctorTable').DataTable().ajax.reload();
                clearDoctorForm();
                loadUserOptions();
            } else {
                toastr.error(result && result.Item2 ? result.Item2 : 'Save failed');
            }
        },
        error: function () {
            toastr.error('Save failed');
        }
    });

    return false;
}

function setDoctorActive(id, active) {
    var msg = active ? 'Activate this doctor?' : 'Deactivate this doctor?';
    if (!confirm(msg)) {
        return false;
    }

    $.ajax({
        url: '/DoctorMaster/SetActive',
        type: 'POST',
        dataType: 'json',
        data: { Id: id, Active: active },
        success: function (result) {
            var statusCode = result ? parseInt(result.Item1, 10) : 0;
            if (statusCode === 1) {
                toastr.success(result.Item2 || 'Updated');
                $('#doctorTable').DataTable().ajax.reload();
            } else {
                toastr.error(result && result.Item2 ? result.Item2 : 'Update failed');
            }
        },
        error: function () {
            toastr.error('Update failed');
        }
    });

    return false;
}

function buildDoctorPayload() {
    var id = parseInt($('#DoctorId').val() || '0');
    var userId = parseInt($('#UserId').val() || '0');
    var designation = ($('#Designation').val() || '').trim();
    var regNo = ($('#RegistrationNumber').val() || '').trim();
    var subIds = $('#SubDepartmentIds').val() || [];
    var active = $('#DoctorActive').prop('checked');
    var signatureBase64 = $('#SignatureBase64').val() || '';

    if (!userId) {
        toastr.error('User is required');
        return null;
    }
    if (!subIds.length) {
        toastr.error('Select at least one sub department');
        return null;
    }

    return {
        Id: id,
        UserId: userId,
        Designation: designation,
        RegistrationNumber: regNo,
        SubDepartmentIds: subIds.join(','),
        SignatureBase64: signatureBase64,
        Active: active
    };
}

function readSignatureFile(e) {
    var file = (e.target.files || [])[0];
    if (!file) {
        return;
    }

    var allowed = ['image/png', 'image/jpeg', 'image/jpg'];
    if (allowed.indexOf((file.type || '').toLowerCase()) < 0) {
        toastr.error('Only PNG/JPG files are allowed');
        $('#SignatureFile').val('');
        $('#SignatureBase64').val('');
        return;
    }

    var reader = new FileReader();
    reader.onload = function (evt) {
        var dataUrl = evt.target.result || '';
        $('#SignatureBase64').val(dataUrl);
        setSignaturePreview(dataUrl, true);
    };
    reader.onerror = function () {
        toastr.error('Unable to read signature file');
        $('#SignatureFile').val('');
        $('#SignatureBase64').val('');
        setSignaturePreview('', false);
    };
    reader.readAsDataURL(file);
}

function setSignaturePreview(src, hasValue) {
    if (hasValue && src) {
        $('#SignaturePreview').attr('src', src).show();
        $('#NoSignatureText').hide();
    } else {
        $('#SignaturePreview').attr('src', '').hide();
        $('#NoSignatureText').show();
    }
}

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}

$(document).ready(function () {
    loadPackageTable();
    loadInvestigationOptions();
});

function loadPackageTable() {
    var table = $("#packageTable").DataTable({
        destroy: true,
        order: [],
        ajax: {
            url: '/PackageMaster/List',
            method: 'GET',
            dataSrc: function (json) { return json || []; }
        },
        columns: [
            { data: 'Id' },
            { data: 'Id' },
            { data: 'PackageCode', defaultContent: '-' },
            { data: 'PackageName' },
            {
                data: 'Price',
                render: function (x) { return parseFloat(x || 0).toFixed(2); }
            },
            {
                data: 'DiscountAmount',
                render: function (x) { return parseFloat(x || 0).toFixed(2); }
            },
            { data: 'Investigations', defaultContent: '-' },
            {
                data: 'Active',
                render: function (x) {
                    return x
                        ? '<span class="badge badge-pill badge-outline-success">ACTIVE</span>'
                        : '<span class="badge badge-pill badge-outline-danger">INACTIVE</span>';
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    var toggleText = row.Active ? 'Deactivate' : 'Activate';
                    var toggleActive = row.Active ? 'false' : 'true';
                    return '<a href="#" data-toggle="modal" data-target="#package_modal" onclick="return editPackage(' + row.Id + ');">Edit</a> | ' +
                        '<a href="#" onclick="return setPackageActive(' + row.Id + ',' + toggleActive + ');">' + toggleText + '</a>';
                }
            }
        ],
        responsive: true,
        columnDefs: [
            { searchable: false, orderable: false, targets: 0 },
            { searchable: false, orderable: false, targets: 1, visible: false },
            { responsivePriority: 1, targets: 3 },
            { responsivePriority: 2, targets: -1 }
        ]
    });

    table.on('order.dt search.dt', function () {
        table.column(0, { search: 'applied', order: 'applied' }).nodes().each(function (cell, i) {
            cell.innerHTML = i + 1;
        });
    }).draw();
}

function loadInvestigationOptions() {
    $.ajax({
        url: '/Investigation/List',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            var ddl = $('#PackageInvestigations');
            ddl.empty();
            (data || []).forEach(function (item) {
                ddl.append('<option value="' + item.Id + '">' + htmlEncode(item.InvName) + '</option>');
            });
            initInvestigationSelect();
        },
        error: function () {
            toastr.error('Unable to load investigations');
        }
    });
}

function initInvestigationSelect() {
    if (!$.fn.select2) return;
    var ddl = $('#PackageInvestigations');
    if (ddl.hasClass('select2-hidden-accessible')) {
        ddl.select2('destroy');
    }
    ddl.select2({
        width: '100%',
        dropdownParent: $('#package_modal'),
        placeholder: 'Select Investigations'
    });
}

function clearPackageForm() {
    $('#packageModalHeader').text('Add Package');
    $('#PackageId').val(0);
    $('#PackageCode').val('');
    $('#PackageName').val('');
    $('#ReportingName').val('');
    $('#PackagePrice').val('0');
    $('#PackageDiscount').val('0');
    $('#PackageDescription').val('');
    $('#PackageInvestigations').val([]).trigger('change');
    $('#PackageActive').prop('checked', true);
    $('#btnPackageAdd').removeClass('d-none');
    $('#btnPackageUpdate').addClass('d-none');
}

function editPackage(id) {
    clearPackageForm();
    $('#packageModalHeader').text('Edit Package');
    $.ajax({
        url: '/PackageMaster/GetbyID/' + id,
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            if (!data || !data.length) {
                toastr.error('Package details not found');
                return;
            }
            var row = data[0];
            $('#PackageId').val(row.Id || 0);
            $('#PackageCode').val(row.PackageCode || '');
            $('#PackageName').val(row.PackageName || '');
            $('#ReportingName').val(row.ReportingName || '');
            $('#PackagePrice').val(parseFloat(row.Price || 0).toFixed(2));
            $('#PackageDiscount').val(parseFloat(row.DiscountAmount || 0).toFixed(2));
            $('#PackageDescription').val(row.Description || '');
            $('#PackageActive').prop('checked', row.Active === true);

            var invIds = [];
            if (row.InvestigationIds) {
                invIds = row.InvestigationIds.split(',').map(function (x) { return x.trim(); });
            }
            $('#PackageInvestigations').val(invIds).trigger('change');

            $('#btnPackageAdd').addClass('d-none');
            $('#btnPackageUpdate').removeClass('d-none');
        },
        error: function () {
            toastr.error('Failed to load package details');
        }
    });
    return false;
}

function savePackage(mode) {
    var payload = buildPackagePayload();
    if (!payload) return false;

    var url = mode === 'Update' ? '/PackageMaster/Update' : '/PackageMaster/Add';
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
                $('#package_modal').modal('hide');
                $('#packageTable').DataTable().ajax.reload();
                clearPackageForm();
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

function buildPackagePayload() {
    var id = parseInt($('#PackageId').val() || '0');
    var packageCode = ($('#PackageCode').val() || '').trim();
    var packageName = ($('#PackageName').val() || '').trim();
    var reportingName = ($('#ReportingName').val() || '').trim();
    var price = parseFloat($('#PackagePrice').val() || '0');
    var discount = parseFloat($('#PackageDiscount').val() || '0');
    var description = ($('#PackageDescription').val() || '').trim();
    var invIds = $('#PackageInvestigations').val() || [];
    var active = $('#PackageActive').prop('checked');

    if (!packageName) {
        toastr.error('Package Name is required');
        return null;
    }
    if (!invIds.length) {
        toastr.error('Select at least one investigation');
        return null;
    }

    return {
        Id: id,
        PackageCode: packageCode,
        PackageName: packageName,
        ReportingName: reportingName,
        Price: isNaN(price) ? 0 : price,
        DiscountAmount: isNaN(discount) ? 0 : discount,
        Description: description,
        Active: active,
        InvestigationIds: invIds.join(',')
    };
}

function setPackageActive(id, active) {
    var msg = active ? 'Activate this package?' : 'Deactivate this package?';
    if (!confirm(msg)) return false;

    $.ajax({
        url: '/PackageMaster/SetActive',
        type: 'POST',
        dataType: 'json',
        data: { Id: id, Active: active },
        success: function (result) {
            var statusCode = result ? parseInt(result.Item1, 10) : 0;
            if (statusCode === 1) {
                toastr.success(result.Item2 || 'Updated');
                $('#packageTable').DataTable().ajax.reload();
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

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}

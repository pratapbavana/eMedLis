// #region Create
function Add() {
    var res = validate();
    if (res == false) {
        return false;
    }
    var empObj = {
        HeaderName: $('#HeaderName').val().trim(),
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/ParameterHeader/Add",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#parameter_header_modal').modal('hide');
                $('#parameterheadertable').DataTable().ajax.reload();
            }
            else
                toastr.error(result.Item2);
        },
        error: function (errormessage) {
            toastr.error('Something Wrong!');
        }
    });
}
// #endregion

// #region Update
function Update() {
    var res = validate();
    if (res == false) {
        return false;
    }
    var empObj = {
        Id: $('#Id').val(),
        HeaderName: $('#HeaderName').val().trim(),
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/ParameterHeader/Update",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#parameter_header_modal').modal('hide');
                $('#parameterheadertable').DataTable().ajax.reload();
            }
            else
                toastr.error(result.Item2);
        },
        error: function (errormessage) {
            toastr.error('Something Wrong!');
        }
    });
}
// #endregion

// #region Fetch Header By ID
function GetHeaderbyId(Id) {
    $('#header').html("Edit Parameter Header");
    $('#HeaderName').removeClass("valid is-invalid");
    $.ajax({
        url: "/ParameterHeader/getbyID/" + Id,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            $('#Id').val(result[0].Id);
            $('#HeaderName').val(result[0].HeaderName);
            $('#active1').prop('checked', result[0].Active);
            $("#btnUpdate").removeClass("d-none");
            $("#btnAdd").addClass("d-none");
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
    return false;
}
// #endregion

// #region Delete Header
function DeleteHeader(Id) {
    if (!confirm("Delete this header?")) {
        return false;
    }
    $.ajax({
        url: "/ParameterHeader/Delete/" + Id,
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#parameterheadertable').DataTable().ajax.reload();
            }
            else
                toastr.error(result.Item2);
        },
        error: function (errormessage) {
            toastr.error('Something Wrong!');
        }
    });
    return false;
}
// #endregion

// #region Clear Form Fields
function clearfields() {
    $('#header').html("Create Parameter Header");
    $('#Id').val("");
    $('#HeaderName').val("");
    $("#btnUpdate").addClass("d-none");
    $("#btnAdd").removeClass("d-none");
    $('#HeaderName').removeClass("valid is-invalid");
    $("#active1").prop('checked', true);
}
// #endregion

// #region Load Data Table
$(() => {
    loaddatatable()
});
function loaddatatable() {
    var a = $("#parameterheadertable").DataTable({
        order: [],
        ajax: {
            url: '/ParameterHeader/List',
            method: "GET",
            dataSrc: function (json) {
                return json;
            }
        },
        columns: [
            { data: 'Id' },
            { data: 'Id' },
            { data: 'HeaderName' },
            {
                data: 'Active',
                render: function (data, type, row) {
                    if (data == true) {
                        return '<span class="badge badge-pill badge-outline-success">ACTIVE</span>'
                    }
                    else {
                        return '<span class="badge badge-pill badge-outline-danger">INACTIVE</span>'
                    }
                }
            },
            {
                data: 'Id',
                render: function (data, type, row) {
                    return '<a href="#" data-toggle="modal" data-target="#parameter_header_modal" onclick="return GetHeaderbyId(' + data + ');">Edit</a> | ' +
                        '<a href="#" onclick="return DeleteHeader(' + data + ');">Delete</a>'
                }
            }
        ],
        responsive: true,
        columnDefs: [{
            searchable: false,
            orderable: false,
            targets: 0
        },
            { responsivePriority: 1, targets: 2 },
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

// #region Form Validation
function validate() {
    var isValid = true;
    if ($('#HeaderName').val().trim() == "") {
        $('#HeaderName').addClass('is-invalid');
        isValid = false;
    }
    return isValid;
}
// #endregion

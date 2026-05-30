// #region Create
function Add() {
    var res = validate();
    if (res == false) {
        return false;
    }
    var empObj = {
        MethodName: $('#MethodName').val().trim(),
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/TestMethod/Add",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#method_modal').modal('hide');
                $('#methodtable').DataTable().ajax.reload();
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
        MethodName: $('#MethodName').val().trim(),
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/TestMethod/Update",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#method_modal').modal('hide');
                $('#methodtable').DataTable().ajax.reload();
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

// #region Fetch By ID
function GetMethodbyId(Id) {
    $('#header').html("Edit Test Method");
    $('#MethodName').removeClass("valid is-invalid");
    $.ajax({
        url: "/TestMethod/getbyID/" + Id,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            $('#Id').val(result[0].Id);
            $('#MethodName').val(result[0].MethodName);
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

// #region Delete
function DeleteMethod(Id) {
    if (!confirm("Delete this method?")) {
        return false;
    }
    $.ajax({
        url: "/TestMethod/Delete/" + Id,
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#methodtable').DataTable().ajax.reload();
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
    $('#header').html("Create Test Method");
    $('#Id').val("");
    $('#MethodName').val("");
    $("#btnUpdate").addClass("d-none");
    $("#btnAdd").removeClass("d-none");
    $('#MethodName').removeClass("valid is-invalid");
    $("#active1").prop('checked', true);
}
// #endregion

// #region Load Data Table
$(() => {
    loaddatatable()
});
function loaddatatable() {
    var a = $("#methodtable").DataTable({
        order: [],
        ajax: {
            url: '/TestMethod/List',
            method: "GET",
            dataSrc: function (json) {
                return json;
            }
        },
        columns: [
            { data: 'Id' },
            { data: 'Id' },
            { data: 'MethodName' },
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
                    return '<a href="#" data-toggle="modal" data-target="#method_modal" onclick="return GetMethodbyId(' + data + ');">Edit</a> | ' +
                        '<a href="#" onclick="return DeleteMethod(' + data + ');">Delete</a>'
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
    if ($('#MethodName').val().trim() == "") {
        $('#MethodName').addClass('is-invalid');
        isValid = false;
    }
    return isValid;
}
// #endregion

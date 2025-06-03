// #region Create
function Add() {
    var res = validate();
    if (res == false) {
        return false;
    }
    var empObj = {
        ItemName: $('#VacName').val(),
        ItemType: 'Vacutainer',
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/Lookup/Add",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#vacutainer_modal').modal('hide');
                $('#vacutainertable').DataTable().ajax.reload();
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
        ItemName: $('#VacName').val(),
        ItemType: 'Vacutainer',
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/Lookup/Update",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#vacutainer_modal').modal('hide');
                $('#vacutainertable').DataTable().ajax.reload();
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

// #region Fetch Specimen By ID
function GetSpecbyId(Id) {
    $('#header').html("Edit Vacutainer");
    $('#VacName').removeClass("valid is-invalid");
    $.ajax({
        url: "/Lookup/getbyID/" + Id,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            $('#Id').val(result[0].Id);
            $('#VacName').val(result[0].ItemName);
            // $('#Desc').val(result[0].Description);
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

// #region Clear Form Fields
function clearfields() {
    $('#header').html("Create Vacutainer");
    $('#Id').val("");
    $('#VacName').val("");
    $('#VacName').removeClass("valid is-invalid");
    // $('#Desc').val("");
    $("#btnUpdate").addClass("d-none");
    $("#btnAdd").removeClass("d-none");
    $("#active1").prop('checked', true)
}
// #endregion

// #region Load Data Table
$(() => {
    loaddatatable()
});
function loaddatatable() {

    var a = $("#vacutainertable").DataTable({
        order: [],
        ajax: {
            url: '/Lookup/List?ItemType=Vacutainer',
            method: "GET",
            dataSrc: function (json) {
                return json;
            }
        },
        columns: [
            { data: 'Id' },
            { data: 'Id' },
            { data: 'ItemName' },

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

                    return '<a href="#" data-toggle="modal" data-target="#vacutainer_modal" onclick="return GetSpecbyId(' + data + ');">Edit</a>'
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

// #region Form Valdidation
function validate() {
    var isValid = true;
    if ($('#VacName').val().trim() == "") {
        $('#VacName').addClass("is-invalid")
        isValid = false;
    }
    return isValid;
}
// #endregion
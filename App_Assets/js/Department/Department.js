// #region Create
function Add() {
    var res = validate();
    if (res == false) {
        return false;
    }
    var empObj = {
        DepartmentName: $('#DeptName').val(),
        Description: $('#Desc').val(),
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/Department/Add",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#dept_modal').modal('hide');
                $('#depttable').DataTable().ajax.reload();
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
        DepartmentName: $('#DeptName').val(),
        Description: $('#Desc').val(),
        Active: $('#active1').prop('checked'),
    };
    $.ajax({
        url: "/Department/Update",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#dept_modal').modal('hide');
                $('#depttable').DataTable().ajax.reload();
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

// #region Fetch Data By ID
function GetDeptbyId(Id) {
    $('#header').html("Edit Department");
    $('#DeptName').removeClass("valid is-invalid");
    $('#Desc').removeClass("valid is-invalid");
    $.ajax({
        url: "/Department/getbyID/" + Id,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
           
            $('#Id').val(result[0].Id);
            $('#DeptName').val(result[0].DepartmentName);
            $('#Desc').val(result[0].Description);
            $('#active1').prop('checked', result[0].Active);
            $('#btnUpdate').removeClass("d-none");
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
function clearfields  () {
    $('#header').html("Create Department");
    $('#Id').val("");
    $('#DeptName').val("");
    $('#Desc').val("");
    $('#btnUpdate').addClass("d-none");
    $('#btnAdd').removeClass("d-none");
    $('#DeptName').removeClass("valid is-invalid");
    $('#Desc').removeClass("valid is-invalid");
    $("#active1").prop('checked', true)
    M.updateTextFields();
}
// #endregion

// #region Load Data Table
$(() => {
    loaddatatable()
});
function loaddatatable() {
    var a = $("#depttable").DataTable({
        order : [],
        ajax: {
            url: '/Department/List',
            method: "GET",
            dataSrc: function (json) {
                return json;
            }
        },
        columns: [
            { data: 'Id' },
            { data: 'Id' },
            { data: 'DepartmentName' },
            { data: 'Description' },
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
                    return '<a href="#" data-toggle="modal" data-target="#dept_modal" onclick="return GetDeptbyId(' + data + ');">Edit</a>'
                }
            }
        ],
        responsive: true,
        columnDefs: [{
            searchable: false,
            orderable: false,
            targets:0
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
//#endregion

// #region Form Validation 
function validate() {
    var isValid = true;
    if ($('#DeptName').val().trim() == "") {
        $('#DeptName').addClass('is-invalid');
        isValid = false;
    }
    
    if ($('#Desc').val().trim() == "") {
        $('#Desc').addClass('is-invalid');
        isValid = false;
    }

   return isValid;
}  
// #endregion

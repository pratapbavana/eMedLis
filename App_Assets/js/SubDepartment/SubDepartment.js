// #region Create
function Add() {
    var res = validate();
    if (res == false) {
        return false;
    }
    var empObj = {
        SubDeptName: $('#SubDeptName').val(),
        //DepartmentId: $('#DeptId').val(),
        DepartmentId: $('#deptname').val(),
        Header: $('#Header').val(),
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/SubDepartment/Add",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#sub_dept_modal').modal('hide');
                $('#subdepttable').DataTable().ajax.reload();
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
        SubDeptName: $('#SubDeptName').val(),
        //DepartmentId: $('#DeptId').val(),
        DepartmentId: $('#deptname').val(),
        Header: $('#Header').val(),
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/SubDepartment/Update",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#sub_dept_modal').modal('hide');
                $('#subdepttable').DataTable().ajax.reload();
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

// #region Fetch SubDepartment By ID
function GetSubDeptbyId(Id) {
    $('#header').html("Edit Sub Department");
    $('#SubDeptName').removeClass("valid is-invalid");
    $('#deptname').removeClass("valid is-invalid");
    $('#Header').removeClass("valid is-invalid");
    $.ajax({
        url: "/SubDepartment/getbyID/" + Id,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            $('#Id').val(result[0].Id);
            $('#SubDeptName').val(result[0].SubDeptName);
            //$('#DeptId').text(result[0].DepartmentId);
            $('#deptname').val(result[0].DepartmentId).trigger("change");
            $('#Header').val(result[0].Header);
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
    $('#header').html("Create Sub Department");
    $('#Id').val("");
    $('#SubDeptName').val("");
    $('#deptname').val(null).trigger('change');
    $('#Header').val("");
    $("#btnUpdate").addClass("d-none");
    $("#btnAdd").removeClass("d-none");
    $('#SubDeptName').removeClass("valid is-invalid");
    $('#deptname').removeClass("valid is-invalid");
    $('#Header').removeClass("valid is-invalid");
    $("#active1").prop('checked', true);
}
// #endregion

// #region Load Data Table
$(() => {
    loaddatatable()
});
function loaddatatable() {
    var a = $("#subdepttable").DataTable({
        order: [],
        ajax: {
            url: '/SubDepartment/List',
            method: "GET",
            dataSrc: function (json) {
                return json;
            }
        },
        columns: [
            { data: 'Id' },
            { data: 'Id' },
            { data: 'SubDeptName' },
            { data: 'DepartmentName' },
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

                    return '<a href="#" data-toggle="modal" data-target="#sub_dept_modal" onclick="return GetSubDeptbyId(' + data + ');">Edit</a>'
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
//#endregion

// #region Form Valdidation
function validate() {

    var isValid = true;
    
    if ($('#SubDeptName').val().trim() == "") {
        $('#SubDeptName').addClass('is-invalid');
        isValid = false;
    }
    
    if ($('#Header').val().trim() == "") {
        $('#Header').addClass('is-invalid');
        isValid = false;
    }

    if ($('#deptname').val() == null) {
        $('#deptname').addClass('is-invalid');
        isValid = false;
    }

    return isValid;
}

// #endregion

// #region Department Dropdown
$(document).ready(function () {
    $.ajax({
        url: '/Department/List', // Replace with your API endpoint URL
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            const formattedData = data.map(item => ({
                id: item.Id, // Replace with your actual ID property name
                text: item.DepartmentName, // Replace with your actual text property name
            }));
            $(".deptsearch").select2({
                data: formattedData,
                width: "100%",
                // Add other Select2 options as needed
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error fetching data:', textStatus, errorThrown);
            // Handle error gracefully (e.g., display an error message)
        },
        dropdownParent: $("#sub_dept_modal")
    });
});
// #endregion
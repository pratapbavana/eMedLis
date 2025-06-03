
// #region Create Investigation
function Add() {
    var res = validate();
    if (res == false) {
        return false;
    }
    var empObj = {
        InvName: $('#InvName').val().trim(),
        Rate: $('#Rate').val(),
        InvCode: $('#Code').val(),
        ReportHdr: $('#RepHdr').val(),
        SubDeptId: $('#subdeptname').val(),
        SpecimenId: $('#spec').val(),
        VacutainerId: $('#vac').val(),
        ReportTime: $('#RptTime').val(),
        GuideLines: $('#GLines').val(),
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/Investigation/Add",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#inv_modal').modal('hide');
                $('#invtable').DataTable().ajax.reload();
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

// #region Update Investigation
function Update() {
    var res = validate();
    if (res == false) {
        return false;
    }
    var empObj = {
        Id: $('#Id').val(),
        InvName: $('#InvName').val().trim(),
        Rate: $('#Rate').val(),
        InvCode: $('#Code').val(),
        ReportHdr: $('#RepHdr').val(),
        SubDeptId: $('#subdeptname').val(),
        SpecimenId: $('#spec').val(),
        VacutainerId: $('#vac').val(),
        ReportTime: $('#RptTime').val(),
        GuideLines: $('#GLines').val(),
        Active: $('#active1').prop('checked')
    };
    $.ajax({
        url: "/Investigation/Update",
        data: JSON.stringify(empObj),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            if (result.Item1 == 1) {
                toastr.success(result.Item2);
                $('#inv_modal').modal('hide');
                $('#invtable').DataTable().ajax.reload();
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

// #region Get Investigations By ID
function GetInvbyId(Id) {
    $('#subdeptname').select2({ width: "100%" });
    $('#header').html("Edit Investigation");
    $('#InvName').removeClass("valid is-invalid");
    $('#Rate').removeClass("valid is-invalid");
    $('#Code').removeClass("valid is-invalid");
    $('#RepHdr').removeClass("valid is-invalid");
    $('#deptname').removeClass("valid is-invalid");
    $('#spec').removeClass("valid is-invalid");
    $('#subdeptname').removeClass("valid is-invalid");
    $('#vac').removeClass("valid is-invalid");
    $('#RptTime').removeClass("valid is-invalid");
    $('#GLines').removeClass("valid is-invalid");
    $.ajax({
        url: "/Investigation/getbyID/" + Id,
        type: "GET",
        contentType: "application/json;charset=UTF-8",
        dataType: "json",
        success: function (result) {
            $('#Id').val(result[0].Id);
            $('#InvName').val(result[0].InvName);
            $('#Rate').val(result[0].Rate);
            $('#Code').val(result[0].InvCode);
            $('#RepHdr').val(result[0].ReportHdr);
            $('#deptname').val(result[0].DeptId).trigger("change");
            $('#spec').val(result[0].SpecimenId).trigger("change");
            $('#vac').val(result[0].VacutainerId).trigger("change");
            $('#RptTime').val(result[0].ReportTime);
            $('#GLines').val(result[0].GuideLines);
            $('#VacId').text(result[0].VacutainerId);
            $('#SpecId').text(result[0].SpecimenId);
            $('#SubDeptId').text(result[0].SubDeptId);
            $('#DeptId').text(result[0].DeptId);
            $('#active1').prop('checked', result[0].Active);
            $("#btnUpdate").removeClass("d-none");
            $("#btnAdd").addClass("d-none");
            setTimeout(function () {
                $('#subdeptname').val(result[0].SubDeptId).trigger("change");
            }, 100);
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
    $('#closeablecard').fadeIn();
    $('#header').html("Create Investigation");
    $('#Id').val("");
    $('#InvName').val("");
    $('#Rate').val("");
    $('#Code').val("");
    $('#RepHdr').val("");
    $('#deptname').val(null).trigger("change");
    $('#spec').val(null).trigger("change");
    $('#subdeptname').val(null).trigger("change");
    $('#subdeptname').empty();
    $('#vac').val(null).trigger("change");
    $('#RptTime').val("");
    $('#GLines').val("");
    $('#VacId').text("");
    $('#SpecId').text("");
    $('#SubDeptId').text("");
    $('#DeptId').text("");
    $("#btnUpdate").addClass("d-none");
    $("#btnAdd").removeClass("d-none");
    $('#InvName').removeClass("valid is-invalid");
    $('#Rate').removeClass("valid is-invalid");
    $('#Code').removeClass("valid is-invalid");
    $('#RepHdr').removeClass("valid is-invalid");
    $('#deptname').removeClass("valid is-invalid");
    $('#spec').removeClass("valid is-invalid");
    $('#subdeptname').removeClass("valid is-invalid");
    $('#vac').removeClass("valid is-invalid");
    $('#RptTime').removeClass("valid is-invalid");
    $('#GLines').removeClass("valid is-invalid");
    $("#active1").prop('checked', true)
    //document.getElementById("InvName").focus();
    $('#inv_modal').on('shown.bs.modal', function () {
        $('#InvName').focus();
    })
}
// #endregion

// #region Load Data Table
$(() => {
    loaddatatable()
});
function loaddatatable() {

    var a = $("#invtable").DataTable({
        order: [],
        ajax: {
            url: '/Investigation/List',
            method: "GET",
            dataSrc: function (json) {
                return json;
            }
        },
        columns: [
            { data: 'Id'},
            { data: 'InvName' },
            { data: 'SubDeptName' },
            { data: 'Rate' },
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

                    return '<a href="#" data-toggle="modal" data-target="#inv_modal" onclick="return GetInvbyId(' + data + ');">Edit</a>'
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
    if ($('#InvName').val().trim() == "") {
        $('#InvName').addClass("valid is-invalid");
        isValid = false;
    }
    if ($('#Rate').val().trim() == "") {
        $('#Rate').addClass("valid is-invalid");
        isValid = false;
    }
    if ($('#Code').val().trim() == "") {
        $('#Code').addClass("valid is-invalid");
        isValid = false;
    }
    if ($('#RepHdr').val().trim() == "") {
        $('#RepHdr').addClass("valid is-invalid");
        isValid = false;
    }
    if ($('#RptTime').val().trim() == "") {
        $('#RptTime').addClass("valid is-invalid");
        isValid = false;
    }
    if ($('#GLines').val().trim() == "") {
        $('#GLines').addClass("valid is-invalid");
        isValid = false;
    }
    if ($('#deptname').val().trim() == "") {
        $('#deptname').addClass("valid is-invalid");
        isValid = false;
    }
    if ($('#spec').val().trim() == "") {
        $('#spec').addClass("valid is-invalid");
        isValid = false;
    }
    if ($('#vac').val().trim() == "") {
        $('#vac').addClass("valid is-invalid");
        isValid = false;
    }
    if ($('#subdeptname').val().trim() == "") {
        $('#subdeptname').addClass("valid is-invalid");
        isValid = false;
    }
    
    return isValid;
}
// #endregion

// #region Department & Sub Dept Dropdown
$(document).ready(function () {

    $.ajax({
        url: '/Department/List',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            const formattedData = data.map(item => ({
                id: item.Id,
                text: item.DepartmentName,
            }));
            $(".deptsearch").select2({
                data: formattedData,
                width: "100%",
                placeholder: 'Select Department'
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error fetching data:', textStatus, errorThrown);
        },
        dropdownParent: $("#inv_modal")
    });
    $('#subdeptname').select2({
        data: []
    });
    $('.deptsearch').on('change', function () {
        const SelectedDept = $(this).val();
        loadsubdept(SelectedDept)
    });
});


function loadsubdept(SelectedDept) {
    if (!SelectedDept) {
        $('#subdeptname').select2({ width: "100%"});
        return;
    }
    $.getJSON('/SubDepartment/ListByDeptId/' + SelectedDept, function (subdepts) {
        $('#subdeptname').empty();
        subdepts.forEach(subdept => {
            $('#subdeptname').append($('<option>', {
                value: subdept.Id,
                text: subdept.SubDeptName
            }));
        });
        $('#subdeptname').val(null).trigger('change');
    });
}
// #endregion

// #region Specimen Dropdown
$(document).ready(function () {
    $.ajax({
        url: '/Lookup/List?ItemType=Specimen',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            const formattedData = data.map(item => ({
                id: item.Id,
                text: item.ItemName,
            }));
            $(".specsearch").select2({
                data: formattedData,
                width: "100%",
                placeholder: 'Select Specimen'
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error fetching data:', textStatus, errorThrown);
            toastr.error('Error fetching data');
        },
        dropdownParent: $("#inv_modal")
    });
});
// #endregion

// #region vacutainer Dropdown
$(document).ready(function () {
    $.ajax({
        url: '/Lookup/List?ItemType=Vacutainer',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            const formattedData = data.map(item => ({
                id: item.Id,
                text: item.ItemName,
            }));
            $(".vacsearch").select2({
                data: formattedData,
                width: "100%",
                placeholder: 'Select Vacutainer'
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('Error fetching data:', textStatus, errorThrown);
            toastr.error('Error fetching data');
        },
        dropdownParent: $("#inv_modal")
    });
});
// #endregion
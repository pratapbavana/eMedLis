const DeptApi = {
    add: "/Department/Add",
    update: "/Department/Update",
    getById: "/Department/getbyID/",
    list: "/Department/List"
};

function showError(msg = "Something went wrong!") {
    toastr.error(msg);
}
function validate() {
    let isValid = true;

    const deptName = $('#DeptName');
    const desc = $('#Desc');

    deptName.removeClass('is-invalid');
    desc.removeClass('is-invalid');

    if (!deptName.val().trim()) {
        deptName.addClass('is-invalid');
        isValid = false;
    }

    if (!desc.val().trim()) {
        desc.addClass('is-invalid');
        isValid = false;
    }

    return isValid;
}

function saveDepartment(isUpdate = false) {
    if (!validate()) return false;

    const payload = {
        Id: isUpdate ? $('#Id').val() : 0,
        DepartmentName: $('#DeptName').val(),
        Description: $('#Desc').val(),
        Active: $('#active1').prop('checked')
    };

    $.ajax({
        url: isUpdate ? DeptApi.update : DeptApi.add,
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        data: JSON.stringify(payload),
        success: function (result) {
            if (result.Item1 === 1) {
                toastr.success(result.Item2);
                $('#dept_modal').modal('hide');
                $('#depttable').DataTable().ajax.reload(null, false);
            } else {
                toastr.error(result.Item2);
            }
        },
        error: function () {
            showError();
        }
    });
}

function GetDeptbyId(id) {
    $('#header').text("Edit Department");
    $('#DeptName, #Desc').removeClass("valid is-invalid");

    $.ajax({
        url: DeptApi.getById + id,
        type: "GET",
        dataType: "json",
        success: function (result) {
            const d = result[0];

            $('#Id').val(d.Id);
            $('#DeptName').val(d.DepartmentName);
            $('#Desc').val(d.Description);
            $('#active1').prop('checked', d.Active);

            $('#btnUpdate').removeClass("d-none");
            $('#btnAdd').addClass("d-none");
        },
        error: function () {
            showError("Unable to load department");
        }
    });

    return false;
}

function clearfields() {
    $('#header').text("Create Department");
    $('#Id').val("");
    $('#DeptName').val("");
    $('#Desc').val("");

    $('#btnUpdate').addClass("d-none");
    $('#btnAdd').removeClass("d-none");

    $('#DeptName, #Desc').removeClass("valid is-invalid");
    $('#active1').prop('checked', true);

    M.updateTextFields();
}

let deptTable;

$(document).ready(function () {
    deptTable = $("#depttable").DataTable({
        order: [],
        ajax: {
            url: DeptApi.list,
            type: "GET",
            dataSrc: ""
        },
        columns: [
            { data: null },
            { data: 'Id' },
            { data: "DepartmentName" },
            { data: "Description" },
            {
                data: "Active",
                render: d =>
                    d
                        ? '<span class="badge badge-pill badge-outline-success">ACTIVE</span>'
                        : '<span class="badge badge-pill badge-outline-danger">INACTIVE</span>'
            },
            {
                data: "Id",
                render: id =>
                    `<a href="#" data-toggle="modal" data-target="#dept_modal"
                        onclick="return GetDeptbyId(${id});">Edit</a>`
            }
        ],
        columnDefs: [
            { targets: 0, searchable: false, orderable: false }
        ],
        responsive: true
    });

    deptTable.on('order.dt search.dt', function () {
        deptTable.column(0).nodes().each((cell, i) => cell.innerHTML = i + 1);
    }).draw();
});

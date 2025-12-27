
    $(document).ready(function() {
    // Set current date and time
    var now = new Date();
    $('#collectionDate').val(now.toISOString().split('T')[0]);
    $('#collectionTime').val(now.toTimeString().slice(0,5));
    $('#collectedBy').val('Admin'); // Set current user

    // Toggle address section for home collection
    $('#homeCollection').change(function() {
        if ($(this).is(':checked')) {
        $('#addressSection').show();
        } else {
        $('#addressSection').hide();
        }
    });

    // Save collection
    $('#saveCollection').click(function() {
        saveCollection();
    });

    // Print labels functionality
    $('.print-label').click(function() {
        var row = $(this).closest('tr');
    printSampleLabel(row);
    });

    $('#printAllLabels').click(function() {
        printAllLabels();
    });
});

    function saveCollection() {
    if (!validateForm()) {
        return;
    }

    var sampleCollection = {
        BillSummaryId: @Model.BillSummary.BillSummaryId,
    PatientInfoId: @Model.PatientInfo.PatientInfoId,
    CollectionDate: $('#collectionDate').val(),
    CollectionTime: $('#collectionTime').val(),
    CollectedBy: $('#collectedBy').val(),
    Priority: $('#priority').val(),
    HomeCollection: $('#homeCollection').is(':checked'),
    PatientAddress: $('#patientAddress').val(),
    CollectionStatus: 'Pending',
    CreatedBy: 'Admin'
    };

    var sampleDetails = [];
    $('#samplesTable tbody tr').each(function() {
        var row = $(this);
    sampleDetails.push({
        InvMasterId: row.data('inv-id'),
    InvestigationName: row.find('td:first').text(),
    SpecimenType: row.find('.specimen-type').val(),
    ContainerType: row.find('.container-type').val(),
    FastingRequired: row.find('.fasting-required').is(':checked'),
    CollectionInstructions: row.find('.collection-instructions').val(),
    SampleStatus: row.find('.sample-status').val()
        });
    });

    $.ajax({
        url: '@Url.Action("SaveCollection")',
    type: 'POST',
    data: {
        sampleCollection: sampleCollection,
    sampleDetails: sampleDetails
        },
    success: function(response) {
            if (response.success) {
        toastr.success(response.message);
    setTimeout(function() {
        window.location.href = '@Url.Action("Index")';
                }, 2000);
            } else {
        toastr.error(response.message);
            }
        },
    error: function() {
        toastr.error('Error saving sample collection.');
        }
    });
}

    function validateForm() {
    if (!$('#collectionDate').val()) {
        toastr.error('Please select collection date');
    return false;
    }
    if (!$('#collectionTime').val()) {
        toastr.error('Please select collection time');
    return false;
    }
    if (!$('#collectedBy').val()) {
        toastr.error('Please enter collected by');
    return false;
    }
    return true;
}

    function printSampleLabel(row) {
        // Implementation for printing individual sample labels with barcodes
        toastr.info('Print label functionality will be implemented');
}

    function printAllLabels() {
        // Implementation for printing all sample labels
        toastr.info('Print all labels functionality will be implemented');
}

// Populate Sample Collection Modal
function populateSampleCollectionModal(data) {
    // Set patient info
    $('#scPatName').text(data.patientInfo.patName);
    $('#scUHID').text(data.patientInfo.uhid);
    $('#scMobile').text(data.patientInfo.mobileNo);
    $('#scAgeGender').text(data.patientInfo.age + ' / ' + data.patientInfo.gender);

    // Set bill info
    $('#scBillNo').text(data.billNo);

    // Set current date and time
    var now = new Date();
    $('#scCollectionDate').val(now.toISOString().split('T')[0]);
    $('#scCollectionTime').val(now.toTimeString().slice(0, 5));

    // Clear and populate samples table
    var tbody = $('#scSamplesTable tbody');
    tbody.empty();

    data.billDetails.forEach(function (item, index) {
        var row = `
            <tr data-inv-id="${item.invId}">
                <td>${item.invName}</td>
                <td>
                    <select class="form-control form-control-sm specimen-type">
                        <option value="${item.specimenType}">${item.specimenType}</option>
                        <option value="Serum">Serum</option>
                        <option value="Plasma">Plasma</option>
                        <option value="Whole Blood">Whole Blood</option>
                        <option value="Urine">Urine</option>
                        <option value="Stool">Stool</option>
                    </select>
                </td>
                <td>
                    <select class="form-control form-control-sm container-type">
                        <option value="${item.containerType}">${item.containerType}</option>`;

        data.containers.forEach(function (container) {
            row += `<option value="${container.containerName}">${container.containerName} (${container.capColor})</option>`;
        });

        row += `
                    </select>
                </td>
                <td>
                    <input type="checkbox" class="fasting-required">
                </td>
                <td>
                    <select class="form-control form-control-sm sample-status">
                        <option value="Not Collected">Not Collected</option>
                        <option value="Collected">Collected</option>
                        <option value="Rejected">Rejected</option>
                    </select>
                </td>
            </tr>`;

        tbody.append(row);
    });

    // Reset other fields
    $('#scPriority').val('Normal');
    $('#scCollectedBy').val('Admin');
    $('#scRemarks').val('');
    $('#scHomeCollection').prop('checked', false);
    $('#scAddressSection').hide();
}

// Handle home collection checkbox
$('#sampleCollectionModal').on('change', '#scHomeCollection', function () {
    if ($(this).is(':checked')) {
        $('#scAddressSection').show();
    } else {
        $('#scAddressSection').hide();
    }
});

// Save Sample Collection
$('#btnSaveCollection').click(function () {
    saveSampleCollection();
});

function saveSampleCollection() {
    var sampleCollection = {
        BillSummaryId: currentBillToCollect,
        PatientInfoId: 0, // Will be retrieved from bill
        CollectionDate: $('#scCollectionDate').val(),
        CollectionTime: $('#scCollectionTime').val(),
        CollectedBy: $('#scCollectedBy').val(),
        Priority: $('#scPriority').val(),
        HomeCollection: $('#scHomeCollection').is(':checked'),
        PatientAddress: $('#scAddress').val(),
        Remarks: $('#scRemarks').val(),
        CollectionStatus: 'Pending',
        CreatedBy: 'Admin'
    };

    var sampleDetails = [];
    $('#scSamplesTable tbody tr').each(function () {
        var row = $(this);
        sampleDetails.push({
            InvMasterId: row.data('inv-id'),
            InvestigationName: row.find('td:eq(0)').text(),
            SpecimenType: row.find('.specimen-type').val(),
            ContainerType: row.find('.container-type').val(),
            FastingRequired: row.find('.fasting-required').is(':checked'),
            SampleStatus: row.find('.sample-status').val()
        });
    });

    $.ajax({
        url: '/SampleCollection/SaveCollection',
        type: 'POST',
        data: {
            sampleCollection: sampleCollection,
            sampleDetails: sampleDetails
        },
        success: function (response) {
            if (response.success) {
                toastr.success(response.message + ' - Barcode: ' + response.collectionBarcode);
                $('#sampleCollectionModal').modal('hide');

                // Refresh the bills table
                $('#recentBillsTable').DataTable().ajax.reload();

                // Ask if user wants to print labels
                setTimeout(function () {
                    if (confirm('Sample collection created! Do you want to print labels?')) {
                        window.open('/SampleCollection/PrintCollectionLabels/' + response.collectionBarcode, '_blank');
                    }
                }, 1000);
            } else {
                toastr.error(response.message);
            }
        },
        error: function () {
            toastr.error('Error saving sample collection.');
        }
    });
}


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
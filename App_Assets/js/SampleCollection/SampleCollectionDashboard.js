var currentBillToCollect = null;
var AppRoutes = AppRoutes || {};

AppRoutes.SampleCollection = {
    getCollectionData: '/SampleCollection/GetCollectionData',
    saveCollection: '/SampleCollection/SaveCollection',
    getPendingCollections: '/SampleCollection/GetPendingCollections',
    printLabels: '/SampleCollection/PrintCollectionLabels'
};

$(document).ready(function () {
    loadCollectionsTable();
    setupModalEvents();
    setupFilterEvents();
});

function loadCollectionsTable() {
    if ($.fn.DataTable.isDataTable('#collectionsTable')) {
        $('#collectionsTable').DataTable().destroy();
    }

    $('#collectionsTable').DataTable({
        ajax: {
            url: AppRoutes.SampleCollection.getPendingCollections,
            dataSrc: ''
        },
        order: [[1, 'desc']],
        columns: [
            {
                data: 'collectionBarcode',
                render: function (data) {
                    return data === 'New' ? '<span class="badge badge-warning">New</span>' : data;
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    return row.collectionDate + ' ' + row.collectionTime;
                }
            },
            { data: 'billNo' },
            { data: 'patientName' },
            { data: 'uhid' },
            { data: 'ageGender' },
            {
                data: 'priority',
                render: function (data) {
                    var badgeClass = data === 'STAT' ? 'badge-danger' :
                        data === 'Urgent' ? 'badge-warning' : 'badge-info';
                    return '<span class="badge ' + badgeClass + '">' + data + '</span>';
                }
            },
            {
                data: 'status',
                render: function (data) {
                    var badgeClass =
                        data === 'New' ? 'badge-secondary' :
                            data === 'Collected' ? 'badge-success' :
                                data === 'Partially Collected' ? 'badge-info' :
                                    data === 'Partial Rejection' ? 'badge-warning' :
                                        data === 'Rejected' ? 'badge-danger' : 'badge-secondary';
                    return '<span class="badge ' + badgeClass + '">' + data + '</span>';
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    return `<div class="progress" style="height: 20px;">
                        <div class="progress-bar bg-success" role="progressbar" 
                             style="width: ${row.progressPercent}%" 
                             aria-valuenow="${row.progressPercent}" 
                             aria-valuemin="0" aria-valuemax="100">
                            ${row.collectedCount}/${row.totalInvestigations}
                        </div>
                    </div>`;
                }
            },
            {
                data: 'homeCollection',
                render: function (data) {
                    return data ? '<i class="fa fa-home text-success"></i>' : '-';
                }
            },
            { data: 'collectedBy' },
            { data: 'mobileNo' },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    // Only allow edit if status is New, Pending, or In Progress
                    var canEdit = ['New', 'Pending', 'In Progress', 'Partially Collected', 'Partial Rejection'].includes(row.status);

                    var buttons = `<div class="btn-group btn-group-sm" role="group">`;

                    if (canEdit) {
                        buttons += `<button class="btn btn-primary" onclick="editCollection(${row.billSummaryId})" title="Edit Collection">
                            <i class="fa fa-edit"></i>
                        </button>`;
                    } else {
                        buttons += `<button class="btn btn-info" onclick="viewCollectionDetails(${row.sampleCollectionId})" title="View">
                            <i class="fa fa-eye"></i>
                        </button>`;
                    }

                    if (row.sampleCollectionId > 0) {
                        buttons += `<button class="btn btn-success" onclick="printCollectionLabels(${row.sampleCollectionId})" title="Print Labels">
                            <i class="fa fa-barcode"></i>
                        </button>`;
                    }

                    buttons += `</div>`;

                    return buttons;
                }
            }
        ],
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]]
    });
}

function setupModalEvents() {
    // Home collection checkbox
    $('#sampleCollectionModal').on('change', '#scHomeCollection', function () {
        if ($(this).is(':checked')) {
            $('#scAddressSection').show();
        } else {
            $('#scAddressSection').hide();
        }
    });

    // Save Collection button
    $('#btnSaveCollection').click(function () {
        saveSampleCollection();
    });

    // Print Labels button
    $('#btnPrintLabels').click(function () {
        if (!currentBillToCollect) {
            toastr.error('Please load collection data first.');
            return;
        }
        window.open(AppRoutes.SampleCollection.printLabels + '/' + currentBillToCollect, '_blank');
    });
}

function setupFilterEvents() {
    $('#statusFilter').change(function () {
        var table = $('#collectionsTable').DataTable();
        table.column(7).search($(this).val()).draw();
    });

    $('#priorityFilter').change(function () {
        var table = $('#collectionsTable').DataTable();
        table.column(6).search($(this).val()).draw();
    });

    $('#searchBox').keyup(function () {
        $('#collectionsTable').DataTable().search($(this).val()).draw();
    });
}


function openNewCollection() {
    // Open a dialog to select bill number for new collection
    toastr.info('Select a bill from Patient Billing to collect samples.');
    // Or open a select bill modal
}

function editCollection(billSummaryId) {
    currentBillToCollect = billSummaryId;

    if (!billSummaryId || billSummaryId === 0) {
        toastr.error('Invalid Bill ID');
        return;
    }

    console.log('Loading collection data for billId:', billSummaryId);

    $.ajax({
        url: '/SampleCollection/GetCollectionData/' + billSummaryId,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            console.log('Response received:', response);

            if (response.success) {
                populateSampleCollectionModal(response.data);

                // Check if already has collection
                if (response.data.sampleCollectionId && response.data.sampleCollectionId > 0) {
                    loadExistingCollection(response.data.sampleCollectionId);
                }

                $('#sampleCollectionModal').modal('show');
            } else {
                toastr.error('Error: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX Error:', error);
            console.error('Status:', status);
            console.error('Response:', xhr.responseText);
            toastr.error('Error loading collection details: ' + error);
        }
    });
}

function loadExistingCollection(sampleCollectionId) {
    console.log('Loading existing collection:', sampleCollectionId);

    $.ajax({
        url: '/SampleCollection/GetCollectionDetails/' + sampleCollectionId,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            console.log('Existing collection data:', response);

            if (response.success && response.data) {
                var data = response.data;

                // Update master info
                $('#scCollectionBarcode').text(data.collectionBarcode || 'New');
                $('#scPriority').val(data.priority || 'Normal');
                $('#scCollectedBy').val(data.collectedBy || 'Admin');
                $('#scRemarks').val(data.remarks || '');
                $('#scHomeCollection').prop('checked', data.homeCollection || false);

                // Update sample details
                updateModalWithExistingData(data.sampleDetails || []);
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading collection details:', error);
        }
    });
}

function updateModalWithExistingData(sampleDetails) {
    console.log('Updating modal with existing samples:', sampleDetails);

    $('#scSamplesTable tbody tr').each(function () {
        var invId = parseInt($(this).data('inv-id'));
        var $row = $(this);

        // Find matching existing detail
        var existingDetail = sampleDetails.find(d => d.invMasterId === invId);

        if (existingDetail) {
            console.log('Found existing detail for InvId:', invId, existingDetail);

            $row.data('sample-detail-id', existingDetail.sampleDetailId || 0);

            // Set status
            $row.find('.sample-status').val(existingDetail.sampleStatus || 'Pending');

            // Set quantity if collected
            if (existingDetail.sampleStatus === 'Collected') {
                $row.find('.collected-quantity').val(existingDetail.collectedQuantity || '');
                $row.find('.sample-status').prop('disabled', true).addClass('bg-success text-white');
                $row.find('.collected-quantity').prop('disabled', true);

                // Show collection date/time
                if (existingDetail.collectionDate) {
                    var dateObj = new Date(existingDetail.collectionDate);
                    var dateStr = dateObj.toLocaleDateString('en-GB');
                    var timeStr = existingDetail.collectionTime ? existingDetail.collectionTime.substring(0, 5) : '';
                    $row.find('.collection-datetime').val(dateStr + ' ' + timeStr);
                }
            } else if (existingDetail.sampleStatus === 'Rejected') {
                $row.find('.rejection-reason').val(existingDetail.rejectionReason || '');
                $row.find('.rejection-reason').show();
                $row.find('.sample-status').prop('disabled', true).addClass('bg-danger text-white');
                $row.find('.collected-quantity').prop('disabled', true);
            }
        }
    });
}

// Handle status change
$(document).on('change', '#scSamplesTable .sample-status', function () {
    var $row = $(this).closest('tr');
    var status = $(this).val();
    var $rejectionReason = $row.find('.rejection-reason');

    if (status === 'Rejected') {
        $rejectionReason.show();
    } else {
        $rejectionReason.hide().val('');
    }

    // Auto-fill collection date/time when marking as collected
    if (status === 'Collected') {
        var now = new Date();
        var dateStr = now.toLocaleDateString('en-GB');
        var timeStr = now.toTimeString().slice(0, 5);
        $row.find('.collection-datetime').val(dateStr + ' ' + timeStr);
    }
});
function viewCollection(sampleCollectionId) {
    // Implement view-only mode (similar to edit but read-only)
    toastr.info('View collection functionality will be implemented.');
}

function populateSampleCollectionModal(data) {
    console.log('Populating modal with data:', data);

    // Set patient info
    $('#scPatName').text(data.patientInfo?.patName || '');
    $('#scUHID').text(data.patientInfo?.uhid || '');
    $('#scMobile').text(data.patientInfo?.mobileNo || '');
    $('#scAgeGender').text((data.patientInfo?.age || '') + ' / ' + (data.patientInfo?.gender || ''));
    $('#scAddress').text((data.patientInfo?.area || '') + ', ' + (data.patientInfo?.city || ''));

    // Set bill info
    $('#scBillNo').text(data.billSummaryId ? data.billNo : '');
    $('#scCollectionBarcode').text(data.collectionBarcode || 'New');

    // Set current date and time if new
    if (!data.sampleCollectionId || data.sampleCollectionId === 0) {
        var now = new Date();
        $('#scCollectionDate').val(now.toISOString().split('T')[0]);
        $('#scCollectionTime').val(now.toTimeString().slice(0, 5));
    } else {
        // If existing, set from data
        if (data.collectionDate) {
            var dateObj = new Date(data.collectionDate);
            $('#scCollectionDate').val(dateObj.toISOString().split('T')[0]);
        }
        if (data.collectionTime) {
            $('#scCollectionTime').val(data.collectionTime);
        }
    }

    // Clear and populate samples table
    var tbody = $('#scSamplesTable tbody');
    tbody.empty();

    if (!data.billDetails || data.billDetails.length === 0) {
        tbody.append('<tr><td colspan="6">No investigations found for this bill</td></tr>');
        return;
    }

    data.billDetails.forEach(function (item, index) {
        // Validate InvId
        if (!item.invId || item.invId == 0) {
            console.warn('Skipping item with invalid InvId:', item);
            return;
        }

        var row = `
            <tr data-inv-id="${item.invId}" data-sample-detail-id="0">
                <td>${item.invName || ''}</td>
                <td>
                    <select class="form-control form-control-sm specimen-type" disabled>
                        <option value="${item.specimenType || 'Serum'}">${item.specimenType || 'Serum'}</option>
                    </select>
                </td>
                <td>
                    <select class="form-control form-control-sm container-type" disabled>
                        <option value="${item.containerType || 'Plain Vacutainer'}">${item.containerType || 'Plain Vacutainer'}</option>
                    </select>
                </td>
                <td style="text-align: center;">
                    <input type="checkbox" class="fasting-required" ${item.fastingRequired ? 'checked' : ''} disabled>
                </td>
                <td>
                    <select class="form-control form-control-sm sample-status">
                        <option value="Pending">Pending</option>
                        <option value="Collected">Collected</option>
                        <option value="Rejected">Rejected</option>
                    </select>
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm collected-quantity" placeholder="e.g., 2ml">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm collection-datetime" readonly placeholder="Auto-filled">
                </td>
                <td>
                    <input type="text" class="form-control form-control-sm rejection-reason" placeholder="If rejected" style="display:none;">
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


function saveSampleCollection() {
    if (!validateSampleCollection()) {
        return;
    }

    var sampleCollection = {
        BillSummaryId: currentBillToCollect,
        CollectionDate: $('#scCollectionDate').val(),
        CollectionTime: $('#scCollectionTime').val(),
        CollectedBy: $('#scCollectedBy').val(),
        Priority: $('#scPriority').val(),
        HomeCollection: $('#scHomeCollection').is(':checked'),
        PatientAddress: $('#scCollectionAddress').val(),
        Remarks: $('#scRemarks').val(),
        CreatedBy: 'Admin'
    };

    var sampleDetails = [];
    var now = new Date();

    $('#scSamplesTable tbody tr').each(function () {
        var $row = $(this);
        var invId = parseInt($row.data('inv-id'));
        var status = $row.find('.sample-status').val();

        if (!invId || invId == 0) {
            console.warn('Skipping row with invalid InvId');
            return true; // continue
        }

        var detail = {
            SampleDetailId: parseInt($row.data('sample-detail-id')) || 0,
            InvMasterId: invId,
            InvestigationName: $row.find('td:eq(0)').text(),
            SpecimenType: $row.find('.specimen-type').val(),
            ContainerType: $row.find('.container-type').val(),
            FastingRequired: $row.find('.fasting-required').is(':checked'),
            SampleStatus: status,
            CollectedQuantity: status === 'Collected' ? $row.find('.collected-quantity').val() : null,
            RejectionReason: status === 'Rejected' ? $row.find('.rejection-reason').val() : null,
            CollectionDate: status === 'Collected' ? now.toISOString().split('T')[0] : null,
            CollectionTime: status === 'Collected' ? now.toTimeString().slice(0, 5) : null
        };

        sampleDetails.push(detail);
    });

    if (sampleDetails.length === 0) {
        toastr.error('No samples to save');
        return;
    }

    console.log('Saving sample collection:', sampleCollection);
    console.log('Sample details:', sampleDetails);

    $.ajax({
        url: AppRoutes.SampleCollection.saveCollection,
        type: 'POST',
        data: {
            sampleCollection: sampleCollection,
            sampleDetails: sampleDetails
        },
        success: function (response) {
            console.log('Save response:', response);

            if (response.success) {
                toastr.success(response.message + ' - Barcode: ' + response.collectionBarcode);
                $('#sampleCollectionModal').modal('hide');
                $('#collectionsTable').DataTable().ajax.reload();

                setTimeout(function () {
                    if (confirm('Sample collection created! Do you want to print labels?')) {
                        window.open('/SampleCollection/PrintCollectionLabels/' + response.sampleCollectionId, '_blank');
                    }
                }, 500);
            } else {
                toastr.error('Error: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('Save Error:', error);
            console.error('Response:', xhr.responseText);
            toastr.error('Error saving sample collection: ' + error);
        }
    });
}

function validateSampleCollection() {
    if (!$('#scCollectionDate').val()) {
        toastr.error('Please select collection date');
        return false;
    }
    if (!$('#scCollectionTime').val()) {
        toastr.error('Please select collection time');
        return false;
    }
    if (!$('#scCollectedBy').val()) {
        toastr.error('Please enter collected by name');
        return false;
    }
    return true;
}

function printCollectionLabels(sampleCollectionId) {
    window.open(AppRoutes.SampleCollection.printLabels + '/' + sampleCollectionId, '_blank');
}

function updateStatus(sampleCollectionId) {
    toastr.info('Update status functionality will be implemented.');
}

function applyFilters() {
    var table = $('#collectionsTable').DataTable();

    var status = $('#statusFilter').val();
    var priority = $('#priorityFilter').val();

    // Apply column filters
    table.column(6).search(priority).column(7).search(status).draw();
}

function refreshCollections() {
    $('#collectionsTable').DataTable().ajax.reload();
    toastr.info('Collections refreshed.');
}

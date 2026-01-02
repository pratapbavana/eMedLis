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

    // STEP 1: Get bill and investigation details
    $.ajax({
        url: '/SampleCollection/GetCollectionData/' + billSummaryId,
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            console.log('GetCollectionData Response:', response);

            if (response.success) {
                // Step 1: Populate modal with bill and investigation details
                populateSampleCollectionModal(response.data);

                // Step 2: Check if this bill already has a sample collection
                var sampleCollectionId = response.data.sampleCollectionId;

                if (sampleCollectionId && sampleCollectionId > 0) {
                    console.log('Bill has existing collection, loading it:', sampleCollectionId);

                    // IMPORTANT: Load and APPLY existing collection status AFTER populating the form
                    loadAndApplyExistingCollection(sampleCollectionId);
                } else {
                    console.log('New collection for this bill');
                    // Show modal for new collection
                    $('#sampleCollectionModal').modal('show');
                }
            } else {
                toastr.error('Error: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX Error:', error);
            toastr.error('Error loading collection details');
        }
    });
}

function loadAndApplyExistingCollection(sampleCollectionId) {
    console.log('Loading and applying existing collection:', sampleCollectionId);

    $.ajax({
        url: '/SampleCollection/GetCollectionDetails/' + sampleCollectionId,
        type: 'GET',
        dataType: 'json',
        timeout: 10000,
        success: function (response) {
            console.log('GetCollectionDetails response:', response);

            if (response.success && response.data) {
                console.log('Collection loaded successfully, applying status...');
                applyExistingCollectionStatus(response.data);
                $('#sampleCollectionModal').modal('show');
            } else {
                console.warn('Collection load failed:', response.message);
                toastr.warning('Could not load existing collection details');
                $('#sampleCollectionModal').modal('show');
            }
        },
        error: function (xhr, status, error) {
            console.error('AJAX Error Status:', xhr.status);
            console.error('AJAX Error:', error);
            console.error('Response Text:', xhr.responseText);

            if (xhr.status === 500) {
                toastr.error('Server error loading collection details');
            } else if (xhr.status === 404) {
                toastr.error('Collection not found');
            } else {
                toastr.error('Error loading collection details: ' + error);
            }

            // Still show modal
            $('#sampleCollectionModal').modal('show');
        }
    });
}

function applyExistingCollectionStatus(collectionData) {
    console.log('Applying collection status:', collectionData);

    if (!collectionData.sampleDetails || collectionData.sampleDetails.length === 0) {
        console.warn('No sample details to apply');
        return;
    }

    // Update master info
    $('#scCollectionBarcode').text(collectionData.collectionBarcode || 'New');
    $('#scPriority').val(collectionData.priority || 'Normal');
    $('#scCollectedBy').val(collectionData.collectedBy || 'Admin');
    $('#scRemarks').val(collectionData.remarks || '');
    $('#scHomeCollection').prop('checked', collectionData.homeCollection || false);

    // Update each sample row
    var applied = 0;
    $('#scSamplesTable tbody tr').each(function () {
        var invId = parseInt($(this).data('inv-id'));
        var $row = $(this);

        // Find matching detail
        var detail = collectionData.sampleDetails.find(d => d.invMasterId === invId);

        if (detail) {
            applied++;
            console.log('Applying detail for InvId ' + invId + ': Status=' + detail.sampleStatus);

            $row.data('sample-detail-id', detail.sampleDetailId);

            // Set status
            var statusDropdown = $row.find('.sample-status');
            statusDropdown.val(detail.sampleStatus);

            if (detail.sampleStatus === 'Collected') {
                // Disable - already collected
                statusDropdown.prop('disabled', true).addClass('bg-success text-white');
                $row.find('.collected-quantity')
                    .val(detail.collectedQuantity || '')
                    .prop('disabled', true);
                $row.find('.rejection-reason').hide();

                if (detail.collectionDate || detail.collectionTime) {
                    var ts = detail.collectionDate + ' ' + (detail.collectionTime || '');
                    $row.find('.collection-datetime').val(ts.trim());
                }

            } else if (detail.sampleStatus === 'Rejected') {
                // Disable - already rejected
                statusDropdown.prop('disabled', true).addClass('bg-danger text-white');
                $row.find('.collected-quantity').prop('disabled', true).val('');
                $row.find('.rejection-reason')
                    .val(detail.rejectionReason || '')
                    .show()
                    .prop('disabled', true);

                if (detail.rejectionDate) {
                    $row.find('.collection-datetime').val('Rejected: ' + detail.rejectionDate);
                }

            } else {
                // Pending - allow edit
                statusDropdown.prop('disabled', false)
                    .removeClass('bg-success text-white bg-danger text-white');
                $row.find('.collected-quantity').prop('disabled', false).val('');
                $row.find('.rejection-reason').hide().val('');
                $row.find('.collection-datetime').val('');
            }
        }
    });

    console.log('Applied status to ' + applied + ' samples');
}

function updateEachSampleWithExistingStatus(sampleDetails) {
    console.log('Updating each sample with existing status:', sampleDetails);

    $('#scSamplesTable tbody tr').each(function () {
        var invId = parseInt($(this).data('inv-id'));
        var $row = $(this);

        // Find the existing detail for this investigation
        var existingDetail = sampleDetails.find(d => d.invMasterId === invId);

        if (existingDetail) {
            console.log('Found existing detail for InvId ' + invId + ': Status=' + existingDetail.sampleStatus);

            // Store sample detail ID for tracking
            $row.data('sample-detail-id', existingDetail.sampleDetailId || 0);

            // Update status dropdown with actual status
            var statusDropdown = $row.find('.sample-status');
            statusDropdown.val(existingDetail.sampleStatus);
            console.log('Set dropdown to:', existingDetail.sampleStatus);

            // Apply styling and disable/enable based on status
            if (existingDetail.sampleStatus === 'Collected') {
                console.log('Sample is COLLECTED - disabling for edit');

                // Disable editing
                statusDropdown.prop('disabled', true)
                    .addClass('bg-success text-white');

                $row.find('.collected-quantity').val(existingDetail.collectedQuantity || '')
                    .prop('disabled', true);

                $row.find('.rejection-reason').hide();

                // Show collection timestamp
                if (existingDetail.collectionDate) {
                    var timestamp = existingDetail.collectionDate;
                    if (existingDetail.collectionTime) {
                        timestamp += ' ' + existingDetail.collectionTime;
                    }
                    $row.find('.collection-datetime').val(timestamp);
                }

            } else if (existingDetail.sampleStatus === 'Rejected') {
                console.log('Sample is REJECTED - disabling for edit');

                // Disable editing
                statusDropdown.prop('disabled', true)
                    .addClass('bg-danger text-white');

                $row.find('.collected-quantity').prop('disabled', true).val('');
                $row.find('.rejection-reason').val(existingDetail.rejectionReason || '')
                    .show()
                    .prop('disabled', true);

                // Show rejection timestamp
                if (existingDetail.rejectionDate) {
                    $row.find('.collection-datetime').val('Rejected: ' + existingDetail.rejectionDate);
                }

            } else {
                console.log('Sample is PENDING - allowing edit');

                // Allow editing
                statusDropdown.prop('disabled', false)
                    .removeClass('bg-success text-white bg-danger text-white');

                $row.find('.collected-quantity').prop('disabled', false).val('');
                $row.find('.rejection-reason').hide().val('');
                $row.find('.collection-datetime').val('');
            }
        } else {
            console.log('No existing detail found for InvId ' + invId + ' - treating as new');

            // No existing record - all fields editable, status Pending
            $row.data('sample-detail-id', 0);
            $row.find('.sample-status').val('Pending').prop('disabled', false)
                .removeClass('bg-success text-white bg-danger text-white');
            $row.find('.collected-quantity').val('').prop('disabled', false);
            $row.find('.rejection-reason').hide().val('');
            $row.find('.collection-datetime').val('');
        }
    });
}

function updateModalWithExistingData(sampleDetails) {
    console.log('Updating modal with existing samples:', sampleDetails);

    $('#scSamplesTable tbody tr').each(function () {
        var invId = parseInt($(this).data('inv-id'));
        var $row = $(this);

        // Find matching existing detail by InvMasterId
        var existingDetail = sampleDetails.find(d => d.invMasterId === invId);

        if (existingDetail) {
            console.log('Found existing detail for InvId:', invId, existingDetail);

            // Store the sample detail ID for updates
            $row.data('sample-detail-id', existingDetail.sampleDetailId || 0);

            // Set the status dropdown to the existing status
            var statusDropdown = $row.find('.sample-status');
            statusDropdown.val(existingDetail.sampleStatus || 'Pending');

            console.log('Set status to:', existingDetail.sampleStatus);

            // Handle based on current status
            if (existingDetail.sampleStatus === 'Collected') {
                // Show as collected - disable editing
                statusDropdown.prop('disabled', true)
                    .addClass('bg-success text-white');

                $row.find('.collected-quantity').val(existingDetail.collectedQuantity || '')
                    .prop('disabled', true);

                // Show collection date/time with timestamp
                if (existingDetail.collectionDate) {
                    var dateStr = existingDetail.collectionDate;
                    var timeStr = existingDetail.collectionTime || '';
                    var timestamp = dateStr + ' ' + timeStr;
                    $row.find('.collection-datetime').val(timestamp);
                    console.log('Displayed collection time:', timestamp);
                }

                $row.find('.rejection-reason').hide();

            } else if (existingDetail.sampleStatus === 'Rejected') {
                // Show as rejected - disable editing
                statusDropdown.prop('disabled', true)
                    .addClass('bg-danger text-white');

                $row.find('.collected-quantity').prop('disabled', true).val('');
                $row.find('.rejection-reason').val(existingDetail.rejectionReason || '')
                    .show()
                    .prop('disabled', true);

                // Show rejection date
                if (existingDetail.rejectionDate) {
                    var rejectionDate = new Date(existingDetail.rejectionDate).toLocaleDateString('en-GB');
                    $row.find('.collection-datetime').val('Rejected: ' + rejectionDate);
                }

            } else {
                // Status is Pending - allow editing
                statusDropdown.prop('disabled', false)
                    .removeClass('bg-success text-white bg-danger text-white');

                $row.find('.collected-quantity').prop('disabled', false);
                $row.find('.rejection-reason').hide();
                $row.find('.collection-datetime').val('');
            }
        } else {
            console.log('No existing detail found for InvId:', invId);
            // New sample - all fields editable
            $row.find('.sample-status').val('Pending').prop('disabled', false);
            $row.find('.collected-quantity').prop('disabled', false).val('');
            $row.find('.rejection-reason').hide();
            $row.find('.collection-datetime').val('');
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
    console.log('Populating modal with bill data:', data);

    // Set patient info
    $('#scPatName').text(data.patientInfo?.patName || '');
    $('#scUHID').text(data.patientInfo?.uhid || '');
    $('#scMobile').text(data.patientInfo?.mobileNo || '');
    $('#scAgeGender').text((data.patientInfo?.age || '') + ' / ' + (data.patientInfo?.gender || ''));
    $('#scAddress').text((data.patientInfo?.area || '') + ', ' + (data.patientInfo?.city || ''));

    // Set bill info
    $('#scBillNo').text(data.billNo || '');
    $('#scCollectionBarcode').text(data.collectionBarcode || 'New');

    // Set default date/time for NEW collections only
    if (!data.sampleCollectionId || data.sampleCollectionId === 0) {
        var now = new Date();
        $('#scCollectionDate').val(now.toISOString().split('T')[0]);
        $('#scCollectionTime').val(now.toTimeString().slice(0, 5));
    }

    // Clear and populate samples table with investigations from bill
    var tbody = $('#scSamplesTable tbody');
    tbody.empty();

    if (!data.billDetails || data.billDetails.length === 0) {
        tbody.append('<tr><td colspan="8">No investigations found for this bill</td></tr>');
        return;
    }

    // Add each investigation as a row with default "Pending" status
    // (Real status will be applied by applyExistingCollectionStatus if it exists)
    data.billDetails.forEach(function (item, index) {
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

    // Reset other fields to defaults
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
        var sampleDetailId = parseInt($row.data('sample-detail-id')) || 0;
        var status = $row.find('.sample-status').val();

        if (!invId || invId == 0) {
            console.warn('Skipping row with invalid InvId');
            return true;
        }

        // Only include if status has changed OR it's a new sample
        var detail = {
            SampleDetailId: sampleDetailId,  // Will be 0 for new, >0 for existing
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
                toastr.success('Sample collection saved - Barcode: ' + response.collectionBarcode);
                $('#sampleCollectionModal').modal('hide');
                $('#collectionsTable').DataTable().ajax.reload();

                setTimeout(function () {
                    if (confirm('Do you want to print labels?')) {
                        window.open('/SampleCollection/PrintCollectionLabels/' + response.sampleCollectionId, '_blank');
                    }
                }, 500);
            } else {
                toastr.error('Error: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            console.error('Save Error:', error, xhr.responseText);
            toastr.error('Error saving collection');
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

function formatDateForInput(dateValue) {
    if (!dateValue) {
        return '';
    }

    try {
        // If already a string in YYYY-MM-DD format
        if (typeof dateValue === 'string') {
            if (dateValue.match(/^\d{4}-\d{2}-\d{2}/)) {
                return dateValue.substring(0, 10);
            }

            // If DD/MM/YYYY format
            if (dateValue.includes('/')) {
                var parts = dateValue.split('/');
                if (parts.length === 3) {
                    return parts[2] + '-' + parts[1] + '-' + parts[0];
                }
            }
        }

        // If JavaScript Date object
        if (dateValue instanceof Date && !isNaN(dateValue)) {
            return dateValue.toISOString().split('T')[0];
        }

        return '';
    } catch (e) {
        console.error('Date formatting error:', e);
        return '';
    }
}

/**
 * Convert time string to HH:MM format
 */
function formatTimeForInput(timeValue) {
    if (!timeValue) {
        return '';
    }

    try {
        // If already HH:MM or HH:MM:SS format
        if (typeof timeValue === 'string') {
            return timeValue.substring(0, 5);
        }

        // If TimeSpan object or milliseconds
        if (typeof timeValue === 'number') {
            var hours = Math.floor(timeValue / 3600000);
            var minutes = Math.floor((timeValue % 3600000) / 60000);
            return String(hours).padStart(2, '0') + ':' + String(minutes).padStart(2, '0');
        }

        return '';
    } catch (e) {
        console.error('Time formatting error:', e);
        return '';
    }
}

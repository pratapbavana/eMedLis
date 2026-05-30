$(document).ready(function () {
    bindLabMasterEvents();
    loadLabMaster();
});

function bindLabMasterEvents() {
    $('#btnLabMasterSave').on('click', function () {
        saveLabMaster();
    });

    $('#lmLogoFile').on('change', function (e) {
        readImageFile(e, '#lmLogoBase64', function (dataUrl) {
            setLogoPreview(dataUrl, !!dataUrl);
        });
    });

    $('#lmHeaderFile').on('change', function (e) {
        readImageFile(e, '#lmHeaderBase64', null);
    });

    $('#lmFooterFile').on('change', function (e) {
        readImageFile(e, '#lmFooterBase64', null);
    });
}

function loadLabMaster() {
    $.ajax({
        url: '/LabMaster/Get',
        type: 'GET',
        dataType: 'json',
        success: function (x) {
            if (!x) return;

            $('#lmLabName').val(x.LabName || '');
            $('#lmShortName').val(x.ShortName || '');
            $('#lmTagline').val(x.Tagline || '');
            $('#lmAddressLine1').val(x.AddressLine1 || '');
            $('#lmAddressLine2').val(x.AddressLine2 || '');
            $('#lmCity').val(x.City || '');
            $('#lmState').val(x.State || '');
            $('#lmPincode').val(x.Pincode || '');
            $('#lmCountry').val(x.Country || '');
            $('#lmMobile').val(x.MobileNumber || '');
            $('#lmAltMobile').val(x.AlternateMobile || '');
            $('#lmLandline').val(x.Landline || '');
            $('#lmEmail').val(x.Email || '');
            $('#lmWebsite').val(x.Website || '');
            $('#lmGST').val(x.GSTNumber || '');
            $('#lmPAN').val(x.PANNumber || '');
            $('#lmRegNo').val(x.LabRegistrationNumber || '');
            $('#lmNABL').val(x.NABLNumber || '');
            $('#lmDrugLicense').val(x.DrugLicenseNumber || '');
            $('#lmBranchName').val(x.BranchName || '');
            $('#lmBranchCode').val(x.BranchCode || '');
            $('#lmReceiptFooter').val(x.ReceiptFooter || '');
            $('#lmShowLogo').prop('checked', toBool(x.ShowLogoInReport, true));
            $('#lmShowGst').prop('checked', toBool(x.ShowGSTInReport, true));
            $('#lmShowAccreditation').prop('checked', toBool(x.ShowAccreditationInReport, true));
            $('#lmActive').prop('checked', toBool(x.Active, true));

            if (toBool(x.HasLogo, false)) {
                setLogoPreview('/LabMaster/Image?type=Logo&_=' + new Date().getTime(), true);
            } else {
                setLogoPreview('', false);
            }
        },
        error: function () {
            toastr.error('Failed to load lab profile');
        }
    });
}

function saveLabMaster() {
    var payload = {
        LabName: ($('#lmLabName').val() || '').trim(),
        ShortName: ($('#lmShortName').val() || '').trim(),
        Tagline: ($('#lmTagline').val() || '').trim(),
        AddressLine1: ($('#lmAddressLine1').val() || '').trim(),
        AddressLine2: ($('#lmAddressLine2').val() || '').trim(),
        City: ($('#lmCity').val() || '').trim(),
        State: ($('#lmState').val() || '').trim(),
        Pincode: ($('#lmPincode').val() || '').trim(),
        Country: ($('#lmCountry').val() || '').trim(),
        MobileNumber: ($('#lmMobile').val() || '').trim(),
        AlternateMobile: ($('#lmAltMobile').val() || '').trim(),
        Landline: ($('#lmLandline').val() || '').trim(),
        Email: ($('#lmEmail').val() || '').trim(),
        Website: ($('#lmWebsite').val() || '').trim(),
        GSTNumber: ($('#lmGST').val() || '').trim(),
        PANNumber: ($('#lmPAN').val() || '').trim(),
        LabRegistrationNumber: ($('#lmRegNo').val() || '').trim(),
        NABLNumber: ($('#lmNABL').val() || '').trim(),
        DrugLicenseNumber: ($('#lmDrugLicense').val() || '').trim(),
        BranchName: ($('#lmBranchName').val() || '').trim(),
        BranchCode: ($('#lmBranchCode').val() || '').trim(),
        ReceiptFooter: ($('#lmReceiptFooter').val() || '').trim(),
        ShowLogoInReport: $('#lmShowLogo').prop('checked'),
        ShowGSTInReport: $('#lmShowGst').prop('checked'),
        ShowAccreditationInReport: $('#lmShowAccreditation').prop('checked'),
        Active: $('#lmActive').prop('checked'),
        LogoBase64: $('#lmLogoBase64').val() || '',
        ReportHeaderImageBase64: $('#lmHeaderBase64').val() || '',
        ReportFooterImageBase64: $('#lmFooterBase64').val() || ''
    };

    if (!payload.LabName) {
        toastr.error('Lab Name is required');
        return;
    }

    $.ajax({
        url: '/LabMaster/Save',
        type: 'POST',
        contentType: 'application/json;charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(payload),
        success: function (res) {
            var code = res ? parseInt(res.Item1, 10) : 0;
            if (code === 1) {
                toastr.success(res.Item2 || 'Saved');
                $('#lmLogoBase64').val('');
                $('#lmHeaderBase64').val('');
                $('#lmFooterBase64').val('');
                $('#lmLogoFile').val('');
                $('#lmHeaderFile').val('');
                $('#lmFooterFile').val('');
                loadLabMaster();
            } else {
                toastr.error(res && res.Item2 ? res.Item2 : 'Save failed');
            }
        },
        error: function () {
            toastr.error('Save failed');
        }
    });
}

function readImageFile(e, targetInput, onDone) {
    var file = (e.target.files || [])[0];
    if (!file) {
        $(targetInput).val('');
        if (onDone) onDone('');
        return;
    }

    var allowed = ['image/png', 'image/jpeg', 'image/jpg'];
    if (allowed.indexOf((file.type || '').toLowerCase()) < 0) {
        toastr.error('Only PNG/JPG files are allowed');
        e.target.value = '';
        $(targetInput).val('');
        if (onDone) onDone('');
        return;
    }

    var reader = new FileReader();
    reader.onload = function (evt) {
        var dataUrl = evt.target.result || '';
        $(targetInput).val(dataUrl);
        if (onDone) onDone(dataUrl);
    };
    reader.onerror = function () {
        toastr.error('Unable to read image');
        e.target.value = '';
        $(targetInput).val('');
        if (onDone) onDone('');
    };
    reader.readAsDataURL(file);
}

function setLogoPreview(src, hasValue) {
    if (hasValue && src) {
        $('#lmLogoPreview').attr('src', src).show();
        $('#lmNoLogo').hide();
    } else {
        $('#lmLogoPreview').attr('src', '').hide();
        $('#lmNoLogo').show();
    }
}

function toBool(value, fallback) {
    if (value === undefined || value === null) return fallback;
    if (typeof value === 'boolean') return value;
    var t = (value + '').toLowerCase();
    return t === 'true' || t === '1';
}

$(document).ready(function () {
    bindReportSettingsEvents();
    loadReportSettings();
});

function bindReportSettingsEvents() {
    $('#btnRsPreview').on('click', function () {
        renderReportSettingsPreview(buildReportSettingsPayload());
    });

    $('#btnRsSave').on('click', function () {
        saveReportSettings();
    });

    $('#reportSettingsForm').on('change input', 'input, textarea, select', function () {
        renderReportSettingsPreview(buildReportSettingsPayload());
    });
}

function loadReportSettings() {
    $.ajax({
        url: '/ReportSettings/Get',
        type: 'GET',
        dataType: 'json',
        success: function (res) {
            setReportSettingsForm(res || {});
            renderReportSettingsPreview(buildReportSettingsPayload());
        },
        error: function () {
            toastr.error('Failed to load report settings');
            renderReportSettingsPreview(buildReportSettingsPayload());
        }
    });
}

function saveReportSettings() {
    var payload = buildReportSettingsPayload();
    $.ajax({
        url: '/ReportSettings/Save',
        type: 'POST',
        contentType: 'application/json;charset=utf-8',
        dataType: 'json',
        data: JSON.stringify(payload),
        success: function (res) {
            if (res && parseInt(res.Item1, 10) === 1) {
                toastr.success(res.Item2 || 'Saved');
            } else {
                toastr.error(res && res.Item2 ? res.Item2 : 'Save failed');
            }
        },
        error: function () {
            toastr.error('Save failed');
        }
    });
}

function setReportSettingsForm(data) {
    var mode = (data.PrintMode || 'PlainPaper') === 'PrePrinted' ? 'PrePrinted' : 'PlainPaper';
    $('input[name="rsPrintMode"][value="' + mode + '"]').prop('checked', true);
    $('#rsPrintHeader').prop('checked', toBool(data.PrintHeader, true));
    $('#rsHeaderHeightPx').val(toInt(data.HeaderHeightPx, 120));
    $('#rsPrintFooter').prop('checked', toBool(data.PrintFooter, true));
    $('#rsFooterHeightPx').val(toInt(data.FooterHeightPx, 60));
    $('#rsFooterText').val(data.FooterText || 'This is a system generated report.');
    $('#rsTopMarginPx').val(toInt(data.TopMarginPx, 38));
    $('#rsLeftMarginPx').val(toInt(data.LeftMarginPx, 38));
    $('#rsRightMarginPx').val(toInt(data.RightMarginPx, 38));
    $('#rsBottomMarginPx').val(toInt(data.BottomMarginPx, 38));
    $('#rsContentStartPx').val(toInt(data.ContentStartPx, 0));
}

function buildReportSettingsPayload() {
    return {
        PrintMode: $('input[name="rsPrintMode"]:checked').val() || 'PlainPaper',
        PrintHeader: $('#rsPrintHeader').prop('checked'),
        HeaderHeightPx: toInt($('#rsHeaderHeightPx').val(), 120),
        ShowLogo: true,
        ShowLabDetails: true,
        PrintFooter: $('#rsPrintFooter').prop('checked'),
        FooterHeightPx: toInt($('#rsFooterHeightPx').val(), 60),
        FooterText: ($('#rsFooterText').val() || '').trim(),
        TopMarginPx: toInt($('#rsTopMarginPx').val(), 38),
        LeftMarginPx: toInt($('#rsLeftMarginPx').val(), 38),
        RightMarginPx: toInt($('#rsRightMarginPx').val(), 38),
        BottomMarginPx: toInt($('#rsBottomMarginPx').val(), 38),
        ContentStartPx: toInt($('#rsContentStartPx').val(), 0),
        LabName: '',
        LabAddress: '',
        LabPhone: ''
    };
}

function renderReportSettingsPreview(s) {
    s = s || buildReportSettingsPayload();
    var prePrinted = s.PrintMode === 'PrePrinted';
    var printHeader = s.PrintHeader && !prePrinted;
    var headerHeight = printHeader ? s.HeaderHeightPx : 0;
    var contentTop = prePrinted ? s.ContentStartPx : 0;

    var html = '';
    html += '<div style="background:#fff; width:100%; height:100%; box-sizing:border-box; padding:' + s.TopMarginPx + 'px ' + s.RightMarginPx + 'px ' + s.BottomMarginPx + 'px ' + s.LeftMarginPx + 'px;">';

    if (printHeader) {
        html += '<div style="height:' + headerHeight + 'px; border-bottom:1px solid #dee2e6; margin-bottom:8px; display:flex; align-items:center; justify-content:center;">';
        html += '<img src="/LabMaster/Image?type=Header" style="max-width:100%;max-height:100%;object-fit:contain;" onerror="this.style.display=\'none\'; this.parentNode.innerHTML=\'<div style=&quot;color:#6c757d;font-size:12px;&quot;>Lab header image not configured in Lab Master</div>\';" />';
        html += '</div>';
    } else if (prePrinted && contentTop > 0) {
        html += '<div style="height:' + contentTop + 'px; border:1px dashed #ced4da; margin-bottom:8px; color:#6c757d; font-size:12px; padding:4px;">Reserved top area for pre-printed header</div>';
    }

    html += '<div style="border:1px solid #dee2e6; min-height:180px; padding:8px;">';
    html += '<div style="font-weight:600;">Patient Details</div>';
    html += '<div style="font-size:13px; color:#495057;">Patient: Ramesh | Age/Gender: 35 / Male | Investigation: CBC</div>';
    html += '<hr />';
    html += '<div style="font-size:13px;">Report content starts here</div>';
    html += '</div>';

    if (s.PrintFooter) {
        html += '<div style="height:' + s.FooterHeightPx + 'px; border-top:1px solid #dee2e6; margin-top:8px; font-size:12px; color:#495057; padding-top:6px;">';
        if (!prePrinted) {
            html += '<div style="height:' + Math.max(0, s.FooterHeightPx - 24) + 'px; display:flex; align-items:center; justify-content:center;">' +
                '<img src="/LabMaster/Image?type=Footer" style="max-width:100%;max-height:100%;object-fit:contain;" onerror="this.style.display=\'none\';" />' +
                '</div>';
        }
        html += '<div>' + htmlEncode(s.FooterText || '') + '</div>';
        html += '</div>';
    }

    html += '</div>';
    $('#rsPreviewPane').html(html);
}

function toInt(value, fallback) {
    var n = parseInt(value, 10);
    return isNaN(n) ? fallback : n;
}

function toBool(value, fallback) {
    if (value === undefined || value === null) return fallback;
    if (typeof value === 'boolean') return value;
    var t = (value + '').toLowerCase();
    return t === 'true' || t === '1';
}

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}

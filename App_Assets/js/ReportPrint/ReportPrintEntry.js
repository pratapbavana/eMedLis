var rpeCurrentPreviewHtml = '';
var rpePreviewDocuments = [];
var rpeLayoutSettings = getDefaultRpeLayoutSettings();
var rpeLabProfile = null;

$(document).ready(function () {
    ensureRpePreviewStyles();
    bindRpeEvents();

    if (!reportPrintBillSummaryId || parseInt(reportPrintBillSummaryId, 10) <= 0) {
        toastr.error('Invalid bill selected');
        return;
    }

    loadBillInvestigations(parseInt(reportPrintBillSummaryId, 10));
});

function bindRpeEvents() {
    $('#rpeSelectAll').on('change', function () {
        $('.rpe-row-select').prop('checked', $(this).prop('checked'));
    });

    $('#btnRpePreview').on('click', function () {
        fetchPreviewDocuments(false);
    });

    $('#btnRpePrint').on('click', function () {
        fetchPreviewDocuments(true);
    });

    $('#btnRpeExportPdf').on('click', function () {
        if (!rpePreviewDocuments.length) {
            fetchPreviewDocuments(false, function () {
                exportRpePdf();
            });
            return;
        }
        exportRpePdf();
    });

    $('#btnPreviewBack').on('click', function () {
        $('#rpePreviewModal').modal('hide');
    });

    $('#btnPreviewPrint').on('click', function () {
        doRpePrint();
    });

    $('#btnPreviewPdf').on('click', function () {
        exportRpePdf();
    });

    $('input[name="rpePrintOption"]').on('change', function () {
        var mode = $('input[name="rpePrintOption"]:checked').val() || 'Individual';
        $('input[name="rpePreviewMode"][value="' + mode + '"]').prop('checked', true);
        if (rpePreviewDocuments.length) {
            renderPreviewFromState();
        }
    });

    $('input[name="rpePreviewMode"]').on('change', function () {
        var mode = $('input[name="rpePreviewMode"]:checked').val() || 'Individual';
        $('input[name="rpePrintOption"][value="' + mode + '"]').prop('checked', true);
        if (rpePreviewDocuments.length) {
            renderPreviewFromState();
        }
    });
}

function loadBillInvestigations(billSummaryId) {
    $.ajax({
        url: '/ReportPrint/GetBillInvestigations',
        type: 'GET',
        dataType: 'json',
        data: { billSummaryId: billSummaryId },
        success: function (rows) {
            renderRpeRows(rows || []);
        },
        error: function () {
            toastr.error('Failed to load bill investigations');
        }
    });
}

function renderRpeRows(rows) {
    var tbody = $('#rpeGrid tbody');
    tbody.empty();

    if (!rows || rows.length === 0) {
        tbody.append('<tr><td colspan="6" class="text-center">No authorized investigations found for this bill</td></tr>');
        return;
    }

    var first = rows[0];
    $('#rpeBillNo').text(first.BillNo || '-');
    $('#rpePatientName').text(first.PatientName || '-');
    $('#rpeAgeGender').text((first.PatientAge || 0) + ' ' + normalizeAgeType(first.PatientAgeType) + ' / ' + normalizeGender(first.PatientGender));
    $('#rpeMobileNo').text(first.MobileNo || '-');

    rows.forEach(function (row) {
        var tr = '<tr>' +
            '<td><input type="checkbox" class="rpe-row-select" value="' + row.SampleDetailId + '" /></td>' +
            '<td>' + htmlEncode(row.SampleBarcode || '-') + '</td>' +
            '<td>' + htmlEncode(row.InvestigationName || '-') + '</td>' +
            '<td>' + htmlEncode(row.DepartmentName || '-') + '</td>' +
            '<td>' + htmlEncode(formatDate(row.CollectionDate)) + '</td>' +
            '<td><span class="badge badge-success">Authorized</span></td>' +
            '</tr>';
        tbody.append(tr);
    });
}

function getRpeSelectedIds() {
    var ids = [];
    $('.rpe-row-select:checked').each(function () {
        ids.push(parseInt($(this).val(), 10));
    });
    return ids;
}

function fetchPreviewDocuments(triggerPrint, onReady) {
    var ids = getRpeSelectedIds();
    if (!ids.length) {
        toastr.error('Select at least one investigation');
        return;
    }

    $.ajax({
        url: '/ReportPrint/GetPreviewData',
        type: 'POST',
        contentType: 'application/json;charset=utf-8',
        dataType: 'json',
        data: JSON.stringify({
            BillSummaryId: parseInt(reportPrintBillSummaryId, 10),
            SampleDetailIds: ids
        }),
        success: function (res) {
            if (!res || !res.success || !res.documents) {
                toastr.error(res && res.message ? res.message : 'Preview failed');
                return;
            }

            rpePreviewDocuments = res.documents || [];
            rpeLayoutSettings = normalizeRpeLayoutSettings(res.settings);
            rpeLabProfile = res.labProfile || null;
            ensureRpePreviewStyles();
            renderPreviewFromState();
            $('#rpePreviewModal').modal('show');

            if (triggerPrint) {
                setTimeout(function () { doRpePrint(); }, 250);
            }

            if (typeof onReady === 'function') {
                onReady();
            }
        },
        error: function () {
            toastr.error('Preview failed');
        }
    });
}

function renderPreviewFromState() {
    var mode = $('input[name="rpePreviewMode"]:checked').val() || 'Individual';
    var html = mode === 'Grouped'
        ? buildGroupedPreviewHtml(rpePreviewDocuments)
        : buildIndividualPreviewHtml(rpePreviewDocuments);

    rpeCurrentPreviewHtml = html;
    $('#rpePreviewBody').html(html);
}

function buildIndividualPreviewHtml(documents) {
    var html = '<div id="rpePrintableArea">';
    var rows = documents || [];
    rows.forEach(function (doc, idx) {
        html += renderDocumentPage(doc, idx < rows.length - 1);
    });
    html += '</div>';
    return html;
}

function buildGroupedPreviewHtml(documents) {
    var groups = {};
    (documents || []).forEach(function (doc) {
        var key = (doc.DepartmentName || 'Others');
        if (!groups[key]) groups[key] = [];
        groups[key].push(doc);
    });

    var html = '<div id="rpePrintableArea">';
    var depts = Object.keys(groups);
    depts.forEach(function (dept, idx) {
        var bodyHtml = '<h4 class="text-center mb-3">' + htmlEncode(dept) + '</h4>';
        groups[dept].forEach(function (doc, docIndex) {
            bodyHtml += renderDocumentBlock(doc);
            if (docIndex < groups[dept].length - 1) {
                bodyHtml += '<hr />';
            }
        });
        html += renderPageShell(bodyHtml, idx < depts.length - 1);
    });
    html += '</div>';
    return html;
}

function renderDocumentPage(doc, addPageBreak) {
    return renderPageShell(renderDocumentBlock(doc), addPageBreak);
}

function renderPageShell(contentHtml, addPageBreak) {
    var s = normalizeRpeLayoutSettings(rpeLayoutSettings);
    var lab = rpeLabProfile || {};
    var prePrinted = s.PrintMode === 'PrePrinted';
    var showHeader = s.PrintHeader && !prePrinted;
    var extraTop = prePrinted ? s.ContentStartPx : 0;
    var headerHtml = '';
    var footerHtml = '';
    var pageClass = 'rpe-report-page border bg-white mb-3';
    if (addPageBreak) {
        pageClass += ' rpe-page-break';
    }

    if (showHeader) {
        headerHtml = renderPageHeader(s, lab);
    } else if (prePrinted && extraTop > 0) {
        headerHtml = '<div class="rpe-preprinted-spacer" style="height:' + extraTop + 'px;"></div>';
    }

    if (s.PrintFooter) {
        footerHtml = renderPageFooter(s, lab, prePrinted);
    }

    var html = '<div class="' + pageClass + '">';
    html += '<div class="rpe-page-inner" style="padding:' + s.TopMarginPx + 'px ' + s.RightMarginPx + 'px ' + s.BottomMarginPx + 'px ' + s.LeftMarginPx + 'px;">';
    html += '<table class="rpe-page-layout">';
    html += '<thead><tr><td>' + headerHtml + '</td></tr></thead>';
    html += '<tbody><tr><td><div class="rpe-page-content">' + contentHtml + renderReportBodyFooterText(s) + '</div></td></tr></tbody>';
    html += '<tfoot><tr><td>' + footerHtml + '</td></tr></tfoot>';
    html += '</table>';
    html += '</div>';
    html += '</div>';
    return html;
}

function renderPageHeader(s, lab) {
    var profileShowLogo = toBool(lab.ShowLogoInReport, true);
    var logoSrc = toBool(lab.HasLogo, false) ? '/LabMaster/Image?type=Logo' : '/Content/Images/AdminLTELogo.png';
    var displayLabName = lab.LabName || s.LabName || 'SSK Diagnostics';
    var displayAddress = buildLabAddressText(lab) || s.LabAddress || '';
    var displayPhone = lab.MobileNumber || lab.Landline || s.LabPhone || '';
    var hasHeaderImage = toBool(lab.HasReportHeaderImage, false);

    var html = '<div class="rpe-page-header" style="min-height:' + s.HeaderHeightPx + 'px;">';

    if (hasHeaderImage) {
        html += '<div style="height:' + s.HeaderHeightPx + 'px; display:flex; align-items:center; justify-content:center; overflow:hidden;">' +
            '<img src="/LabMaster/Image?type=Header" alt="Lab Header" style="max-width:100%; max-height:100%; object-fit:contain;" />' +
            '</div>';
        html += '</div>';
        return html;
    }

    html += '<div class="d-flex align-items-center">';

    if (s.ShowLogo && profileShowLogo) {
        html += '<img src="' + logoSrc + '" alt="Lab Logo" style="width:56px;height:56px;margin-right:10px;" />';
    }

    if (s.ShowLabDetails) {
        html += '<div>';
        html += '<div class="font-weight-bold" style="font-size:18px;">' + htmlEncode(displayLabName) + '</div>';
        if (displayAddress) {
            html += '<div style="font-size:12px;">' + htmlEncode(displayAddress) + '</div>';
        }
        if (displayPhone) {
            html += '<div style="font-size:12px;">' + htmlEncode(displayPhone) + '</div>';
        }
        html += '</div>';
    }

    html += '</div></div>';
    return html;
}

function renderPageFooter(s, lab, prePrinted) {
    var html = '<div class="rpe-page-footer" style="min-height:' + s.FooterHeightPx + 'px;">';
    var hasFooterImage = toBool(lab.HasReportFooterImage, false);

    if (!prePrinted && hasFooterImage) {
        html += '<div style="min-height:' + Math.max(0, s.FooterHeightPx - 24) + 'px; display:flex; align-items:center; justify-content:center; overflow:hidden;">' +
            '<img src="/LabMaster/Image?type=Footer" alt="Lab Footer" style="max-width:100%; max-height:100%; object-fit:contain;" />' +
            '</div>';
    }

    html += '</div>';
    return html;
}

function renderReportBodyFooterText(s) {
    if (!s.FooterText) {
        return '';
    }

    return '<div class="rpe-body-footer-text">' + htmlEncode(s.FooterText) + '</div>';
}

function renderDocumentBlock(doc) {
    var html = '';
    html += '<h5 class="text-center mb-2">' + htmlEncode(doc.InvestigationName || '-') + '</h5>';
    html += renderReportInfoHeader(doc);

    html += renderParametersTable(doc.Parameters || []);

    html += '<div class="mt-3"><strong>Interpretation:</strong><br />' +
        '<div>' + htmlEncode(doc.DoctorInterpretation || '-') + '</div></div>';

    html += '<div class="row mt-4">' +
        '<div class="col-6"></div>' +
        '<div class="col-6 text-center">';
    if (doc.HasSignature && doc.AuthorizedDoctorId) {
        html += '<img src="/DoctorMaster/Signature/' + doc.AuthorizedDoctorId + '" style="max-width:200px; max-height:80px;" />';
    } else {
        html += '<div style="height:80px;"></div>';
    }
    html += '<div><strong>' + htmlEncode(doc.AuthorizedDoctor || '-') + '</strong></div>' +
        '<div>Authorized Doctor</div>' +
        '</div></div>';

    return html;
}

function renderReportInfoHeader(doc) {
    var ageSex = (doc.PatientAge || 0) + ' ' + normalizeAgeType(doc.PatientAgeType) + ' / ' + normalizeGender(doc.PatientGender);
    var qrUrl = buildReportQrUrl(doc);
    var html = '<div class="rpe-report-info mb-2">';
    html += '<div class="rpe-report-info-grid">';
    html += renderReportInfoItem('Name', doc.PatientName || '-');
    html += renderReportInfoItem('Age/Sex', ageSex);
    html += renderReportInfoItem('Mobile No', doc.MobileNo || '-');
    html += renderReportInfoItem('Test Registered', formatDateTime(doc.BillDate));
    html += renderReportInfoItem('Bill No', doc.BillNo || '-');
    html += renderReportInfoItem('Test Accepted', formatDateTime(doc.CollectionDate));
    html += renderReportInfoItem('Refferal Doctor', doc.ReferralDoctor || '-');
    html += renderReportInfoItem('Test Reported', formatDateTime(doc.AuthorizedOn));
    html += '</div>';
    html += '<div class="rpe-report-qr"><img src="' + htmlEncode(qrUrl) + '" alt="Report QR" /></div>';
    html += '</div>';
    return html;
}

function renderReportInfoItem(label, value) {
    return '<div class="rpe-report-info-item"><span>' + htmlEncode(label) + ':</span> ' + htmlEncode(value || '-') + '</div>';
}

function buildReportQrUrl(doc) {
    var reportUrl = 'https://example.com/report/' + encodeURIComponent(doc.BillNo || doc.SampleDetailId || 'sample');
    return 'https://chart.googleapis.com/chart?cht=qr&chs=82x82&chl=' + encodeURIComponent(reportUrl);
}

function renderParametersTable(parameters) {
    if (!parameters.length) {
        return '<div class="text-muted">No parameters found.</div>';
    }

    var html = '<table class="table table-sm table-bordered"><thead><tr>' +
        '<th style="width:30%;">Parameter</th>' +
        '<th style="width:15%;">Method</th>' +
        '<th style="width:15%;">Result</th>' +
        '<th style="width:10%;">Unit</th>' +
        '<th style="width:20%;">Range</th>' +
        '<th style="width:10%;">Flag</th>' +
        '</tr></thead><tbody>';

    var currentHeader = '__NONE__';
    parameters.forEach(function (p) {
        var header = (p.HeaderName || '').trim();
        if (header && header !== currentHeader) {
            currentHeader = header;
            html += '<tr class="bg-light"><td colspan="6"><strong>' + htmlEncode(header) + '</strong></td></tr>';
        }
        if (!header) {
            currentHeader = '__NONE__';
        }

        var rowClass = '';
        var flag = p.Flag || '';
        if (p.IsCritical || flag === 'C') rowClass = 'table-danger';
        else if (flag === 'H') rowClass = 'table-warning';
        else if (flag === 'L') rowClass = 'table-info';

        html += '<tr class="' + rowClass + '">' +
            '<td>' + htmlEncode(p.ParameterName || '-') + '</td>' +
            '<td>' + htmlEncode(p.MethodName || '-') + '</td>' +
            '<td>' + htmlEncode(p.ResultValue || '-') + '</td>' +
            '<td>' + htmlEncode(p.Unit || '-') + '</td>' +
            '<td>' + htmlEncode(p.NormalRange || '-') + '</td>' +
            '<td class="text-center"><strong>' + htmlEncode(flag) + '</strong></td>' +
            '</tr>';
    });

    html += '</tbody></table>';
    return html;
}

function doRpePrint() {
    if (!rpeCurrentPreviewHtml) {
        toastr.error('Preview not available');
        return;
    }

    var w = window.open('', '_blank');
    if (!w) {
        toastr.error('Popup blocked');
        return;
    }

    w.document.write('<html><head><title>Report Preview</title>');
    w.document.write('<link rel="stylesheet" href="/Content/bootstrap.min.css">');
    w.document.write('<style>' + getRpePrintStyles() + '</style>');
    w.document.write('</head><body class="rpe-print-body">');
    w.document.write(renderPrintFixedFooter());
    w.document.write(rpeCurrentPreviewHtml);
    w.document.write('</body></html>');
    w.document.close();
    w.focus();
    setTimeout(function () {
        w.print();
    }, 250);
}

function renderPrintFixedFooter() {
    var s = normalizeRpeLayoutSettings(rpeLayoutSettings);
    if (!s.PrintFooter) {
        return '';
    }

    var lab = rpeLabProfile || {};
    var prePrinted = s.PrintMode === 'PrePrinted';
    var hasFooterImage = !prePrinted && toBool(lab.HasReportFooterImage, false);
    var html = '<div class="rpe-print-fixed-footer">';

    if (hasFooterImage) {
        html += '<img src="/LabMaster/Image?type=Footer" alt="Lab Footer" />';
    }

    html += '</div>';
    return html;
}

function exportRpePdf() {
    if (!rpeCurrentPreviewHtml) {
        toastr.error('Preview not available');
        return;
    }

    var target = document.getElementById('rpePrintableArea');
    if (!target || typeof html2canvas === 'undefined' || !window.jspdf) {
        toastr.error('PDF export library not available');
        return;
    }

    var pages = target.querySelectorAll('.rpe-report-page');
    var pdf = new window.jspdf.jsPDF('p', 'mm', 'a4');
    var s = normalizeRpeLayoutSettings(rpeLayoutSettings);
    var margin = {
        top: parseFloat(pxToMm(s.TopMarginPx)),
        left: parseFloat(pxToMm(s.LeftMarginPx)),
        right: parseFloat(pxToMm(s.RightMarginPx)),
        bottom: parseFloat(pxToMm(s.BottomMarginPx))
    };
    var pageWidth = 210 - margin.left - margin.right;
    var pageHeight = 297 - margin.top - margin.bottom;

    if (!pages || !pages.length) {
        pages = [target];
    }

    renderPagesToPdf(Array.prototype.slice.call(pages), pdf, pageWidth, pageHeight, margin).then(function () {
        pdf.save(buildPdfFileName());
    }).catch(function () {
        toastr.error('PDF export failed');
    });
}

function renderPagesToPdf(pageElements, pdf, pageWidth, pageHeight, margin) {
    var index = 0;
    return new Promise(function (resolve, reject) {
        function next() {
            if (index >= pageElements.length) {
                resolve();
                return;
            }

            var el = pageElements[index];
            html2canvas(el, { scale: 2, useCORS: true, backgroundColor: '#ffffff' }).then(function (canvas) {
                if (index > 0) {
                    pdf.addPage();
                }

                appendCanvasToPdf(pdf, canvas, pageWidth, pageHeight, margin);
                index += 1;
                next();
            }).catch(function () {
                reject();
            });
        }

        next();
    });
}

function appendCanvasToPdf(pdf, canvas, pageWidth, pageHeight, margin) {
    var img = canvas.toDataURL('image/png');
    var imgHeight = (canvas.height * pageWidth) / canvas.width;

    if (imgHeight <= pageHeight) {
        pdf.addImage(img, 'PNG', margin.left, margin.top, pageWidth, imgHeight);
        return;
    }

    var rendered = 0;
    while (rendered < imgHeight) {
        var y = margin.top - rendered;
        pdf.addImage(img, 'PNG', margin.left, y, pageWidth, imgHeight);
        rendered += pageHeight;
        if (rendered < imgHeight) {
            pdf.addPage();
        }
    }
}

function buildPdfFileName() {
    var today = new Date();
    var datePart = today.getFullYear().toString() + String(today.getMonth() + 1).padStart(2, '0') + String(today.getDate()).padStart(2, '0');
    var docs = rpePreviewDocuments || [];
    var patient = sanitizeFilePart(docs.length ? docs[0].PatientName : 'Patient');
    var mode = $('input[name="rpePreviewMode"]:checked').val() || 'Individual';

    if (docs.length === 1) {
        var inv = sanitizeFilePart(docs[0].InvestigationName || 'Report');
        return patient + '_' + inv + '_' + datePart + '.pdf';
    }

    if (mode === 'Grouped') {
        var depts = {};
        docs.forEach(function (d) {
            var key = (d.DepartmentName || 'Department').trim();
            if (key) {
                depts[key] = true;
            }
        });
        var deptNames = Object.keys(depts);
        if (deptNames.length === 1) {
            return patient + '_' + sanitizeFilePart(deptNames[0]) + '_Report_' + datePart + '.pdf';
        }
    }

    return patient + '_AllReports_' + datePart + '.pdf';
}

function sanitizeFilePart(value) {
    var v = (value || '').toString().trim();
    if (!v) return 'NA';
    return v.replace(/[\\\/:*?"<>|]+/g, '').replace(/\s+/g, '');
}

function normalizeAgeType(ageType) {
    var value = (ageType || '').toString().toLowerCase();
    if (value === '1' || value.indexOf('year') >= 0) return 'Year(s)';
    if (value === '2' || value.indexOf('month') >= 0) return 'Month(s)';
    if (value === '3' || value.indexOf('day') >= 0) return 'Day(s)';
    return 'Year(s)';
}

function normalizeGender(gender) {
    var value = (gender || '').toString().trim().toLowerCase();
    if (value === '1' || value === 'male' || value === 'm') return 'Male';
    if (value === '2' || value === 'female' || value === 'f') return 'Female';
    if (value === '3' || value === 'others' || value === 'other' || value === 'o') return 'Others';
    return gender || '-';
}

function formatDate(input) {
    if (!input) return '-';
    var d = parseJsonDate(input);
    if (isNaN(d.getTime())) return input;
    var dd = String(d.getDate()).padStart(2, '0');
    var mm = String(d.getMonth() + 1).padStart(2, '0');
    var yy = d.getFullYear();
    return dd + '/' + mm + '/' + yy;
}

function formatDateTime(input) {
    if (!input) return '-';
    var d = parseJsonDate(input);
    if (isNaN(d.getTime())) return input;
    var dd = String(d.getDate()).padStart(2, '0');
    var mm = String(d.getMonth() + 1).padStart(2, '0');
    var yy = d.getFullYear();
    var hh = String(d.getHours()).padStart(2, '0');
    var mi = String(d.getMinutes()).padStart(2, '0');
    return dd + '/' + mm + '/' + yy + ' ' + hh + ':' + mi;
}

function parseJsonDate(input) {
    if (typeof input === 'string') {
        var match = /\/Date\((-?\d+)(?:[+-]\d+)?\)\//.exec(input);
        if (match) {
            return new Date(parseInt(match[1], 10));
        }
    }
    return new Date(input);
}

function htmlEncode(value) {
    return $('<div/>').text(value || '').html();
}

function getRpePrintStyles() {
    var s = normalizeRpeLayoutSettings(rpeLayoutSettings);
    var footerClearancePx = 24;
    var footerBottomNudgePx = s.PrintFooter ? Math.min(8, Math.max(0, s.BottomMarginPx)) : 0;
    var mmTop = pxToMm(s.TopMarginPx);
    var mmRight = pxToMm(s.RightMarginPx);
    var footerSpacePx = s.PrintFooter ? s.FooterHeightPx + s.BottomMarginPx + footerClearancePx : 0;
    var mmBottom = pxToMm(s.PrintFooter ? footerSpacePx : s.BottomMarginPx);
    var mmLeft = pxToMm(s.LeftMarginPx);
    var printableHeight = 297 - parseFloat(mmTop) - parseFloat(mmBottom);

    return '' +
        'body.rpe-print-body{background:#f4f6f9;margin:0;padding:12px;}' +
        '#rpePrintableArea{max-width:210mm;margin:0 auto;}' +
        '.rpe-print-fixed-footer{display:none;}' +
        '.rpe-report-page{width:210mm;min-height:287mm;margin:0 auto 10px auto;box-sizing:border-box;background:#fff;}' +
        '.rpe-page-inner{height:100%;box-sizing:border-box;}' +
        '.rpe-page-layout{width:100%;border-collapse:collapse;border-spacing:0;}' +
        '.rpe-page-layout>thead>tr>td,.rpe-page-layout>tbody>tr>td,.rpe-page-layout>tfoot>tr>td{padding:0;border:0;}' +
        '.rpe-page-layout>tbody>tr,.rpe-page-layout>tbody>tr>td{page-break-inside:auto;break-inside:auto;}' +
        '.rpe-page-header{border-bottom:1px solid #dee2e6;padding-bottom:8px;margin-bottom:10px;}' +
        '.rpe-page-content{font-size:14px;}' +
        '.rpe-page-footer{border-top:1px solid #dee2e6;padding-top:8px;margin-top:12px;font-size:12px;color:#495057;}' +
        '.rpe-body-footer-text{font-size:12px;color:#495057;text-align:center;margin-top:16px;}' +
        '.rpe-report-info{display:flex;gap:10px;border:1px solid #dee2e6;padding:6px 8px;font-size:12px;align-items:flex-start;}' +
        '.rpe-report-info-grid{flex:1;display:grid;grid-template-columns:1fr 1fr;column-gap:12px;row-gap:2px;}' +
        '.rpe-report-info-item span{font-weight:700;}' +
        '.rpe-report-qr{width:82px;min-width:82px;text-align:right;}' +
        '.rpe-report-qr img{width:76px;height:76px;display:block;margin-left:auto;}' +
        '.rpe-report-page table{width:100%;border-collapse:collapse;}' +
        '.rpe-report-page th,.rpe-report-page td{vertical-align:top;}' +
        '.rpe-report-page tr{page-break-inside:avoid;break-inside:avoid;}' +
        '.rpe-page-break{page-break-after:always;break-after:page;}' +
        '.no-print{display:none !important;}' +
        '@page{size:A4;margin:0;}' +
        '@media print{' +
        'body.rpe-print-body{background:#fff;padding:0;margin:0;}' +
        'body.rpe-print-body .rpe-print-fixed-footer{display:block;position:fixed;left:' + s.LeftMarginPx + 'px;right:' + s.RightMarginPx + 'px;bottom:-' + footerBottomNudgePx + 'px;height:' + s.FooterHeightPx + 'px;overflow:hidden;z-index:10;}' +
        'body.rpe-print-body .rpe-print-fixed-footer img{display:block;width:100%;height:100%;object-fit:contain;object-position:center bottom;}' +
        '#rpePrintableArea{max-width:none;}' +
        '.rpe-report-page{width:210mm;min-height:297mm;margin:0;padding:0;border:none !important;box-shadow:none !important;box-sizing:border-box;}' +
        'body.rpe-print-body .rpe-page-inner{padding:' + s.TopMarginPx + 'px ' + s.RightMarginPx + 'px ' + footerSpacePx + 'px ' + s.LeftMarginPx + 'px !important;box-sizing:border-box;}' +
        'body.rpe-print-body .rpe-page-content{padding-bottom:' + footerSpacePx + 'px;box-sizing:border-box;}' +
        'body.rpe-print-body .rpe-page-layout{height:' + printableHeight.toFixed(2) + 'mm;min-height:' + printableHeight.toFixed(2) + 'mm;}' +
        '.rpe-report-page:last-child{page-break-after:auto;break-after:auto;}' +
        '.rpe-page-layout>thead{display:table-header-group;}' +
        '.rpe-page-layout>tfoot{display:none !important;}' +
        '.rpe-page-layout>tbody{display:table-row-group;}' +
        '.rpe-page-layout>tbody>tr,.rpe-page-layout>tbody>tr>td{page-break-inside:auto;break-inside:auto;}' +
        'thead{display:table-header-group;}tfoot{display:none !important;}' +
        'tr{page-break-inside:avoid;break-inside:avoid;}' +
        'body.rpe-print-body .rpe-page-layout>tbody,body.rpe-print-body .rpe-page-layout>tbody>tr,body.rpe-print-body .rpe-page-layout>tbody>tr>td{height:100%;}' +
        'button,.btn,.navbar,.sidebar,.main-header,.main-sidebar,.content-header,.no-print{display:none !important;}' +
        '}';
}

function ensureRpePreviewStyles() {
    if ($('#rpePreviewStyles').length) {
        $('#rpePreviewStyles').html(getRpePrintStyles());
        return;
    }
    $('head').append('<style id="rpePreviewStyles">' + getRpePrintStyles() + '</style>');
}

function getDefaultRpeLayoutSettings() {
    return {
        PrintMode: 'PlainPaper',
        PrintHeader: true,
        HeaderHeightPx: 120,
        ShowLogo: true,
        ShowLabDetails: true,
        PrintFooter: true,
        FooterHeightPx: 60,
        FooterText: 'This is a system generated report.',
        TopMarginPx: 38,
        LeftMarginPx: 38,
        RightMarginPx: 38,
        BottomMarginPx: 38,
        ContentStartPx: 0,
        LabName: 'SSK Diagnostics',
        LabAddress: '',
        LabPhone: ''
    };
}

function normalizeRpeLayoutSettings(data) {
    var d = getDefaultRpeLayoutSettings();
    var x = data || {};
    d.PrintMode = (x.PrintMode || d.PrintMode) === 'PrePrinted' ? 'PrePrinted' : 'PlainPaper';
    d.PrintHeader = toBool(x.PrintHeader, d.PrintHeader);
    d.HeaderHeightPx = toInt(x.HeaderHeightPx, d.HeaderHeightPx);
    d.ShowLogo = toBool(x.ShowLogo, d.ShowLogo);
    d.ShowLabDetails = toBool(x.ShowLabDetails, d.ShowLabDetails);
    d.PrintFooter = toBool(x.PrintFooter, d.PrintFooter);
    d.FooterHeightPx = toInt(x.FooterHeightPx, d.FooterHeightPx);
    d.FooterText = (x.FooterText || d.FooterText);
    d.TopMarginPx = toInt(x.TopMarginPx, d.TopMarginPx);
    d.LeftMarginPx = toInt(x.LeftMarginPx, d.LeftMarginPx);
    d.RightMarginPx = toInt(x.RightMarginPx, d.RightMarginPx);
    d.BottomMarginPx = toInt(x.BottomMarginPx, d.BottomMarginPx);
    d.ContentStartPx = toInt(x.ContentStartPx, d.ContentStartPx);
    d.LabName = x.LabName || d.LabName;
    d.LabAddress = x.LabAddress || '';
    d.LabPhone = x.LabPhone || '';
    return d;
}

function buildLabAddressText(lab) {
    if (!lab) return '';
    var parts = [];
    if (lab.AddressLine1) parts.push(lab.AddressLine1);
    if (lab.AddressLine2) parts.push(lab.AddressLine2);
    if (lab.City) parts.push(lab.City);
    if (lab.State) parts.push(lab.State);
    if (lab.Pincode) parts.push(lab.Pincode);
    if (lab.Country) parts.push(lab.Country);
    return parts.join(', ');
}

function pxToMm(px) {
    return Math.max(0, (toInt(px, 0) * 0.264583)).toFixed(2);
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

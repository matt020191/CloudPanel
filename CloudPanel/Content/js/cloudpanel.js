function ReloadTable(id) {
    var table = $("#" + id).DataTable();
    table.ajax.reload();
}

function FormatUsersChildTable(d) {
    var returnData = '<table class="table table-striped table-condensed table-bordered">' +
                              '<tr><td align="right" width="200px">GUID:</td><td>' + d.userGuid + '</td></tr>' +
                              '<tr><td align="right">SamAccountName:</td><td>' + d.samAccountName + '</td></tr>' +
                              '<tr><td align="right">Created:</td><td>' + moment(d.created).format('LLLL') + '</td></tr>' +
                              '</table>';

    if (d.mailboxPlan > 0) {
        returnData += '<table class="table table-striped table-condensed table-bordered">' +
                      '<tr><td align="right" width="200px">Plan:</td><td>' + d.mailboxPlanName + '</td></tr>' +
                      '<tr><td align="right">Features:</td><td>' + GetMailboxFeatures(d) + '</td></tr>' +
                      '<tr><td align="right">Size:</td><td>' + GetMailboxSizeProgressBar(d.mailboxSizeInKB, d.mailboxPlanSize + d.additionalMB) + '</td></tr>' +
                      '<tr><td align="right">Deleted Size:</td><td>' + bytesToSize(d.mailboxDeletedSizeInKB * 1024) + '</td></tr>' +
                      '<tr><td align="right">Item Count:</td><td>' + d.mailboxItemCount + '</td></tr>' +
                      '<tr><td align="right">Deleted Item Count:</td><td>' + d.mailboxDeletedItemCount + '</td></tr>' +
                      '<tr><td align="right">Info Retrieved:</td><td>' + d.mailboxInfoRetrieved + '</td></tr>' +
                      '</table>';
    }

    return returnData + '</table>';
}

function GetMailboxFeatures(d) {
    var returnData = "";

    if (d.mailboxPlanPOP3) { returnData += '<span class="label label-default">POP3</span>' }
    if (d.mailboxPlanIMAP) { returnData += '<span class="label label-primary">IMAP</span>' }
    if (d.mailboxPlanOWA) { returnData += '<span class="label label-success">OWA</span>' }
    if (d.mailboxPlanMAPI) { returnData += '<span class="label label-info">MAPI</span>' }
    if (d.mailboxPlanAS) { returnData += '<span class="label label-warning">ActiveSync</span>' }

    return returnData;
}

function GetUserFeatures(isEnabled, isCompanyAdmin, isResellerAdmin, hasMailbox) {
    var html = [];

    if (!isEnabled)
        html.push('<span class="badge badge-danger tooltip-ex" data-toggle="tooltip" data-original-title="User is Disabled"><i class="fa fa-frown-o"></i></span><span class="tab-space"></span>');

    if (isCompanyAdmin)
        html.push('<span class="badge badge-success tooltip-ex" data-toggle="tooltip" data-original-title="Company Administrator"><i class="fa fa-gear"></i></span><span class="tab-space"></span>');

    if (isResellerAdmin)
        html.push('<span class="badge badge-info tooltip-ex" data-toggle="tooltip" data-original-title="Reseller Administrator"><i class="fa fa-gears"></i></span><span class="tab-space"></span>');

    if (hasMailbox)
        html.push('<span class="badge badge-warning tooltip-ex" data-toggle="tooltip" data-original-title="Has a Mailbox"><i class="fa fa-envelope"></i></span><span class="tab-space"></span>');

    return html.join("");
}

function bytesToSize(bytes) {
    var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    if (bytes == 0) return 'Unknown';
    var i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
    if (i == 0) return bytes + ' ' + sizes[i];
    return (bytes / Math.pow(1024, i)).toFixed(1) + ' ' + sizes[i];
};

function GetMailboxSizeProgressBar(minInKB, maxInMB)
{
    if (maxInMB == 0)
        return "";
    else {
        var maxInKB = (maxInMB * 1024);

        if (minInKB > maxInKB)
            maxInKB = minInKB;

        var percent = Math.round((minInKB * 100) / maxInKB);
        var title = bytesToSize(minInKB * 1024) + " / " + bytesToSize(maxInKB * 1024);
        return '<div class="">\
                <div class="clearfix">\
                    <div class="progress-title" style="float: left">'+ title + '</div>\
                    <div class="progress-percentage" style="float: right; position: relative">'+ percent + '%</div>\
                </div>\
                <div class="progress">\
                    <div class="progress-bar progress-bar-info" style="width: '+ percent + '%"></div>\
                </div>\
            </div>';
    }
}
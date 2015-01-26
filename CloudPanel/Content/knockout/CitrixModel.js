function DesktopGroup(data) {
    this.desktopGroupID = ko.observable(data.desktopGroupID);
    this.publishedName = ko.observable(data.publishedName);
}

function Application(data) {
    this.applicationID = ko.observable(data.applicationID);
    this.publishedName = ko.observable(data.publishedName);
}

function CitrixViewModel(desktopGroupUrl, applicationUrl) {
    var self = this;
    self.desktopGroups = ko.observableArray([]);
    self.applications = ko.observableArray([]);

    // Get the citrix desktop groups
    $.getJSON(desktopGroupUrl, function (allData) {
        var mappedGroups = $.map(allData, function (item) { return new DesktopGroup(item) });
        self.desktopGroups(mappedGroups);
    }).fail(function (data) {
        console.log(data);
        ShowError(data);
    });

    // Get the citrix applications
    $.getJSON(applicationUrl, function (allData) {
        var mappedApps = $.map(allData, function (item) { return new Application(item) });
        self.applications(mappedApps);
    }).fail(function (data) {
        console.log(data);
        ShowError(data);
    });
}
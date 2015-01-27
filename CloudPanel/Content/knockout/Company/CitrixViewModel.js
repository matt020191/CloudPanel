var User = function (userGuid, displayName) {
    this.userGuid = userGuid;
    this.displayName = displayName;
};

var citrixViewModel = function() {
    var self = this;
    self.allUsers = ko.observableArray([]);

    var data = $.getJSON(window.location.pathname + "/users");
    console.log(data);

    $.each(data, function (i, item) {
        console.log(i);
        console.log(item);
        console.log(data[i]);
        //self.allUsers.push(new User(item.userguid, item.displayName));
    });

};

ko.applyBindings(new citrixViewModel());
(function () {
    'use strict';

    var app = angular.module('app', [
        // Angular modules
        'ngAnimate',
        'ui.router',
        // 'ngRoute', 

        // Custom modules

        // 3rd Party Modules

    ]);
    app.config(function ($stateProvider, $urlRouterProvider) {

        $urlRouterProvider.otherwise('/home');

        $stateProvider

            // HOME STATES AND NESTED VIEWS ========================================
            .state('home', {
                url: '',
                templateUrl: 'partials/custom-links.html',
                controller: function ($scope, datacontext) {
                    // $scope.filterType = "UW";
                   // console.log($scope.filterType);
                }
            })

            // nested list with custom controller
            .state('home.Accounting', {
                url: '/Accounting',
                controller: function ($scope, datacontext) {
                    $scope.filterType = "Accounting";
                    datacontext.getLinks().then(function (result) {
                        $scope.links = result.data;
                    });
                },
                templateUrl: 'partials/custom-links.html'

            })

    });
    app.service("datacontext", function ($http) {
        this.getAllLinks = function () {
            return $http.get("/AllLinks");
        }
        this.getLinks = function () {
            return $http.get("/Links");
        }
   
        this.getUser = function () {
            return $http.get("/User");
        }
        this.saveEvent = function (url, data, config) {
            var res = $http.post(url, data, config);
            return res;
        }
        this.teamCalendar = function (url) {
            $("#hidden-div").fadeOut(100);
            $.ajax({
                type: 'get',
                url: url,
                dateType: 'json',
                success: function (data) {
                    $('#calendar').fullCalendar('removeEvents');
                    $('#calendar').fullCalendar('addEventSource', data);
                    $("#hidden-div").fadeIn(1000);
                    $('#calendar').fullCalendar('rerenderEvents');
                }
            });
        }
        this.eventExists = function (url) {
            var res = $http.get(url);
            return res;
        }
        this.deleteEvent = function (url, data, config) {
            var res = $http.post(url, data, config);
            return res;
        }
        this.updateEvent = function (url) {
            $.ajax({
                type: 'post',
                url: url,
                dateType: 'json',
                success: function (data) {
                    $('#mycalendar').fullCalendar('removeEvents');
                    $('#mycalendar').fullCalendar('addEventSource', data);
                    $('#mycalendar').fullCalendar('rerenderEvents');
                    if ($('#modalEdit').is(':visible')) {
                        $("#modalEdit").modal("toggle");
                    }
                }
            });
            //var res = $http.post(url, data, config);
            //return res;
        }
    });

    app.controller("csmController", function ($scope, datacontext) {
        datacontext.getAllLinks().then(function (result) {
            $scope.Alllinks = result.data.AllLinks;
        });
        //datacontext.getLinks().then(function (result) {
        //    $scope.links = result.data;
        //});
        //datacontext.getUser().then(function (result) {
        //    $scope.user = result.data;
        //    $scope.currentCalendar = $scope.user.UserRole;
        //});
        $scope.getLinkName = function (name) {
            $scope.customLink = name;
            // console.log(name);
        }
        $scope.AddEvent = function () {
            $("#modalEdit").modal({ backdrop: "static" });
            $("#calendarModalLabel").text("Add Event");
            $("#btnDelete").hide();
            clearfields();
        }
        $scope.ReturnHome = function () {
            window.location.href = "/";
        }
        $scope.check = false;
        $scope.originalEventObject = null;
        //$scope.chkAllDay = function (value) {
        //    $("#endDate").prop("disabled", value);
        //}

        //loadSpinner();
        showSpinner();

        $scope.GetTeamCalendar = function (team) {
            if (team === "My Calendar") {
                window.location.href = "/myCalendar.html";
            } else {
                datacontext.teamCalendar('TeamCalendar/' + team);
                $scope.currentCalendar = team;
            }
        }
        $scope.saveEvent = function () {
            var isValid = true;
            var startDate = new Date($('#startDate').val());
            var endDate = new Date($('#endDate').val());
            var eventTitle = $.trim($("#txtEventTitle").val());
            var errorMsg = "";
           //validation
            if (startDate > endDate) {
                errorMsg = 'The start date cannot be greater than end date';
                isValid = false;
            }
            if (eventTitle.toUpperCase() === "PTO - HALF DAY") {
                var timeDiff = Math.abs(startDate.getTime() - endDate.getTime());
                var diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24));
                if (diffDays > 0) {
                    errorMsg = 'PTO - Half Day cannot span more than one day.';
                    isValid = false;
                }
            }
            if (eventTitle.toUpperCase() === "HALF SICK DAY") {
                var timeDiff = Math.abs(startDate.getTime() - endDate.getTime());
                var diffDays = Math.ceil(timeDiff / (1000 * 3600 * 24));
                if (diffDays > 0) {
                    errorMsg = 'Half Sick Day cannot span more than one day.';
                    isValid = false;
                }
            }
            if ($.trim($("#startDate").val()) === '') {
                $("#startDate").css({
                    "border": "1px solid red",
                   // "background": "#FFCECE"
                });
                errorMsg = 'Please fill in all the fields below';
                isValid = false;
            }
            if ($.trim($("#txtEventTitle").val()) === '') {
                $("#txtEventTitle").css({
                    "border": "1px solid red",
                   // "background": "#FFCECE"
                });
                errorMsg = 'Please fill in all the fields below';
                isValid = false;
            }
         
            if (isValid === false) {
                $('#lbFormError').text(errorMsg).css({ "color": "red" });
                return;
            } else {
                datacontext.eventExists('/EventExists?startDate=' + $("#startDate").val() + '&endDate=' + $("#endDate").val() + '&eventDescription=' + $("#txtEventTitle").val()).then(function (result) {
                    if (result.data) {
                        errorMsg = 'You cannot add duplicate entries on the same day(s)';
                        $("#txtEventTitle").css({
                            "border": "1px solid red",
                            // "background": "#FFCECE"
                        });
                        $('#lbFormError').text(errorMsg).css({ "color": "red" });
                        return;
                    }
                    else {
                        var url = $("#calendarModalLabel").text() === "Add Event" ? '/SaveEvent?startDate=' + $("#startDate").val() + '&endDate=' + $("#endDate").val() + '&eventDescription=' + $("#txtEventTitle").val() :
                            '/UpdateEvent?userCalendarID=' + $("#userCalendarId").val() + '&startDate=' + $("#startDate").val() + '&endDate=' + $("#endDate").val() + '&eventDescription=' + $("#txtEventTitle").val();
                        datacontext.updateEvent(url);
                    }
                });
            }
        }
        $scope.deleteEvent = function () {
            var data = $.param({
                userCalendarId: $("#userCalendarId").val()
            });
            var config = {
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                'dataType': 'json',
                'data': data
            }
            var url = '/DeleteEvent?userCalendarID=' + $("#userCalendarId").val();
            datacontext.updateEvent(url);
        }
        //Date for the calendar events (dummy data)

        //function to refresh calendar after update
        function loadSpinner() {
            $("#loading").fadeIn();
            var opts = {
                lines: 12, // The number of lines to draw
                length: 9, // The length of each line
                width: 4, // The line thickness
                radius: 10, // The radius of the inner circle
                color: '#00573d', // #rgb or #rrggbb
                speed: 1, // Rounds per second
                trail: 60, // Afterglow percentage
                shadow: false, // Whether to render a shadow
                hwaccel: false // Whether to use hardware acceleration
            };
            var target = document.getElementById('loading');
            var spinner = new Spinner(opts).spin(target);
        }

        var myVar;
        function unloadSpinner() {
            myVar = setTimeout(showPage, 0);
            $("#loading").fadeOut();

        }
        function showSpinner() {
            document.getElementById("loading").style.display = "block";
            document.getElementById("loader").style.display = "block";
        }
        function showPage() {
            document.getElementById("loader").style.display = "none";
            //document.getElementById("myDiv").style.display = "block";
        }
        function clearfields() {
            $('#startDate').val("");
            $('#endDate').val("");
            $('#txtEventTitle').val("");

            $('#startDate').css({
                "border": "1px solid #ccc",
                "background": "#FFFFF"
            });
            $('#endDate').css({
                "border": "1px solid #ccc",
                "background": "#FFFFF"
            });
            $('#txtEventTitle').css({
                "border": "1px solid #ccc",
                "background": "#FFFFF"
            });
            $('#lbFormError').text("");
        }
        //loadSpinner();
        var date = new Date();
        var d = date.getDate(),
            m = date.getMonth(),
            y = date.getFullYear();
        $.ajax({
            type: 'get',
            url: '/UserEntries/',
            dateType: 'json',
            success: function (data) {
                $('#mycalendar').fullCalendar({
                    header: {
                        left: '',
                        center: 'title',
                       // right: 'year,month,agendaWeek,agendaDay'
                    },
                    buttonText: {
                        today: 'today',
                        // month: 'month',
                        //week: 'week',
                        //day: 'day',
                        //year: 'year'
                    },
                    events: data,
                    editable: true,
                    droppable: true, // this allows things to be dropped onto the calendar !!!
                    height: 600,
                    drop: function (date, allDay) { // this function is called when something is dropped

                       // $("#modalEdit").modal({ backdrop: "static" });
                        $("#calendarModalLabel").text("Add Event");
                        $("#btnDelete").hide();

                        // retrieve the dropped element's stored Event Object
                        var originalEventObject = $(this).data('eventObject');
                        $scope.originalEventObject = originalEventObject;
                      //  console.log($scope.originalEventObject);
                        // we need to copy it, so that multiple events don't have a reference to the same object
                        var copiedEventObject = $.extend({}, originalEventObject);

                        //in case validation was fired, make sure input boxes are marked gray
                        clearfields();

                        //// assign it the date that was reported
                        copiedEventObject.start = date;
                        copiedEventObject.allDay = allDay;
                        copiedEventObject.backgroundColor = $(this).css("background-color");
                        copiedEventObject.borderColor = $(this).css("border-color");
                        $("#txtEventTitle").val(copiedEventObject.title);
                        $("#startDate").val(copiedEventObject.start.format('MM/DD/YYYY'));
                        //$("#endDate").val(copiedEventObject.end != null ? copiedEventObject.end.format('MM/DD/YYYY') : "");
                        $("#startDate").datepicker('setDate', date.format('MM/DD/YYYY'));
                        var dt = new Date(copiedEventObject.start.format('MM/DD/YYYY'));
                        var endDate = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate() + 1);
                        $("#endDate").datepicker('setDate', endDate);
                        $("#endDate").val('');
                        // render the event on the calendar
                        // the last `true` argument determines if the event "sticks" (http://arshaw.com/fullcalendar/docs/event_rendering/renderEvent/)
                         $('#mycalendar').fullCalendar('renderEvent', copiedEventObject, true);

                        // is the "remove after drop" checkbox checked?
                        if ($('#drop-remove').is(':checked')) {
                            // if so, remove the element from the "Draggable Events" list
                            $(this).remove();
                        }

                        $('#btnConfirm').click();
                    },
                    eventDrop: function (event, dayDelta, minuteDelta, allDay, revertFunc) {
                        var endDate = event.end != null ? event.end.format('MM/DD/YYYY') : '';
                        var data = $.param({
                            startDate: event.start.format('MM/DD/YYYY'),
                            endDate: endDate,
                            eventDescription: $("#txtEventTitle").val()
                        });
                        var config = {
                            headers: {
                                'Accept': 'application/json',
                                'Content-Type': 'application/json'
                            },
                            'dataType': 'json',
                            'data': data
                        }
                        var url = '/UpdateEvent?userCalendarID=' + event.userCalendarId + '&startDate=' + event.start.format('MM/DD/YYYY') + '&endDate=' + endDate + '&eventDescription=' + event.title;
                        datacontext.updateEvent(url);

                    },
                    eventResize: function (event, dayDelta, minuteDelta, allDay, revertFunc) {
                        var endDate = event.end != null ? event.end.format('MM/DD/YYYY') : '';
                        var data = $.param({
                            startDate: event.start.format('MM/DD/YYYY'),
                            endDate: endDate,
                            eventDescription: $("#txtEventTitle").val()
                        });
                        var config = {
                            headers: {
                                'Accept': 'application/json',
                                'Content-Type': 'application/json'
                            },
                            'dataType': 'json',
                            'data': data
                        }
                        var url = '/UpdateEvent?userCalendarID=' + event.userCalendarId + '&startDate=' + event.start.format('MM/DD/YYYY') + '&endDate=' + endDate + '&eventDescription=' + event.title;
                        datacontext.updateEvent(url);
                    },
                    eventRender: function (event, element, view) {
                        if ($.trim(event.UserID) == "Shared") {
                            event.editable = false;
                        }
                        return !event.hidden;
                    },
                    eventClick: function (event, jsEvent, view) {
                        event.hidden = true;
                        if (event.EventDescription.indexOf("CSM Holiday") != -1)
                        { } else {
                            $("#modalEdit").modal({ backdrop: "static" });
                            $("#calendarModalLabel").text("Edit Event");
                            clearfields();

                            $("#txtEventTitle").val(event.title);
                            $("#startDate").val(event.start.format('MM/DD/YYYY')),
                                $("#endDate").val(event.end != null ? event.end.format('MM/DD/YYYY') : ""),
                                $("#btnDelete").show();
                            $("#userCalendarId").val(event.userCalendarId != null ? event.userCalendarId : "");
                            $("#startDate").datepicker('setDate', event.start.format('MM/DD/YYYY'));
                            var dt = new Date(event.end.format('MM/DD/YYYY'));
                            var endDate = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate());
                            $("#endDate").datepicker('setDate', endDate);
                        }
                    },
                    // hiddenDays: [0, 6],
                    displayEventTime: false,
                    fixedWeekCount: false,
                    dayClick: function (date, jsEvent, view) {
                        // event.hidden = true;
                        $("#modalEdit").modal({ backdrop: "static" });
                        $("#calendarModalLabel").text("Add Event");
                        clearfields();

                        $("#txtEventTitle").val('');
                        $("#startDate").val(date.format('MM/DD/YYYY'));
                        var dt = new Date(date.format('MM/DD/YYYY'));
                        var endDate = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate() + 1);
                        $("#endDate").datepicker('setDate', endDate);
                        $("#endDate").val('');
                        $("#btnDelete").hide();
                        $("#startDate").datepicker('setDate', date.format('MM/DD/YYYY'));

                        //  $("#userCalendarId").val(event.userCalendarId != null ? event.userCalendarId : "");
                    },
                });
                //$("#loading").fadeOut();
                unloadSpinner();
            }
        });


    });
})();

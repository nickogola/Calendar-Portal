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
                  //  console.log($scope.filterType);
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
        //this.getLinks = function() {
        //    return $http.get("/Links");
        //}
        this.getAnnouncements = function () {
            return $http.get("/Announcements");
        }
        this.getUser = function () {
            return $http.get("/User");
        }
        this.saveEvent = function (url, data, config) {
            var res = $http.post(url, data, config);
            return res;
        }
        this.clearCache = function (url) {
            $.ajax({
                type: 'post',
                url: url,
                dateType: 'json',
                success: function (data) {
                    //$('#mycalendar').fullCalendar('removeEvents');
                    //$('#mycalendar').fullCalendar('addEventSource', data);
                    //$('#mycalendar').fullCalendar('rerenderEvents');
                    //if ($('#modalEdit').is(':visible')) {
                    //    $("#modalEdit").modal("toggle");
                    //}
                }
            });
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

       // loadSpinner();
        showSpinner();
       // loadCalendar();
        datacontext.getAllLinks().then(function (result) {
            $scope.Alllinks = result.data.AllLinks;
            $scope.UserLinks = result.data.UserLinks;
        });

        datacontext.getUser().then(function (result) {
            $scope.user = result.data;
            var curCal = ''
            switch ($scope.user.DefaultCalendar) {
                //case "UW":
                //    curCal = "Underwriting"
                //    break;
                case "UWOps":
                    curCal = "UW Ops"
                    break;
                default:
                    curCal = $scope.user.DefaultCalendar;
            }
            $scope.currentCalendar = curCal;
            //  $scope.currentCalendar = $scope.user.UserRole;
        });
        $scope.getLinkName = function (name) {
            $scope.customLink = name;
           // console.log(name);
        }
       
        $scope.ReturnHome = function () {
            window.location.href = "/";
        }
        $scope.check = false;
        $scope.originalEventObject = null;
        $scope.chkAllDay = function (value) {
            $("#endDate").prop("disabled", value);
        }
        $scope.GetTeamCalendar = function (team) {
            if (team === "My Calendar") {
                window.location.href = "/myCalendar.html";
            } else {
                datacontext.teamCalendar('TeamCalendar/' + team);
                $scope.currentCalendar = team;
            }
        }
        $scope.clearCache = function () {
            datacontext.clearCache("/ClearCache");
        }
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
           // document.getElementById("myDiv").style.display = "block";
        }
        function loadCalendar() {
           // $(function () {

                // console.log(getUrlParameter('c'));

             
                // create the notification
                var announcements = null;
                $.ajax({
                    type: 'get',
                    url: '/Announcements',
                    dateType: 'json',
                    success: function (data) {
                        announcements = data.Result[0];
                        var notification = new NotificationFx({
                            message: '<div style="float:left;"><span class="fa fa-calendar fa-2x"></span></div><div><p><b>Announcements: </b> ' + announcements + ' </p></div>', //'<p>This is just a simple notice. Everything is in order and this is a <a href="#">simple link</a>.</p>',
                            layout: 'attached',
                            effect: 'bouncyflip',
                            type: 'notice', // notice, warning or error
                            ttl: 80000000,
                            onClose: function () {
                                // bttn.disabled = false;
                            }
                        });

                        // show the notification
                        if (announcements != null)
                            notification.show();
                    }
                });


                $('ul.dropdown-menu [data-toggle=dropdown]').on('click', function (event) {
                    // Avoid following the href location when clicking
                    event.preventDefault();
                    // Avoid having the menu to close when clicking
                    event.stopPropagation();
                    // If a menu is already open we close it
                    $('ul.dropdown-menu [data-toggle=dropdown]').parent().removeClass('open');
                    // opening the one you clicked on
                    $(this).parent().addClass('open');
                    log('click');
                });


         

                ini_events($('#external-events div.external-event'));

                /* initialize the calendar
                 -----------------------------------------------------------------*/
                //Date for the calendar events (dummy data)
                var date = new Date();
                var d = date.getDate(),
                    m = date.getMonth(),
                    y = date.getFullYear();

                $.ajax({
                    type: 'get',
                    url: '/Entries',
                    dateType: 'json',
                    success: function (data) {
                        $('#calendar').fullCalendar({
                            header: {
                                left: '',
                                center: 'title',
                                // right: 'month'
                            },
                            buttonText: {
                                today: 'today',
                                // month: 'month',
                                //week: 'week',
                                //day: 'day'
                            },
                            events: data,
                            editable: false,
                            draggable: false,
                            droppable: false,
                            eventMouseover: function (data, event, view) {

                                tooltip = '<div class="tooltiptopicevent" style="width:auto;height:auto;background:#eee;border:1px solid #ccc;position:absolute;z-index:10001;padding:8px 8px 8px 8px;font-size:11px;line-height: 200%;">' + '<b>' + data.tooltip + '</b></div>';


                                $("body").append(tooltip);
                                $(this).mouseover(function (e) {
                                    $(this).css('z-index', 10000);
                                    $('.tooltiptopicevent').fadeIn('500');
                                    $('.tooltiptopicevent').fadeTo('10', 1.9);
                                })

                                    .mousemove(function (e) {
                                        $('.tooltiptopicevent').css('top', e.pageY + 10);
                                        $('.tooltiptopicevent').css('left', e.pageX + 20);
                                    });
                            },
                            eventMouseout: function (data, event, view) {
                                $(this).css('z-index', 8);

                                $('.tooltiptopicevent').remove();

                            },
                            // hiddenDays: [0, 6],
                            displayEventTime: false,
                            fixedWeekCount: false,
                            height: 600,
                            dayClick: function () {
                                tooltip.hide()
                            },
                            eventResizeStart: function () {
                                tooltip.hide()
                            },
                            eventDragStart: function () {
                                tooltip.hide()
                            },
                            viewDisplay: function () {
                                tooltip.hide()
                            },

                        });

                        datacontext.getAllLinks().then(function (result) {
                            $scope.Alllinks = result.data.AllLinks;
                            $scope.UserLinks = result.data.UserLinks;
                            console.log(Alllinks);
                        });

                        datacontext.getUser().then(function (result) {
                            $scope.user = result.data;
                            var curCal = ''
                            switch ($scope.user.DefaultCalendar) {
                                case "UWOps":
                                    curCal = "UW Ops"
                                    break;
                                default:
                                    curCal = $scope.user.DefaultCalendar;
                            }
                            $scope.currentCalendar = curCal;
                     
                        });
                      
                        unloadSpinner();

                    }
                });


                /* ADDING EVENTS */
                var currColor = "#3c8dbc"; //Red by default
                //Color chooser button
                var colorChooser = $("#color-chooser-btn");
                $("#color-chooser > li > a").click(function (e) {
                    e.preventDefault();
                    //Save color
                    currColor = $(this).css("color");
                    //Add color effect to button
                    $('#add-new-event').css({ "background-color": currColor, "border-color": currColor });
                });
                $("#add-new-event").click(function (e) {
                    e.preventDefault();
                    //Get value and make sure it is not null
                    var val = $("#new-event").val();
                    if (val.length == 0) {
                        return;
                    }

                    //Create events
                    var event = $("<div />");
                    event.css({ "background-color": currColor, "border-color": currColor, "color": "#fff" }).addClass("external-event");
                    event.html(val);
                    $('#external-events').prepend(event);

                    //Add draggable funtionality
                    ini_events(event);

                    //Remove event from text input
                    $("#new-event").val("");
                });

               // $(window).scrollTop(0);

         
        }
        function getUrlParameter(param, dummyPath) {
            var sPageURL = dummyPath || window.location.search.substring(1),
                sURLVariables = sPageURL.split(/[&||?]/),
                res;

            for (var i = 0; i < sURLVariables.length; i += 1) {
                var paramName = sURLVariables[i],
                    sParameterName = (paramName || '').split('=');

                if (sParameterName[0] === param) {
                    res = sParameterName[1];
                }
            }

            return res;
        }
        function log(msg) {
            $('#logger').add('<p>' + msg + '</p>');
        }
        /* initialize the external events
         -----------------------------------------------------------------*/
        function ini_events(ele) {
            ele.each(function () {

                // create an Event Object (http://arshaw.com/fullcalendar/docs/event_data/Event_Object/)
                // it doesn't need to have a start or end
                var eventObject = {
                    title: $.trim($(this).text()) // use the element's text as the event title
                };

                // store the Event Object in the DOM element so we can get to it later
                $(this).data('eventObject', eventObject);

                // make the event draggable using jQuery UI
                $(this).draggable({
                    zIndex: 1070,
                    revert: true, // will cause the event to go back to its
                    revertDuration: 0  //  original position after the drag
                });

            });
        }
                 
    });
})();

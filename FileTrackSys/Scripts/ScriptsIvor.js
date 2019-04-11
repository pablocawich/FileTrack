$(document).ready(function () {

    //-----will check if pending file has data to further enable a notification alert signal--------

    //if ensures the function that gives notifications alerts registry only
    if (AppGlobal.role == "FMS_Registry") {
        determineRegistryNotificationAlert();
    } else {
        determineRegularNotificationAlert();//signifies user is regular 
    }
   
    function determineRegistryNotificationAlert() {
       $.ajax({
            url: '/Requests/GetPendingFiles/',
            type: 'POST',
            dataType: 'json',
            success: function (data, type, instance) {
               // console.log(data);
               var size = Object.keys(data.data).length; 
                if (size > 0) {
                    $("#notifIcon").css('color', 'red');
                    $("#notifAlert").prepend("<li id='pendingMssg'><a href='Requests/PendingFiles'><div><i class='fa fa-pencil-square-o'>" +
                        "</i>Pending<span class='pull-right text-muted small'>You have file/s that require attention</span></div></a></li>");   
                }
            }
        });
        $.ajax({
            url: '/FileVolumes/GetReturnedRequests',
            type: 'POST',
            dataType: 'json',
            success: function (returns) {
                //console.log(returns);
                var size = Object.keys(returns.data).length;

                if (size > 0) {
                    $("#notifIcon").css('color', 'red');
                     $("#notifAlert").prepend(
                         "<li><a href='/FileVolumes/ReturnApproval'><div><i class='fa fa-check-square-o'>" +
                         "</i>Returns<span class='pull-right text-muted small'>File awaiting return approval</span></div></a></li>");
                 }
            }
        }); 
        
    }

    function determineRegularNotificationAlert() {
        $.ajax({
            url: '/Requests/GetConfirmCheckout',
            type: 'POST',
            dataType: 'json',
            success: function (returns) {
                //console.log(returns);
                var size = Object.keys(returns.data).length;
                
                if (size > 0) {
                    $("#notifIcon").css('color', 'red');
                    $("#notifAlert").prepend(
                        "<li><a href='/Requests/ConfirmCheckout'><div><i class='fa fa-check-square-o'>" +
                        "</i>Approval<span class='pull-right text-muted small'>Confirm checkout</span></div></a></li>");
                }
            }
        }); 
    }

    $("#indexFile").removeClass('active');

    $("#createFile").on('click', function () {  
        $(this).addClass('active');        
    });

    $("#indexFile").on('click', function () { 
      $(this).addClass('active');
      //background-color: #eee;
    });
   //Implementation for action buttons on the dataTable (File/index)
   $("#fileTable").on("click", ".js-addVol", function () {
       var button = $(this);
       bootbox.confirm("Are you sure you want to add volume to this file?", function (result) {
           if (result) {
               var url = "/Files/AddNewVolume/" + button.attr("data-file-id");
              
               $('#modelContent').load(url);
               $('#myModal').modal('show');
           }
       });
   });
   $("#fileTable").on("click", ".js-editFile", function () {
       var button = $(this);
       bootbox.confirm("Are you sure you want to edit this file?", function (result) {
           if (result) {
             
               window.location.href = "/Files/Update/" + button.attr("data-edit-file-id");
           }
       });
    });

    $("#fileTable").on("click", ".js-viewVolumes", function () {
       var button = $(this);
       window.location.href = "/Files/FileVolumes/" + button.attr("data-view-vol-id");
          
      
   });
    //-------Implementing a front end feature that attaches an event to districts select and determines the location list-------
    $("#File_DistrictsId").on('change', function() {
        var value = $(this).val();
        
        $.ajax({
            url: '/Files/GetLocationsByDistrict/' + value,
            type: 'POST',
            dataType: 'json',
            success: function (data) {
                $("#File_LocationId").empty();
                $("#File_LocationId").append("<option value>Select client's location</option>");
                $.each(data, function(key, loc) {
                   // console.log('id: ' + value.LocationId + '  District Id: ' + value.DistrictsId + '  Location: ' + value.Name);
                    $("#File_LocationId").append($('<option>', {
                        value: loc.LocationId,
                        text: loc.Name
                    }));
                });
            }
        });
    });

    //Scroll to top feature below
    var btn = $('#regScrollToTop');

    $(window).scroll(function () {
        if ($(window).scrollTop() > 20) {
            btn.addClass('show');
        } else {
            btn.removeClass('show');
        }
    });

    btn.on('click', function (e) {
        e.preventDefault();
        $('html, body').animate({ scrollTop: 0 }, '300');
    });
});
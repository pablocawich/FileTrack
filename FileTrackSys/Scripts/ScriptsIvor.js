$(document).ready(function () {

    //notification front end code
    //listens for a click event so that link can be mark as read and removed from the database
    $(".alertList").on("click", function () {
        var list = $(this);
        var notifId = $(this).attr("notif-id-attr");

        $.ajax({
            url: siteURL+'/Notifications/ChangeToRead/' + notifId,
            success: function () {
                list.remove();
                //alert($(".alertList").length);
                toastr.info("Notification marked as read","Notification",{ positionClass: "toast-bottom-right" });
                if ($(".alertList").length == 0) {
                    $("#notifIcon").css('color', 'white'); 
                    $("#caretIcon").css('color', 'white');
                }    

            },
            statusCode: {
                404: function (content) { alert('cannot find resource'); },
                500: function (content) { alert('internal server error'); }
            }
        });        

    });

    $("#createFile").on('click', function () {  
        $(this).addClass('active');        
    });

   //Implementation for action buttons on the dataTable (File/index)
   $("#fileTable").on("click", ".js-addVol", function () {
       var button = $(this);
       bootbox.confirm("Are you sure you want to add another volume to this file?", function (result) {
           if (result) {
               var url = siteURL+"/Files/AddNewVolume/" + button.attr("data-file-id");
              
               $('#modelContent').load(url);
               $('#myModal').modal('show');
           }
       });
    });

   $("#fileTable").on("click", ".js-editFile", function () {
       var button = $(this);
       bootbox.confirm("Are you sure you want to edit this file?", function (result) {
           if (result) {
               window.location.href = siteURL + "/Files/Update/" + button.attr("data-edit-file-id");
           }
       });
    });

    $("#fileTable").on("click", ".js-viewVolumes", function () {
       var button = $(this);
        window.location.href = siteURL+"/Files/FileVolumes/" + button.attr("data-view-vol-id");
          
      
   });
    //-------Implementing a front end feature that attaches an event to districts select and determines the location list-------
    $("#File_DistrictsId").on('change', function() {
        var value = $(this).val();
        
        $.ajax({
            url: siteURL+'/Files/GetLocationsByDistrict/' + value,
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

    //functionality for the drop down list navigation menu on external transfer interface
    $("#extTransferNav").on("click", function () {

        if($("#extTransferUl").hasClass("in"))
             $("#extTransferUl").removeClass("in");
        else
            $("#extTransferUl").addClass("in");
    });
    //tooltip implementation
    $('[data-toggle="tooltip"]').tooltip();
});
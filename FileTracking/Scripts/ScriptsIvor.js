$(document).ready(function () {
 
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
       bootbox.confirm("You are about to view volumes for this file. Continue", function (result) {
           if (result) {

               window.location.href = "/Files/FileVolumes/" + button.attr("data-view-vol-id");
           }
       });
   });
  
});
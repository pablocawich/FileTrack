$(document).ready(function () {
 
   //Implementation for action buttons on the dataTable (File/index)
   $("#fileTable").on("click", ".js-addVol", function () {
       var button = $(this);
       bootbox.confirm("Are you sure you want to add volume to this file?", function (result) {
           if (result) {
               window.location.href = "/Files/AddVolume/" + button.attr("data-file-id");
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
   $("#fileTable").on("click", ".js-requestFile", function () {
       var button = $(this);
       var name = button.attr("data-request-name");
       bootbox.confirm("You are to request a file for "+name.toUpperCase()+". To request this file you need to specify a volume. Do you wish to proceed", function (result) {
           if (result) {
               window.location.href = "/FileVolumes/RequestFile/" + button.attr("data-request-file-id");
           }
       });
   });
  
});
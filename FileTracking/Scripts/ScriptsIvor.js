$(document).ready(function () {
   
    //Ensures Identification select list responds to the ID number text box according to input
    //if a value is selected the text box is enabled else remains disabled
    
   
    $("#File_IdentificationOptionId").on('change',function () {
        
        var listVal = $(this).val();
        
        if (listVal != null)
            $("#File_IdentificationNumber").prop("disabled", false);  

        if ($("#File_IdentificationOptionId :selected").text() === "Select Identification Type") {
            $("#File_IdentificationNumber").prop("disabled", true);  
            $("#File_IdentificationNumber").val("");  
        }
    });
   //form cancel button. Simply resets all fields
   $("#cancelBtn").on('click',
       () => {
           $("#fileFormId").trigger("reset");
           $("#File_IdentificationNumber").prop("disabled", true);
           $("#File_IdentificationNumber").val("");  
        });
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
       bootbox.confirm("To request this file you need to specify a volume. Do you wish to proceed", function (result) {
           if (result) {
               window.location.href = "/FileVolumes/RequestFile/" + button.attr("data-request-file-id");
           }
       });
   });
  
});
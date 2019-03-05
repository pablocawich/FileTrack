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
});
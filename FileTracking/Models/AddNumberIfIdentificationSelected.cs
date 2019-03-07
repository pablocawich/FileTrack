using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.WebPages;

namespace FileTracking.Models
{
    public class AddNumberIfIdentificationSelected : ValidationAttribute
    {
        //our customer validation
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //return base.IsValid(value, validationContext);
            var file = (File)validationContext.ObjectInstance;
            bool error = false;
            if (file.IdentificationOptionId.HasValue)
            {
                if (file.IdentificationNumber.IsEmpty())
                {
                    error = true;
                }
            }

            return (error == false) ? ValidationResult.Success : new ValidationResult("number field is empty");
        }
    }
}
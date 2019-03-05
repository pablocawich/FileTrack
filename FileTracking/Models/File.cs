using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class File
    {
        public int Id { get; set; }

        //Basic Profile like info----------------------------------------------------------
        [Required]
        [MaxLength(54)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(54)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [MaxLength(54)]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required]
        [MaxLength(64)]
        public string Street { get; set; }

        [Required]
        [MaxLength(82)]
        [Display(Name = "City/Town/Village")]
        public string CityOrTown { get; set; }

        public Districts Districts { get; set; }

        [Required]
        [Display(Name = "District")]
        public byte DistrictsId { get; set; }

        //----------End basic profile info----------------------------------------------------


        public string Comments { get; set; }

        [Required]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }

        //establishes an association to file type table----------------------------------------
        public FileType FileType { get; set; }
        
        [Required]
        [Display(Name = "File Type")]
        public byte FileTypeId { get; set; }

        //establishes association to File Status table----------------------------------------
        public FileStatus FileStatus { get; set; }

        [Required]
        [Display(Name = "File Status")]
        public byte FileStatusId { get; set; }

        // Establishes association with Identification Options ---------------------------------

        public IdentificationOption IdentificationOption { get; set; }

        [Display(Name = "Identification Type")]
        public byte? IdentificationOptionId { get; set; }

        [Display(Name = "Identification Number")]
        [StringLength(64)]
        public string IdentificationNumber { get; set; }

        //Establish association with volumes tables, possibly ----------------------------------
        //public Volume Volume {get;set;}
        //public int VolumeId { get; set;}
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FileTracking.Models
{
    public class File
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        
        public int Id { get; set; }
        
        //File Number 
        public int FileNumber { get; set; }

        [Required]
        public byte Volume { get; set; }

        //Basic Profile like info----------------------------------------------------------
        [Required]
        [MaxLength(54)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(54)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [MaxLength(54)]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [MaxLength(64)]
        public string Street { get; set; }

        public Location Location { get; set; }
        //[Required] //--- making location nullable
        [Display(Name = "Address")]
        public string LocationId { get; set; }

        public Districts Districts { get; set; }

        
        [Display(Name = "District")]
        [DistrictToLocationValidation]
        public byte? DistrictsId { get; set; }

        //----------End basic profile info----------------------------------------------------

        [StringLength(255)]
        public string Comments { get; set; }

        [StringLength(200)]
        [Display(Name = "File Access")]
        public string FileAccess { get; set; }//added to suite a migration field

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
        [AddNumberIfIdentificationSelected]
        public string IdentificationNumber { get; set; }

        [Display(Name = "Loan Number/s")]
        [StringLength(164)]
        public string LoanNumber { get; set; }

        //recently added to cater for the data migration and act as a field for storing the previous file number
        [StringLength(94)]
        [Display(Name = "Previous File Number")]
        public string PreviousFileNumber { get; set; }

        //Establish association with volumes, possibly ----------------------------------
        //public static readonly byte DefaultVolume = 1;
        public ICollection<FileVolumes> FileVolumes { get; set; }

        [NotMapped]
        [StringLength(200)]
        [Display(Name = "Volume 1 Description")]
        public string VolumeOneDescription { get; set; }

        
    }
}
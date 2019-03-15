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

        [StringLength(255)]
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
        [AddNumberIfIdentificationSelected]
        public string IdentificationNumber { get; set; }

        
        //Establish association with volumes, possibly ----------------------------------
        //public static readonly byte DefaultVolume = 1;
        public ICollection<FileVolumes> FileVolumes { get; set; }
    }
}
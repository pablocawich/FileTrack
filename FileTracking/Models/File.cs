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


        public string Comments { get; set; }

        //public FileType FileType { get; set; }
        //public int FileTypeId { get; set; }
        [Required]
        [Display(Name = "Date Created")]
        public DateTime DateCreated { get; set; }
    }
}
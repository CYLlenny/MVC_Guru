using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace Gurutw.ViewModels
{
    public class RegisterViewModel
    {
        [StringLength(50, ErrorMessage = "The Username must be at least {2} characters long.", MinimumLength = 6)]
        [Required]
      
        public string UserName { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Field can't be empty")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Email is not valid")]
        
        public string Email { get; set; }

        [StringLength(50, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 8)]
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "*I accept GURU.com's Privacy Policy & Terms and Conditions.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You gotta tick the box!")]
        public bool TermsAndConditions { get; set; }
    }
}
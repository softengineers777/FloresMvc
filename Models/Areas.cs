using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FloresMvc;
using System.ComponentModel.DataAnnotations;

namespace FloresMvc.Models
{
    public class Areas
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdArea { get; set; }
        [Required]
        [Display(Name = "Nombre Area")]
        public string NomArea { get; set; }
        public string? Owner { get; set; }          
        public DateTime DateOwner { get; set; }     
        public string? LastModify { get; set; }     
        public DateTime? LastDate { get; set; }
    }
}
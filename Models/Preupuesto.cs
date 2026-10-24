using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using FloresMvc.Models;
using Microsoft.AspNetCore.Mvc;


namespace FloresMvc.Models
{
    public class Presupuesto
    {
        [Key]
        [Display(Name = "Consecutivo")]
        public int IdPresupuesto {get; set;}
        
    }
}
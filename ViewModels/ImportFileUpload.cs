﻿using System.ComponentModel.DataAnnotations;

namespace ELabel.ViewModels
{
    public class ImportFileUpload
    {
        [Required]
        [Display(Name = "Excel File", Description = "File to import", Prompt = "e-label-products.xlsx")]
        [DataType(DataType.Upload)]
        public required IFormFile File { get; set; }
    }
}

﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ELabel.Models
{
    [Owned]
    public class WineInformation
    {
        [Column("WineVintage")]
        [Display(Name = "Vintage", Description = "The year that the wine was produced. Do not fill for non-vintage wines.")]
        [DisplayFormat(DataFormatString = "{0:D4}")]
        [Range(1, 2099)]
        public ushort? Vintage { get; set; }

        [Column("WineType")]
        [Display(Name = "Type", Description = "Wine classification by vinification process. Sometimes refered as wine 'colour'.")]
        [EnumDataType(typeof(WineType))]
        public WineType? Type { get; set; }

        [Column("WineStyle")]
        [Display(Name = "Style", Description = "Wine style, sweetness of wine or 'product type', according to EU Commission Regulation.")]
        [EnumDataType(typeof(WineStyle))]
        public WineStyle? Style { get; set; }

        [Column("WineAppellation")]
        [Display(Name = "Appellation", Description = "Wine legally defined and protected geographical indication.")]
        [StringLength(100)]
        [DataType(DataType.Text)]
        [MaxLength(100)]
        public string? Appellation { get; set; }

        [Column("WineAlcohol")]
        [Display(Name = "Alcohol", Description = "Alcohol on label (% vol.)")]
        [DisplayFormat(DataFormatString = "{0:G} % vol.")]
        public float? Alcohol { get; set; }

        // TODO: Grape varieties

        // TODO: Production method (Traditional method, Barrel aged, Stainless steel fermented, Cask aged, etc.)
    }
}

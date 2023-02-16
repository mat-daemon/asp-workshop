using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.Models
{
    public class Article
    {
        public int ArticleId { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }
        
        [Range(0, float.MaxValue)]
        [Required]
        public float Price { get; set; }

        [NotMapped]
        public IFormFile Image { get; set; }

        [StringLength(30)]
        public string ImageName { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
    }
}

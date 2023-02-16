using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(30)]
        public string Name { get; set; }

        public ICollection<Article> Articles { get; set; }
    }
}

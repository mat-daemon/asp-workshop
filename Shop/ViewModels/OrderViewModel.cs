using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.ViewModels
{
    public class OrderViewModel
    {
        public CartViewModel cartContent { get; set; }

        [Required]
        [StringLength(30)]
        public String Name { get; set; }

        [Required]
        [StringLength(30)]
        public String Surname { get; set; }

        [Required]
        [StringLength(30)]
        public String Street { get; set; }

        [Required]
        [StringLength(30)]
        public String homeNumber { get; set; }

        [Required]
        [StringLength(30)]
        public String City { get; set; }

        [Required]
        public String Payment { get; set; }
    }
}

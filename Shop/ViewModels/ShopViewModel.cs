using Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.ViewModels
{
    public class ShopViewModel
    {
        public ICollection<Article> articles { get; set; }
        public ICollection<Category> categories { get; set; }

    }
}

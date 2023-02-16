using Shop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.ViewModels
{
    public class CartViewModel
    {
        public ICollection<(Article, string)> Articles { get; set; }
        public float totalSum { get; set; }

        public CartViewModel()
        {
            Articles = new List<(Article, string)>();
            totalSum = 0;
        }
        
    }
}

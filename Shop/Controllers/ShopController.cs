using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop.Controllers
{
    public class ShopController : Controller
    {

        private readonly ShopDbContext _context;
        private int cookieLifeTime = 7 * 3600;

        public ShopController(ShopDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? id)
        {
            var shopDbContextCategories = _context.Category;
            var categories = await shopDbContextCategories.ToListAsync();

            var categoriesProducts = new ShopViewModel();
            categoriesProducts.categories = categories;

            if (id == null)
            {
                var shopDbContext = _context.Article.Include(a => a.Category);
                var articles = await shopDbContext.ToListAsync();
                categoriesProducts.articles = articles;
            }
            else
            {
                var shopDbContext = _context.Article.Include(a => a.Category).Where(a => a.CategoryId == id);
                var articles = await shopDbContext.ToListAsync();
                categoriesProducts.articles = articles;
            }


            return View(categoriesProducts);
        }

        [Authorize(Roles="Client")]
        [AllowAnonymous]
        public IActionResult Add(int? id)
        {
            if (id != null)
            {
                string articleIdCookieKey = "article" + id.ToString();

                if (Request.Cookies[articleIdCookieKey] == null) SetCookie(articleIdCookieKey, "1", cookieLifeTime);
                else
                {
                    var articleQuantity = Int32.Parse(Request.Cookies[articleIdCookieKey]) + 1;
                    Response.Cookies.Delete(articleIdCookieKey);
                    SetCookie(articleIdCookieKey, articleQuantity.ToString(), cookieLifeTime);
                }

            }

            return RedirectToAction("Index");
        }


        private async Task<CartViewModel> getCartContent()
        {
            Dictionary<int, string> articlesInCart = new Dictionary<int, string>();
            var articlesForView = new CartViewModel();
            float totalSum = 0;

            foreach (var cookie in Request.Cookies)
            {
                if (cookie.Key.StartsWith("article"))
                {
                    var articleId = Int32.Parse(cookie.Key.Substring("article".Length));
                    var articleQuantity = cookie.Value;
                    articlesInCart[articleId] = articleQuantity;

                }
            }

            //Convert to list, because Linq can't process Where clause with dictionary
            List<int> articleKeys = articlesInCart.Keys.ToList();

            var cartDbContext = await _context.Article.Include(a => a.Category).Where(a => articleKeys.Contains(a.ArticleId)).ToListAsync();

            foreach (var article in cartDbContext)
            {
                articlesForView.Articles.Add((article, articlesInCart[article.ArticleId]));
                totalSum += (float)Int32.Parse(articlesInCart[article.ArticleId]) * article.Price;
            }

            articlesForView.totalSum = totalSum;

            return articlesForView;
        }
        //--------------------------------------------
        //Cart methods
        //--------------------------------------------
        [Authorize(Roles = "Client")]
        [AllowAnonymous]
        public async Task<IActionResult> Cart()
        {
            Dictionary<int, string> articlesInCart = new Dictionary<int, string>();
            var articlesForView = new CartViewModel();
            float totalSum = 0;

            foreach (var cookie in Request.Cookies)
            {
                if (cookie.Key.StartsWith("article"))
                {
                    var articleId = Int32.Parse(cookie.Key.Substring("article".Length));
                    var articleQuantity = cookie.Value;
                    articlesInCart[articleId] = articleQuantity;

                }
            }

            //Convert to list, because Linq can't process Where clause with dictionary
            List<int> articleKeys = articlesInCart.Keys.ToList();

            var cartDbContext = await _context.Article.Include(a => a.Category).Where(a => articleKeys.Contains(a.ArticleId)).ToListAsync();

            foreach (var article in cartDbContext)
            {
                articlesForView.Articles.Add((article, articlesInCart[article.ArticleId]));
                totalSum += (float)Int32.Parse(articlesInCart[article.ArticleId]) * article.Price;
            }

            articlesForView.totalSum = totalSum;

            return View(articlesForView);
        }

        [Authorize(Roles = "Client")]
        [AllowAnonymous]
        public IActionResult AddQuantityInCart(int id)
        {
            string articleKey = id.ToString();

            int articleQuantity;
            try
            {
                articleKey = "article" + articleKey;
                if (Request.Cookies[articleKey] == null) SetCookie(articleKey, "1", cookieLifeTime);
                else
                {
                    articleQuantity = Int32.Parse(Request.Cookies[articleKey]) + 1;
                    Response.Cookies.Delete(articleKey);
                    SetCookie(articleKey, articleQuantity.ToString(), cookieLifeTime);
                }

            }
            catch
            {

            }
            return RedirectToAction("Cart");
        }

        [Authorize(Roles = "Client")]
        [AllowAnonymous]
        public IActionResult DecreaseQuantityInCart(int id)
        {
            string articleKey = id.ToString();

            int articleQuantity;
            try
            {
                articleKey = "article" + articleKey;

                articleQuantity = Int32.Parse(Request.Cookies[articleKey]) - 1;
                Response.Cookies.Delete(articleKey);

                if (articleQuantity > 0) SetCookie(articleKey, articleQuantity.ToString(), cookieLifeTime);

            }
            catch
            {

            }
            return RedirectToAction("Cart");
        }

        [Authorize(Roles = "Client")]
        [AllowAnonymous]
        public IActionResult DeleteFromCart(int id)
        {
            string articleKey = id.ToString();
            try
            {
                articleKey = "article" + articleKey;
                Response.Cookies.Delete(articleKey);
            }
            catch
            {

            }

            return RedirectToAction("Cart");
        }

        //--------------------------------------------
        //End of cart methods
        //--------------------------------------------


        public void SetCookie(string key, string value, int? numberOfSeconds = null)
        {
            CookieOptions option = new CookieOptions();
            if (numberOfSeconds.HasValue)
                option.Expires = DateTime.Now.AddSeconds(numberOfSeconds.Value);
            Response.Cookies.Append(key, value, option);
        }

        private List<String> getPaymentMethods()
        {
            List<String> paymentMethods = new List<string>();
            paymentMethods.Add("Cash");
            paymentMethods.Add("BLIK");
            paymentMethods.Add("Bank transfer");
            paymentMethods.Add("Card");

            return paymentMethods;
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Order()
        {
            var cartContent = await getCartContent();

            if (cartContent.Articles.Count == 0) return RedirectToAction("cart");

            var order = new OrderViewModel();
            order.cartContent = cartContent;

            
            ViewData["Payment"] = new SelectList(getPaymentMethods());

            return View(order);
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Summary([Bind("Name, Surname, Street, homeNumber, City, Payment")] OrderViewModel order)
        {
            var cartContent = await getCartContent(); 
            order.cartContent = cartContent;



            if (ModelState.IsValid)
            {
                // empty cart
                foreach (var cookie in Request.Cookies)
                {
                    if (cookie.Key.StartsWith("article"))
                    {
                        CookieOptions option = new CookieOptions();
                        option.Expires = DateTime.Now.AddDays(-1d);
                        Response.Cookies.Append(cookie.Key, cookie.Value, option);
                    }
                }
                return View(order);
            }

            ViewData["Payment"] = new SelectList(getPaymentMethods());

            return View("Order", order);
        }

    }
}

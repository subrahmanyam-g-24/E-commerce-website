using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ecommerce_App_prac1.Models;
using Humanizer;
using Ecommerce_App_prac1.Services;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json.Linq;
using Microsoft.CodeAnalysis;
using System.Numerics;
using Microsoft.AspNetCore.Http;

namespace Ecommerce_App_prac1.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ExchangeRateService _exchangeRateService;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
            _exchangeRateService = new ExchangeRateService("e57aee90af3d5c3ff3504049");
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Currency")))
            {
                HttpContext.Session.SetString("Currency", "USD");
            }
            return View(await _context.Products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Name,Price,Description,Stock")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            HttpContext.Session.SetString("Currency", "USD");
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Price,Description,Stock")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        public async Task<IActionResult> ConvertToUSD()
        {
            if (HttpContext.Session.GetString("Currency") == "USD") return RedirectToAction(nameof(Index));
            else
            {
                var exchangeRateData = await _exchangeRateService.GetLatestRatesAsync("INR");
                decimal usdRate = exchangeRateData["conversion_rates"]["USD"].Value<decimal>();

                var products = await _context.Products.ToListAsync();
                foreach (var product in products)
                {
                    product.Price = product.Price * usdRate; // Update price in memory

                }
                HttpContext.Session.SetString("Currency", "USD");                
                _context.UpdateRange(products); // Mark products as updated
                await _context.SaveChangesAsync(); // Save changes to the database

                return RedirectToAction(nameof(Index)); // Redirect to the Index view
            }
        }


        public async Task<IActionResult> ConvertToINR()
        {
            if (HttpContext.Session.GetString("Currency") == "INR") return RedirectToAction(nameof(Index));
            else
            {
                var exchangeRateData = await _exchangeRateService.GetLatestRatesAsync("USD");
                decimal inrRate = exchangeRateData["conversion_rates"]["INR"].Value<decimal>();

                var products = await _context.Products.ToListAsync();
                foreach (var product in products)
                {
                    product.Price = product.Price * inrRate; // Update price in memory

                }
                HttpContext.Session.SetString("Currency", "INR");

                _context.UpdateRange(products); // Mark products as updated
                await _context.SaveChangesAsync(); // Save changes to the database

                return RedirectToAction(nameof(Index)); // Redirect to the Index view
            }
        }

        public IActionResult AddToCart(Dictionary<int, int> quantities)
        {
            var cart = new List<Cart>();

            foreach (var entry in quantities)
            {
                var product = _context.Products.Find(entry.Key);
                if (entry.Value >= 1)
                {
                    cart.Add(new Cart
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Quantity = entry.Value,
                        Price = entry.Value * product.Price,
                    });
                }
            }

            var totalCart = cart.Sum(item => item.Price);
            if (totalCart <= 0)
            {
                TempData["Warning"] = "Please select at least one item to add to the cart.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                HttpContext.Session.SetString("TotalCart", totalCart.ToString());

                HttpContext.Session.SetObject("Cart", cart);
                return RedirectToAction("ViewCart");
            }
        }
        public IActionResult ViewCart()
        {
            var cart = HttpContext.Session.GetObject<List<Cart>>("Cart") ?? new List<Cart>();
            return View(cart);
        }

        public async Task<IActionResult> ProceedtoPay()
        {
            var products = await _context.Products.ToListAsync();
            var cart = HttpContext.Session.GetObject<List<Cart>>("Cart") ?? new List<Cart>();
            foreach (var cartItem in cart)
            {
                var product = products.FirstOrDefault(p => p.ProductId == cartItem.ProductId);
                if (product != null)
                {
                    product.Stock -= cartItem.Quantity;
                }

            }
            await _context.SaveChangesAsync();
            return View("PaymentSuccess");
        }



    }
}

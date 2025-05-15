using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStoreMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            var Products = context.Products.OrderByDescending(p => p.Id).ToList();

            return View(Products);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(ProductDto productDto)

        {
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }

            if (!ModelState.IsValid)
            {
                return View(productDto);
            }

            string newFileName = DateTime.Now.ToString("yyyyMMDDHHmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

            string imageFullPath = environment.WebRootPath + "/Products/" + newFileName;
            using (var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }
            Product product = new Product()
            {
                ProductName = productDto.ProductName,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreateAt = DateTime.Now,
            };

            context.Products.Add(product);
            context.SaveChanges();
            return RedirectToAction("Index", "Product");
        }
        public IActionResult Edit(int Id)
        {
            var product = context.Products.Find(Id);

            if (product == null)
            {
                return RedirectToAction("Index", "Product");
            }

            var productDto = new ProductDto()
            {
                ProductName = product.ProductName,
                Brand = product.Brand,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreateAt.ToString("dd/MM/yyyy");

            return View(productDto);
        }
        [HttpPost]
        public IActionResult Edit(int Id, ProductDto productDto)
        {
            var product = context.Products.Find(Id);
            if (product == null)
            {
                return RedirectToAction("Index", "Product");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreateAt.ToString("dd/MM/yyyy");

                return View(productDto);
            }

            string newFileName = product.ImageFileName;
            if (productDto.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMDDHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile.FileName);

                string imageFullPath = environment.WebRootPath + "/Products/" + newFileName;
                using (var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                string oldImageFullPath = environment.WebRootPath + "/products" + product.ImageFileName;
                System.IO.File.Delete(oldImageFullPath);
            }

            product.ProductName = productDto.ProductName;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();
            return RedirectToAction("Index", "Product");
        }
        public IActionResult Delete(int Id, ProductDto productDto)
        {
            var product = context.Products.Find(Id);
            if (product == null)
            {
                return RedirectToAction("Index", "Product");
            }
            string imageFullPath = environment.WebRootPath + "/Products/" + product.ImageFileName;
            System.IO.File.Delete(imageFullPath);

            context.Products.Remove(product);
            context.SaveChanges(true);
            return RedirectToAction("Index", "Product");

        }
    }
}

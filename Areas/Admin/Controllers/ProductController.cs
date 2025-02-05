﻿using AutoMapper;
using ELabel.Data;
using ELabel.Extensions;
using ELabel.Models;
using ELabel.ViewModels;
using Ganss.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ELabel.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize()]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly Producer _producer;

        public ProductController(ApplicationDbContext context, IMapper mapper, IOptions<Producer> producerConfiguration)
        {
            _context = context;
            _mapper = mapper;
            _producer = producerConfiguration.Value;
        }

        // GET: Product
        public IActionResult Index(string filterText = "", string sortOrder = "")
        {
            if (_context.Product == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Product'  is null.");
            }

            ViewBag.SortParmName = string.IsNullOrEmpty(sortOrder) ? "-Name" : "";
            ViewBag.SortParmVolume = sortOrder == "Volume" ? "-Volume" : "Volume";
            ViewBag.SortParmWineVintage = sortOrder == "WineVintage" ? "-WineVintage" : "WineVintage";
            ViewBag.SortParmWineType = sortOrder == "WineType" ? "-WineType" : "WineType";
            ViewBag.SortParmWineStyle = sortOrder == "WineStyle" ? "-WineStyle" : "WineStyle";
            ViewBag.SortParmWineAppelation = sortOrder == "WineAppelation" ? "-WineAppelation" : "WineAppelation";
            ViewBag.SortParmSku = sortOrder == "Sku" ? "-Sku" : "Sku";

            var query = _context.Product
                                .AsNoTracking()
                                .OrderBy(i => i.Name)
                                .ThenBy(i => i.Volume)
                                .ThenBy(i => i.WineInformation.Vintage)
                                .AsQueryable()
                                .AsEnumerable();

            // Filter

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                filterText = filterText.Trim().ToLower();
                float filterFloat;
                ushort filterUShort;

                query = query.Where(p => p.Name.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
                                          p.Volume != null && float.TryParse(filterText, out filterFloat) && p.Volume == filterFloat ||
                                          p.WineInformation.Vintage != null && ushort.TryParse(filterText, out filterUShort) && p.WineInformation.Vintage == filterUShort ||
                                          p.WineInformation.Type != null && EnumHelper.GetDisplayName(p.WineInformation.Type)?.ToLower() == filterText ||
                                          p.WineInformation.Style != null && EnumHelper.GetDisplayName(p.WineInformation.Style)?.ToLower() == filterText ||
                                          p.WineInformation.Appellation != null && p.WineInformation.Appellation.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) ||
                                          p.Sku != null && p.Sku.Contains(filterText, StringComparison.InvariantCultureIgnoreCase)
                                         
                                   );
            }

            // Sort

            switch (sortOrder)
            {
                default: // "Name":
                    query = query.OrderBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "-Name":
                    query = query.OrderByDescending(p => p.Name).ThenByDescending(p => p.Volume).ThenByDescending(p => p.WineInformation.Vintage);
                    break;
                case "Volume":
                    query = query.OrderBy(p => p.Volume).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "-Volume":
                    query = query.OrderByDescending(p => p.Volume).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "WineVintage":
                    query = query.OrderBy(p => p.WineInformation.Vintage).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "-WineVintage":
                    query = query.OrderByDescending(p => p.WineInformation.Vintage).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "WineType":
                    query = query.OrderBy(p => p.WineInformation.Type).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "-WineType":
                    query = query.OrderByDescending(p => p.WineInformation.Type).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "WineStyle":
                    query = query.OrderBy(p => p.WineInformation.Style).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "-WineStyle":
                    query = query.OrderByDescending(p => p.WineInformation.Style).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "WineAppelation":
                    query = query.OrderBy(p => p.WineInformation.Appellation).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "-WineAppelation":
                    query = query.OrderByDescending(p => p.WineInformation.Appellation).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "Sku":
                    query = query.OrderBy(p => p.Sku).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
                case "-Sku":
                    query = query.OrderByDescending(p => p.Sku).ThenBy(p => p.Name).ThenBy(p => p.Volume).ThenBy(p => p.WineInformation.Vintage);
                    break;
            }

            ViewBag.FilterText = filterText;
            return View(_mapper.Map<IEnumerable<WineProductDetailsDto>>(query.ToList()));

        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                                        .Include(p => p.Image)
                                        .Include(p => p.ProductIngredients.OrderBy(pi => pi.Order))
                                        .ThenInclude(pi => pi.Ingredient)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            WineProductDetailsDto wineProductDetailsDto = _mapper.Map<WineProductDetailsDto>(product);

            return View(wineProductDetailsDto);
        }

        // GET: Product/Preview/5
        [AllowAnonymous]
        public IActionResult Preview(Guid? id)
        {
            return RedirectToAction("Product", "Label", new { area = "", id });
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Volume,WineInformation")] WineProductCreateDto wineProductCreateDto)
        {
            if (ModelState.IsValid)
            {
                Product product = _mapper.Map<Product>(wineProductCreateDto);
                product.Id = Guid.NewGuid();
                product.Kind = ProductKind.Wine;
                //product.NutritionInformation = new NutritionInformation();

                _context.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Edit), new { id = product.Id });
            }
            return View(wineProductCreateDto);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                                        .Include(p => p.Image)
                                        .Include(p => p.ProductIngredients.OrderBy(pi => pi.Order))
                                        //.ThenInclude(pi => pi.Ingredient)
                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            WineProductEditDto wineProductEditDto = _mapper.Map<WineProductEditDto>(product);

            ViewBag.Ingredients = GetAvailableIngredientsList();
            return View(wineProductEditDto);
        }

        // POST: Product/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Volume,WineInformation,ProductIngredients,NutritionInformation,ResponsibleConsumption,Certifications,Country,Sku,Ean")] WineProductEditDto wineProductEditDto)
        {
            if (id != wineProductEditDto.Id)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update Product

                    _mapper.Map(wineProductEditDto, product);

                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // Update Product Ingredients

                    var productIngredients = await _context.ProductIngredient
                                                           .Where(pi => pi.ProductId == product.Id)
                                                           .ToListAsync();

                    //_context.ChangeTracker.DetectChanges();
                    //Console.WriteLine(_context.ChangeTracker.DebugView.LongView);

                    productIngredients.ForEach(pi =>
                    {
                        _context.Entry(pi).State = EntityState.Deleted;
                    });

                    short order = 0;
                    foreach (ProductIngredientDto productIngredientDto in wineProductEditDto.ProductIngredients.OrderBy(pi => pi.Order))
                    {
                        ProductIngredient? auxProductIngredient = productIngredients
                                                                    .Where(pi => pi.Id == productIngredientDto.Id && pi.ProductId == product.Id).FirstOrDefault();

                        if (productIngredientDto.ToDelete)
                        {
                            if (auxProductIngredient == null)
                                continue;

                            _mapper.Map(productIngredientDto, auxProductIngredient);

                            _context.Entry(auxProductIngredient).State = EntityState.Deleted;
                            _context.Remove(auxProductIngredient);

                            continue;
                        }

                        if (auxProductIngredient == null)
                        {
                            auxProductIngredient = new ProductIngredient()
                            {
                                Id = Guid.NewGuid(),
                                ProductId = product.Id,
                                IngredientId = productIngredientDto.IngredientId
                            };

                            auxProductIngredient.Order = ++order;

                            _context.Entry(auxProductIngredient).State = EntityState.Added;
                            _context.Add(auxProductIngredient);

                        }
                        else
                        {
                            _mapper.Map(productIngredientDto, auxProductIngredient);

                            auxProductIngredient.Order = ++order;

                            _context.Entry(auxProductIngredient).State = EntityState.Modified;
                            _context.Update(auxProductIngredient);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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

            ViewBag.Ingredients = GetAvailableIngredientsList();
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(_mapper.Map<WineProductDetailsDto>(product));
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Product == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Product'  is null.");
            }
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(Guid id)
        {
            return (_context.Product?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private Guid? FindProductId(string name, float? volume, ushort? wineVintage)
        {
            return _context.Product.Where(e => e.Name == name && e.Volume == volume && e.WineInformation.Vintage == wineVintage).AsNoTracking().FirstOrDefault()?.Id;
        }

        private SelectList GetAvailableIngredientsList()
        {
            var ingredients = _context.Ingredient
                                .AsNoTracking()
                                .Select(i => new
                                {
                                    i.Id,
                                    Name = i.Name + " (" + EnumHelper.GetDisplayName(i.Category) + ")"
                                })
                                .ToList();

            return new SelectList(ingredients, "Id", "Name");
        }

        private bool ImageExists(Guid id)
        {
            return (_context.Image?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // GET: Product/Export
        public async Task<IActionResult> Export()
        {
            var query = await _context.Product
                                      .Include(p => p.Image)
                                      .Include(p => p.ProductIngredients.OrderBy(pi => pi.Order))
                                        .ThenInclude(pi => pi.Ingredient)
                                      .AsNoTracking()
                                      .OrderBy(p => p.Name)
                                      .ToListAsync();

            List<WineProductDetailsDto> products = _mapper.Map<List<WineProductDetailsDto>>(query);

            byte[] byteArray;
            var excel = new ExcelMapper();

            using (MemoryStream stream = new MemoryStream())
            {
                excel.Save(stream, products, "Products");

                byteArray = stream.ToArray();
            }

            return File(byteArray, "application/xlsx", $"elabel-products.xlsx");
        }

        // GET: Product/Import
        public IActionResult Import()
        {
            return View();
        }

        // POST: Product/Import
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import([Bind("File")] ImportFileUpload importFileUpload)
        {
            if (!ModelState.IsValid)
            {
                return View(importFileUpload);
            }

            if (importFileUpload.File == null || importFileUpload.File.Length == 0)
            {
                ModelState.AddModelError("CustomError", "Empty file!");
                return View(importFileUpload);
            }

            IEnumerable<WineProductDetailsDto> importedProducts;

            using (var memoryStream = new MemoryStream())
            {
                await importFileUpload.File.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                try
                {
                    ExcelMapper excelMapper = new ExcelMapper(memoryStream);
                    importedProducts = excelMapper.Fetch<WineProductDetailsDto>("Products");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("CustomError", e.Message);
                    return View(importFileUpload);
                }
            }

            if (importedProducts == null || !importedProducts.Any())
            {
                ModelState.AddModelError("CustomError", "Empty excel!");
                return View(importFileUpload);
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                foreach (WineProductDetailsDto importedProduct in importedProducts)
                {
                    Product product = _mapper.Map<Product>(importedProduct);

                    if (product.Id == Guid.Empty)
                    {
                        Guid? existingId = FindProductId(product.Name, product.Volume, product.WineInformation.Vintage);

                        product.Id = existingId == null ? Guid.NewGuid() : existingId.Value;
                    }

                    if (ProductExists(product.Id))
                    {
                        _context.Update(product);
                    }
                    else
                    {
                        _context.Add(product);
                    }

                    await _context.SaveChangesAsync();

                    if (importedProduct.Image != null && !string.IsNullOrEmpty(importedProduct.Image.DataUrl))
                    {
                        bool newImage = false;
                        var image = await _context.Image.FirstOrDefaultAsync(m => m.ProductId == product.Id);

                        if (image == null)
                        {
                            newImage = true;
                            image = new Image()
                            {
                                Id = Guid.NewGuid(),
                                ProductId = product.Id,
                                ContentType = string.Empty,
                                Content = new byte[0]
                            };
                        }

                        byte[]? imageByteBuffer = ImageHelper.ConvertFromDataUrl(importedProduct.Image.DataUrl);

                        if (imageByteBuffer == null)
                            continue;

                        OptimizedImage? optimizedImage = ImageHelper.Optimize(imageByteBuffer, ImageFileUpload.MaxWidth, ImageFileUpload.MaxHeight, ImageFileUpload.Quality);

                        if (optimizedImage == null || optimizedImage.Width < ImageFileUpload.MinWidth || optimizedImage.Height < ImageFileUpload.MinHeight)
                            continue;

                        image.ContentType = optimizedImage.ContentType;
                        image.Content = optimizedImage.Content;
                        image.Width = optimizedImage.Width;
                        image.Height = optimizedImage.Height;

                        if (newImage)
                            _context.Add(image);
                        else
                            _context.Update(image);
                        await _context.SaveChangesAsync();
                    }
                }

                // Commit transaction if all commands succeed, transaction will auto-rollback
                // when disposed if either commands fails
                transaction.Commit();
            }

            return RedirectToAction(nameof(Index));
        }

        // Get: Product/DeleteImage/5
        public async Task<IActionResult> DeleteImage(Guid id)
        {
            if (_context.Image == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Image'  is null.");
            }

            await _context.Image.Where(i => i.ProductId == id).ExecuteDeleteAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Product/ChangeImage
        public IActionResult ChangeImage(Guid id)
        {
            ImageFileUpload imageFileUpload = new ImageFileUpload()
            {
                ProductId = id
            };

            return View(imageFileUpload);
        }

        // POST: Analysis/ChangeImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeImage(Guid id, [Bind("File,ProductId")] ImageFileUpload imageFileUpload)
        {
            if (id != imageFileUpload.ProductId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(imageFileUpload);
            }

            if (imageFileUpload.File == null || imageFileUpload.File.Length == 0)
            {
                ModelState.AddModelError("CustomError", "Empty file!");
                return View(imageFileUpload);
            }

            byte[] byteArray;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await imageFileUpload.File.CopyToAsync(memoryStream);

                memoryStream.Position = 0;
                byteArray = memoryStream.ToArray();
            }

            try
            {
                // Resize image

                OptimizedImage? optimizedImage = ImageHelper.Optimize(byteArray, ImageFileUpload.MaxWidth, ImageFileUpload.MaxHeight, ImageFileUpload.Quality);

                if (optimizedImage == null)
                {
                    ModelState.AddModelError("CustomError", $"Invalid image format! Try another file.");
                    return View(imageFileUpload);
                }

                if (optimizedImage.Width < ImageFileUpload.MinWidth || optimizedImage.Height < ImageFileUpload.MinHeight)
                {
                    ModelState.AddModelError("CustomError", $"Image is too small ({optimizedImage.Width}x{optimizedImage.Height})! Chose an image with, at least, {ImageFileUpload.MinWidth}x{ImageFileUpload.MinHeight} pixels.");
                    return View(imageFileUpload);
                }

                // Delete existing image

                await _context.Image.Where(i => i.ProductId == id).ExecuteDeleteAsync();

                // Add new image to database

                Image image = new Image()
                {
                    Id = Guid.NewGuid(),
                    Content = optimizedImage.Content,
                    ContentType = optimizedImage.ContentType,
                    Width = optimizedImage.Width,
                    Height = optimizedImage.Height,
                    ProductId = id
                };

                _context.Add(image);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("CustomError", e.Message);
                return View(imageFileUpload);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddIngredient([Bind("ProductIngredients")] WineProductEditDto wineProductEditDto)
        {
            wineProductEditDto.ProductIngredients.Add(new ProductIngredientDto()
            {
                Id = Guid.Empty,
                ProductId = wineProductEditDto.Id,
                IngredientId = Guid.Empty,
                Order = 99
            });

            ViewBag.Ingredients = GetAvailableIngredientsList();
            return PartialView("ProductIngredients", wineProductEditDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveIngredient([Bind("ProductIngredients")] WineProductEditDto wineProductEditDto)
        {
            if (wineProductEditDto.ProductIngredients.Count == 0)
            {
                ViewBag.Ingredients = GetAvailableIngredientsList();
                return PartialView("ProductIngredients", wineProductEditDto);
            }

            wineProductEditDto.ProductIngredients.RemoveAt(wineProductEditDto.ProductIngredients.Count - 1);

            ViewBag.Ingredients = GetAvailableIngredientsList();
            return PartialView("ProductIngredients", wineProductEditDto);
        }

    }
}

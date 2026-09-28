using Microsoft.AspNetCore.Mvc;
using NewBalanceShop.DAL.Entities;
using NewBalanceShop.DAL.Interfaces;

namespace NewBalanceShop.PL.Controllers;

public class ProductsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return View(products.OrderBy(p => p.Id).ToList());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var product = await _unitOfWork.Products.GetByIdAsync(id.Value);
        if (product is null) return NotFound();

        return View(product);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Brand,Category,Size,Color,Price,Stock,Description")] Product product)
    {
        if (!ModelState.IsValid) return View(product);

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var product = await _unitOfWork.Products.GetByIdAsync(id.Value);
        if (product is null) return NotFound();

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Brand,Category,Size,Color,Price,Stock,Description")] Product product)
    {
        if (id != product.Id) return NotFound();

        if (!ModelState.IsValid) return View(product);

        var existing = await _unitOfWork.Products.GetByIdAsync(id);
        if (existing is null) return NotFound();

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var product = await _unitOfWork.Products.GetByIdAsync(id.Value);
        if (product is null) return NotFound();

        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is not null)
        {
            _unitOfWork.Products.Delete(product);
            await _unitOfWork.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}

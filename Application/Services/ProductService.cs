using NewBalanceShop.Application.DTO;
using NewBalanceShop.Application.Interfaces;
using NewBalanceShop.Application.Mapping;
using NewBalanceShop.Domain.Interfaces;

namespace NewBalanceShop.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(p => p.ToDto()).ToList();
    }

    public async Task<List<ProductDto>> GetByCategoryAsync(int categoryId)
    {
        var products = await _productRepository.GetByCategoryAsync(categoryId);
        return products.Select(p => p.ToDto()).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product?.ToDto();
    }

    public async Task<ProductDto> CreateAsync(ProductDto dto)
    {
        var entity = dto.ToEntity();
        entity.Id = 0;
        await _productRepository.AddAsync(entity);
        await _productRepository.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task<bool> UpdateAsync(int id, ProductDto dto)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing is null) return false;

        existing.Name = dto.Name;
        existing.Brand = dto.Brand;
        existing.CategoryId = dto.CategoryId;
        existing.Size = dto.Size;
        existing.Color = dto.Color;
        existing.Price = dto.Price;
        existing.Stock = dto.Stock;
        existing.ImageUrl = dto.ImageUrl;
        existing.Description = dto.Description;

        _productRepository.Update(existing);
        await _productRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing is null) return false;

        _productRepository.Remove(existing);
        await _productRepository.SaveChangesAsync();
        return true;
    }
}

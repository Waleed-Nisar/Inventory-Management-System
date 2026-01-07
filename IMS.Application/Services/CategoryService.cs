using IMS.Application.Interfaces;
using IMS.Domain.Entities;
using IMS.Infrastructure.Data;
using IMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.Services
{
    /// <summary>
    /// Category service implementation
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _categoryRepository;
        private readonly ApplicationDbContext _context;

        public CategoryService(IRepository<Category> categoryRepository, ApplicationDbContext context)
        {
            _categoryRepository = categoryRepository;
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _categoryRepository.FindAsync(c => c.IsActive);
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            try
            {
                category.CreatedDate = DateTime.UtcNow;
                return await _categoryRepository.AddAsync(category);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating category: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                var existing = await _categoryRepository.GetByIdAsync(category.Id);
                if (existing == null)
                    return false;

                existing.Name = category.Name;
                existing.Description = category.Description;
                existing.IsActive = category.IsActive;
                existing.ModifiedDate = DateTime.UtcNow;

                await _categoryRepository.UpdateAsync(existing);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating category: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return false;

                // Business rule: Cannot delete category with products
                if (await CategoryHasProductsAsync(id))
                    throw new InvalidOperationException("Cannot delete category with existing products.");

                await _categoryRepository.DeleteAsync(category);
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting category: {ex.Message}", ex);
            }
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _categoryRepository.ExistsAsync(id);
        }

        public async Task<bool> CategoryHasProductsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == id);
        }
    }
}

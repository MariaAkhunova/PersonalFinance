using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.API.Data;
using PersonalFinance.API.Models;
using PersonalFinance.API.Models.DTOs;

namespace PersonalFinance.API.Controllers
{
    [Authorize]
    public class CategoriesController : BaseApiController
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetCategories()
        {
            var userId = GetUserId();
            var categories = await _context.Categories
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                IsIncome = c.IsIncome,
                Description = c.Description,
                UserId = c.UserId
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponseDto>> GetCategory(int id)
        {
            var userId = GetUserId();
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
                return NotFound();

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                IsIncome = category.IsIncome,
                Description = category.Description,
                UserId = category.UserId
            };
        }

        [HttpPost]
        public async Task<ActionResult<CategoryResponseDto>> PostCategory([FromBody] CategoryDto categoryDto)
        {
            var userId = GetUserId();

            var category = new Category
            {
                Name = categoryDto.Name,
                IsIncome = categoryDto.IsIncome,
                Description = categoryDto.Description,
                UserId = userId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var response = new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                IsIncome = category.IsIncome,
                Description = category.Description,
                UserId = category.UserId
            };

            return CreatedAtAction("GetCategory", new { id = category.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, [FromBody] CategoryDto categoryDto)
        {
            var userId = GetUserId();
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (existingCategory == null)
                return NotFound();

            existingCategory.Name = categoryDto.Name;
            existingCategory.Description = categoryDto.Description;
            existingCategory.IsIncome = categoryDto.IsIncome;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetUserId();
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
                return NotFound();

            var hasTransactions = await _context.Transactions
                .AnyAsync(t => t.CategoryId == id);

            if (hasTransactions)
                return BadRequest("Cannot delete category with existing transactions");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
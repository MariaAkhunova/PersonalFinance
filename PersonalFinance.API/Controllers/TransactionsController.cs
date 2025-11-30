using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.API.Data;
using PersonalFinance.API.Models;
using PersonalFinance.API.Models.DTOs;

namespace PersonalFinance.API.Controllers
{
    [Authorize]
    public class TransactionsController : BaseApiController
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetTransactions(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? categoryId = null)
        {
            var userId = GetUserId();
            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.Date <= endDate.Value);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            return transactions.Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Description = t.Description,
                Date = t.Date,
                CategoryId = t.CategoryId,
                UserId = t.UserId,
                CategoryName = t.Category.Name,
                IsIncome = t.Category.IsIncome
            }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionResponseDto>> GetTransaction(int id)
        {
            var userId = GetUserId();
            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null)
                return NotFound();

            return new TransactionResponseDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Description = transaction.Description,
                Date = transaction.Date,
                CategoryId = transaction.CategoryId,
                UserId = transaction.UserId,
                CategoryName = transaction.Category.Name,
                IsIncome = transaction.Category.IsIncome
            };
        }

        [HttpPost]
        public async Task<ActionResult<TransactionResponseDto>> PostTransaction([FromBody] TransactionDto transactionDto)
        {
            var userId = GetUserId();
            
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == transactionDto.CategoryId && c.UserId == userId);

            if (category == null)
                return BadRequest("Invalid category");

            var transaction = new Transaction
            {
                Amount = transactionDto.Amount,
                Description = transactionDto.Description,
                Date = transactionDto.Date,
                CategoryId = transactionDto.CategoryId,
                UserId = userId
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            await _context.Entry(transaction).Reference(t => t.Category).LoadAsync();

            var response = new TransactionResponseDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Description = transaction.Description,
                Date = transaction.Date,
                CategoryId = transaction.CategoryId,
                UserId = transaction.UserId,
                CategoryName = transaction.Category.Name,
                IsIncome = transaction.Category.IsIncome
            };

            return CreatedAtAction("GetTransaction", new { id = transaction.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, [FromBody] TransactionDto transactionDto)
        {
            var userId = GetUserId();
            var existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (existingTransaction == null)
                return NotFound();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == transactionDto.CategoryId && c.UserId == userId);

            if (category == null)
                return BadRequest("Invalid category");

            existingTransaction.Amount = transactionDto.Amount;
            existingTransaction.Description = transactionDto.Description;
            existingTransaction.Date = transactionDto.Date;
            existingTransaction.CategoryId = transactionDto.CategoryId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var userId = GetUserId();
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (transaction == null)
                return NotFound();

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetSummary([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var userId = GetUserId();
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            var income = transactions.Where(t => t.Category.IsIncome).Sum(t => t.Amount);
            var expenses = transactions.Where(t => !t.Category.IsIncome).Sum(t => t.Amount);
            var balance = income - expenses;

            return new
            {
                Income = income,
                Expenses = expenses,
                Balance = balance,
                TransactionCount = transactions.Count
            };
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}
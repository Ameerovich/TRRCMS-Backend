using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repository implementation for Person entity
    /// </summary>
    public class PersonRepository : IPersonRepository
    {
        private readonly ApplicationDbContext _context;

        public PersonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Persons
                .Include(p => p.PropertyRelations)
                .Include(p => p.Household)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        }

        public async Task<List<Person>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Persons
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.FamilyNameArabic)
                .ThenBy(p => p.FirstNameArabic)
                .ToListAsync(cancellationToken);
        }

        public async Task<Person?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                return null;

            return await _context.Persons
                .FirstOrDefaultAsync(p => p.NationalId == nationalId && !p.IsDeleted, cancellationToken);
        }
        public async Task<List<Person>> GetByHouseholdIdAsync(Guid householdId, CancellationToken cancellationToken = default)
        {
            return await _context.Persons
                .Where(p => p.HouseholdId == householdId && !p.IsDeleted)
                .OrderBy(p => p.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Person>> SearchByNameAsync(
            string? firstName,
            string? fatherName,
            string? familyName,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Persons
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                query = query.Where(p => p.FirstNameArabic.Contains(firstName));
            }

            if (!string.IsNullOrWhiteSpace(fatherName))
            {
                query = query.Where(p => p.FatherNameArabic.Contains(fatherName));
            }

            if (!string.IsNullOrWhiteSpace(familyName))
            {
                query = query.Where(p => p.FamilyNameArabic.Contains(familyName));
            }

            return await query
                .OrderBy(p => p.FamilyNameArabic)
                .ThenBy(p => p.FirstNameArabic)
                .ToListAsync(cancellationToken);
        }

        public async Task<Person> AddAsync(Person person, CancellationToken cancellationToken = default)
        {
            await _context.Persons.AddAsync(person, cancellationToken);
            return person;
        }

        public async Task UpdateAsync(Person person, CancellationToken cancellationToken = default)
        {
            _context.Persons.Update(person);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Persons
                .AnyAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
        }

        public async Task<bool> NationalIdExistsAsync(string nationalId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                return false;

            return await _context.Persons
                .AnyAsync(p => p.NationalId == nationalId && !p.IsDeleted, cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
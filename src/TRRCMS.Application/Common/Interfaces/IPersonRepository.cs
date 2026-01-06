using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces
{
    /// <summary>
    /// Repository interface for Person entity
    /// </summary>
    public interface IPersonRepository
    {
        /// <summary>
        /// Get person by ID
        /// </summary>
        Task<Person?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all persons
        /// </summary>
        Task<List<Person>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get person by National ID
        /// </summary>
        Task<Person?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Search persons by name (Arabic)
        /// </summary>
        Task<List<Person>> SearchByNameAsync(string? firstName, string? fatherName, string? familyName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add new person
        /// </summary>
        Task<Person> AddAsync(Person person, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update existing person
        /// </summary>
        Task UpdateAsync(Person person, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if person exists by ID
        /// </summary>
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if National ID already exists
        /// </summary>
        Task<bool> NationalIdExistsAsync(string nationalId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Save changes to database
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Vocabularies.Commands.ActivateVocabulary;
using TRRCMS.Application.Vocabularies.Commands.CreateVocabulary;
using TRRCMS.Application.Vocabularies.Commands.CreateVocabularyVersion;
using TRRCMS.Application.Vocabularies.Commands.DeactivateVocabulary;
using TRRCMS.Application.Vocabularies.Commands.ImportVocabularies;
using TRRCMS.Application.Vocabularies.Commands.UpdateVocabularyMetadata;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Queries.ExportVocabularies;
using TRRCMS.Application.Vocabularies.Queries.GetAllVocabularies;
using TRRCMS.Application.Vocabularies.Queries.GetVocabularyById;
using TRRCMS.Application.Vocabularies.Queries.GetVocabularyVersionHistory;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Vocabulary Management API — UC-010 (ادارة المفردات)
/// </summary>
/// <remarks>
/// Manages controlled vocabularies (dropdown lists) used across the entire TRRCMS system.
/// Vocabularies are seeded from C# enums on first install, then managed by admins via this API.
/// The vocabulary table is the **single source of truth** for all validation after initial seeding.
///
/// **Architecture:**
/// - All dropdown values (BuildingType, ClaimSource, EvidenceType, etc.) are stored as vocabulary entries
/// - Each vocabulary has semantic versioning (MAJOR.MINOR.PATCH) and full version history
/// - Validation cache is in-memory (singleton) and auto-invalidated on any mutation
/// - Mobile tablets cache vocabularies locally; version compatibility is checked during import
///
/// **Categories:**
/// | Category      | Vocabularies                                                                    |
/// |---------------|---------------------------------------------------------------------------------|
/// | Demographics  | gender, nationality, age_category, relationship_to_head                         |
/// | Property      | building_type, building_status, damage_level, occupancy_type, occupancy_nature,  |
/// |               | tenure_contract_type, property_unit_type, property_unit_status                   |
/// | Relations     | relation_type                                                                   |
/// | Legal         | evidence_type, document_type, verification_status                               |
/// | Claims        | claim_status, claim_source, case_priority, lifecycle_stage, certificate_status   |
/// | Survey        | survey_type, survey_status, survey_source                                       |
/// | Operations    | transfer_status, referral_role                                                  |
/// | System        | user_role, import_status, permission, audit_action_type                          |
///
/// **Endpoints:**
/// | Method | Path                                  | Auth                     | Description                         |
/// |--------|---------------------------------------|--------------------------|-------------------------------------|
/// | GET    | /api/v1/vocabularies                  | Public                   | List all current vocabularies       |
/// | GET    | /api/v1/vocabularies/{id}             | Public                   | Get vocabulary by ID                |
/// | GET    | /api/v1/vocabularies/{name}/versions  | Public                   | Get vocabulary version history      |
/// | POST   | /api/v1/vocabularies                  | CanManageVocabularies     | Create new vocabulary               |
/// | PUT    | /api/v1/vocabularies/{id}             | CanManageVocabularies     | Update vocabulary metadata          |
/// | POST   | /api/v1/vocabularies/{id}/versions    | CanManageVocabularies     | Create new vocabulary version       |
/// | PATCH  | /api/v1/vocabularies/{id}/activate    | CanManageVocabularies     | Activate vocabulary                 |
/// | PATCH  | /api/v1/vocabularies/{id}/deactivate  | CanManageVocabularies     | Deactivate vocabulary               |
/// | GET    | /api/v1/vocabularies/export           | CanManageVocabularies     | Export vocabulary snapshot           |
/// | POST   | /api/v1/vocabularies/import           | CanManageVocabularies     | Import vocabularies from snapshot    |
///
/// **Disaster Recovery Flow:**
/// 1. Admin exports vocabularies from working system → GET /export → saves JSON file
/// 2. System crashes and is reinstalled fresh → enums re-seed default vocabularies
/// 3. Admin imports saved snapshot → POST /import → all admin-managed values restored
/// 4. Mobile tablets sync normally — vocabulary versions match again
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class VocabulariesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<VocabulariesController> _logger;

    public VocabulariesController(IMediator mediator, ILogger<VocabulariesController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ==================== READ ENDPOINTS (Public) ====================

    /// <summary>
    /// List all current vocabularies (عرض جميع المفردات الحالية)
    /// </summary>
    /// <remarks>
    /// Returns all current-version, active vocabularies with their bilingual values.
    /// This is the primary endpoint for mobile/web clients to populate dropdown lists.
    ///
    /// **Public endpoint** — no authentication required. Clients should call this on startup
    /// and cache the results locally, refreshing periodically or when vocabulary versions change.
    ///
    /// **Filtering by category:**
    /// - `?category=Demographics` — gender, nationality, age_category, relationship_to_head
    /// - `?category=Property` — building_type, building_status, damage_level, occupancy_type, etc.
    /// - `?category=Legal` — evidence_type, document_type, verification_status
    /// - `?category=Claims` — claim_status, claim_source, case_priority, lifecycle_stage, etc.
    /// - `?category=Survey` — survey_type, survey_status, survey_source
    /// - No category parameter → returns all vocabularies
    ///
    /// **Example response (abbreviated):**
    /// ```json
    /// [
    ///   {
    ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "vocabularyName": "building_type",
    ///     "displayNameArabic": "نوع البناء",
    ///     "displayNameEnglish": "Building Type",
    ///     "version": "1.0.0",
    ///     "category": "Property",
    ///     "isActive": true,
    ///     "valueCount": 5,
    ///     "values": [
    ///       { "code": 1, "labelArabic": "سكني", "labelEnglish": "Residential", "displayOrder": 0 },
    ///       { "code": 2, "labelArabic": "تجاري", "labelEnglish": "Commercial", "displayOrder": 1 }
    ///     ]
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="category">Optional category filter (e.g., "Demographics", "Property", "Legal", "Claims", "Survey")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all current vocabularies with bilingual labels and integer codes</returns>
    /// <response code="200">Returns the list of vocabularies (may be empty if category filter has no matches)</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<VocabularyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VocabularyDto>>> GetVocabularies(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllVocabulariesQuery { Category = category };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a single vocabulary by ID (عرض مفردة بالمعرف)
    /// </summary>
    /// <remarks>
    /// Returns a specific vocabulary with all its values. Use this to fetch
    /// a vocabulary after creating or updating it (the 201 Created response includes the ID).
    ///
    /// **Public endpoint** — no authentication required.
    /// </remarks>
    /// <param name="id">Vocabulary ID (GUID)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The vocabulary with its bilingual values</returns>
    /// <response code="200">Returns the requested vocabulary</response>
    /// <response code="404">Vocabulary with the given ID was not found</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VocabularyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VocabularyDto>> GetVocabularyById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetVocabularyByIdQuery { Id = id }, cancellationToken);

        if (result is null)
            return NotFound(new { error = $"Vocabulary with ID '{id}' not found." });

        return Ok(result);
    }

    /// <summary>
    /// Get version history for a vocabulary (عرض سجل إصدارات المفردة)
    /// </summary>
    /// <remarks>
    /// Returns all versions of a vocabulary ordered by version descending (newest first).
    /// Useful for auditing changes, comparing versions, or rolling back.
    ///
    /// **Public endpoint** — no authentication required.
    ///
    /// **Common vocabulary names:**
    /// `gender`, `nationality`, `age_category`, `relationship_to_head`,
    /// `building_type`, `building_status`, `damage_level`, `occupancy_type`,
    /// `occupancy_nature`, `tenure_contract_type`, `relation_type`,
    /// `evidence_type`, `document_type`, `claim_source`, `case_priority`,
    /// `survey_type`, `survey_status`, `property_unit_type`, `property_unit_status`
    /// </remarks>
    /// <param name="name">Vocabulary name (e.g., "building_type", "evidence_type", "claim_source")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all versions for the specified vocabulary (newest first)</returns>
    /// <response code="200">Returns the version history (may be empty if vocabulary name doesn't exist)</response>
    [HttpGet("{name}/versions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<VocabularyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VocabularyDto>>> GetVocabularyVersionHistory(
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetVocabularyVersionHistoryQuery { VocabularyName = name },
            cancellationToken);
        return Ok(result);
    }

    // ==================== WRITE ENDPOINTS (Authorized) ====================

    /// <summary>
    /// Create a new vocabulary (إنشاء مفردة جديدة) — UC-010 S03
    /// </summary>
    /// <remarks>
    /// Creates a brand-new vocabulary with version "1.0.0". The vocabulary name must be
    /// unique and follow snake_case convention (letters, digits, underscores, starts with a letter).
    ///
    /// **Required permission:** `CanManageVocabularies`
    ///
    /// **Validation rules:**
    /// - `vocabularyName`: Required, unique, pattern `^[A-Za-z][A-Za-z0-9_]*$`
    /// - `displayNameArabic`: Required
    /// - `values`: At least one value required, codes must be unique integers
    /// - Each value must have `labelArabic`
    ///
    /// **Example request:**
    /// ```json
    /// {
    ///   "vocabularyName": "wall_material",
    ///   "displayNameArabic": "مادة الجدران",
    ///   "displayNameEnglish": "Wall Material",
    ///   "description": "Construction material used for building walls",
    ///   "category": "Property",
    ///   "isSystemVocabulary": false,
    ///   "allowCustomValues": true,
    ///   "values": [
    ///     { "code": 1, "labelArabic": "خرسانة مسلحة", "labelEnglish": "Reinforced Concrete", "displayOrder": 0 },
    ///     { "code": 2, "labelArabic": "طوب", "labelEnglish": "Brick", "displayOrder": 1 },
    ///     { "code": 3, "labelArabic": "حجر", "labelEnglish": "Stone", "displayOrder": 2 },
    ///     { "code": 4, "labelArabic": "طين", "labelEnglish": "Mud/Adobe", "displayOrder": 3 },
    ///     { "code": 99, "labelArabic": "أخرى", "labelEnglish": "Other", "displayOrder": 99 }
    ///   ]
    /// }
    /// ```
    ///
    /// **Example response (201 Created):**
    /// ```json
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "vocabularyName": "wall_material",
    ///   "displayNameArabic": "مادة الجدران",
    ///   "displayNameEnglish": "Wall Material",
    ///   "version": "1.0.0",
    ///   "category": "Property",
    ///   "isActive": true,
    ///   "valueCount": 5,
    ///   "values": [ ... ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Vocabulary creation data with name, labels, and values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created vocabulary with assigned ID and version 1.0.0</returns>
    /// <response code="201">Vocabulary created successfully — Location header points to GET by ID</response>
    /// <response code="400">Validation failed — duplicate name, invalid pattern, missing required fields, or duplicate codes in values</response>
    /// <response code="401">Not authenticated — JWT token missing or expired</response>
    /// <response code="403">Not authorized — requires CanManageVocabularies permission</response>
    [HttpPost]
    [Authorize(Policy = "CanManageVocabularies")]
    [ProducesResponseType(typeof(VocabularyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VocabularyDto>> CreateVocabulary(
        [FromBody] CreateVocabularyCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating vocabulary '{VocabularyName}'", command.VocabularyName);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVocabularyById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update vocabulary metadata (تحديث بيانات المفردة الوصفية) — UC-010 S05
    /// </summary>
    /// <remarks>
    /// Updates **only metadata** (display names, description, category) without changing
    /// the vocabulary's values or version number. Use this to fix typos in labels or
    /// recategorize a vocabulary.
    ///
    /// To change values (add/remove/modify codes), use the **Create New Version** endpoint instead.
    ///
    /// **Required permission:** `CanManageVocabularies`
    ///
    /// **Example request:**
    /// ```json
    /// {
    ///   "displayNameArabic": "نوع البناء المحدث",
    ///   "displayNameEnglish": "Building Type (Updated)",
    ///   "description": "Classification of building structure types in Aleppo",
    ///   "category": "Property"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Vocabulary ID (GUID) — must be the current version</param>
    /// <param name="request">Updated metadata fields</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated vocabulary with unchanged values and version</returns>
    /// <response code="200">Metadata updated successfully</response>
    /// <response code="400">Validation failed — missing required fields</response>
    /// <response code="401">Not authenticated — JWT token missing or expired</response>
    /// <response code="403">Not authorized — requires CanManageVocabularies permission</response>
    /// <response code="404">Vocabulary with the given ID was not found</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManageVocabularies")]
    [ProducesResponseType(typeof(VocabularyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VocabularyDto>> UpdateVocabularyMetadata(
        Guid id,
        [FromBody] UpdateVocabularyMetadataRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating metadata for vocabulary {VocabularyId}", id);

        var command = new UpdateVocabularyMetadataCommand
        {
            Id = id,
            DisplayNameArabic = request.DisplayNameArabic,
            DisplayNameEnglish = request.DisplayNameEnglish,
            Description = request.Description,
            Category = request.Category
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new version of a vocabulary (إنشاء إصدار جديد للمفردة) — UC-010 S06
    /// </summary>
    /// <remarks>
    /// Creates a new version of an existing vocabulary with updated values.
    /// The old version is marked as non-current but preserved in version history.
    /// The validation cache is automatically invalidated after this operation.
    ///
    /// **Required permission:** `CanManageVocabularies`
    ///
    /// **Version types (Semantic Versioning):**
    /// | Type    | When to use                                           | Example         |
    /// |---------|-------------------------------------------------------|-----------------|
    /// | `minor` | Add new values or reorder existing values              | 1.0.0 → 1.1.0  |
    /// | `major` | Remove values or make breaking changes                | 1.1.0 → 2.0.0  |
    /// | `patch` | Fix typos in labels without changing codes             | 1.1.0 → 1.1.1  |
    ///
    /// **Important:** The `id` parameter must be the ID of the **current version** of the vocabulary.
    /// You cannot create a new version from an old (non-current) version.
    ///
    /// **Example request — add a new building type value:**
    /// ```json
    /// {
    ///   "versionType": "minor",
    ///   "changeLog": "Added 'Mixed Use' building type for Aleppo reconstruction",
    ///   "values": [
    ///     { "code": 1, "labelArabic": "سكني", "labelEnglish": "Residential", "displayOrder": 0 },
    ///     { "code": 2, "labelArabic": "تجاري", "labelEnglish": "Commercial", "displayOrder": 1 },
    ///     { "code": 3, "labelArabic": "صناعي", "labelEnglish": "Industrial", "displayOrder": 2 },
    ///     { "code": 4, "labelArabic": "حكومي", "labelEnglish": "Government", "displayOrder": 3 },
    ///     { "code": 5, "labelArabic": "متعدد الاستخدامات", "labelEnglish": "Mixed Use", "displayOrder": 4 }
    ///   ]
    /// }
    /// ```
    ///
    /// **Example request — fix a typo (patch):**
    /// ```json
    /// {
    ///   "versionType": "patch",
    ///   "changeLog": "Fixed Arabic label typo for Industrial",
    ///   "values": [
    ///     { "code": 1, "labelArabic": "سكني", "labelEnglish": "Residential", "displayOrder": 0 },
    ///     { "code": 2, "labelArabic": "تجاري", "labelEnglish": "Commercial", "displayOrder": 1 },
    ///     { "code": 3, "labelArabic": "صناعي", "labelEnglish": "Industrial", "displayOrder": 2 }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">ID of the **current** vocabulary version (GUID)</param>
    /// <param name="request">Version type, new values, and changelog description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The newly created vocabulary version with incremented version number</returns>
    /// <response code="201">New version created — Location header points to GET by ID</response>
    /// <response code="400">Validation failed — invalid version type, not the current version, duplicate codes, or missing required fields</response>
    /// <response code="401">Not authenticated — JWT token missing or expired</response>
    /// <response code="403">Not authorized — requires CanManageVocabularies permission</response>
    /// <response code="404">Vocabulary with the given ID was not found</response>
    [HttpPost("{id:guid}/versions")]
    [Authorize(Policy = "CanManageVocabularies")]
    [ProducesResponseType(typeof(VocabularyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VocabularyDto>> CreateVocabularyVersion(
        Guid id,
        [FromBody] CreateVocabularyVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating {VersionType} version for vocabulary {VocabularyId}",
            request.VersionType, id);

        var command = new CreateVocabularyVersionCommand
        {
            VocabularyId = id,
            VersionType = request.VersionType,
            Values = request.Values,
            ChangeLog = request.ChangeLog
        };

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetVocabularyById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Activate a vocabulary (تفعيل المفردة) — UC-010 S08
    /// </summary>
    /// <remarks>
    /// Re-activates a previously deactivated vocabulary. Active vocabularies
    /// are included in the validation cache and returned by the public GET endpoint.
    ///
    /// **Required permission:** `CanManageVocabularies`
    ///
    /// **No request body needed** — just pass the vocabulary ID in the URL.
    ///
    /// **Validation:**
    /// - Returns 400 if the vocabulary is already active
    /// </remarks>
    /// <param name="id">Vocabulary ID (GUID)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The activated vocabulary with IsActive = true</returns>
    /// <response code="200">Vocabulary activated successfully</response>
    /// <response code="400">Vocabulary is already active</response>
    /// <response code="401">Not authenticated — JWT token missing or expired</response>
    /// <response code="403">Not authorized — requires CanManageVocabularies permission</response>
    /// <response code="404">Vocabulary with the given ID was not found</response>
    [HttpPatch("{id:guid}/activate")]
    [Authorize(Policy = "CanManageVocabularies")]
    [ProducesResponseType(typeof(VocabularyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VocabularyDto>> ActivateVocabulary(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Activating vocabulary {VocabularyId}", id);
        var result = await _mediator.Send(new ActivateVocabularyCommand { Id = id }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deactivate a vocabulary (تعطيل المفردة) — UC-010 S09
    /// </summary>
    /// <remarks>
    /// Deactivates a vocabulary so it is no longer returned by the public GET endpoint
    /// and no longer included in the validation cache. Deactivated vocabularies are NOT deleted —
    /// they remain in the database and can be re-activated.
    ///
    /// **Required permission:** `CanManageVocabularies`
    ///
    /// **No request body needed** — just pass the vocabulary ID in the URL.
    ///
    /// **Restrictions:**
    /// - **System vocabularies cannot be deactivated** (returns 400). System vocabularies are
    ///   core enums like gender, building_type, claim_source that are required by the domain model.
    /// - Returns 400 if the vocabulary is already inactive
    /// </remarks>
    /// <param name="id">Vocabulary ID (GUID)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deactivated vocabulary with IsActive = false</returns>
    /// <response code="200">Vocabulary deactivated successfully</response>
    /// <response code="400">Cannot deactivate — system vocabulary or already inactive</response>
    /// <response code="401">Not authenticated — JWT token missing or expired</response>
    /// <response code="403">Not authorized — requires CanManageVocabularies permission</response>
    /// <response code="404">Vocabulary with the given ID was not found</response>
    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Policy = "CanManageVocabularies")]
    [ProducesResponseType(typeof(VocabularyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VocabularyDto>> DeactivateVocabulary(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating vocabulary {VocabularyId}", id);
        var result = await _mediator.Send(new DeactivateVocabularyCommand { Id = id }, cancellationToken);
        return Ok(result);
    }

    // ==================== EXPORT/IMPORT ENDPOINTS (Disaster Recovery) ====================

    /// <summary>
    /// Export all current vocabularies as a JSON snapshot (تصدير جميع المفردات) — UC-010 S10
    /// </summary>
    /// <remarks>
    /// Exports **all current-version vocabularies** as a complete JSON snapshot for backup
    /// and disaster recovery. The export includes system flags (`isSystemVocabulary`, `allowCustomValues`)
    /// that are needed for faithful reimport but are not exposed in the regular public API.
    ///
    /// **Required permission:** `CanManageVocabularies`
    ///
    /// **Disaster Recovery Workflow:**
    /// 1. **Before disaster:** Admin periodically calls `GET /export` and saves the JSON file securely
    /// 2. **After reinstall:** System starts with default enum-seeded vocabularies
    /// 3. **Restore:** Admin calls `POST /import` with the saved snapshot → all admin-managed values restored
    ///
    /// **What is exported:**
    /// - All vocabulary metadata (name, bilingual labels, category, version)
    /// - All vocabulary values with codes, bilingual labels, descriptions, and display order
    /// - System flags: `isSystemVocabulary` (protected from deactivation) and `allowCustomValues`
    /// - Active/inactive status
    ///
    /// **What is NOT exported:**
    /// - Vocabulary IDs (new IDs are assigned on import)
    /// - Version history (only current version is exported)
    /// - Audit trail (createdBy, createdAt, etc.)
    ///
    /// **Example response (abbreviated):**
    /// ```json
    /// [
    ///   {
    ///     "vocabularyName": "building_type",
    ///     "displayNameArabic": "نوع البناء",
    ///     "displayNameEnglish": "Building Type",
    ///     "description": "Classification of building structure types",
    ///     "version": "1.2.0",
    ///     "category": "Property",
    ///     "isActive": true,
    ///     "isSystemVocabulary": true,
    ///     "allowCustomValues": false,
    ///     "values": [
    ///       { "code": 1, "labelArabic": "سكني", "labelEnglish": "Residential", "displayOrder": 0 },
    ///       { "code": 2, "labelArabic": "تجاري", "labelEnglish": "Commercial", "displayOrder": 1 },
    ///       { "code": 3, "labelArabic": "صناعي", "labelEnglish": "Industrial", "displayOrder": 2 },
    ///       { "code": 4, "labelArabic": "حكومي", "labelEnglish": "Government", "displayOrder": 3 },
    ///       { "code": 5, "labelArabic": "متعدد الاستخدامات", "labelEnglish": "Mixed Use", "displayOrder": 4 }
    ///     ]
    ///   },
    ///   {
    ///     "vocabularyName": "gender",
    ///     "displayNameArabic": "الجنس",
    ///     "displayNameEnglish": "Gender",
    ///     "version": "1.0.0",
    ///     "category": "Demographics",
    ///     "isActive": true,
    ///     "isSystemVocabulary": true,
    ///     "allowCustomValues": false,
    ///     "values": [
    ///       { "code": 1, "labelArabic": "ذكر", "labelEnglish": "Male", "displayOrder": 0 },
    ///       { "code": 2, "labelArabic": "أنثى", "labelEnglish": "Female", "displayOrder": 1 }
    ///     ]
    ///   }
    /// ]
    /// ```
    ///
    /// **Tip:** Save the exported JSON to a secure location (cloud storage, encrypted drive).
    /// The file can be reimported on any TRRCMS instance running the same or newer version.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete JSON snapshot of all current vocabularies with system flags and values</returns>
    /// <response code="200">Vocabulary snapshot exported successfully — array of vocabulary export DTOs</response>
    /// <response code="401">Not authenticated — JWT token missing or expired</response>
    /// <response code="403">Not authorized — requires CanManageVocabularies permission</response>
    [HttpGet("export")]
    [Authorize(Policy = "CanManageVocabularies")]
    [ProducesResponseType(typeof(List<VocabularyExportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<VocabularyExportDto>>> ExportVocabularies(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting all vocabularies");
        var result = await _mediator.Send(new ExportVocabulariesQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Import vocabularies from a JSON snapshot (استيراد المفردات من نسخة احتياطية) — UC-010 S11
    /// </summary>
    /// <remarks>
    /// Restores vocabularies from a previously exported JSON snapshot. This is the counterpart
    /// to the `GET /export` endpoint and is primarily used for **disaster recovery**.
    ///
    /// **Required permission:** `CanManageVocabularies`
    ///
    /// **Import behavior for each vocabulary:**
    /// | Scenario                        | Action                                                    |
    /// |---------------------------------|-----------------------------------------------------------|
    /// | Vocabulary doesn't exist by name | Creates a new vocabulary (version 1.0.0)                 |
    /// | Vocabulary already exists        | Creates a new minor version with imported values          |
    ///
    /// **Validation rules:**
    /// - At least one vocabulary must be provided
    /// - Each vocabulary must have a valid `vocabularyName` (pattern: `^[A-Za-z][A-Za-z0-9_]*$`)
    /// - Each vocabulary must have `displayNameArabic`
    /// - Each vocabulary must have at least one value
    /// - Value codes must be unique within each vocabulary
    ///
    /// **Example request — import two vocabularies:**
    /// ```json
    /// {
    ///   "vocabularies": [
    ///     {
    ///       "vocabularyName": "building_type",
    ///       "displayNameArabic": "نوع البناء",
    ///       "displayNameEnglish": "Building Type",
    ///       "description": "Classification of building structure types",
    ///       "version": "1.2.0",
    ///       "category": "Property",
    ///       "isActive": true,
    ///       "isSystemVocabulary": true,
    ///       "allowCustomValues": false,
    ///       "values": [
    ///         { "code": 1, "labelArabic": "سكني", "labelEnglish": "Residential", "displayOrder": 0 },
    ///         { "code": 2, "labelArabic": "تجاري", "labelEnglish": "Commercial", "displayOrder": 1 },
    ///         { "code": 3, "labelArabic": "صناعي", "labelEnglish": "Industrial", "displayOrder": 2 },
    ///         { "code": 5, "labelArabic": "متعدد الاستخدامات", "labelEnglish": "Mixed Use", "displayOrder": 4 }
    ///       ]
    ///     },
    ///     {
    ///       "vocabularyName": "wall_material",
    ///       "displayNameArabic": "مادة الجدران",
    ///       "displayNameEnglish": "Wall Material",
    ///       "description": "Construction material used for building walls",
    ///       "version": "1.0.0",
    ///       "category": "Property",
    ///       "isActive": true,
    ///       "isSystemVocabulary": false,
    ///       "allowCustomValues": true,
    ///       "values": [
    ///         { "code": 1, "labelArabic": "خرسانة مسلحة", "labelEnglish": "Reinforced Concrete", "displayOrder": 0 },
    ///         { "code": 2, "labelArabic": "طوب", "labelEnglish": "Brick", "displayOrder": 1 }
    ///       ]
    ///     }
    ///   ]
    /// }
    /// ```
    ///
    /// **Example response:**
    /// ```json
    /// {
    ///   "created": 1,
    ///   "updated": 1,
    ///   "skipped": 0,
    ///   "messages": [
    ///     "Updated vocabulary 'building_type' from v1.0.0 to v1.1.0",
    ///     "Created vocabulary 'wall_material'"
    ///   ]
    /// }
    /// ```
    ///
    /// **Workflow — Disaster Recovery:**
    /// 1. On a working system, export: `GET /api/v1/vocabularies/export` → save JSON file
    /// 2. After fresh install, system starts with default enum-seeded vocabularies
    /// 3. Import the saved file: `POST /api/v1/vocabularies/import` with the JSON body
    /// 4. Result shows how many vocabularies were created vs. updated
    /// 5. Validation cache is automatically invalidated — new values take effect immediately
    ///
    /// **Note:** The import is atomic — if any vocabulary fails validation, none are imported.
    /// The `version` field in the import payload is informational only; the system assigns
    /// version numbers automatically based on the current state.
    /// </remarks>
    /// <param name="command">Vocabulary snapshot data — array of vocabularies with their values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result with counts of created, updated, and skipped vocabularies plus detailed messages</returns>
    /// <response code="200">Import completed successfully — check the result for details on each vocabulary</response>
    /// <response code="400">Validation failed — invalid vocabulary name, missing required fields, duplicate codes, or empty vocabulary list</response>
    /// <response code="401">Not authenticated — JWT token missing or expired</response>
    /// <response code="403">Not authorized — requires CanManageVocabularies permission</response>
    [HttpPost("import")]
    [Authorize(Policy = "CanManageVocabularies")]
    [ProducesResponseType(typeof(ImportVocabulariesResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ImportVocabulariesResult>> ImportVocabularies(
        [FromBody] ImportVocabulariesCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Importing {Count} vocabularies", command.Vocabularies.Count);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}

// ==================== Request Models ====================

/// <summary>
/// Request model for updating vocabulary metadata (labels, description, category).
/// Does NOT change vocabulary values or version — use CreateVocabularyVersion for that.
/// </summary>
public class UpdateVocabularyMetadataRequest
{
    /// <summary>Arabic display name (required) — e.g., "نوع البناء"</summary>
    /// <example>نوع البناء</example>
    public string DisplayNameArabic { get; set; } = string.Empty;

    /// <summary>English display name (optional) — e.g., "Building Type"</summary>
    /// <example>Building Type</example>
    public string? DisplayNameEnglish { get; set; }

    /// <summary>Description of the vocabulary's purpose and usage context</summary>
    /// <example>Classification of building structure types in Aleppo</example>
    public string? Description { get; set; }

    /// <summary>Category for grouping — Demographics, Property, Legal, Claims, Survey, Operations, System</summary>
    /// <example>Property</example>
    public string? Category { get; set; }
}

/// <summary>
/// Request model for creating a new vocabulary version with updated values.
/// The old version is preserved in history. Validation cache is auto-invalidated.
/// </summary>
public class CreateVocabularyVersionRequest
{
    /// <summary>
    /// Version bump type: "minor" (add values), "major" (breaking changes), or "patch" (label fixes)
    /// </summary>
    /// <example>minor</example>
    public string VersionType { get; set; } = string.Empty;

    /// <summary>
    /// Complete list of vocabulary values for the new version.
    /// Must include ALL values (not just changes) — this replaces the entire value set.
    /// Each value needs: code (unique integer), labelArabic (required), labelEnglish (optional), displayOrder.
    /// </summary>
    public List<VocabularyValueDto> Values { get; set; } = new();

    /// <summary>
    /// Human-readable description of what changed in this version.
    /// Displayed in version history for audit purposes.
    /// </summary>
    /// <example>Added 'Mixed Use' building type for Aleppo reconstruction phase 2</example>
    public string ChangeLog { get; set; } = string.Empty;
}

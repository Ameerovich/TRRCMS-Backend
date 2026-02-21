using System.Text.Json;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Sync.DTOs;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Sync.Queries.GetSyncAssignments;

/// <summary>
/// Handles <see cref="GetSyncAssignmentsQuery"/> — Sync Protocol Step 3.
///
/// Execution flow:
/// <list type="number">
///   <item>Resolve and validate the active <c>SyncSession</c>.</item>
///   <item>Determine the field collector identity from the authenticated user.</item>
///   <item>Load all <c>Pending/Failed</c> assignments with eager-loaded
///         <c>Building</c> and <c>PropertyUnits</c>.</item>
///   <item>Load all active <c>Vocabulary</c> records for the offline code lists.</item>
///   <item>Build the <see cref="SyncAssignmentPayloadDto"/> and record the
///         download event on the session.</item>
/// </list>
///
/// Security: the field collector only ever receives their own assignments.
/// The session's <c>FieldCollectorId</c> is used as the scope; the current
/// user's identity is verified to match that collector.
/// </summary>
public sealed class GetSyncAssignmentsQueryHandler
    : IRequestHandler<GetSyncAssignmentsQuery, SyncAssignmentPayloadDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IBuildingAssignmentRepository _assignmentRepo;
    private readonly IVocabularyRepository _vocabularyRepo;
    private readonly ICurrentUserService _currentUser;

    public GetSyncAssignmentsQueryHandler(
        IUnitOfWork uow,
        IBuildingAssignmentRepository assignmentRepo,
        IVocabularyRepository vocabularyRepo,
        ICurrentUserService currentUser)
    {
        _uow = uow;
        _assignmentRepo = assignmentRepo;
        _vocabularyRepo = vocabularyRepo;
        _currentUser = currentUser;
    }

    public async Task<SyncAssignmentPayloadDto> Handle(
        GetSyncAssignmentsQuery request,
        CancellationToken ct)
    {
        // ── 1. Resolve authenticated user ──────────────────────────────────────────
        var currentUserId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        // ── 2. Resolve and validate the sync session ───────────────────────────────
        var session = await _uow.SyncSessions.GetByIdAsync(request.SyncSessionId, ct)
            ?? throw new InvalidOperationException(
                $"Sync session '{request.SyncSessionId}' not found.");

        // Security guard: the requesting user must be the field collector that owns
        // this session.  Prevents one collector from downloading another's work.
        if (session.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException(
                "The current user is not the field collector for this sync session.");

        // Guard: only InProgress sessions may serve downloads.
        if (session.SessionStatus != SyncSessionStatus.InProgress)
            throw new InvalidOperationException(
                $"Sync session '{request.SyncSessionId}' is not active (status: {session.SessionStatus}).");

        // ── 3. Load pending / failed assignments with full building details ─────────
        var assignments = await _assignmentRepo.GetPendingOrFailedByFieldCollectorAsync(
            fieldCollectorId: session.FieldCollectorId,
            modifiedSinceUtc: request.ModifiedSinceUtc,
            cancellationToken: ct);

        // ── 4. Load all active vocabularies (offline code lists) ───────────────────
        var vocabularies = await _vocabularyRepo.GetAllCurrentAsync(ct);

        // ── 5. Build vocabulary version map for the session audit record ───────────
        // Format: { "ownership_type": "2.1.0", "document_type": "1.0.3", ... }
        var versionMap = vocabularies.ToDictionary(
            v => v.VocabularyName,
            v => v.Version);

        var vocabVersionsJson = JsonSerializer.Serialize(versionMap);

        // ── 6. Map assignments to DTOs ─────────────────────────────────────────────
        var assignmentDtos = assignments
            .Select(a => MapToSyncBuildingDto(a))
            .ToList();

        // ── 7. Map vocabularies to DTOs ────────────────────────────────────────────
        var vocabularyDtos = vocabularies
            .Select(v => new SyncVocabularyDto
            {
                VocabularyName    = v.VocabularyName,
                DisplayNameArabic = v.DisplayNameArabic,
                DisplayNameEnglish = v.DisplayNameEnglish,
                Version           = v.Version,
                ValuesJson        = v.ValuesJson
            })
            .ToList();

        // ── 8. Record the download event on the session ────────────────────────────
        session.RecordDownloadResult(
            assignmentsCount: assignments.Count,
            vocabVersionsSentJson: vocabVersionsJson);

        await _uow.SyncSessions.UpdateAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        // ── 9. Assemble and return the payload ─────────────────────────────────────
        return new SyncAssignmentPayloadDto
        {
            SyncSessionId           = session.Id,
            FieldCollectorId        = session.FieldCollectorId,
            GeneratedAtUtc          = DateTime.UtcNow,
            Assignments             = assignmentDtos,
            Vocabularies            = vocabularyDtos,
            VocabularyVersionsSentJson = vocabVersionsJson
        };
    }

    // ── Private mapping helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Maps a <see cref="BuildingAssignment"/> (with eagerly loaded
    /// <c>Building</c> and <c>PropertyUnits</c>) to a <see cref="SyncBuildingDto"/>.
    /// </summary>
    private static SyncBuildingDto MapToSyncBuildingDto(BuildingAssignment assignment)
    {
        var building = assignment.Building;

        // Compute the human-readable building code display (GG-DD-SS-CCC-NNN-BBBBB)
        // from the 17-digit stored code.
        var codeDisplay = FormatBuildingCodeDisplay(building.BuildingId);

        // For revisit assignments only include the units listed in UnitsForRevisit;
        // for full-building assignments include all property units.
        var propertyUnits = ResolvePropertyUnits(assignment, building);

        return new SyncBuildingDto
        {
            // Assignment metadata
            AssignmentId         = assignment.Id,
            AssignedDate         = assignment.AssignedDate,
            TargetCompletionDate = assignment.TargetCompletionDate,
            Priority             = assignment.Priority,
            AssignmentNotes      = assignment.AssignmentNotes,
            IsRevisit            = assignment.IsRevisit,
            UnitsForRevisit      = assignment.UnitsForRevisit,

            // Building identification
            BuildingCode         = building.BuildingId,
            BuildingCodeDisplay  = codeDisplay,

            // Administrative codes
            GovernorateCode      = building.GovernorateCode,
            DistrictCode         = building.DistrictCode,
            SubDistrictCode      = building.SubDistrictCode,
            CommunityCode        = building.CommunityCode,
            NeighborhoodCode     = building.NeighborhoodCode,
            BuildingNumber       = building.BuildingNumber,

            // Location names (Arabic)
            GovernorateName      = building.GovernorateName,
            DistrictName         = building.DistrictName,
            SubDistrictName      = building.SubDistrictName,
            CommunityName        = building.CommunityName,
            NeighborhoodName     = building.NeighborhoodName,

            // Building attributes
            Address              = building.Address,
            Landmark             = building.Landmark,
            LocationDescription  = building.LocationDescription,
            Notes                = building.Notes,

            // Property units
            PropertyUnits = propertyUnits
        };
    }

    /// <summary>
    /// Resolves which property units to include in the payload:
    /// <list type="bullet">
    ///   <item>Full assignment → all units in the building.</item>
    ///   <item>Revisit assignment → only the units whose IDs appear in
    ///         <c>BuildingAssignment.UnitsForRevisit</c> (JSON array of Guid strings).</item>
    /// </list>
    /// </summary>
    private static IReadOnlyList<SyncPropertyUnitDto> ResolvePropertyUnits(
        BuildingAssignment assignment,
        Building building)
    {
        var allUnits = building.PropertyUnits
            .Select(MapToSyncPropertyUnitDto)
            .ToList();

        if (!assignment.IsRevisit || string.IsNullOrWhiteSpace(assignment.UnitsForRevisit))
            return allUnits;

        // Parse the revisit unit ID list and filter accordingly.
        // Malformed JSON falls back to returning all units so the tablet
        // can still display the building.
        try
        {
            var revisitIds = JsonSerializer.Deserialize<List<Guid>>(assignment.UnitsForRevisit);
            if (revisitIds is { Count: > 0 })
            {
                var revisitIdSet = new HashSet<Guid>(revisitIds);
                return allUnits
                    .Where(u => revisitIdSet.Contains(u.Id))
                    .ToList();
            }
        }
        catch (JsonException)
        {
            // Fall through — return all units on parse error.
        }

        return allUnits;
    }

    /// <summary>
    /// Maps a <see cref="PropertyUnit"/> entity to a <see cref="SyncPropertyUnitDto"/>.
    /// </summary>
    private static SyncPropertyUnitDto MapToSyncPropertyUnitDto(PropertyUnit unit) =>
        new()
        {
            Id               = unit.Id,
            UnitIdentifier   = unit.UnitIdentifier,
            FloorNumber      = unit.FloorNumber,
            PositionOnFloor  = unit.PositionOnFloor,
            UnitType         = unit.UnitType,
            Status           = unit.Status,
            AreaSquareMeters = unit.AreaSquareMeters,
            DamageLevel      = unit.DamageLevel
        };

    /// <summary>
    /// Converts the 17-digit compact building code to the display format
    /// <c>GG-DD-SS-CCC-NNN-BBBBB</c>.
    /// Returns the raw code unchanged if it is not exactly 17 characters long.
    /// </summary>
    private static string FormatBuildingCodeDisplay(string buildingCode)
    {
        if (string.IsNullOrWhiteSpace(buildingCode) || buildingCode.Length != 17)
            return buildingCode ?? string.Empty;

        // GGDDSSCCNCNNBBBBB → GG-DD-SS-CCC-NNN-BBBBB
        return $"{buildingCode[..2]}-{buildingCode[2..4]}-{buildingCode[4..6]}" +
               $"-{buildingCode[6..9]}-{buildingCode[9..12]}-{buildingCode[12..]}";
    }
}

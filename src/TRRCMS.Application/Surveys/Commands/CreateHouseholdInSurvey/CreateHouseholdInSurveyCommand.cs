using MediatR;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Surveys.Commands.CreateHouseholdInSurvey;

/// <summary>
/// Command to create a household in the context of a field survey
/// Matches frontend form: تسجيل الأسرة
/// </summary>
public class CreateHouseholdInSurveyCommand : IRequest<HouseholdDto>
{
    /// <summary>
    /// Survey ID this household is being created for (required)
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Property unit ID (if not using survey's linked property unit)
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    /// <summary>
    /// Total household size (عدد الأفراد) - required
    /// </summary>
    public int HouseholdSize { get; set; }

    /// <summary>
    /// Notes/observations (ملاحظات)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Occupancy type (نوع الإشغال) - Enum: 1=OwnerOccupied, 2=TenantOccupied, etc.
    /// </summary>
    public int? OccupancyType { get; set; }

    /// <summary>
    /// Occupancy nature (طبيعة الإشغال) - Enum: 1=LegalFormal, 2=Informal, 3=Customary, etc.
    /// </summary>
    public int? OccupancyNature { get; set; }

    /// <summary>
    /// Number of adult males (عدد البالغين الذكور)
    /// </summary>
    public int MaleCount { get; set; }

    /// <summary>
    /// Number of adult females (عدد البالغين الإناث)
    /// </summary>
    public int FemaleCount { get; set; }

    /// <summary>
    /// Number of male children under 18 (عدد الأطفال الذكور - أقل من 18)
    /// </summary>
    public int MaleChildCount { get; set; }

    /// <summary>
    /// Number of female children under 18 (عدد الأطفال الإناث - أقل من 18)
    /// </summary>
    public int FemaleChildCount { get; set; }

    /// <summary>
    /// Number of male elderly over 65 (عدد كبار السن الذكور - أكثر من 65)
    /// </summary>
    public int MaleElderlyCount { get; set; }

    /// <summary>
    /// Number of female elderly over 65 (عدد كبار السن الإناث - أكثر من 65)
    /// </summary>
    public int FemaleElderlyCount { get; set; }

    /// <summary>
    /// Number of male persons with disabilities (عدد المعاقين الذكور)
    /// </summary>
    public int MaleDisabledCount { get; set; }

    /// <summary>
    /// Number of female persons with disabilities (عدد المعاقين الإناث)
    /// </summary>
    public int FemaleDisabledCount { get; set; }
}

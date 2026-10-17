using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Common.Services;

/// <summary>
/// Keeps the legacy <c>Person.IsContactPerson</c> flag consistent with <c>Survey.ContactPersonId</c>
/// when a survey's contact point is replaced.
/// </summary>
public static class ContactPointFlagSync
{
    /// <summary>
    /// Clears the flag on the survey's previous contact person unless they are still
    /// the contact of another survey. Call before saving, after the survey is re-pointed.
    /// </summary>
    public static async Task ReleasePreviousContactAsync(
        IUnitOfWork unitOfWork,
        Guid? previousContactId,
        Guid newContactId,
        Guid surveyId,
        Guid modifiedByUserId,
        CancellationToken cancellationToken)
    {
        if (!previousContactId.HasValue || previousContactId.Value == newContactId)
            return;

        var stillContact = await unitOfWork.Surveys.GetContactPersonIdsAsync(
            new[] { previousContactId.Value }, excludeSurveyId: surveyId, cancellationToken);
        if (stillContact.Count > 0)
            return;

        var previous = await unitOfWork.Persons.GetByIdAsync(previousContactId.Value, cancellationToken);
        previous?.SetAsContactPerson(false, modifiedByUserId);
    }
}

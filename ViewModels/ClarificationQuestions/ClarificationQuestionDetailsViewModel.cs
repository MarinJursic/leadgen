using Leadgen.Model.Entities;

namespace leadgen.ViewModels.ClarificationQuestions;

public sealed class ClarificationQuestionDetailsViewModel
{
    public required ClarificationQuestion Question { get; init; }

    public BusinessDnaMission? Mission { get; init; }
}

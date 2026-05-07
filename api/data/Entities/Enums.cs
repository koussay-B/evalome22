using System;
using api.data.Entities;

namespace api.data.Entities
{
    public enum QuestionType
    {
        QCU = 0,
        QCM = 1,
        TrueFalse = 2
    }

    public enum ComplexityLevel
    {
        Beginner = 0,
        Intermediate = 1,
        Advanced = 2,
        Expert = 3
    }

    public enum ScoringMode
    {
        SumWeighted = 0,
        Average = 1,
        Percentage = 2
    }

    public enum QuestionnaireStatus
    {
        Draft = 0,
        Published = 1,
        Archived = 2
    }

    public enum CompanyStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public enum CampaignStatus
    {
        Draft = 0,
        Scheduled = 1,
        Active = 2,
        Closed = 3,
        Archived = 4
    }

    public enum AttemptStatus
    {
        InProgress = 0,
        Submitted = 1,
        TimedOut = 2,
        Abandoned = 3
    }

    public enum CampaignCandidateStatus
    {
        Invited = 0,
        InProgress = 1,
        Completed = 2,
        Expired = 3,
        Withdrawn = 4
    }
}

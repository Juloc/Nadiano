namespace Nadiano.Core.Content.Manifests;

/// <summary>One entry of the versioned skill catalogue (docs/CONTENT_MODEL.md §6).</summary>
public sealed class SkillDefinition
{
    public required string Id { get; init; }
    public required CompetencyArea Competency { get; init; }
    public required string IntroducedStage { get; init; }
    public required SkillMeasurability Measurability { get; init; }
    public IReadOnlyList<string> RelatedSkills { get; init; } = [];
}

public sealed class SkillCatalogue
{
    public required int SchemaVersion { get; init; }
    public required IReadOnlyList<SkillDefinition> Skills { get; init; }
}
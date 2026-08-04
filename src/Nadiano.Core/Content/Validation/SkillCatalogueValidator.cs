using Nadiano.Core.Content.Manifests;

namespace Nadiano.Core.Content.Validation;

public static class SkillCatalogueValidator
{
    public static ContentValidationResult Validate(SkillCatalogue catalogue, string filePath)
    {
        var result = new ContentValidationResult();
        var seenIds = new HashSet<string>();
        var allIds = catalogue.Skills.Select(s => s.Id).ToHashSet();

        foreach (var skill in catalogue.Skills)
        {
            if (string.IsNullOrWhiteSpace(skill.Id))
            {
                result.Add(filePath, nameof(skill.Id), "Skill id must not be blank.");
                continue;
            }

            if (!seenIds.Add(skill.Id))
            {
                result.Add(filePath, nameof(skill.Id), $"Duplicate skill id '{skill.Id}'.");
            }

            foreach (var related in skill.RelatedSkills)
            {
                if (!allIds.Contains(related))
                {
                    result.Add(filePath, nameof(skill.RelatedSkills), $"Skill '{skill.Id}' references unknown related skill '{related}'.");
                }
            }
        }

        return result;
    }
}
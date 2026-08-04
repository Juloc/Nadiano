using Nadiano.Core.Content;
using Nadiano.Core.Content.Validation;

var contentRoot = args.Length > 0 ? args[0] : "content";
var fullContentRoot = Path.GetFullPath(contentRoot);

if (!Directory.Exists(fullContentRoot))
{
    Console.Error.WriteLine($"Content root not found: {fullContentRoot}");
    return 1;
}

var repository = new BundledContentRepository(fullContentRoot);
var validator = new ContentValidator(repository);
var result = validator.ValidateAll();

if (result.IsValid)
{
    Console.WriteLine($"Content validation passed. Content root: {fullContentRoot}");
    return 0;
}

Console.WriteLine($"Content validation found {result.Issues.Count} issue(s):");
foreach (var issue in result.Issues)
{
    Console.WriteLine($"  {issue}");
}

return 1;
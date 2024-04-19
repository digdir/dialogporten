using System.Globalization;
using Bogus;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Domain.Dialogporten.Domain.Http;

namespace Digdir.Tool.Dialogporten.GenerateFakeData;

public static class DialogGenerator
{
    private static readonly DateTime RefTime = new(2026, 1, 1);

    public static CreateDialogCommand GenerateFakeDialog(
        int? seed = null,
        Guid? id = null,
        string? serviceResource = null,
        string? party = null,
        int? progress = null,
        string? extendedStatus = null,
        string? externalReference = null,
        DateTimeOffset? dueAt = null,
        DateTimeOffset? expiresAt = null,
        DialogStatus.Values? status = null,
        List<CreateDialogContentDto>? content = null,
        List<CreateDialogSearchTagDto>? searchTags = null,
        List<CreateDialogDialogElementDto>? elements = null,
        List<CreateDialogDialogGuiActionDto>? guiActions = null,
        List<CreateDialogDialogApiActionDto>? apiActions = null,
        List<CreateDialogDialogActivityDto>? activities = null)
    {
        return GenerateFakeDialogs(
            seed,
            1,
            id,
            serviceResource,
            party,
            progress,
            extendedStatus,
            externalReference,
            dueAt,
            expiresAt,
            status,
            content,
            searchTags,
            elements,
            guiActions,
            apiActions,
            activities
        )[0];
    }

    public static List<CreateDialogCommand> GenerateFakeDialogs(
        int? seed = null,
        int count = 1,
        Guid? id = null,
        string? serviceResource = null,
        string? party = null,
        int? progress = null,
        string? extendedStatus = null,
        string? externalReference = null,
        DateTimeOffset? dueAt = null,
        DateTimeOffset? expiresAt = null,
        DialogStatus.Values? status = null,
        List<CreateDialogContentDto>? content = null,
        List<CreateDialogSearchTagDto>? searchTags = null,
        List<CreateDialogDialogElementDto>? elements = null,
        List<CreateDialogDialogGuiActionDto>? guiActions = null,
        List<CreateDialogDialogApiActionDto>? apiActions = null,
        List<CreateDialogDialogActivityDto>? activities = null)
    {
        Randomizer.Seed = seed.HasValue ? new Random(seed.Value) : new Random();
        return new Faker<CreateDialogCommand>()
            .RuleFor(o => o.Id, f => id)
            .RuleFor(o => o.ServiceResource, _ => serviceResource ?? GenerateFakeResource())
            .RuleFor(o => o.Party, _ => party ?? GenerateRandomParty())
            .RuleFor(o => o.Progress, f => progress ?? f.Random.Number(0, 100))
            .RuleFor(o => o.ExtendedStatus, f => extendedStatus ?? f.Random.AlphaNumeric(10))
            .RuleFor(o => o.ExternalReference, f => externalReference ?? f.Random.AlphaNumeric(10))
            .RuleFor(o => o.DueAt, f => dueAt ?? f.Date.Future(10, RefTime))
            .RuleFor(o => o.ExpiresAt, f => expiresAt ?? f.Date.Future(20, RefTime.AddYears(11)))
            .RuleFor(o => o.Status, f => status ?? f.PickRandom<DialogStatus.Values>())
            .RuleFor(o => o.Content, _ => content ?? GenerateFakeDialogContent())
            .RuleFor(o => o.SearchTags, _ => searchTags ?? GenerateFakeSearchTags())
            .RuleFor(o => o.Elements, _ => elements ?? GenerateFakeDialogElements())
            .RuleFor(o => o.GuiActions, _ => guiActions ?? GenerateFakeDialogGuiActions())
            .RuleFor(o => o.ApiActions, _ => apiActions ?? GenerateFakeDialogApiActions())
            .RuleFor(o => o.Activities, _ => activities ?? GenerateFakeDialogActivities())
            .Generate(count);
    }

    private const string ResourcePrefix = "urn:altinn:resource:";

    public static CreateDialogCommand GenerateSimpleFakeDialog(Guid? id = null)
    {
        return GenerateFakeDialog(
            id: id,
            activities: [],
            elements: [],
            guiActions: [],
            apiActions: [],
            searchTags: []);
    }

    public static string GenerateFakeResource()
    {
        var r = new Randomizer();
        // Apply a power function to skew the distribution towards higher numbers
        // The exponent controls the shape of the distribution curve
        const int numberOfDistinctResources = 1000;
        const int exponent = 15; // Uses to adjust the distribution curve. Higher value = more skewed towards higher numbers
        var biasedRandom = Math.Pow(r.Double(), 1.0 / exponent);

        var result = 1 + (int)(biasedRandom * (numberOfDistinctResources - 1));

        return ResourcePrefix + result.ToString("D4", CultureInfo.InvariantCulture);
    }

    public static string GenerateRandomParty()
    {
        var r = new Randomizer();
        return r.Bool() ? $"urn:altinn:organization:identifier-no::{GenerateFakeOrgNo()}" : $"urn:altinn:person:identifier-no::{GenerateFakePid()}";
    }

    private static readonly int[] SocialSecurityNumberWeights1 = [3, 7, 6, 1, 8, 9, 4, 5, 2];
    private static readonly int[] SocialSecurityNumberWeights2 = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];
    private static readonly int[] OrgNumberWeights = [3, 2, 7, 6, 5, 4, 3, 2];

    public static string GenerateFakePid()
    {

        int c1, c2;
        string pidWithoutControlDigits;
        do
        {
            var dateOfBirth = GenerateRandomDateOfBirth();
            var individualNumber = GetRandomIndividualNumber(dateOfBirth.Year);

            pidWithoutControlDigits = dateOfBirth.ToString("ddMMyy", CultureInfo.InvariantCulture) + individualNumber;

            c1 = CalculateControlDigit(pidWithoutControlDigits, SocialSecurityNumberWeights1);
            c2 = CalculateControlDigit(pidWithoutControlDigits + c1, SocialSecurityNumberWeights2);

        } while (c1 == -1 || c2 == -1);

        return pidWithoutControlDigits + c1 + c2;
    }

    public static string GenerateFakeOrgNo()
    {
        var r = new Randomizer();
        string orgNumberWithoutControlDigit;
        int c;
        do
        {
            // We clamp the range to avoid generating far too many distinct org numbers
            orgNumberWithoutControlDigit = r.Number(99000000, 99999999).ToString(CultureInfo.InvariantCulture);
            c = CalculateControlDigit(orgNumberWithoutControlDigit, OrgNumberWeights);
        } while (c == -1);

        return orgNumberWithoutControlDigit + c;
    }

    private static string GetRandomIndividualNumber(int year)
    {
        var r = new Randomizer();
        if (year < 1900)
            throw new ArgumentException($"Invalid birth year: {year}", nameof(year));

        var individualNumber = year < 2000
            ? year < 1940 ? r.Number(1, 500) : r.Number(900, 999)
            : year < 2040
                ? r.Number(500, 999)
                : throw new ArgumentException($"Invalid birth year: {year}", nameof(year));
        return individualNumber.ToString("D3", CultureInfo.InvariantCulture);
    }

    // 5 years - up to 100 valid individual numbers per year (in this range)
    // = appx 1,7million distinct PIDs. The distribution is uniform, which is probably not
    // realistic, but hopefully it works for our purposes.
    private static readonly DateTime BirthDateRangeBegin = new(1965, 1, 1);
    private static readonly DateTime BirthDateRangeEnd = new(1970, 1, 1);
    private static readonly TimeSpan Range = BirthDateRangeEnd - BirthDateRangeBegin;
    private static DateTime GenerateRandomDateOfBirth()
    {
        var r = new Randomizer();
        return BirthDateRangeBegin.AddDays(r.Number(Range.Days));
    }

    private static int CalculateControlDigit(string input, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
        {
            sum += (input[i] - '0') * weights[i];
        }

        var mod = sum % 11;
        if (mod == 0) return 0;
        mod = 11 - mod;
        return mod == 10 ? -1 : mod;
    }

    public static CreateDialogDialogActivityDto GenerateFakeDialogActivity(DialogActivityType.Values? type = null)
        => GenerateFakeDialogActivities(1, type)[0];

    public static List<CreateDialogDialogActivityDto> GenerateFakeDialogActivities(int? count = null, DialogActivityType.Values? type = null)
    {
        return new Faker<CreateDialogDialogActivityDto>()
            .RuleFor(o => o.Id, f => f.Random.Guid())
            .RuleFor(o => o.CreatedAt, f => f.Date.Past())
            .RuleFor(o => o.ExtendedType, f => new Uri(f.Internet.UrlWithPath()))
            .RuleFor(o => o.Type, f => type ?? f.PickRandom<DialogActivityType.Values>())
            .RuleFor(o => o.PerformedBy, f => GenerateFakeLocalizations(f.Random.Number(2, 4)))
            .RuleFor(o => o.Description, f => GenerateFakeLocalizations(f.Random.Number(4, 8)))
            .Generate(count ?? new Randomizer().Number(1, 4));
    }

    public static List<CreateDialogDialogApiActionDto> GenerateFakeDialogApiActions()
    {
        return new Faker<CreateDialogDialogApiActionDto>()
            .RuleFor(o => o.Action, f => f.Random.AlphaNumeric(8))
            .RuleFor(o => o.Endpoints, _ => GenerateFakeDialogApiActionEndpoints())
            .Generate(new Randomizer().Number(1, 4));
    }

    public static List<CreateDialogDialogApiActionEndpointDto> GenerateFakeDialogApiActionEndpoints()
    {
        return new Faker<CreateDialogDialogApiActionEndpointDto>()
            .RuleFor(o => o.Url, f => new Uri(f.Internet.UrlWithPath()))
            .RuleFor(o => o.HttpMethod, f => f.PickRandom<HttpVerb.Values>())
            .RuleFor(o => o.Version, f => "v" + f.Random.Number(100, 999))
            .RuleFor(o => o.Deprecated, f => f.Random.Bool())
            .RuleFor(o => o.RequestSchema, f => new Uri(f.Internet.UrlWithPath()))
            .RuleFor(o => o.ResponseSchema, f => new Uri(f.Internet.UrlWithPath()))
            .RuleFor(o => o.DocumentationUrl, f => new Uri(f.Internet.UrlWithPath()))
            .Generate(new Randomizer().Number(min: 1, 4));
    }

    public static List<CreateDialogDialogGuiActionDto> GenerateFakeDialogGuiActions()
    {
        var hasPrimary = false;
        var hasSecondary = false;
        return new Faker<CreateDialogDialogGuiActionDto>()
            .RuleFor(o => o.Action, f => f.Random.AlphaNumeric(8))
            .RuleFor(o => o.Priority, _ =>
            {
                if (hasPrimary)
                {
                    if (hasSecondary)
                    {
                        return DialogGuiActionPriority.Values.Tertiary;
                    }

                    hasSecondary = true;
                    return DialogGuiActionPriority.Values.Secondary;
                }

                hasPrimary = true;
                return DialogGuiActionPriority.Values.Primary;
            })
            .RuleFor(o => o.Url, f => new Uri(f.Internet.UrlWithPath()))
            .RuleFor(o => o.Title, f => GenerateFakeLocalizations(f.Random.Number(1, 3)))
            .Generate(new Randomizer().Number(min: 1, 4));
    }

    public static CreateDialogDialogElementDto GenerateFakeDialogElement()
        => GenerateFakeDialogElements(1)[0];

    public static List<CreateDialogDialogElementDto> GenerateFakeDialogElements(int? count = null)
    {
        return new Faker<CreateDialogDialogElementDto>()
            .RuleFor(o => o.Id, _ => Guid.NewGuid())
            .RuleFor(o => o.Type, f => new Uri("urn:" + f.Random.AlphaNumeric(10)))
            .RuleFor(o => o.DisplayName, f => GenerateFakeLocalizations(f.Random.Number(2, 5)))
            .RuleFor(o => o.Urls, _ => GenerateFakeDialogElementUrls())
            .Generate(count ?? new Randomizer().Number(1, 6));
    }

    private static readonly string[] MimeTypes = [
        "application/json",
        "application/xml",
        "text/html",
        "application/pdf"
    ];

    public static List<CreateDialogDialogElementUrlDto> GenerateFakeDialogElementUrls()
    {
        return new Faker<CreateDialogDialogElementUrlDto>()
            .RuleFor(o => o.Url, f => new Uri(f.Internet.UrlWithPath()))
            .RuleFor(o => o.ConsumerType, f => f.PickRandom<DialogElementUrlConsumerType.Values>())
            .RuleFor(o => o.MimeType, f => f.PickRandom(MimeTypes))
            .Generate(new Randomizer().Number(1, 3));
    }

    public static List<CreateDialogSearchTagDto> GenerateFakeSearchTags()
    {
        return new Faker<CreateDialogSearchTagDto>()
            .RuleFor(o => o.Value, f => f.Random.AlphaNumeric(10))
            .Generate(new Randomizer().Number(1, 6));
    }

    public static List<CreateDialogContentDto> GenerateFakeDialogContent()
    {
        // We always need Title and Summary. Coin flip to determine to include AdditionalInfo
        // and/or SendersName
        var r = new Randomizer();
        var content = new List<CreateDialogContentDto> {
            new()
            {
                Type = DialogContentType.Values.Title,
                Value =  GenerateFakeLocalizations(r.Number(1, 4))
            },
            new()
            {
                Type = DialogContentType.Values.Summary,
                Value = GenerateFakeLocalizations(r.Number(7, 10))
            }
        };

        if (r.Bool())
        {
            content.Add(
                new()
                {
                    Type = DialogContentType.Values.SenderName,
                    Value = GenerateFakeLocalizations(r.Number(1, 3))
                }
            );
        }

        if (r.Bool())
        {
            content.Add(
                new()
                {
                    Type = DialogContentType.Values.AdditionalInfo,
                    Value = GenerateFakeLocalizations(r.Number(10, 20))
                }
            );
        }

        return content;
    }

    public static List<LocalizationDto> GenerateFakeLocalizations(int wordCount)
    {
        var r = new Randomizer();
        return
        [
            new()
            {
                CultureCode = "nb_NO",
                Value = r.Words(wordCount)
            },
            new()
            {
                CultureCode = "nn_NO",
                Value = r.Words(wordCount)
            },
            new()
            {
                CultureCode = "en_US",
                Value = r.Words(wordCount)
            }
        ];
    }
}

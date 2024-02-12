using System.Globalization;
using System.Text;
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
    public static List<CreateDialogDto> GenerateFakeDialogs(int seed, int count)
    {
        Randomizer.Seed = new Random(seed);
        return new Faker<CreateDialogDto>()
            //.RuleFor(o => o.Id, f => f.Random.Uuid())
            .RuleFor(o => o.ServiceResource, f => "urn:altinn:resource:" + f.Random.AlphaNumeric(10))
            .RuleFor(o => o.Party, f => GenerateRandomParty())
            .RuleFor(o => o.Progress, f => f.Random.Number(0, 100))
            .RuleFor(o => o.ExtendedStatus, f => f.Random.AlphaNumeric(10))
            .RuleFor(o => o.ExternalReference, f => f.Random.AlphaNumeric(10))
            .RuleFor(o => o.DueAt, f => f.Date.Future(10, RefTime))
            .RuleFor(o => o.ExpiresAt, f => f.Date.Future(20, RefTime.AddYears(11)))
            .RuleFor(o => o.Status, f => f.PickRandom<DialogStatus.Values>())
            .RuleFor(o => o.Content, _ => GenerateFakeDialogContent())
            .RuleFor(o => o.SearchTags, _ => GenerateFakeSearchTags())
            .RuleFor(o => o.Elements, _ => GenerateFakeDialogElements())
            .RuleFor(o => o.GuiActions, _ => GenerateFakeDialogGuiActions())
            .RuleFor(o => o.ApiActions, _ => GenerateFakeDialogApiActions())
            .RuleFor(o => o.Activities, _ => GenerateFakeDialogActivities())
            .Generate(count);
    }

    private static string GenerateRandomParty()
    {
        var r = new Randomizer();
        return r.Bool() ? $"urn:altinn:organization:identifier-no::{GenerateFakeOrgNo()}" : $"urn:altinn:person:identifier-no::{GenerateFakePid()}";
    }

    private static readonly int[] SocialSecurityNumberWeights1 = { 3, 7, 6, 1, 8, 9, 4, 5, 2 };
    private static readonly int[] SocialSecurityNumberWeights2 = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
    private static readonly int[] OrgNumberWeights = { 3, 2, 7, 6, 5, 4, 3, 2 };

    private static string GenerateFakePid()
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

    private static string GenerateFakeOrgNo()
    {
        var r = new Randomizer();
        string orgNumberWithoutControlDigit;
        int c;
        do
        {
            orgNumberWithoutControlDigit = r.Number(10000000, 99999999).ToString(CultureInfo.InvariantCulture);
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
            : year < 2040 ? r.Number(500, 999) : throw new ArgumentException($"Invalid birth year: {year}", nameof(year));

        return individualNumber.ToString("D3", CultureInfo.InvariantCulture);
    }

    private static readonly DateTime BirthDateRangeBegin = new(1900, 1, 1);
    private static readonly DateTime BirthDateRangeEnd = new(2010, 1, 1);
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
    private static List<CreateDialogDialogActivityDto> GenerateFakeDialogActivities()
    {
        return new Faker<CreateDialogDialogActivityDto>()
            .RuleFor(o => o.CreatedAt, f => f.Date.Past())
            .RuleFor(o => o.ExtendedType, f => new Uri(f.Internet.UrlWithPath()))
            .RuleFor(o => o.Type, f => f.PickRandom<DialogActivityType.Values>())
            .RuleFor(o => o.PerformedBy, f => GenerateFakeLocalizations(f.Random.Number(2, 4)))
            .RuleFor(o => o.Description, f => GenerateFakeLocalizations(f.Random.Number(4, 8)))
            .Generate(new Randomizer().Number(1, 4));
    }

    private static List<CreateDialogDialogApiActionDto> GenerateFakeDialogApiActions()
    {
        return new Faker<CreateDialogDialogApiActionDto>()
            .RuleFor(o => o.Action, f => f.Random.AlphaNumeric(8))
            .RuleFor(o => o.Endpoints, _ => GenerateFakeDialogApiActionEndpoints())
            .Generate(new Randomizer().Number(1, 4));
    }

    private static List<CreateDialogDialogApiActionEndpointDto> GenerateFakeDialogApiActionEndpoints()
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

    private static List<CreateDialogDialogGuiActionDto> GenerateFakeDialogGuiActions()
    {
        var hasPrimary = false;
        var hasSecondary = false;
        return new Faker<CreateDialogDialogGuiActionDto>()
            .RuleFor(o => o.Action, f => f.Random.AlphaNumeric(8))
            .RuleFor(o => o.Priority, f =>
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

    private static List<CreateDialogDialogElementDto> GenerateFakeDialogElements()
    {
        return new Faker<CreateDialogDialogElementDto>()
            .RuleFor(o => o.Type, f => new Uri("urn:" + f.Random.AlphaNumeric(10)))
            .RuleFor(o => o.DisplayName, f => GenerateFakeLocalizations(f.Random.Number(2, 5)))
            .RuleFor(o => o.Urls, _ => GenerateFakeDialogElementUrls())
            .Generate(new Randomizer().Number(1, 6));
    }

    private static readonly string[] MimeTypes = { "application/json", "application/xml", "text/html", "application/pdf" };

    private static List<CreateDialogDialogElementUrlDto> GenerateFakeDialogElementUrls()
    {
        return new Faker<CreateDialogDialogElementUrlDto>()
            .RuleFor(o => o.Url, f => new Uri(f.Internet.UrlWithPath()))
            .RuleFor(o => o.ConsumerType, f => f.PickRandom<DialogElementUrlConsumerType.Values>())
            .RuleFor(o => o.MimeType, f => f.PickRandom(MimeTypes))
            .Generate(new Randomizer().Number(1, 3));
    }

    private static List<CreateDialogSearchTagDto> GenerateFakeSearchTags()
    {
        return new Faker<CreateDialogSearchTagDto>()
            .RuleFor(o => o.Value, f => f.Random.AlphaNumeric(10))
            .Generate(new Randomizer().Number(1, 6));
    }

    private static List<CreateDialogContentDto> GenerateFakeDialogContent()
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

    private static List<LocalizationDto> GenerateFakeLocalizations(int wordCount)
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

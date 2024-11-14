using Attachment = Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById.Attachment;
using DomainAttachmentUrl = Digdir.Domain.Dialogporten.Domain.Attachments.AttachmentUrl;

namespace Digdir.Domain.Dialogporten.GraphQl.Unit.Tests.ObjectTypes;

public class AttachmentTests
{
    [Fact]
    public void Attachment_Object_Type_Should_Match_Property_Names_On_AttachmentEntity()
    {
        // Arrange
        var ignoreList = new List<string>
        {
            nameof(Domain.Attachments.Attachment.CreatedAt),
            nameof(Domain.Attachments.Attachment.UpdatedAt)
        };

        var attachmentProperties = typeof(Attachment)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        var domainAttachmentProperties = typeof(Domain.Attachments.Attachment)
            .GetProperties()
            .Select(p => p.Name)
            .Where(name => !ignoreList.Contains(name))
            .ToList();

        var missingProperties = domainAttachmentProperties
            .Except(attachmentProperties, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Assert
        Assert.True(missingProperties.Count == 0,
            $"Properties missing in graphql Attachment: {string.Join(", ", missingProperties)}");
    }

    [Fact]
    public void AttachmentUrl_Object_Type_Should_Match_Property_Names_On_DialogAttachmentUrl()
    {
        // Arrange
        var ignoreList = new List<string>
        {
            nameof(DomainAttachmentUrl.CreatedAt),
            nameof(DomainAttachmentUrl.UpdatedAt),
            nameof(DomainAttachmentUrl.ConsumerTypeId),
            nameof(DomainAttachmentUrl.AttachmentId),
            nameof(DomainAttachmentUrl.Attachment)
        };

        var attachmentUrlProperties = typeof(GraphQL.EndUser.DialogById.AttachmentUrl)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();

        var domainAttachmentUrlProperties = typeof(DomainAttachmentUrl)
            .GetProperties()
            .Select(p => p.Name)
            .Where(name => !ignoreList.Contains(name))
            .ToList();

        var missingProperties = domainAttachmentUrlProperties
            .Except(attachmentUrlProperties, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Assert
        Assert.True(missingProperties.Count == 0,
            $"Properties missing in graphql AttachmentUrl: {string.Join(", ", missingProperties)}");
    }
}

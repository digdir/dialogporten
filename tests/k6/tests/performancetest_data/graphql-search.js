export function getGraphqlParty(identifier) {
    return `
        query getAllDialogsForParties {
            searchDialogs(input: { party: ["urn:altinn:person:identifier-no:${identifier}"]}) {
            items {
                id
                party
                org
                progress
                guiAttachmentCount
                status
                createdAt
                updatedAt
                extendedStatus
                seenSinceLastUpdate {
                    id
                    seenAt
                    seenBy {
                        actorType
                        actorId
                        actorName
                    }
                    isCurrentEndUser
                }
                latestActivity {
                    description {
                        value
                        languageCode
                    }
                    performedBy {
                        actorType
                        actorId
                        actorName
                    }
                }
                content {
                    title {
                        mediaType
                        value {
                            value
                            languageCode
                        }
                    }
                    summary {
                        mediaType
                        value {
                            value
                        languageCode
                    }
                }
                senderName {
                    mediaType
                    value {
                        value
                        languageCode
                    }
                }
                extendedStatus {
                    mediaType
                    value {
                        value
                        languageCode
                    }
                }
            }
            systemLabel
          }
        }
    }`
}
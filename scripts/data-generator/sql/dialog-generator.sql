ROLLBACK;
BEGIN TRANSACTION;

-- Dialog
CREATE TEMPORARY TABLE dialog_keys (
    "Id" UUID, PRIMARY KEY ("Id")
);

WITH dialog_data AS (
    SELECT
        uuid_generate_v7_from_timestamp(ts, '023b067d-2440-4b80-85d5-3d1cd7833838') AS "Id",
        ts AS "CreatedAt",
        FALSE AS "Deleted",
        NULL::timestamptz AS "DeletedAt",
        NULL::timestamptz AS "DueAt",
        NULL::timestamptz AS "ExpiresAt",
        'sql-generated' AS "ExtendedStatus",
        NULL AS "ExternalReference",
        'ttd' AS "Org",
        NULL AS "PrecedingProcess",
        NULL AS "Process",
        11 AS "Progress",
        gen_random_uuid() AS "Revision",
        'GenericAccessResource' AS "ServiceResourceType",
        round(random() * 5 + 1) AS "StatusId",
        ts AS "UpdatedAt",
        NULL::timestamptz AS "VisibleFrom",
        ROW_NUMBER() OVER () - 1 AS rownum
    FROM generate_series('1970-01-01 00:00:00', '1970-02-01 00:00:00', '2 seconds'::INTERVAL) ts
)
, dialog_inserts AS (
INSERT INTO "Dialog" ("Id", "CreatedAt", "Deleted", "DeletedAt", "DueAt", "ExpiresAt",
                      "ExtendedStatus", "ExternalReference", "Org", "Party", "PrecedingProcess", "Process",
                      "Progress", "Revision", "ServiceResource", "ServiceResourceType", "StatusId", "UpdatedAt", "VisibleFrom")
SELECT
    d."Id", d."CreatedAt", d."Deleted", d."DeletedAt", d."DueAt", d."ExpiresAt",
    d."ExtendedStatus", d."ExternalReference", d."Org",
    p.value AS "Party", d."PrecedingProcess", d."Process", d."Progress", d."Revision",
    s.value AS "ServiceResource", d."ServiceResourceType", d."StatusId", d."UpdatedAt", d."VisibleFrom"
FROM dialog_data d
-- This table needs to be created and populated before this query
JOIN temp_parties p
    ON p.id = (d.rownum % (SELECT COUNT(*) FROM temp_parties)) + 1
-- This table needs to be created and populated before this query
JOIN temp_services s
    ON s.id = (d.rownum % (SELECT COUNT(*) FROM temp_services)) + 1
RETURNING "Id" AS id
)
INSERT INTO dialog_keys SELECT id FROM dialog_inserts;



-- DialogContent
CREATE TEMPORARY TABLE dialog_content_keys (
    "Id" UUID, PRIMARY KEY ("Id")
);

WITH source AS (
    SELECT
        uuid_generate_v7_from_timestamp(dialog."CreatedAt", uuid_generate_v5(dialog."Id", concat('DialogContent', dialogContentType))) AS id,
        dialog."Id" AS dialogId,
        dialog."CreatedAt" AS ts,
        CASE WHEN dialogContentType = 1 THEN 1 -- Title
             WHEN dialogContentType = 2 THEN 3 -- Summary
             END AS dialogContentType
    FROM "Dialog" dialog
        INNER JOIN generate_series(1, 2) dialogContentType ON TRUE
    INNER JOIN dialog_keys ON dialog_keys."Id" = dialog."Id"
), dialog_content_inserts AS (
    INSERT INTO "DialogContent" ("Id", "CreatedAt", "UpdatedAt", "MediaType", "DialogId", "TypeId")
    SELECT id, ts, ts, 'text/plain', dialogId, dialogContentType
    FROM source
    RETURNING "Id" AS id
) INSERT INTO dialog_content_keys SELECT id FROM dialog_content_inserts;



-- GuiAction
CREATE TEMPORARY TABLE gui_action_keys (
    "Id" UUID, PRIMARY KEY ("Id")
);

WITH source AS (
    SELECT
        uuid_generate_v7_from_timestamp(dialog."CreatedAt", uuid_generate_v5(dialog."Id", concat('GuiAction', priorityId))) AS id,
        dialog."CreatedAt" AS ts,
        CASE
            WHEN priorityId = 1 THEN 1 -- Primary
            WHEN priorityId = 2 THEN 2 -- Secondary
            ELSE 3 -- Tertiary
            END AS priority,
        dialog."Id" AS dialogId
    FROM "Dialog" dialog
        -- For every dialog, create 3 gui actions via inner join on true:
        INNER JOIN generate_series(1, 3) priorityId ON TRUE
    INNER JOIN dialog_keys ON dialog_keys."Id" = dialog."Id"
), gui_action_inserts AS (
    INSERT INTO "DialogGuiAction" ("Id", "CreatedAt", "UpdatedAt", "Action", "Url", "AuthorizationAttribute", "IsDeleteDialogAction", "PriorityId", "HttpMethodId", "DialogId")
    SELECT id, ts, ts, 'submit','https://digdir.apps.tt02.altinn.no', NULL,FALSE, priority, 2, dialogId
    FROM source
    RETURNING "Id" AS id
)
INSERT INTO gui_action_keys SELECT id FROM gui_action_inserts;



-- EndUserContext
WITH source AS (
    SELECT
        dialog."Id" AS dialogId,
        dialog."CreatedAt" AS ts
    FROM "Dialog" dialog
    INNER JOIN dialog_keys ON dialog_keys."Id" = dialog."Id"
)
INSERT INTO "DialogEndUserContext" ("Id", "CreatedAt", "UpdatedAt", "Revision", "DialogId", "SystemLabelId")
SELECT dialogId, ts, ts, gen_random_uuid(), dialogId, 1
FROM source;



-- SeenLog
CREATE TEMPORARY TABLE seen_log_keys (
    "Id" UUID, PRIMARY KEY ("Id")
);

WITH source AS (
    SELECT
        dialog."Id" AS dialogId,
        dialog."CreatedAt" AS ts
    FROM "Dialog" dialog
    INNER JOIN dialog_keys ON dialog_keys."Id" = dialog."Id"
), seen_log_inserts AS (
INSERT INTO "DialogSeenLog" ("Id", "CreatedAt", "IsViaServiceOwner", "DialogId", "EndUserTypeId")
SELECT dialogId, ts, FALSE, dialogId, 1
FROM source
RETURNING "Id" AS id
)
INSERT INTO seen_log_keys SELECT id FROM seen_log_inserts;



-- SearchTags
WITH source AS (
    SELECT
        uuid_generate_v7_from_timestamp(dialog."CreatedAt", uuid_generate_v5(dialog."Id", concat('SearchTag', searchTagNumber))) AS id,
        dialog."Id" AS dialogId,
        dialog."CreatedAt" AS ts,
        CASE WHEN searchTagNumber = 1 THEN 'This is a search tag'
             WHEN searchTagNumber = 2 THEN 'gibberish'
             ELSE 'Should we randomize this?' -- TODO: Randomize
             END AS searchTagValue
    FROM "Dialog" dialog
        INNER JOIN generate_series(1, 3) searchTagNumber ON TRUE
    INNER JOIN dialog_keys ON dialog_keys."Id" = dialog."Id"
)
INSERT INTO "DialogSearchTag" ("Id", "Value", "CreatedAt", "DialogId")
SELECT id, searchTagValue, ts, dialogId
FROM source;



-- Transmissions
CREATE TEMPORARY TABLE transmission_keys (
    "Id" UUID, PRIMARY KEY ("Id")
);

WITH source AS(
    SELECT
        uuid_generate_v7_from_timestamp(dialog."CreatedAt", uuid_generate_v5(dialog."Id", concat('Transmission', transmissionNumber))) AS id,
        dialog."Id" AS dialogId,
        dialog."CreatedAt" AS ts,
        CASE WHEN transmissionNumber = 1 THEN 1
             WHEN transmissionNumber = 2 THEN 2
             END AS transmissionValue
    FROM "Dialog" dialog
        INNER JOIN generate_series(1, 2) transmissionNumber ON TRUE
    INNER JOIN dialog_keys ON dialog_keys."Id" = dialog."Id"
), transmission_inserts AS (
    INSERT INTO "DialogTransmission" ("Id", "CreatedAt", "AuthorizationAttribute", "ExtendedType", "TypeId", "DialogId", "RelatedTransmissionId")
    SELECT id, ts, NULL, NULL, transmissionValue, dialogId, NULL
    FROM source
    RETURNING "Id" AS id
)
INSERT INTO transmission_keys SELECT id FROM transmission_inserts;



-- TransmissionContent
CREATE TEMPORARY TABLE transmission_content_keys (
    "Id" UUID, PRIMARY KEY ("Id")
);

WITH source AS (
    SELECT
        uuid_generate_v7_from_timestamp(transmission."CreatedAt", uuid_generate_v5(transmission."Id", concat('TransmissionContent', transmissionContentType))) AS id,
        transmission."Id" AS transmissionId,
        transmission."CreatedAt" AS ts,
        CASE WHEN transmissionContentType = 1 THEN 1 -- Title
             WHEN transmissionContentType = 2 THEN 2 -- Summary
             END AS transmissionContentValueId
    FROM "DialogTransmission" transmission
        INNER JOIN generate_series(1, 2) transmissionContentType ON TRUE
    INNER JOIN transmission_keys ON transmission_keys."Id" = transmission."Id"
), transmission_content_inserts AS (
    INSERT INTO "DialogTransmissionContent" ("Id", "CreatedAt", "UpdatedAt", "MediaType", "TransmissionId", "TypeId")
    SELECT id, ts, ts, 'text/plain', transmissionId, transmissionContentValueId
    FROM source
    RETURNING "Id" AS id
) INSERT INTO transmission_content_keys SELECT id FROM transmission_content_inserts;



-- Activity
CREATE TEMPORARY TABLE activity_keys (
    "Id" UUID, PRIMARY KEY ("Id")
);

WITH source AS (
    SELECT
        uuid_generate_v7_from_timestamp(dialog."CreatedAt", uuid_generate_v5(dialog."Id", concat('Activity', activityTypeId))) AS id,
        dialog."Id" AS dialogId,
        dialog."CreatedAt" AS ts,
        CASE WHEN activityTypeId = 1 THEN 1 -- DialogCreated
             WHEN activityTypeId = 2 THEN 3 -- Information
             END AS activityTypeId
    FROM "Dialog" dialog
        INNER JOIN generate_series(1, 2) activityTypeId ON TRUE
    INNER JOIN dialog_keys ON dialog_keys."Id" = dialog."Id"
), activity_inserts AS (
    INSERT INTO "DialogActivity" ("Id", "CreatedAt", "ExtendedType", "TypeId", "DialogId", "TransmissionId")
    SELECT id, ts, NULL, activityTypeId, dialogId, NULL
    FROM source
    RETURNING "Id" AS id
)
INSERT INTO activity_keys SELECT id FROM activity_inserts;



-- Attachment
CREATE TEMPORARY TABLE attachment_keys (
    "Id" UUID, PRIMARY KEY ("Id")
);

WITH source AS (
    SELECT 'DialogAttachment' discriminator
         ,dialog."Id" id
         ,"CreatedAt" created_at
         ,dialog."Id" a, NULL::uuid b
    FROM "Dialog" dialog
    INNER JOIN dialog_keys ON dialog_keys."Id" = dialog."Id"
    UNION ALL SELECT 'DialogTransmissionAttachment' discriminator
               ,transmission."Id" id
               ,"CreatedAt" created_at
               ,NULL a, transmission."Id" b
    FROM "DialogTransmission" transmission
    INNER JOIN transmission_keys ON transmission_keys."Id" = transmission."Id"
)
, attachment_inserts AS (
INSERT INTO "Attachment" ("Id", "CreatedAt", "UpdatedAt", "Discriminator", "DialogId", "TransmissionId")
    SELECT id, created_at, created_at, discriminator, a, b
    FROM source
        RETURNING "Id" AS id
) INSERT INTO attachment_keys SELECT id FROM attachment_inserts;



-- AttachmentUrl
WITH source AS (
    SELECT
        uuid_generate_v7_from_timestamp(attachment."CreatedAt", uuid_generate_v5(attachment."Id", concat('AttachmentUrl', attachmentUrlConsumerType))) AS id,
        attachment."Id" AS attachmentId,
        attachment."CreatedAt" AS ts,
        CASE WHEN attachmentUrlConsumerType = 1 THEN 1
             WHEN attachmentUrlConsumerType = 2 THEN 2
             END AS attachmentUrlValue
    FROM "Attachment" attachment
        INNER JOIN generate_series(1, 2) attachmentUrlConsumerType ON TRUE
    INNER JOIN attachment_keys ON attachment_keys."Id" = attachment."Id"
)
INSERT INTO "AttachmentUrl" ("Id", "CreatedAt", "UpdatedAt", "MediaType", "Url", "ConsumerTypeId", "AttachmentId")
SELECT id, ts, ts, 'text/plain', 'https://digdir.apps.tt02.altinn.no/', attachmentUrlValue, attachmentId
FROM source;



-- Actors
WITH source AS (
    SELECT 'DialogActivityPerformedByActor' discriminator
         ,activity."Id" id
         ,"CreatedAt" created_at
         ,activity."Id" a, NULL::uuid b, NULL::uuid c, NULL::uuid d
    FROM "DialogActivity" activity
    INNER JOIN activity_keys ON activity_keys."Id" = activity."Id"
    UNION ALL SELECT 'DialogTransmissionSenderActor' discriminator
               ,transmission."Id" id
               ,"CreatedAt" created_at
               ,NULL a, transmission."Id" b, NULL c, NULL d
    FROM "DialogTransmission" transmission
    INNER JOIN transmission_keys ON transmission_keys."Id" = transmission."Id"
--     UNION ALL SELECT 'LabelAssignmentLogActor' discriminator
--                ,label."Id" id
--                ,"CreatedAt" created_at
--                ,NULL a, NULL b, label."Id" c, NULL d
--     FROM "LabelAssignmentLog" label
--     INNER JOIN label_assignment_log_keys ON label_assignment_log_keys."Id" = label."Id"
    UNION ALL SELECT 'DialogSeenLogSeenByActor' discriminator
               ,seenlog."Id" id
               ,"CreatedAt" created_at
               ,NULL a, NULL b, NULL c, seenlog."Id" d
    FROM "DialogSeenLog" seenlog
    INNER JOIN seen_log_keys ON seen_log_keys."Id" = seenlog."Id"
)
INSERT INTO "Actor" ("Id", "ActorId", "ActorTypeId", "ActorName", "Discriminator",
                     "ActivityId", "DialogSeenLogId", "TransmissionId", "CreatedAt",
                     "UpdatedAt", "LabelAssignmentLogId")
SELECT id, NULL, 1, 'ActorName', discriminator,
       a, d, b, created_at, created_at, c
FROM source;



-- LocalizationSet
CREATE TEMPORARY TABLE localization_set_keys (
    "Id" UUID, PRIMARY KEY ("Id")
);

WITH source AS (
    SELECT 'AttachmentDisplayName' discriminator
         ,attachment."Id" id
         ,"CreatedAt" created_at
         ,attachment."Id" a, NULL::uuid b, NULL::uuid c, NULL::uuid d, NULL::uuid e
    FROM "Attachment" attachment
    INNER JOIN attachment_keys ON attachment_keys."Id" = attachment."Id"
    -- Ignoring DialogGuiActionPrompt
    UNION ALL SELECT 'DialogGuiActionTitle' discriminator
               ,guiAction."Id" id
               ,"CreatedAt" created_at
               ,NULL a, guiAction."Id" b, NULL c, NULL d, NULL e
    FROM "DialogGuiAction" guiAction
    INNER JOIN gui_action_keys ON gui_action_keys."Id" = guiAction."Id"
    UNION ALL SELECT 'DialogActivityDescription' discriminator
               ,activity."Id" id
               ,"CreatedAt" created_at
               ,NULL a, NULL b, activity."Id" c, NULL d, NULL e
    FROM "DialogActivity" activity
    INNER JOIN activity_keys ON activity_keys."Id" = activity."Id"
    WHERE "TypeId" = 3 -- Only Information is allowed to have descriptions
    UNION ALL SELECT 'DialogContentValue' discriminator
               ,dialogContent."Id" id
               ,"CreatedAt" created_at
               ,NULL a, NULL b, NULL c, dialogContent."Id" d, NULL e
    FROM "DialogContent" dialogContent
    INNER JOIN dialog_content_keys ON dialog_content_keys."Id" = dialogContent."Id"
    UNION ALL SELECT 'DialogTransmissionContentValue' discriminator
               ,transmissionContent."Id" id
               ,"CreatedAt" created_at
               ,NULL a, NULL b, NULL c, NULL d, transmissionContent."Id" e
    FROM "DialogTransmissionContent" transmissionContent
    INNER JOIN transmission_content_keys ON transmission_content_keys."Id" = transmissionContent."Id"
), localization_set_inserts AS (
    INSERT INTO "LocalizationSet" ("Id", "CreatedAt", "Discriminator", "AttachmentId", "GuiActionId", "ActivityId", "DialogContentId", "TransmissionContentId")
    SELECT id, created_at, discriminator, a, b, c, d, e
    FROM source
    RETURNING "Id" AS id
)
INSERT INTO localization_set_keys SELECT id FROM localization_set_inserts;



-- Localization
WITH source AS (
    SELECT
        localizationSet."Id" localizationSetId,
        "CreatedAt" ts,
        CASE
            WHEN language = 1 THEN 'nb'
            WHEN language = 2 THEN 'en'
            END AS language,
        CASE
            WHEN language = 1 THEN 'Norsk ' || substr(md5(random()::text), 0, 8)
            WHEN language = 2 THEN 'Engelsk ' || substr(md5(random()::text), 0, 8)
            END AS languageValue
        FROM "LocalizationSet" localizationSet
    INNER JOIN generate_series(1, 2) language ON TRUE
    INNER JOIN localization_set_keys ON localization_set_keys."Id" = localizationSet."Id"
)
INSERT INTO "Localization" ("LanguageCode", "LocalizationSetId", "CreatedAt", "UpdatedAt", "Value")
SELECT language, localizationSetId, ts, ts, languageValue
FROM source;



COMMIT;

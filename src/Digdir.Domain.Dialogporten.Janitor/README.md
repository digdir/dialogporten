# Dialogporten Janitor

This is a console application used container app jobs or perform various synchronization and janitorial tasks in Dialogporten.

## Commands

The following commands are available:

### UpdateSubjectResources

Synchronizes the mappings of subjects (ie. roles) and resources (ie. apps) from Altinn Resource Registry to Dialogportens local copy used for authorization. A parameter `--since` can be supplied to override the time of the last synchronization (should be parseable as a `DateTimeOffset`, ie. `2024-08-15`).

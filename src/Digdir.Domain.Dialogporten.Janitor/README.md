# Dialogporten Janitor

This is a console application used to perform various synchronization and janitorial tasks in Dialogporten.


## Commands

The following commands are available:

### UpdateSubjectResources

Synchronizes the mappings of subjects (i.e., roles) and resources (i.e., apps) from Altinn Resource Registry to Dialogportens local copy used for authorization. A parameter `--since` can be supplied to override the time of the last synchronization (should be parseable as a `DateTimeOffset`, i.e. `2024-08-15`).

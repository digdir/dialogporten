# Dialogporten Janitor

A console application for container app jobs or performing various synchronization and janitorial tasks in Dialogporten.

## Commands

Below are the available commands (commands are always the first argument):

### synchronize-subject-resource-mappings

- **Description:**  
  Synchronizes the mappings of subjects (i.e., roles) and resources (i.e., apps) from the Altinn Resource Registry to Dialogporten's local copy used for authorization.

- **Argument(s):**
    - *Optional*: A single argument can be provided to override the time of the last synchronization. This argument should be a `DateTimeOffset`, e.g., `2024-08-15`.

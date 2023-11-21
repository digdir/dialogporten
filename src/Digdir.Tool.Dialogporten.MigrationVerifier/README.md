### Migration Job Verifier

An application that is run as an init container for all Container Apps (WebApi, CDC, Service).
It checks for a specific Container App Job by name, and waits for it to complete before exiting.
The job it checks is the job running the database migration for the given deployment.
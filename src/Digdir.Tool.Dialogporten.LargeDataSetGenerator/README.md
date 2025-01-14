### Large data set generator

This project is used for generating large data sets for YT01, our performance environment.  
The data sets are generated as CSV strings and are written to the database directly using `COPY "X" ("Y", "X") FROM STDIN (FORMAT csv, HEADER false)`

Every table is generated independently in parallel, each table needs to generate correct foreign keys to other tables.
This is done via `DeterministicUuidV7.cs`, each dialog has a unique timestamp, and all IDs are generated using this plus the table name and an optional tie-breaker.

The number of threads per table is configurable by the optional `numberOfTasks` parameter in the `CreateCopyTask` method.

The `parties` file has been added to `.gitignore`, as it potentially contains large amounts of data.
This file needs to be created before running the project.

TODO:
* Parameterize the shell scripts, reduce the risk of committing secrets to the repo.
* Add a script for removing all constraints and indexes (SQL found in `Sql.cs`)
* Fine tune parallelism, we might be able to increase the number of threads on Actors, LocalizationSets, and Localizations.
* Add parallelism to the restoring of constraints and indexes. This is by far the slowest part of the process.

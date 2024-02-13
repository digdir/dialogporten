## DialogGenerator

This is a dialog generator that generates the DTOs used for creating new dialogs in the service owner API, 
utilizing the [Bogus](https://github.com/bchavez/Bogus) library. It supports supplying a integer seed in 
order to generate the same set of dialogs across multiple runs.

For improved performance, make sure you build in Release mode (both the tool and the rest of Dialogporten).

## Usage
See `dotnet run -- --help` for usage.

## Examples
* `dotnet run` displays a single dialog. 
* `dotnet run -- --seed 1234` displays a single dialog using a custom seed.
* `dotnet run -- --count 10000 --submit` attempts to generate 10000 dialogs and submit them to the API running at localhost.

## TODO
* Authorization support for use against remote APIs.
* Add options to adjust rule-based generation.
* Expose options to tweak the generation process (batching, consumers).
* Add generate-only mode supporting CSVs suitable for directly importing into a database (e.g. PostgreSQL's `COPY` command).
* Improve performance (inserting 1 million dialogs locally takes about 2 hours on a Mac M3 Pro).

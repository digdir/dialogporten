## DialogGenerator

This is a dialog generator that generates the DTOs used for creating new dialogs in the service owner API, 
utilizing the [Bogus](https://github.com/bchavez/Bogus) library. It supports supplying a integer seed in 
order to generate the same set of dialogs across multiple runs.

For improved performance, make sure you build in Release mode (both the tool and the rest of Dialogporten).

## CLI Usage
### Options 
See `dotnet run -- --help` for usage.

### Examples
* `dotnet run` displays a single dialog. 
* `dotnet run -- --seed 1234` displays a single dialog using a custom seed.
* `dotnet run -- --count 10000 --submit` attempts to generate 10000 dialogs and submit them to the API running at localhost.

## Code Usage

`DialogGenerator` is a static class that can be invoked like this:

```csharp
// Returns a single CreateDialogDto
var singleRandomDialog = DialogGenerator.GenerateDialog();
var singleRandomDialogWithSeed = DialogGenerator.GenerateDialog(1234);
// Returns a List<CreateDialogDto>
var multipleRandomDialogs = DialogGenerator.GenerateDialogs(100);
var multipleRandomDialogsWithSeed = DialogGenerator.GenerateDialogs(100, 1234);
// All CrateDialogDto properties can be overridden with (named) params
var singleRandomDialogWithParty = DialogGenerator.GenerateDialog(party: "urn:altinn:organization:identifier-no:991825827");

// There are several other methods for generating the various parts of the dialog, such as:
var randomParty = DialogGenerator.GenerateParty();
var randomContent = DialogGenerator.GenerateContent();
// etc etc
```

## TODO
* Authorization support for use against remote APIs.
* Expose options to tweak the generation process (batching, consumers).
* Add generate-only mode supporting CSVs suitable for directly importing into a database (e.g. PostgreSQL's `COPY` command).
* Improve performance (inserting 1 million dialogs locally takes about 2 hours on a Mac M3 Pro).

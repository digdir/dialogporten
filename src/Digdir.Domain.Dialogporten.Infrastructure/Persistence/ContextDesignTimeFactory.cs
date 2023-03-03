﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence;

internal sealed class ContextDesignTimeFactory : IDesignTimeDbContextFactory<DialogueDbContext>
{
    private const string LocalPostgreConnectionString = "Server=localhost;Port=5432;Database=Dialogporten;User ID=postgres;Password=changeme;";

    public DialogueDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<DialogueDbContext>()
            .UseNpgsql(LocalPostgreConnectionString)
            .Options);
}

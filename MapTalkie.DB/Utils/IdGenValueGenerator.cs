using System;
using IdGen;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace MapTalkie.DB.Utils;

public class IdGenValueGenerator : ValueGenerator<long>
{
    private readonly IdGenerator _generator;

    public IdGenValueGenerator(IdGenerator idGenerator)
    {
        _generator = idGenerator;
    }

    public override bool GeneratesTemporaryValues { get; } = false;

    public override long Next(EntityEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        return _generator.CreateId();
    }
}
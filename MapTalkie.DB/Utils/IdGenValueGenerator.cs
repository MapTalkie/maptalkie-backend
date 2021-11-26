using System;
using IdGen;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace MapTalkie.DB.Utils
{
    public class IdGenValueGenerator : ValueGenerator<long>
    {
        private IdGenerator? _generator;

        public override bool GeneratesTemporaryValues { get; }

        public override long Next(EntityEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            if (_generator == null) _generator = entry.Context.GetService<IdGenerator>();

            return _generator.CreateId();
        }
    }
}
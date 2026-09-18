using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Abstraction
{
    public abstract class BaseEntityMetadata : ISoftDeletable
    {
        public int Id { get; set; }

        public bool IsDeleted { get; set; }
    }
}

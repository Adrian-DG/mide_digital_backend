using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Abstraction
{
    public interface IAuditableEntityMetadata 
    {
        DateTime CreatedAt { get; set; }
        int CreatedBy { get; set; }
        DateTime UpdatedAt { get; set; }
        int UpdatedBy { get; set; } 
    }
}
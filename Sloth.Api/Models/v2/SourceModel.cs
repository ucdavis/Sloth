using Sloth.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Sloth.Api.Models.v2
{
    public class SourceModel
    {
        [MaxLength(50)]
        [Required]
        public string Name { get; set; }

        [MaxLength(50)]
        [Required]
        public string Type { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public static Expression<Func<Source, SourceModel>> Projection()
        {
            return source => new SourceModel
            {
                Name = source.Name,
                Type = source.Type,
                Description = source.Description
            };
        }
    }
}

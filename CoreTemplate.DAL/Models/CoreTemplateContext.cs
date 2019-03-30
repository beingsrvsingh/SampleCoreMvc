using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CoreTemplate.DAL.Models
{
    public partial class CoreTemplateContext : DbContext
    {
        public CoreTemplateContext()
        {
        }

        public CoreTemplateContext(DbContextOptions<CoreTemplateContext> options)
            : base(options)
        {
        }
    }
}
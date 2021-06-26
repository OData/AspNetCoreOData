// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ETags.EFCore
{
    public class ETagCurrencyTokenEfContext : DbContext
    {
        public ETagCurrencyTokenEfContext(DbContextOptions<ETagCurrencyTokenEfContext> options)
            : base(options)
        {
        }

        public DbSet<Dominio> Dominios { get; set; }

        public DbSet<Server> Servers { get; set; }
    }

    [Table("Domini")]
    public class Dominio
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; }

        [StringLength(200)]
        public string Descrizione { get; set; }

        public string ServerAutenticazioneId { get; set; }

        [ForeignKey("ServerAutenticazioneId")]
        public virtual Server ServerAutenticazione { get; set; }

        [ConcurrencyCheck]
        public int? RECVER { get; set; }
    }

    [Table("Servers")]
    public class Server
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; }

        [StringLength(200)]
        public string Descrizione { get; set; }

        [StringLength(2000)]
        public string Url { get; set; }

        [ConcurrencyCheck]
        public int? RECVER { get; set; }
    }
}

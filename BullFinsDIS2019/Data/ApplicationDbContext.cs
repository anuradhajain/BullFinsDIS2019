﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static BullFinsDIS2019.Models.EF_Models;


namespace BullFinsDIS2019.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Company> Companies { get; set; }
        public DbSet<StockStats> StockStatistics { get; set; }
        public DbSet<SymbolFinancial> SymbolFinancials { get; set; }
    }
}
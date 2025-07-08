using System;
using Microsoft.EntityFrameworkCore;

namespace JuniorTechTest;

public class OrdersDb(DbContextOptions options) : DbContext(options)
{    public DbSet<Order> Orders { get; set; }
}

// DbContext is the main EF class that initiates our Db
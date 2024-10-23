﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_cchall5.Models;

namespace Fall2024_Assignment3_cchall5.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Fall2024_Assignment3_cchall5.Models.Actor> Actor { get; set; } = default!;

    public DbSet<Fall2024_Assignment3_cchall5.Models.Movie> Movie { get; set; } = default!;

    public DbSet<Fall2024_Assignment3_cchall5.Models.MovieActor> MovieActor { get; set; } = default!;
}

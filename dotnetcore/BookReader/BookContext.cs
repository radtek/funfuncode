﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WordAnalyser.Model;

namespace BookReader
{
    public class BookContext : DbContext
    {
        public DbSet<Language> Languages { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookResult> BookResults { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<WordResult> WordResults { get; set; }
        public DbSet<BookException> BookExceptions { get; set; }
        public DbSet<WordResultHistory> WordResultHistories { get; set; }
        public DbSet<WordStatistics> WordStatisticses { get; set; }
        public DbSet<BookTrack> BookTracks { get; set; }
        public DbSet<MapBookCategory> MapBookCategories { get; set; }

        public BookContext(DbContextOptions<BookContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.TopIndexWords).IsUnique(true);
            modelBuilder.Entity<BookResult>()
                .HasIndex(b => b.Top1020).IsUnique(false);
            modelBuilder.Entity<WordStatistics>()
                .HasIndex(b => b.WordUnicode).IsUnique(true);
            modelBuilder.Entity<BookCategory>()
                .HasIndex(b => b.CategoryType).IsUnique(true);
            modelBuilder.Entity<MapBookCategory>()
                .HasIndex(b => new { b.BookId, b.BookCategoryId}).IsUnique(true);
            // 通过CategoryID得到相关的所有图书
            modelBuilder.Entity<MapBookCategory>()
                .HasIndex(b => new { b.BookCategoryId, b.BookId }).IsUnique(true);
            modelBuilder.Entity<MapBookCategory>()
                .HasIndex(b => new { b.BookCategoryId }).IsUnique(false);
            modelBuilder.Entity<MapBookCategory>()
                .HasIndex(b => new { b.BookId}).IsUnique(false);
        }
    }


    /// <summary>
    /// A factory to create an instance of the StudentsContext 
    /// </summary>
    public static class BookContextFactory
    {
        public static BookContext Create(string connectionString)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<BookContext>();
                if(connectionString.StartsWith(@"DataSource="))
                    optionsBuilder.UseSqlite(connectionString);
                else
                {
                    optionsBuilder.UseMySQL(connectionString);
                }

                // Ensure that the SQLite database and sechema is created!
                var context = new BookContext(optionsBuilder.Options);
                context.Database.EnsureCreated();

                return context;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}

﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using QRCodeMain.Models;
using System;

namespace QRCodeMain.Migrations
{
    [DbContext(typeof(MvcQrCodeContext))]
    partial class MvcQrCodeContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011");

            modelBuilder.Entity("QRCodeMain.Models.Article", b =>
                {
                    b.Property<long>("ArticleId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<long>("QrCodeId");

                    b.Property<string>("Title");

                    b.Property<string>("UserName");

                    b.HasKey("ArticleId");

                    b.HasIndex("QrCodeId")
                        .IsUnique();

                    b.ToTable("Articles");
                });

            modelBuilder.Entity("QRCodeMain.Models.Category", b =>
                {
                    b.Property<long>("CategoryId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("ArticleId");

                    b.Property<string>("Title");

                    b.HasKey("CategoryId");

                    b.HasIndex("ArticleId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("QRCodeMain.Models.Comment", b =>
                {
                    b.Property<long>("CommentId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("ArticleId");

                    b.Property<string>("Content");

                    b.HasKey("CommentId");

                    b.HasIndex("ArticleId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("QRCodeMain.Models.QrCode", b =>
                {
                    b.Property<long>("QrCodeId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("QrCodeRelativePath");

                    b.HasKey("QrCodeId");

                    b.ToTable("QrCodes");
                });

            modelBuilder.Entity("QRCodeMain.Models.UserTag", b =>
                {
                    b.Property<long>("UserTagId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("ArticleId");

                    b.Property<string>("Title");

                    b.HasKey("UserTagId");

                    b.HasIndex("ArticleId");

                    b.ToTable("UserTags");
                });

            modelBuilder.Entity("QRCodeMain.Models.WordStatistics", b =>
                {
                    b.Property<long>("WordStatisticsId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("MaxOccur");

                    b.Property<double>("MaxRatio");

                    b.Property<long>("MaxWords");

                    b.Property<long>("TotalBook");

                    b.Property<long>("TotalOccur");

                    b.Property<long>("TotalWords");

                    b.Property<string>("WordDescription");

                    b.Property<string>("WordUnicode");

                    b.HasKey("WordStatisticsId");

                    b.HasIndex("WordUnicode")
                        .IsUnique();

                    b.ToTable("WordStatisticses");
                });

            modelBuilder.Entity("VocabularyAnalyser.Model.Language", b =>
                {
                    b.Property<long>("LanguageId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("LanguageCode");

                    b.Property<string>("LanguageName");

                    b.Property<long>("LanguageType");

                    b.HasKey("LanguageId");

                    b.HasIndex("LanguageCode")
                        .IsUnique();

                    b.ToTable("Languages");
                });

            modelBuilder.Entity("VocabularyAnalyser.Model.MyVocabularyStatistics", b =>
                {
                    b.Property<long>("MyVocabularyStatisticsId")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("CurrentScore");

                    b.Property<long>("LanguageId");

                    b.Property<long>("TotalTestCount");

                    b.Property<string>("UserName");

                    b.HasKey("MyVocabularyStatisticsId");

                    b.HasIndex("LanguageId");

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("MyVocabularyStatisticses");
                });

            modelBuilder.Entity("VocabularyAnalyser.Model.MyVocabularyTest", b =>
                {
                    b.Property<long>("MyVocabularyTestId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CorrectWordCount");

                    b.Property<long>("LanguageId");

                    b.Property<double>("Score");

                    b.Property<DateTime>("TestTime");

                    b.Property<long>("TestWordCount");

                    b.Property<string>("UserName");

                    b.HasKey("MyVocabularyTestId");

                    b.HasIndex("LanguageId");

                    b.ToTable("MyVocabularyTests");
                });

            modelBuilder.Entity("VocabularyAnalyser.Model.UserVocabulary", b =>
                {
                    b.Property<long>("UserVocabularyId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CriticalTimes");

                    b.Property<long>("LanguageId");

                    b.Property<long>("TestTimes");

                    b.Property<string>("UserName");

                    b.Property<string>("WordUnicode");

                    b.Property<long>("WrongTimes");

                    b.HasKey("UserVocabularyId");

                    b.HasIndex("LanguageId");

                    b.ToTable("UserVocabularies");
                });

            modelBuilder.Entity("VocabularyAnalyser.Model.VocabularyTestDetail", b =>
                {
                    b.Property<long>("VocabularyTestDetailId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AnswerContentA");

                    b.Property<string>("AnswerContentB");

                    b.Property<string>("AnswerContentC");

                    b.Property<string>("AnswerContentD");

                    b.Property<char>("CorrectAnswer");

                    b.Property<char>("FinalAnswer");

                    b.Property<string>("LanguageCode");

                    b.Property<long>("VocabularyTestId");

                    b.Property<string>("WordUnicode");

                    b.HasKey("VocabularyTestDetailId");

                    b.HasIndex("VocabularyTestId");

                    b.ToTable("VocabularyTestDetails");
                });

            modelBuilder.Entity("QRCodeMain.Models.Article", b =>
                {
                    b.HasOne("QRCodeMain.Models.QrCode", "QrCode")
                        .WithOne("Article")
                        .HasForeignKey("QRCodeMain.Models.Article", "QrCodeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("QRCodeMain.Models.Category", b =>
                {
                    b.HasOne("QRCodeMain.Models.Article")
                        .WithMany("Categories")
                        .HasForeignKey("ArticleId");
                });

            modelBuilder.Entity("QRCodeMain.Models.Comment", b =>
                {
                    b.HasOne("QRCodeMain.Models.Article")
                        .WithMany("Comments")
                        .HasForeignKey("ArticleId");
                });

            modelBuilder.Entity("QRCodeMain.Models.UserTag", b =>
                {
                    b.HasOne("QRCodeMain.Models.Article")
                        .WithMany("UserTags")
                        .HasForeignKey("ArticleId");
                });

            modelBuilder.Entity("VocabularyAnalyser.Model.MyVocabularyStatistics", b =>
                {
                    b.HasOne("VocabularyAnalyser.Model.Language", "Language")
                        .WithMany()
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VocabularyAnalyser.Model.MyVocabularyTest", b =>
                {
                    b.HasOne("VocabularyAnalyser.Model.Language", "Language")
                        .WithMany()
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VocabularyAnalyser.Model.UserVocabulary", b =>
                {
                    b.HasOne("VocabularyAnalyser.Model.Language", "Language")
                        .WithMany()
                        .HasForeignKey("LanguageId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("VocabularyAnalyser.Model.VocabularyTestDetail", b =>
                {
                    b.HasOne("VocabularyAnalyser.Model.MyVocabularyTest", "VocabularyTest")
                        .WithMany("VocabularyTestDetails")
                        .HasForeignKey("VocabularyTestId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}

﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WepApi.Model;

#nullable disable

namespace WepApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20221203013515_InitialMigration")]
    partial class InitialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("AppClases.DataMigacion", b =>
                {
                    b.Property<string>("Cui")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("DatosCompletos")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Cui")
                        .HasName("pk_datamigracion");

                    b.HasIndex("Cui")
                        .IsUnique()
                        .HasDatabaseName("uk_datamigracion");

                    b.ToTable("DataMigracion", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}

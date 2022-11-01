using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace IssueConsoleTemplate
{
    // 
    // Database Entities:
    // 

    public class IceCream
    {
        public int Id { get; set; }
        public string Name { get; set; }

        //
        // JSON properties:
        //

        public IceCreamProperties Properties { get; set; }
        public JsonDocument PrimarySupplierInformation { get; set; }
        public List<string> FoodAdditives { get; set; }
        public string Tags { get; set; }
        public List<IceCreamSupplierInformation> AllSupplierInformations { get; set; }

        public Dictionary<string, string> Values { get; set; } = new();

        public JsonObject<dynamic> AHData { get; set; }

    }

    // 
    // JSON Objects:
    // 

    public class IceCreamProperties
    {
        public int PopularityRank { get; set; }
        public bool InStock { get; set; }
    }

    public class IceCreamSupplierInformation
    {
        public string Name { get; set; }
        public double StandardHygiene { get; set; }
    }

    // 
    // DbContext:
    // 

    public class Context : DbContext
    {
        public DbSet<IceCream> IceCreams { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            var _connectionString = "Server=localhost;Database=aspehub.master;Uid=root;Pwd=root;Charset=utf8;Port=3336";

            optionsBuilder
                .UseMySql( _connectionString,   ServerVersion.AutoDetect(_connectionString), m =>
                {
                    m.UseMicrosoftJson(MySqlCommonJsonChangeTrackingOptions.FullHierarchyOptimizedSemantically);
                    m.EnableRetryOnFailure();
                })
                .UseLoggerFactory(
                    LoggerFactory.Create(
                        b => b
                            .AddConsole()
                            .AddFilter(level => level >= LogLevel.Information)))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IceCream>(
                entity =>
                {
                    // 
                    // Force JSON as the column type:
                    // 

                    entity.Property(e => e.Properties)
                        .HasColumnType("json");

                    // This is not needed, because DOM classes like JsonDocument are being serialized as JSON
                    // by default.
                    // entity.Property(e => e.PrimarySupplierInformation)
                    //     .HasColumnType("json");

                    entity.Property(e => e.FoodAdditives)
                        .HasColumnType("json");

                    entity.Property(e => e.Tags)
                        .HasColumnType("json");

                    // Demonstrates how to override the default JSON change tracking options previously defined via the
                    // UseMicrosoftJson(options: ) parameter.
                    entity.Property(e => e.AllSupplierInformations)
                        .HasColumnType("json")
                        // .UseJsonChangeTrackingOptions(MySqlCommonJsonChangeTrackingOptions.RootPropertyOnly)
                        ;

                    var options = new JsonSerializerOptions(JsonSerializerDefaults.General);

                    entity
                        .Property(x => x.Values)
                        .HasColumnName("Values")
                            .HasColumnType("json") 
                                .HasConversion(
                            v => JsonSerializer.Serialize(v, options),
                                    s => JsonSerializer.Deserialize<Dictionary<string, string>>(s, options)!,
                                    ValueComparer.CreateDefault(typeof(Dictionary<string, string>), true)
                                );

                    entity
                        .Property(e => e.AHData)
                           .HasColumnType("json");



                    //
                    // Sample Data:
                    //

                    entity.HasData(
                        new IceCream
                        {
                            Id = 1,
                            Name = "Vanilla",
                            Values = new Dictionary<string, string>()
                            {
                                { "01" , "one" },
                                { "02" , "two" },
                            },
                            
                            AHData = @"{""AdminGroupsValue"":1,""CreatedDate"":""2022-06-07T13:44:12.5871278Z"",""DiscardedDate"":""0001-01-01T00:00:00"",""Name"":""G.2.1_DENDROLOGICKÝ-PRŮZKUM"",""Path"":""/1. Příprava staveb/1.04 DSP/G_SOUVISEJÍCÍ-DOKUMENTACE/G.2_PODKLADY-PRŮZKUMY/G.2.1_DENDROLOGICKÝ-PRŮZKUM"",""PermissionInheritance"":true,""ReadGroupsValue"":0,""RequiredCategoryTrees"":{},""UserAcessLevel"":{},""WriteGroupsValue"":0}",

                            Properties = new IceCreamProperties
                            {
                                PopularityRank = 1,
                                InStock = true,
                            },
                            PrimarySupplierInformation = JsonDocument.Parse(
                                JsonSerializer.Serialize(
                                    new IceCreamSupplierInformation
                                    {
                                        Name = "Fasssst Dilivery",
                                        StandardHygiene = 0.45,
                                    })),
                            FoodAdditives = new List<string> { "E102" },
                            Tags = @"[""fluffy"", ""white"", ""yellow""]",
                            AllSupplierInformations = new List<IceCreamSupplierInformation>
                            {
                                new IceCreamSupplierInformation
                                {
                                    Name = "Fasssst Dilivery",
                                    StandardHygiene = 0.45,
                                },
                                new IceCreamSupplierInformation
                                {
                                    Name = "Fast Fooood",
                                    StandardHygiene = 0.61,
                                },
                            },
                        },
                        new IceCream
                        {
                            Id = 2,
                            Name = "Chocolate",

                            Values = new Dictionary<string, string>()
                            {
                                { "10" , "ten" },
                                { "20" , "twenty" },
                            },
                            AHData= "{\"before\":{\"ETag\":null,\"ModifiedById\":null,\"Status\":\"Pending\"},\"after\":{\"ETag\":\"0x8DA488C0F5AA3C8\",\"ModifiedById\":\"d96d666a-3e73-462f-8ecf-155b94aa14e4\",\"Status\":\"Uploaded\"}}",


                            Properties = new IceCreamProperties
                            {
                                PopularityRank = 2,
                                InStock = true,
                            },
                            PrimarySupplierInformation = JsonDocument.Parse(
                                JsonSerializer.Serialize(
                                    new IceCreamSupplierInformation
                                    {
                                        Name = "Sweet Dilivery",
                                        StandardHygiene = 0.65,
                                    })),
                            FoodAdditives = new List<string> { "E124", "E155" },
                            Tags = @"[""creamy"", ""brown""]",
                            AllSupplierInformations = new List<IceCreamSupplierInformation>
                            {
                                new IceCreamSupplierInformation
                                {
                                    Name = "Sweet Dilivery",
                                    StandardHygiene = 0.65,
                                },
                            }
                        },
                        new IceCream
                        {
                            Id = 3,

                            Name = "Strawberry",
                            AHData = "{\"ContentType\":\"image/png\",\"CreatedDate\":\"2022-06-07T13:46:03.8275364Z\",\"Data\":{\"json\":\"{\\\"Object\\\":{},\\\"Json\\\":\\\"{}\\\"}\"},\"DerivateType\":\"ThumbnailSmall\",\"FileName\":null,\"Referenced\":false,\"Size\":0,\"Status\":\"Processing\"}",
                            Properties = new IceCreamProperties
                            {
                                PopularityRank = 3,
                                InStock = false,
                            },
                            PrimarySupplierInformation = JsonDocument.Parse(
                                JsonSerializer.Serialize(
                                    new IceCreamSupplierInformation
                                    {
                                        Name = "Fresh Dilivery",
                                        StandardHygiene = 0.85,
                                    })),
                            FoodAdditives = new List<string> { "E124" },
                            Tags = @"[""sweet"", ""red""]",
                            AllSupplierInformations = new List<IceCreamSupplierInformation>
                            {
                                new IceCreamSupplierInformation
                                {
                                    Name = "Fresh Dilivery",
                                    StandardHygiene = 0.85,
                                },
                            }
                        },
                        new IceCream
                        {
                            Id = 4,
                            Name = "Matcha",
                            Properties = new IceCreamProperties
                            {
                                PopularityRank = 42,
                                InStock = false,
                            },
                            AHData = "{\"CreatedDate\":\"2022-06-07T13:46:01.8823107Z\",\"Description\":\"\",\"DiscardedDate\":\"0001-01-01T00:00:00\",\"RevisionState\":\"published\"}",
                            PrimarySupplierInformation = JsonDocument.Parse(
                                @"{""Name"": ""Fine Dine"", ""StandardHygiene"": 0.98}"),
                            FoodAdditives = new List<string> { "E102", "E142" },
                            Tags = @"[""bitter"", ""green""]",
                            AllSupplierInformations = new List<IceCreamSupplierInformation>
                            {
                                new IceCreamSupplierInformation
                                {
                                    Name = "Fine Dine",
                                    StandardHygiene = 0.98,
                                },
                            }
                        }
                    ); ;
                });
        }
    }

    internal class Program
    {
        private static void Main()
        {
            using var context = new Context();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Query all ice creams.
            var iceCreams = context.IceCreams
                .OrderBy(i => i.Id)
                .ToList();

            Debug.Assert(iceCreams.Count == 4);
            Debug.Assert(iceCreams[3].PrimarySupplierInformation.RootElement.GetProperty("Name").GetString() == "Fine Dine");
            Debug.Assert(iceCreams[0].AllSupplierInformations.Count == 2);
            Debug.Assert(iceCreams[0].AllSupplierInformations[1].Name == "Fast Fooood");

            // Query ice creams by handling the JSON column as plain text and using a simple LIKE clause. 
            var yellowTaggedIceCreams = context.IceCreams
                .Where(i => EF.Functions.JsonSearchAny(i.Tags, "yellow"))
                .ToList();

            Debug.Assert(yellowTaggedIceCreams.Count == 1);

            // Query ice creams by using a sql query and using the MySQL JSON_CONTAINS() function.
            var yellowFoodColoringENumber = "E102";
            var iceCreamsWithYellowFoodColoring = context.IceCreams
                .FromSqlInterpolated($"select * from `IceCreams` where json_contains(`FoodAdditives`, json_quote({yellowFoodColoringENumber}), '$') <> 0")
                .ToList();

            Debug.Assert(iceCreamsWithYellowFoodColoring.Count == 2);

            // Query ice creams by using EF functions.
            var redFoodColoringENumber = "E124";
            var iceCreamsWithRedFoodColoring = context.IceCreams
                .Where(i => EF.Functions.JsonContains(i.FoodAdditives, EF.Functions.JsonQuote(redFoodColoringENumber), "$"))
                .ToList();

            Debug.Assert(iceCreamsWithRedFoodColoring.Count == 2);

            var iceCreamsFromFineDine = context.IceCreams
                .Where(i => EF.Functions.JsonSearchAny(i.AllSupplierInformations, @"Fine Dine", "$[*].Name"))
                .OrderBy(i => i.Id)
                .ToList();

            Debug.Assert(iceCreamsFromFineDine.Count == 1);

            var iceCreamsPrimarilyFromFineDine = context.IceCreams
                .Where(i => i.PrimarySupplierInformation.RootElement.GetProperty("Name").GetString() == "Fine Dine")
                .OrderBy(i => i.Id)
                .ToList();

            Debug.Assert(iceCreamsPrimarilyFromFineDine.Count == 1);
        }
    }
}
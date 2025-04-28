using Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Csharp.Common.EntityFramework.Extensions;

public static class MigrationBuilderExtensions
{
    #region sql script attribute method helpers
    // using sql script entity attributes, these are the only methods you need using reflection
    /// <summary>
    ///
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="entityDetail"></param>
    /// <param name="timestamp"></param>
    /// <param name="startingPath"></param>
    public static void CreateEntity(
        this MigrationBuilder migrationBuilder,
        SqlScriptDetail entityDetail,
        string? timestamp = null,
        string? startingPath = null)
    {
        RunSqlCreate(migrationBuilder, entityDetail.ClassType, entityDetail.IdentifierType, timestamp, startingPath);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="entityDetail"></param>
    /// <param name="startingPath"></param>
    public static void DropEntity(this MigrationBuilder migrationBuilder, SqlScriptDetail entityDetail, string? startingPath = null)
    {
        RunSqlDrop(migrationBuilder, entityDetail.ClassType, entityDetail.IdentifierType, startingPath);
    }

    public static void CreateView(
        this MigrationBuilder migrationBuilder,
        Type typeWithExpectedAttributes,
        string? timestamp = null,
        string? startingPath = null)
    {
        RunSqlCreate(migrationBuilder, typeWithExpectedAttributes, SqlScriptLoader.Types.Views, timestamp, startingPath);
    }

    public static void DropView(
        this MigrationBuilder migrationBuilder,
        Type typeWithExpectedAttributes,
        string? startingPath = null)
    {
        RunSqlDrop(migrationBuilder, typeWithExpectedAttributes, SqlScriptLoader.Types.Views, startingPath);
    }

    public static void CreateTrigger(
        this MigrationBuilder migrationBuilder,
        Type typeWithExpectedAttributes,
        string? timestamp = null,
        string? startingPath = null)
    {
        RunSqlCreate(migrationBuilder, typeWithExpectedAttributes, SqlScriptLoader.Types.Triggers, timestamp, startingPath);
    }

    public static void DropTrigger(
        this MigrationBuilder migrationBuilder,
        Type typeWithExpectedAttributes,
        string? startingPath = null)
    {
        RunSqlDrop(migrationBuilder, typeWithExpectedAttributes, SqlScriptLoader.Types.Triggers, startingPath);
    }

    private static void RunSqlCreate(
        MigrationBuilder migrationBuilder,
        Type typeWithExpectedAttributes,
        SqlScriptLoader.Types sqlScriptType,
        string? timestamp = null,
        string? startingDomainPath = null)
    {
        var sqlScriptLoader = SqlScriptLoaderFactory.New(startingDomainPath);
        var sqlText = sqlScriptLoader.CreateFromSql(typeWithExpectedAttributes, sqlScriptType, timestamp);
        // Console.WriteLine(sqlText);
        migrationBuilder.Sql(sqlText);
    }

    private static void RunSqlDrop(
        MigrationBuilder migrationBuilder,
        Type typeWithExpectedAttributes,
        SqlScriptLoader.Types sqlScriptType,
        string? startingDomainPath = null)
    {
        var sqlScriptLoader = SqlScriptLoaderFactory.New(startingDomainPath);
        var sqlText = sqlScriptLoader.DropFromSql(typeWithExpectedAttributes, sqlScriptType);
        // Console.WriteLine(sqlText);
        migrationBuilder.Sql(sqlText);
    }
    #endregion

    #region Migration builder extensions
    public static void LoadDataScript(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader scriptLoader)
    {
        RunSqlCommand(migrationBuilder, fileName, SqlScriptLoader.Types.Seeder, scriptLoader);
    }
    public static void LoadTrigger(this MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, SqlScriptLoader.Types.Triggers, scriptLoader);
    }

    public static void LoadTrigger(this MigrationBuilder migrationBuilder, string fileName, string timestamp, SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, timestamp, SqlScriptLoader.Types.Triggers, scriptLoader);
    }

    public static void UnloadTrigger(this MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader scriptLoader)
    {
        RunSqlDrop(migrationBuilder, fileName, SqlScriptLoader.Types.Triggers, scriptLoader);
    }

    public static void LoadProcedure(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, SqlScriptLoader.Types.Procedures, scriptLoader);
    }

    public static void LoadProcedure(this MigrationBuilder migrationBuilder, string fileName, string timestamp,
        SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, timestamp, SqlScriptLoader.Types.Procedures, scriptLoader);
    }

    public static void DropProcedure(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader scriptLoader)
    {
        RunSqlDrop(migrationBuilder, fileName, SqlScriptLoader.Types.Procedures, scriptLoader);
    }

    [Obsolete("Nomenclature 'Load' is deprecated in favor of 'Create' more in line with SQL nomenclature")]
    public static void LoadView(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, SqlScriptLoader.Types.Views, scriptLoader);
    }

    public static void CreateView(this MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, SqlScriptLoader.Types.Views, scriptLoader);
    }

    public static void CreateProcedure(this MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, SqlScriptLoader.Types.Procedures, scriptLoader);
    }

    public static void DropView(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader sqlScriptLoader)
    {
        RunSqlDrop(migrationBuilder, fileName, SqlScriptLoader.Types.Views, sqlScriptLoader);
    }
    public static void CreateSpatialIndex(this MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, SqlScriptLoader.Types.SpatialIndex, scriptLoader);
    }

    public static void DropSpatialIndex(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader sqlScriptLoader)
    {
        RunSqlDrop(migrationBuilder, fileName, SqlScriptLoader.Types.SpatialIndex, sqlScriptLoader);
    }
    #endregion


    #region Private Helper Methods
    /// <summary>
    /// Metho
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <param name="fileName"></param>
    /// <param name="type"></param>
    /// <param name="scriptLoader"></param>
    private static void RunSqlCreate(MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader.Types type,
        SqlScriptLoader scriptLoader)
    {
        migrationBuilder.Sql(scriptLoader.CreateFromSql(FileName(fileName), type));
    }

    private static void RunSqlCreate(MigrationBuilder migrationBuilder, string fileName, string timestamp, SqlScriptLoader.Types type,
        SqlScriptLoader scriptLoader)
    {
        migrationBuilder.Sql(scriptLoader.CreateFromSql(FileName(fileName, timestamp), type));
    }

    private static void RunSqlDrop(MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader.Types type,
        SqlScriptLoader scriptLoader)
    {
        migrationBuilder.Sql(scriptLoader.DropFromSql(FileName(fileName), type));
    }

    private static void RunSqlCommand(MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader.Types type,
        SqlScriptLoader scriptLoader)
    {
        migrationBuilder.Sql(scriptLoader.RunScript(FileName(fileName), type));
    }

    private static string FileName(string fileName, string? timestamp = null)
    {
        if (timestamp is null)
        {
            return fileName + ".sql";
        }

        return fileName + "_" + timestamp + ".sql";
    }
    #endregion
}

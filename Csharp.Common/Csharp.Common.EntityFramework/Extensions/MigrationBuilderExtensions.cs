using Csharp.Common.EntityFramework.Domain;
using Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Csharp.Common.EntityFramework.Extensions;

public static class MigrationBuilderExtensions
{
    #region Migration builder extensions

    public static void LoadDataScript(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader scriptLoader)
    {
        RunSqlCommand(migrationBuilder, fileName, Types.Seeder, scriptLoader);
    }
    public static void LoadTrigger(this MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, Types.Triggers, scriptLoader);
    }

    public static void LoadTrigger(this MigrationBuilder migrationBuilder, string fileName, string timestamp, SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, timestamp, Types.Triggers, scriptLoader);
    }

    public static void UnloadTrigger(this MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader scriptLoader)
    {
        RunSqlDrop(migrationBuilder, fileName, Types.Triggers, scriptLoader);
    }

    public static void LoadProcedure(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, Types.Procedures, scriptLoader);
    }

    public static void LoadProcedure(this MigrationBuilder migrationBuilder, string fileName, string timestamp,
        SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, timestamp, Types.Procedures, scriptLoader);
    }

    public static void DropProcedure(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader scriptLoader)
    {
        RunSqlDrop(migrationBuilder, fileName, Types.Procedures, scriptLoader);
    }

    [Obsolete("Nomenclature 'Load' is deprecated in favor of 'Create' more in line with SQL nomenclature")]
    public static void LoadView(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, Types.Views, scriptLoader);
    }

    public static void CreateView(this MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, Types.Views, scriptLoader);
    }

    public static void CreateProcedure(this MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, Types.Procedures, scriptLoader);
    }

    public static void DropView(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader sqlScriptLoader)
    {
        RunSqlDrop(migrationBuilder, fileName, Types.Views, sqlScriptLoader);
    }
    public static void CreateSpatialIndex(this MigrationBuilder migrationBuilder, string fileName, SqlScriptLoader scriptLoader)
    {
        RunSqlCreate(migrationBuilder, fileName, Types.SpatialIndex, scriptLoader);
    }

    public static void DropSpatialIndex(this MigrationBuilder migrationBuilder, string fileName,
        SqlScriptLoader sqlScriptLoader)
    {
        RunSqlDrop(migrationBuilder, fileName, Types.SpatialIndex, sqlScriptLoader);
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
    private static void RunSqlCreate(MigrationBuilder migrationBuilder, string fileName, Types type,
        SqlScriptLoader scriptLoader)
    {
        migrationBuilder.Sql(scriptLoader.CreateFromSql(FileName(fileName), type));
    }

    private static void RunSqlCreate(MigrationBuilder migrationBuilder, string fileName, string timestamp, Types type,
        SqlScriptLoader scriptLoader)
    {
        migrationBuilder.Sql(scriptLoader.CreateFromSql(FileName(fileName, timestamp), type));
    }

    private static void RunSqlDrop(MigrationBuilder migrationBuilder, string fileName, Types type,
        SqlScriptLoader scriptLoader)
    {
        migrationBuilder.Sql(scriptLoader.DropFromSql(FileName(fileName), type));
    }

    private static void RunSqlCommand(MigrationBuilder migrationBuilder, string fileName, Types type,
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

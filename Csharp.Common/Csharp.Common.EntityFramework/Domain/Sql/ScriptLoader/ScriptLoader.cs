namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader;

public class SqlScriptLoader
{
    private readonly string _startupPath;

    private readonly string _dboPath;

    public SqlScriptLoader()
    {
        _startupPath = AppDomain.CurrentDomain.BaseDirectory;
        _dboPath = JoinPath("SqlScripts");
    }

    public SqlScriptLoader(string baseDirectory)
    {
        _startupPath = baseDirectory;
        _dboPath = JoinPath("SqlScripts");
    }

    public string _Path
    {
        get
        {
            if (_startupPath is null)
            {
                return _dboPath;
            }
            return _startupPath + Path.DirectorySeparatorChar + _dboPath;
        }
    }

    public string FunctionsDirectory => JoinPath(_Path, "Functions");
    public string ProceduresDirectory => JoinPath(_Path, "StoredProcedures");
    public string ServiceBrokerDirectory => JoinPath(_Path, "ServiceBroker");
    public string TriggersDirectory => JoinPath(_Path, "Triggers");
    public string SecurityDirectory => JoinPath(_Path, "Security");
    public string SeederDirectory => JoinPath(_Path, "Seeders");
    public string ViewsDirectory => JoinPath(_Path, "Views");
    public string SpatialIndexDirectory => JoinPath(_Path, "Spatial Indexes");
    public string DataCleanupDirectory => JoinPath(_Path, "DataCleanup");

    private string JoinPath(params string[] paths)
    {
        return string.Join(Path.DirectorySeparatorChar, paths);
    }

    private string GetLoadTypePath(string path, LoadType loadType) =>
        JoinPath(path, (loadType == LoadType.Drop ? "Drop" : ""));

    public string CreateFromSql(string sqlFileName, Types enumType)
    {
        return File.ReadAllText(JoinPath(GetPath(enumType, LoadType.Create), sqlFileName));
    }

    public string DropFromSql(string sqlFileName, Types enumType)
    {
        return File.ReadAllText(JoinPath(GetPath(enumType, LoadType.Drop), sqlFileName));
    }

    public string RunScript(string sqlFileName, Types enumType)
    {
        return File.ReadAllText(JoinPath(GetPath(enumType, LoadType.Run), sqlFileName));
    }

    public string GetPath(Types enumType, LoadType loadType)
    {
        var filePath = "";
        switch (enumType)
        {
            case Types.Procedures:
                filePath = GetLoadTypePath(ProceduresDirectory, loadType);
                break;
            case Types.ServiceBrokers:
                filePath = GetLoadTypePath(ServiceBrokerDirectory, loadType);
                break;
            case Types.Functions:
                filePath = GetLoadTypePath(FunctionsDirectory, loadType);
                break;
            case Types.Triggers:
                filePath = GetLoadTypePath(TriggersDirectory, loadType);
                break;
            case Types.Security:
                filePath = GetLoadTypePath(SecurityDirectory, loadType);
                break;
            case Types.Seeder:
                filePath = GetLoadTypePath(SeederDirectory, loadType);
                break;
            case Types.Views:
                filePath = GetLoadTypePath(ViewsDirectory, loadType);
                break;
            case Types.SpatialIndex:
                filePath = GetLoadTypePath(SpatialIndexDirectory, loadType);
                break;
            case Types.DataCleanup:
                filePath = GetLoadTypePath(DataCleanupDirectory, loadType);
                break;
            default:
                break;
        }

        return filePath;
    }
}

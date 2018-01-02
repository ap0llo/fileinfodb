FileInfoDb
==========
FileInfoDb is a tool to store (arbitrary) meta-information for files.

The meta data is modeled as "properties", simple string key-value pairs which
are stored in a MySQL based database. 
Files are not identified by name of path but using a (SHA256) hash value of the
file's content.


Usage
------

### Configuring the database connection: `configure`
To configure the MySQL database to store properties in, run the `configure` 
command and specify the database uri with a ``fileinfodb-mysql`` scheme.
If the database server requires a username and password, provide this as 
part of the uri, too.

```batch
fileinfodb configure --uri fileinfodb-mysql://USER:PASSWORD@server/DATABASENAME
```

**Note: FileInfoDb will not store username and password in the config file. 
  Credentials are saved using the Windows Credetials Manager**

The configure command will save the uri in ``%APPDATA%\FileInfoDb\config.json``.

Configuring the uri using the `configure` command is optional. Alternatively,
you can pass the uri to use to each of the commands using the ``--uri`
commandline argument.


### Creating database and schema: `init`
Before you can use the  database, the database and schema must be created.
This is achieved using the `init` command. You only need to run this command
once for a database.

```batch
:: you can use the previously configured uri
fileinfodb init

:: you can also specify a database uri directory
fileinfodb init --uri fileinfodb-mysql://USER:PASSWORD@server/DATABASENAME
```


### Adding or modifing properties: `set-property`
To add or modify a property for a file, use the `set-property` command:

```batch
fileinfodb set-property --file FILEPATH --name "property1" --value "test"
```

This will add a property named "property1" with a value of test. By default,
FileInfoDb will not overwrite the existing value if a property with the 
specified name already exists for the file and will display a warning instead.
To replace the existing value, use the `--overwrite` switch.

Instead of `--value` you can also use `--value-from-file` and specify the path
to a file containing the property value. In this case, the file contents will
be used as property value.

*Note: The hash for the file is not computed every time a property is set,
       for details, see [File Hashing](#file-hashing).*


### Retrieving properties: `get-property`
Existing properties can be read using the `get-property` command:

```batch
:: get all properties for the file
fileinfodb get-property --file FILEPARH
```

`get-property` can either get all properties for a file, or filter the
result by property names (with support for wildcards):

```batch
:: only get the property named "property1"
fileinfodb get-property --file FILEPARH --name "property1"

:: you can also use wildcards to filter property names
fileinfodb get-property --file FILEPARH --name "property*"
```

*Note: The hash for the file is not computed every time a property is set,
       for details, see [File Hashing](#file-hashing).*


### List existing properties: `get-propertyname`
Using the `get-propertyname` command, you can list the names of all properties
present in the database (regardless which files they are associated with)

```batch
fileinfodb get-propertyname
```



File Hashing
------------
When setting or retrieving properties for a file, the file is not identified 
using its name but a SHA256 Hash value. Because hashing a file can be expensive 
operation, especially for large files, FileInfoDb caches hash values once they
have been computed in a local SQLite database. 
This database is located at `%APPDATA%\FileInfoDb\hashing.cache.db` and contains
the full path, last write time and size of every file ever used in the
`get-property` or `set-property` commands.
When the hash for a file is required, FileInfoDb will first check the cache and,
if neither last wrtie time nor the file's size differ from the value in the 
database, use the cached value. 

To clear the cache, `hashing.cache.db` can simply be deleted.


Build Status
------------
|**master**|**develop**|
|:--:|:--:|
|[![Build status](https://ci.appveyor.com/api/projects/status/boqw1m7byb96n6xf/branch/master?svg=true)](https://ci.appveyor.com/project/ap0llo/compare-and-copy/branch/master)|[![Build status](https://ci.appveyor.com/api/projects/status/boqw1m7byb96n6xf/branch/develop?svg=true)](https://ci.appveyor.com/project/ap0llo/compare-and-copy/branch/develop)|




Acknowledgments
------------------------------------

FileInfoDb was realized using a number of libraries (besides .NET Framework and
.NET Standard), listed in no particular order:

- [CommandLineParser](https://github.com/commandlineparser/commandline)
- [Meziantou.Framework.Win32.CredentialManager](https://github.com/meziantou/Meziantou.Framework)
- [Microsoft.Extensions.Configuration](https://github.com/aspnet/configuration)
- [Microsoft.Extensions.Logging](https://github.com/aspnet/logging)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)
- [Squirrel](https://github.com/Squirrel/Squirrel.Windows)
- [XUnit](http://xunit.github.io/)
- SQLite, specifically [Microsoft.Data.SQLite](https://www.nuget.org/packages/Microsoft.Data.SQLite/)
- [NodaTime](https://nodatime.org/)
- [Dapper](https://github.com/StackExchange/Dapper)
- [MySqlConnector](https://mysql-net.github.io/MySqlConnector/)
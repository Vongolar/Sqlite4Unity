# English Introduction
## Introduction
This project allows for easy use of SQLite databases in Unity and supports running on various platforms. <br>All methods return results through error codes and do not throw exceptions.

## Installation
add package from git url<br>
git@github.com:Vongolar/Sqlite4Unity.git?path=/Assets/Sqlite4Unity

## Usage Examples
### Insert Data
```
using (var db = new Database(DB_PATH))
{
    db.Open();
    var code = db.ExecWithTransaction(@"INSERT INTO TABLE (Field1, Field2, Field3, ...) VALUES (?,?,?, ...)", 
                                    new dynamic[][] {
                                        new dynamic[]{ Value1, Value2, Value3, ... },
                                        new dynamic[]{ Value1, Value2, Value3, ... },
                                        new dynamic[]{ Value1, Value2, Value3, ... },
                                        ...
                                    });
    if (code != RESULT_CODE.SQLITE_OK)
    {
        UnityEngine.Debug.LogError(code);
    }
}
```
This encapsulated method uses dynamic types. <br>For better performance, you can directly use lower-level methods such as Database.Prepare. <br>For smoother game performance, it's recommended to call this outside the main thread, or to segment the data and write a portion of it each frame.

### Update Data
Similar to adding data, simply modify the SQL statement.

### Read Data
```
using (var db = new Database(DB_PATH))
{
    db.Open();
    var code = db.Query(@"SELECT Field1, Field2, Field3, ... FROM TABLE;", new Database.FieldType[] { FieldType1, FieldType2, FieldType3, ... }, out var res);
    if (code != RESULT_CODE.SQLITE_OK)
    {
        UnityEngine.Debug.LogError(code);
    }
}
```
This method returns results using dynamic types. <br>For better performance, you can use lower-level methods such as Database.Prepare. <br>To ensure smoother game performance, you can call this outside the main thread or use the iterator version of Query.

## Cross-Platform
Since it uses C source code as a Native Plugin, it theoretically supports all platforms. However, note that on WebGL platforms and WebGL-based mini-game platforms, 'Application.persistentDataPath' does not provide persistent storage capabilities, and you will need to handle persistence issues specific to the platform.

<br>
<br>
<br>
<br>
<br>

# 中文介绍
## 介绍
这个项目能够方便的在Unity中使用Sqlite数据库，支持在各种平台上运行。<br>
所有方法均通过错误码返回结果，不会有异常抛出。

## 安装
add package from git url<br>
git@github.com:Vongolar/Sqlite4Unity.git?path=/Assets/Sqlite4Unity
## 使用示例
### 增加数据
```
using (var db = new Database(DB_PATH))
{
    db.Open();
    var code = db.ExecWithTransaction(@"INSERT INTO TABLE (Field1, Field2, Field3, ...) VALUES (?,?,?, ...)", 
                                    new dynamic[][] {
                                        new dynamic[]{ Value1, Value2, Value3, ... },
                                        new dynamic[]{ Value1, Value2, Value3, ... },
                                        new dynamic[]{ Value1, Value2, Value3, ... },
                                        ...
                                    });
    if (code != RESULT_CODE.SQLITE_OK)
    {
        UnityEngine.Debug.LogError(code);
    }
}
```
该封装方法中使用dynamic类型。<br>如果需要更好的性能，可以直接使用 Database.Prepare 等底层方法。<br>
如果希望游戏运行更加流程，建议在主线程以外调用，也可以将数据分段，每帧写入部分数据。

### 修改数据
和增加数据的方法类似，只需要修改SQL语句。

### 查询数据
```
using (var db = new Database(DB_PATH))
{
    db.Open();
    var code = db.Query(@"SELECT Field1, Field2, Field3, ... FROM TABLE;", new Database.FieldType[] { FieldType1, FieldType2, FieldType3, ... }, out var res);
    if (code != RESULT_CODE.SQLITE_OK)
    {
        UnityEngine.Debug.LogError(code);
    }
}
```
该方法返回使用dynamic类型。<br>如果需要更好的性能，可以直接使用 Database.Prepare 等底层方法。<br>
如果希望游戏运行更加流程，可以在主线程以外调用，也可以调用Query的迭代器版本。

## 跨平台
因为直接使用C源码作为Native Plugin，所以理论上可以支持全部平台。但要注意，在WebGL平台以及以WebGL为基础的小游戏平台上，Application.persistentDataPath并不具备持久化能力，需要针对平台自行处理持久化问题。
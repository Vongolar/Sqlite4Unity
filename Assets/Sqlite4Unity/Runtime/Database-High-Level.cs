/*
 * High-level encapsulation of sqlite3 interface
 * by Vongolar
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Vongolar.Sqlite
{
    public partial class Database
    {
        // Execute SQL statements with transaction. Multiple statements can be executed if needed.
        public RESULT_CODE ExecWithTransaction(string sql)
        {
            return ExecWithTransaction(sql, null);
        }

        // Execute SQL statements. Multiple statements can be executed if needed.
        public RESULT_CODE Exec(string sql)
        {
            return Exec(sql, null);
        }

        // Bind data and execute SQL statements with transaction. Multiple statements can be executed if needed.
        // If binding data is not required, pass null for the corresponding bind data.
        // If binding is not needed but multiple executions are required, pass null for row data.
        public RESULT_CODE ExecWithTransaction(string sql, params dynamic[][][] bindDatas)
        {
            var code = BeginTransaction();
            if (code != RESULT_CODE.SQLITE_OK) return code;

            var sw = new Stopwatch();
            sw.Start();

            code = Exec(sql, bindDatas);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                Rollback();
                return code;
            }

            sw.Restart();
            code = Commit();
            if (code != RESULT_CODE.SQLITE_OK)
            {
                Rollback();
                return code;
            }
            return RESULT_CODE.SQLITE_OK;
        }

        // Bind data and execute SQL statements. Multiple statements can be executed if needed.
        // If binding data is not required, pass null for the corresponding bind data.
        // If binding is not needed but multiple executions are required, pass null for row data.
        public RESULT_CODE Exec(string sql, params dynamic[][][] bindDatas)
        {
            var code = Prepare(sql, out List<Statement> stmts);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                return code;
            }

            if ((stmts?.Count ?? 0) < (bindDatas?.Length ?? 0))
            {
                UnityEngine.Debug.LogWarning("[sqlite3] the number of SQL statements is less than the number of data binds !");
            }

            Action clear = () =>
            {
                foreach (var stmt in stmts)
                {
                    stmt.Finalize();
                }
            };

            for (var istmt = 0; istmt < (stmts?.Count ?? 0); istmt++)
            {
                var stmt = stmts[istmt];
                var table = istmt < bindDatas?.Length ? bindDatas[istmt] : null;

                if ((table?.Length ?? 0) == 0)
                {
                    code = stmt.Step();
                    if (code != RESULT_CODE.SQLITE_DONE)
                    {
                        clear();
                        return code;
                    }
                    continue;
                }

                for (var irow = 0; irow < (table?.Length ?? 0); irow++)
                {
                    var row = table[irow];

                    code = stmt.Reset();
                    if (code != RESULT_CODE.SQLITE_OK)
                    {
                        clear();
                        return code;
                    }

                    for (var i = 0; i < (row?.Length ?? 0); i++)
                    {
                        var data = row[i];
                        if (data is int)
                        {
                            code = stmt.Bind(i, (data as int?) ?? 0);
                        }
                        else if (data is double)
                        {
                            code = stmt.Bind(i, (data as double?) ?? 0);
                        }
                        else if (data is long)
                        {
                            code = stmt.Bind(i, (data as long?) ?? 0);
                        }
                        else if (data is byte[])
                        {
                            code = stmt.Bind(i, data as byte[]);
                        }
                        else if (data is string)
                        {
                            code = stmt.Bind(i, data as string);
                        }
                        else if (data is null)
                        {
                            code = stmt.Bind(i);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"[sqlite3] not support binded type !");
                            code = stmt.Bind(i);
                        }
                        if (code != RESULT_CODE.SQLITE_OK)
                        {
                            clear();
                            return code;
                        }
                    }

                    code = stmt.Step();
                    if (code != RESULT_CODE.SQLITE_DONE)
                    {
                        clear();
                        return code;
                    }
                }
            }

            clear();
            return RESULT_CODE.SQLITE_OK;
        }

        public RESULT_CODE BeginTransaction() => Exec(@"BEGIN TRANSACTION;");
        public RESULT_CODE Commit() => Exec(@"COMMIT;");
        public RESULT_CODE Rollback() => Exec(@"ROLLBACK;");

        public RESULT_CODE CreateTable(string tableName, params string[] fieldDes) => Exec($"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(',', fieldDes)});");
        public RESULT_CODE DropTable(string tableName) => Exec($"DROP TABLE IF EXISTS {tableName};");

        public enum FieldType
        {
            INT,
            LONG,
            DOUBLE,
            BLOB,
            TEXT,
        }

        public IEnumerable<(RESULT_CODE, dynamic[])> Query(string sql, FieldType[] types)
        {
            if ((types?.Length ?? 0) == 0) yield break;

            var code = Prepare(sql, out Statement stmt);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                yield return (code, null);
                yield break;
            }

            while ((code = stmt.Step()) == RESULT_CODE.SQLITE_ROW)
            {
                var row = new dynamic[types.Length];
                for (var i = 0; i < types.Length; i++)
                {
                    switch (types[i])
                    {
                        case FieldType.INT:
                            {
                                code = stmt.Get(i, out int value);
                                row[i] = value;
                                if (code != RESULT_CODE.SQLITE_ROW)
                                {
                                    stmt.Finalize();
                                    yield return (code, null);
                                    yield break;
                                }
                            }
                            break;
                        case FieldType.LONG:
                            {
                                code = stmt.Get(i, out long value);
                                row[i] = value;
                                if (code != RESULT_CODE.SQLITE_ROW)
                                {
                                    stmt.Finalize();
                                    yield return (code, null);
                                    yield break;
                                }
                            }
                            break;
                        case FieldType.DOUBLE:
                            {
                                code = stmt.Get(i, out double value);
                                row[i] = value;
                                if (code != RESULT_CODE.SQLITE_ROW)
                                {
                                    stmt.Finalize();
                                    yield return (code, null);
                                    yield break;
                                }
                            }
                            break;
                        case FieldType.BLOB:
                            {
                                code = stmt.Get(i, out byte[] value);
                                row[i] = value;
                                if (code != RESULT_CODE.SQLITE_ROW)
                                {
                                    stmt.Finalize();
                                    yield return (code, null);
                                    yield break;
                                }
                            }
                            break;
                        case FieldType.TEXT:
                            {
                                code = stmt.Get(i, out string value);
                                row[i] = value;
                                if (code != RESULT_CODE.SQLITE_ROW)
                                {
                                    stmt.Finalize();
                                    yield return (code, null);
                                    yield break;
                                }
                            }
                            break;
                    }
                }
                yield return (RESULT_CODE.SQLITE_OK, row);
            }

            stmt.Finalize();
        }

        public RESULT_CODE Query(string sql, FieldType[] types, out List<dynamic[]> res)
        {
            res = new List<dynamic[]>();
            foreach (var (code, row) in Query(sql, types))
            {
                if (code != RESULT_CODE.SQLITE_OK) return code;

                res.Add(row);
            }
            return RESULT_CODE.SQLITE_OK;
        }
    }
}
/*
 * Base-level encapsulation of sqlite3 interface
 * by Vongolar
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Pool;

namespace Sqlite
{
    public partial class Database : IDisposable
    {
        ObjectPool<Statement> stmtPool;
        HashSet<Statement> activing;

        public Database(string dbPath)
        {
            path = dbPath;
            stmtPool = new ObjectPool<Statement>(CreateStmt4Pool, actionOnRelease: OnStmtRelease, actionOnDestroy: OnStmtDestory, actionOnGet: OnStmtGet);
            activing = new HashSet<Statement>();
        }

        Statement CreateStmt4Pool()
        {
            return new Statement(this);
        }

        void OnStmtGet(Statement stmt)
        {
            activing.Add(stmt);
        }

        void OnStmtRelease(Statement stmt)
        {
            activing.Remove(stmt);
            stmt.finalize();
        }

        void OnStmtDestory(Statement stmt)
        {
            activing.Remove(stmt);
            stmt.finalize();
        }

        internal void ReleaseStmt2Pool(Statement stmt)
        {
            stmtPool.Release(stmt);
        }

        public RESULT_CODE lstResultCode => sqlite3_errcode(db);
        public RESULT_CODE lstExtendedResultCode => sqlite3_extended_errcode(db);
        public string lstErrMsg => Marshal.PtrToStringUTF8(sqlite3_errmsg(db));

        public RESULT_CODE Open()
        {
            if (isOpen) return RESULT_CODE.SQLITE_OK;

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            return open();
        }

        public RESULT_CODE Close()
        {
            if (stmtPool?.CountActive > 0)
            {
                UnityEngine.Debug.LogWarning($"[sqlite3] close database but {stmtPool?.CountActive} statements have not been finalized.");

                var tmp = activing?.ToArray(); // can't change collection in foreach loop
                foreach (var stmt in tmp)
                {
                    stmt.Release();
                }
            }

            UnityEngine.Debug.Assert(stmtPool?.CountActive == 0, $"[sqlite3] still {stmtPool?.CountActive} statements aren't finalized after release all!");

            stmtPool?.Clear();
            return close();
        }

        public void Dispose()
        {
            var code = Close();
            stmtPool?.Dispose();
            if (code != RESULT_CODE.SQLITE_OK) throw new Exception($"[sqlite3] database close err: {lstExtendedResultCode}.\n{lstErrMsg}");
        }

        public RESULT_CODE Prepare(string sql, out List<Statement> stmts)
        {
            stmts = new List<Statement>();

            var str = Encoding.UTF8.GetBytes(sql);
            var length = str.Length;
            var tail = IntPtr.Zero;
            var code = RESULT_CODE.SQLITE_OK;

            var handle = GCHandle.Alloc(str, GCHandleType.Pinned);
            try
            {
                var strPtr = handle.AddrOfPinnedObject();
                var cursor = 0L;
                while (length > cursor)
                {
                    code = prepare(tail == IntPtr.Zero ? strPtr : tail, (int)(length - cursor), out var stmtPtr, out tail);
                    if (code != RESULT_CODE.SQLITE_OK) break;
                    if (stmtPtr != IntPtr.Zero)
                    {
                        // When there is blank content, the prepare method will still return OK, but it will not return a statement pointer. In this case, skip it.
                        var stmt = stmtPool.Get();
                        stmts.Add(stmt);
                        stmt.SetStmtPointer(stmtPtr);
                    }
                    cursor = tail.ToInt64() - strPtr.ToInt64();
                    UnityEngine.Debug.Assert(cursor >= 0, $"[sqlite3] tail beyond sql length.");
                    UnityEngine.Debug.Assert(cursor <= int.MaxValue, $"[sqlite3] sql is too long to beyond int max value.");
                }
            }
            finally
            {
                handle.Free();
                if (code != RESULT_CODE.SQLITE_OK)
                {
                    foreach (var stmt in stmts)
                    {
                        stmt.Release();
                    }
                    stmts = null;
                }
            }
            return code;
        }

        public RESULT_CODE Prepare(string sql, out Statement stmt)
        {
            var code = Prepare(sql, out List<Statement> stmts);
            stmt = stmts?.FirstOrDefault();

            if (stmts?.Count > 1)
            {
                UnityEngine.Debug.LogWarning($"[sqlite3] prepare single stmt but not a single sql!");
                for (var i = 1; i < stmts.Count; i++)
                {
                    stmts[i].Release();
                }
            }
            return code;
        }
    }
}
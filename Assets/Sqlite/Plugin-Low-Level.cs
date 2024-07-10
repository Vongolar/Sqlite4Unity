/*
 * Low-level encapsulation of sqlite3 interface
 * by Vongolar
 */
using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace Sqlite
{
    partial class Database
    {
        public string path { private set; get; }
        public bool isOpen => db != IntPtr.Zero;
        IntPtr db;

        RESULT_CODE open()
        {
            if (isOpen) return RESULT_CODE.SQLITE_OK;
            var code = sqlite3_open(path, out db);
            if (code == RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.Assert(db != IntPtr.Zero, $"[sqlite3] open database success but return invalid pointer !\npath:{path}");
                UnityEngine.Debug.Log($"[sqlite3] open database success.\npath:{path}");
            }
            else
            {
                UnityEngine.Debug.Assert(db == IntPtr.Zero, $"[sqlite3] open database fail but return a non-zero pointer !\npath:{path}");
                UnityEngine.Debug.LogError($"[sqlite3] open database fail:{code} !\npath:{path}");
            }
            return code;
        }

        RESULT_CODE close()
        {
            if (!isOpen)
            {
                UnityEngine.Debug.LogWarning($"[sqlite3] close not opened database.");
                return RESULT_CODE.SQLITE_OK;
            }

            var code = sqlite3_close_v2(db);
            if (code == RESULT_CODE.SQLITE_OK)
            {
                db = IntPtr.Zero;
                UnityEngine.Debug.Log($"[sqlite3] close database success.");
            }
            else
            {
                UnityEngine.Debug.LogError($"[sqlite3] close database fail:{code} !\n{lstErrMsg}");
            }
            return code;
        }

        RESULT_CODE prepare(IntPtr sql, int nByte, out IntPtr stmt, out IntPtr tail)
        {
            var code = sqlite3_prepare_v2(db, sql, nByte, out stmt, out tail);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.LogError($"[sqlite3] prepare sql err: {code}\n{lstErrMsg}\n");
            }
            return code;
        }

        internal RESULT_CODE step(IntPtr stmt)
        {
            var code = sqlite3_step(stmt);
            if (code != RESULT_CODE.SQLITE_OK && code != RESULT_CODE.SQLITE_DONE && code != RESULT_CODE.SQLITE_ROW)
            {
                UnityEngine.Debug.LogError($"[sqlite3] step err: {code}\n{lstErrMsg}");
            }
            return code;
        }

        internal RESULT_CODE reset(IntPtr stmt)
        {
            var code = sqlite3_reset(stmt);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.LogError($"[sqlite3] stmt reset err: {code}\n{lstErrMsg}");
            }
            return code;
        }

        internal RESULT_CODE clearBindings(IntPtr stmt)
        {
            var code = sqlite3_clear_bindings(stmt);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.LogError($"[sqlite3] stmt clear binds err: {code}\n{lstErrMsg}");
            }
            return code;
        }

        internal RESULT_CODE finalize(IntPtr stmt)
        {
            var code = sqlite3_finalize(stmt);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.LogError($"[sqlite3] stmt finalize err: {code}\n{lstErrMsg}");
            }
            return code;
        }

        internal RESULT_CODE bindInt(IntPtr stmt, int index, int value)
        {
            var code = sqlite3_bind_int(stmt, index, value);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.LogError($"[sqlite3] stmt bind int err: {code}\n{lstErrMsg}");
            }
            return code;
        }

        internal int getInt(IntPtr stmt, int index)
        {
            return sqlite3_column_int(stmt, index);
        }

        internal RESULT_CODE bindDouble(IntPtr stmt, int index, double value)
        {
            var code = sqlite3_bind_double(stmt, index, value);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.LogError($"[sqlite3] stmt bind double err: {code}\n{lstErrMsg}");
            }
            return code;
        }

        internal double getDouble(IntPtr stmt, int index)
        {
            return sqlite3_column_double(stmt, index);
        }

        internal RESULT_CODE bindLong(IntPtr stmt, int index, long value)
        {
            var code = sqlite3_bind_int64(stmt, index, value);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.LogError($"[sqlite3] stmt long int err: {code}\n{lstErrMsg}");
            }
            return code;
        }

        internal long getLong(IntPtr stmt, int index)
        {
            return sqlite3_column_int64(stmt, index);
        }

        [MonoPInvokeCallback(typeof(Action<IntPtr>))]
        static void DelCallback(IntPtr ptr) => Marshal.FreeHGlobal(ptr);

        internal RESULT_CODE bindBlob(IntPtr stmt, int index, byte[] value)
        {
            var len = value?.Length ?? 0;
            var pointer = Marshal.AllocHGlobal(len);
            Marshal.Copy(value, 0, pointer, len);
            var code = sqlite3_bind_blob(stmt, index, pointer, len, DelCallback);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.LogError($"[sqlite3] stmt bind blob err: {code}\n{lstErrMsg}");
            }
            return code;
        }

        internal byte[] getBlob(IntPtr stmt, int index)
        {
            var pointer = sqlite3_column_blob(stmt, index);
            var len = sqlite3_column_bytes(stmt, index);
            var res = new byte[len];
            Marshal.Copy(pointer, res, 0, len);
            return res;
        }

        internal RESULT_CODE bindText(IntPtr stmt, int index, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var len = bytes.Length;
            var pointer = Marshal.AllocHGlobal(len);
            Marshal.Copy(bytes, 0, pointer, len);
            var code = sqlite3_bind_text(stmt, index, pointer, len, DelCallback);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.LogError($"[sqlite3] stmt bind text err: {code}\n{lstErrMsg}");
            }
            return code;
        }

        internal string getText(IntPtr stmt, int index)
        {
            var pointer = sqlite3_column_text(stmt, index);
            var len = sqlite3_column_bytes(stmt, index);
            var res = new byte[len];
            Marshal.Copy(pointer, res, 0, len);
            return Encoding.UTF8.GetString(res);
        }

        internal RESULT_CODE bindNull(IntPtr stmt, int index)
        {
            var code = sqlite3_bind_null(stmt, index);
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.LogError($"[sqlite3] stmt bind null err: {code}\n{lstErrMsg}");
            }
            return code;
        }
    }
}
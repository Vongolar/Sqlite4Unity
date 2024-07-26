/*
 * declare essential methods in sqlite3
 * by Vongolar
 */
using System;
using System.Runtime.InteropServices;

namespace Vongolar.Sqlite
{
        partial class Database
        {
#if UNITY_EDITOR
                const string DllName = "sqlite3.dll";
#else
                const string DllName = "__Internal";
#endif
                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_open(string filename, out IntPtr db);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_close_v2(IntPtr db);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_errcode(IntPtr db);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_extended_errcode(IntPtr db);

                [DllImport(DllName)]
                extern static IntPtr sqlite3_errmsg(IntPtr db);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_prepare_v2(IntPtr db, IntPtr sql, int nByte, out IntPtr stmt, out IntPtr pzTail);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_step(IntPtr stmt);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_reset(IntPtr stmt);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_clear_bindings(IntPtr stmt);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_finalize(IntPtr stmt);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_bind_int(IntPtr stmt, int index, int value);

                [DllImport(DllName)]
                extern static int sqlite3_column_int(IntPtr stmt, int index);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_bind_double(IntPtr stmt, int index, double value);

                [DllImport(DllName)]
                extern static double sqlite3_column_double(IntPtr stmt, int index);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_bind_int64(IntPtr stmt, int index, long value);

                [DllImport(DllName)]
                extern static long sqlite3_column_int64(IntPtr stmt, int index);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_bind_null(IntPtr stmt, int index);

                [DllImport(DllName)]
                extern static int sqlite3_column_bytes(IntPtr stmt, int index);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_bind_blob(IntPtr stmt, int index, IntPtr value, int n, Action<IntPtr> del);
                [DllImport(DllName)]
                extern static IntPtr sqlite3_column_blob(IntPtr stmt, int index);

                [DllImport(DllName)]
                extern static RESULT_CODE sqlite3_bind_text(IntPtr stmt, int index, IntPtr value, int n, Action<IntPtr> del);

                [DllImport(DllName)]
                extern static IntPtr sqlite3_column_text(IntPtr stmt, int index);
        }
}
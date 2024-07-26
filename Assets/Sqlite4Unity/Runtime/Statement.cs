/*
 * the file contains encapsulation methods for Statement.
 * by Vongolar
 */
using System;

namespace Vongolar.Sqlite
{
    public class Statement
    {
        Database db;
        IntPtr ptr;

        internal Statement(Database db)
        {
            this.db = db;
        }

        internal void SetStmtPointer(IntPtr ptr)
        {
            if (this.ptr != IntPtr.Zero)
            {
                UnityEngine.Debug.LogError("[sqlite3] set stmt pointer but has not been finalized!");
                db.finalize(this.ptr);
            }
            this.ptr = ptr;
        }

        public RESULT_CODE Reset()
        {
            return db.reset(ptr);
        }

        public RESULT_CODE ClearBindings()
        {
            return db.clearBindings(ptr);
        }

        public RESULT_CODE Step()
        {
            return db.step(ptr);
        }

#pragma warning disable CS0465 // Introducing a 'Finalize' method can interfere with destructor invocation
        public void Finalize()
#pragma warning restore CS0465 // Introducing a 'Finalize' method can interfere with destructor invocation
        {
            db.ReleaseStmt2Pool(this);
        }

        internal RESULT_CODE finalize()
        {
            if (ptr == IntPtr.Zero) return RESULT_CODE.SQLITE_OK;
            RESULT_CODE code = RESULT_CODE.SQLITE_OK;
            try
            {
                code = db.finalize(ptr);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }

            ptr = IntPtr.Zero;
            return code;
        }

        // index is begin with zero
        public RESULT_CODE Bind(int index, int value)
        {
            return db.bindInt(ptr, index + 1, value);
        }

        public RESULT_CODE Get(int index, out int value)
        {
            value = db.getInt(ptr, index);
            return db.lstResultCode;
        }

        // index is begin with zero
        public RESULT_CODE Bind(int index, double value)
        {
            return db.bindDouble(ptr, index + 1, value);
        }

        public RESULT_CODE Get(int index, out double value)
        {
            value = db.getDouble(ptr, index);
            return db.lstResultCode;
        }

        // index is begin with zero
        public RESULT_CODE Bind(int index, long value)
        {
            return db.bindLong(ptr, index + 1, value);
        }

        public RESULT_CODE Get(int index, out long value)
        {
            value = db.getLong(ptr, index);
            return db.lstResultCode;
        }

        // index is begin with zero
        public RESULT_CODE Bind(int index, byte[] value)
        {
            if (value == null) return db.bindNull(ptr, index + 1);
            return db.bindBlob(ptr, index + 1, value);
        }

        public RESULT_CODE Get(int index, out byte[] value)
        {
            value = db.getBlob(ptr, index);
            return db.lstResultCode;
        }

        // index is begin with zero
        public RESULT_CODE Bind(int index, string value)
        {
            if (value == null) return db.bindNull(ptr, index + 1);
            return db.bindText(ptr, index + 1, value);
        }

        public RESULT_CODE Get(int index, out string value)
        {
            value = db.getText(ptr, index);
            return db.lstResultCode;
        }

        // index is begin with zero
        public RESULT_CODE Bind(int index)
        {
            return db.bindNull(ptr, index + 1);
        }
    }
}
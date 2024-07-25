/*
 * This file is used to detect memory leaks and cover common use cases.
 * by Vongolar
 */
using System.IO;
using Sqlite;
using UnityEngine;
using System.Text;
using UnityEngine.UI;
using System.Collections;
using System;

public class SqliteSample : MonoBehaviour
{
    [SerializeField]
    Button btnStart;

    [SerializeField]
    Button btnEnd;

    Coroutine cur;
    Database db;
    void Start()
    {
        btnStart.onClick.AddListener(StartTest);
        btnEnd.onClick.AddListener(() =>
        {
            if (cur != null)
            {
                StopCoroutine(cur);
                db?.Dispose();
                cur = null;
            }
        });
    }

    void OnApplicationQuit()
    {
        db?.Dispose();
    }

    void StartTest()
    {
        if (cur != null) return;

        cur = StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        var path = Path.Combine(Application.persistentDataPath, "sample.db");
        var loopCount = 0;
        while (true)
        {
            using (db = new Database(path))
            {
                db.Open();
                yield return null;
                yield return CURD();
            }
            db = null;
            Debug.Log($"Loop {++loopCount}");
        }
    }

    IEnumerator CURD()
    {
        yield return Create();
        yield return Read();
        yield return UpdateTable();
        yield return Read2();
    }

    IEnumerator Create()
    {
        var code = db.DropTable("table1");
        if (code != RESULT_CODE.SQLITE_OK) throw new Exception($"{db.lstExtendedResultCode}\n{db.lstErrMsg}");

        code = db.CreateTable("table1", "ID INTEGER PRIMARY KEY", "Name TEXT NO NULL", " HP INTEGER", "SEX REAL", " DES BLOB");
        if (code != RESULT_CODE.SQLITE_OK) throw new Exception($"{db.lstExtendedResultCode}\n{db.lstErrMsg}");

        yield return null;

        var count = 10000;
        var batch = 100;
        var index = 0;
        for (var b = 0; b < batch; b++)
        {
            var data = new dynamic[count][];
            for (var i = 0; i < count; i++, index++)
            {
                data[i] = new dynamic[] { index + 1, $"No. {index + 1}", long.MaxValue, double.MaxValue, UTF8Encoding.UTF8.GetBytes($"This is No. {index + 1}.") };
            }
            code = db.ExecWithTransaction("INSERT INTO table1 (ID, Name, HP, SEX, DES) VALUES (?,?,?,?,?);", data);
            if (code != RESULT_CODE.SQLITE_OK) throw new Exception($"{db.lstExtendedResultCode}\n{db.lstErrMsg}");
            yield return null;
        }
    }

    IEnumerator Read()
    {
        var sql = @"SELECT ID, Name, HP, SEX, DES FROM table1;";
        var count = 0;
        foreach (var (code, row) in db.Query(sql, new Database.FieldType[] { Database.FieldType.INT, Database.FieldType.TEXT, Database.FieldType.LONG, Database.FieldType.DOUBLE, Database.FieldType.BLOB }))
        {
            if (code != RESULT_CODE.SQLITE_OK) throw new Exception($"{db.lstExtendedResultCode}\n{db.lstErrMsg}");
            count++;

            var id = (row[0] as int?) ?? -1;
            if (id != count) throw new Exception($"the {count}th id {id} != {count}");

            var name = row[1] as string;
            if (name != $"No. {count}") throw new Exception($"the {count}th name {name} != No. {count}");

            var hp = (row[2] as long?) ?? 0;
            if (hp != long.MaxValue) throw new Exception($"the {count}th hp {hp} != {long.MaxValue}");

            var sex = (row[3] as double?) ?? 0;
            if (sex != double.MaxValue) throw new Exception($"the {count}th sex {sex} != {double.MaxValue}");

            var des = row[4] as byte[];
            if (!isSameBytes(des, UTF8Encoding.UTF8.GetBytes($"This is No. {count}."))) throw new Exception($"the {count}th desciption does not match");

            if (count % 5000 == 0) yield return null;
        }
        if (count != 10000 * 100) throw new Exception($"only read {count} row");
    }

    IEnumerator UpdateTable()
    {
        var code = db.Exec("UPDATE table1 SET Name = ?, HP = ?, SEX = ?, DES = ? WHERE ID % 2 = 0;", new dynamic[][]{
            new dynamic[]{"No Name", long.MinValue, double.MinValue, null}
        });
        if (code != RESULT_CODE.SQLITE_OK) throw new Exception($"{db.lstExtendedResultCode}\n{db.lstErrMsg}");
        yield return null;
    }

    IEnumerator Read2()
    {
        var sql = @"SELECT ID, Name, SEX, DES FROM table1;";
        var count = 0;
        foreach (var (code, row) in db.Query(sql, new Database.FieldType[] { Database.FieldType.INT, Database.FieldType.TEXT, Database.FieldType.DOUBLE, Database.FieldType.BLOB }))
        {
            if (code != RESULT_CODE.SQLITE_OK) throw new Exception($"{db.lstExtendedResultCode}\n{db.lstErrMsg}");
            count++;

            var id = (row[0] as int?) ?? -1;
            if (id != count) throw new Exception($"the {count}th id {id} != {count}");

            var name = row[1] as string;
            if (id % 2 == 0)
            {
                if (name != "No Name") throw new Exception($"the {count}th name {name} != No Name");
            }
            else
            {
                if (name != $"No. {count}") throw new Exception($"the {count}th name {name} != No. {count}");
            }

            var sex = (row[2] as double?) ?? 0;
            if (id % 2 == 0)
            {
                if (sex != double.MinValue) throw new Exception($"the {count}th sex {sex} != {double.MinValue}");
            }
            else
            {
                if (sex != double.MaxValue) throw new Exception($"the {count}th sex {sex} != {double.MaxValue}");
            }

            var des = row[3] as byte[];
            if (id % 2 == 0)
            {
                if ((des?.Length ?? 0) != 0) throw new Exception($"the {count}th desciption is no null");
            }
            else
            {
                if (!isSameBytes(des, UTF8Encoding.UTF8.GetBytes($"This is No. {count}."))) throw new Exception($"the {count}th desciption does not match");
            }

            if (count % 5000 == 0) yield return null;
        }
        if (count != 10000 * 100) throw new Exception($"only read {count} row");
    }

    bool isSameBytes(byte[] a, byte[] b)
    {
        if (a?.Length != b?.Length) return false;
        for (var i = 0; i < a?.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

}

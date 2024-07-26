using System.IO;
using System.Text;
using NUnit.Framework;
using Vongolar.Sqlite;
using UnityEngine;

public class SqliteTest
{
    [Test]
    public void Create()
    {
        using (var db = new Database(Path.Combine(Application.persistentDataPath, "test.db")))
        {
            db.Open();
            db.DropTable("pokemon");
            db.CreateTable("pokemon", "ID INTEGER PRIMARY KEY", "Name TEXT NO NULL", " HP INTEGER", "SEX REAL", " DES BLOB");
            db.DropTable("card");
            db.CreateTable("card", "ID INTEGER PRIMARY KEY AUTOINCREMENT", "Name TEXT NO NULL");

            var sql = new StringBuilder();
            sql.AppendLine(@"INSERT INTO pokemon (ID, Name, HP, SEX, DES) VALUES (?,?,?,?,?);");
            sql.AppendLine(@"INSERT INTO card (Name) VALUES ('妙蛙种子');");

            var count = 100000;
            var pokemonData = new dynamic[count][];
            for (var i = 1; i <= count; i++)
            {
                pokemonData[i - 1] = new dynamic[] { i, $"Pokemon No. {i} 妙蛙种子", long.MaxValue, double.MaxValue, UTF8Encoding.UTF8.GetBytes($"This is No. {i} 妙蛙种子.") };
            }

            var code = db.ExecWithTransaction(sql.ToString(), pokemonData, new dynamic[][] { null, null, null, null, null });
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.Log(code);
            }
        }
    }

    [Test]
    public void Delete()
    {
        Create();
        using (var db = new Database(Path.Combine(Application.persistentDataPath, "test.db")))
        {
            db.Open();
            var code = db.Exec("DELETE FROM pokemon WHERE ID % 2 = 0;");
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.Log(code);
            }
            code = db.Exec("DELETE FROM card WHERE ID % 2 != 0;");
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.Log(code);
            }
        }
    }

    [Test]
    public void Upate()
    {
        Create();
        using (var db = new Database(Path.Combine(Application.persistentDataPath, "test.db")))
        {
            db.Open();
            var sql = new StringBuilder();
            sql.AppendLine(@"UPDATE pokemon SET Name = ?, HP = ?, SEX = ?, DES = ? WHERE ID % 2 = 0;");
            sql.AppendLine(@"UPDATE card SET Name = ? WHERE ID % 2 != 0;");
            var code = db.ExecWithTransaction(sql.ToString(), new dynamic[][] {
                new dynamic[]{"小火龙",long.MinValue,double.MinValue, null}
             }, new dynamic[][] {
                new dynamic[]{"水箭龟"}
             });
            if (code != RESULT_CODE.SQLITE_OK)
            {
                UnityEngine.Debug.Log(code);
            }
        }
    }

    [Test]
    public void Read()
    {
        Upate();
        using (var db = new Database(Path.Combine(Application.persistentDataPath, "test.db")))
        {
            db.Open();
            var code = db.Query(@"SELECT ID, HP, SEX, DES FROM pokemon;", new Database.FieldType[] { Database.FieldType.INT, Database.FieldType.LONG, Database.FieldType.DOUBLE, Database.FieldType.BLOB }, out var res);
            UnityEngine.Debug.Assert(code == RESULT_CODE.SQLITE_OK);

            UnityEngine.Debug.Assert(res.Count == 100000);

            for (var i = 0; i < res.Count; i++)
            {
                var id = (res[i][0] as int?) ?? -1;
                UnityEngine.Debug.Assert(id == i + 1);
                var hp = (res[i][1] as long?) ?? 0;
                UnityEngine.Debug.Assert(hp == (i % 2 == 0 ? long.MaxValue : long.MinValue));
                var sex = (res[i][2] as double?) ?? 0;
                UnityEngine.Debug.Assert(sex == (i % 2 == 0 ? double.MaxValue : double.MinValue));
                var des = res[i][3] as byte[];
                if (i % 2 == 0)
                {
                    UnityEngine.Debug.Assert(MatchBytes(des, UTF8Encoding.UTF8.GetBytes($"This is No. {id} 妙蛙种子.")));
                }
                else
                {
                    UnityEngine.Debug.Assert(des.Length == 0);
                }
            }
        }
    }

    bool MatchBytes(byte[] a, byte[] b)
    {
        if (a?.Length != b?.Length) return false;

        for (var i = 0; i < a?.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

}
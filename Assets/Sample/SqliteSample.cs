// using System.IO;
// using Sqlite;
// using UnityEngine;
// using System.Text;
// using System.Threading;
// using UnityEngine.UI;
// using System;

// public class SqliteSample : MonoBehaviour
// {
//     [SerializeField]
//     Button btnStart;

//     [SerializeField]
//     Button btnEnd;

//     long running = 0;
//     void Start()
//     {
//         var path = Path.Combine(Application.persistentDataPath, "test.db");
//         btnStart.onClick.AddListener(() =>
//         {
//             if (Interlocked.CompareExchange(ref running, 1, 0) == 0)
//             {
//                 ThreadPool.QueueUserWorkItem((_) =>
//                 {
//                     // while (Interlocked.Read(ref running) == 1)
//                     // {
//                         TestOpenDatabase(path);
//                     // }
//                 });
//             }

//         });

//         btnEnd.onClick.AddListener(() =>
//         {
//             Interlocked.Exchange(ref running, 0);
//         });
//     }

//     void OnApplicationQuit()
//     {
//         Interlocked.Exchange(ref running, 0);
//     }

//     void TestOpenDatabase(string path)
//     {
//         var count = 100000;//new System.Random().Next(0, 100000);
//         using (var db = new Database(path))
//         {
//             var code = db.Open();
//             // while (true)
//             while (Interlocked.Read(ref running) == 1)
//             // #endif
//             {
//                 var errStr = string.Empty;
//                 if (code != RESULT_CODE.SQLITE_OK) return;

//                 code = db.Excute(@"DROP TABLE IF EXISTS test;CREATE TABLE IF NOT EXISTS test (ID INTEGER PRIMARY KEY,Name TEXT NO NULL,ATK INTEGER,DEF REAL,DES BLOB);");
//                 if (code != RESULT_CODE.SQLITE_OK) return;
//                 // 增
//                 {
//                     code = db.BeginTransaction();
//                     if (code != RESULT_CODE.SQLITE_OK) return;

//                     errStr = db.lstErrMsg;

//                     code = db.Prepare(@"INSERT INTO test (ID, Name, ATK,DEF ,DES) VALUES (?,?,?,?,?);", out Statement stmt);
//                     if (code != RESULT_CODE.SQLITE_OK)
//                     {
//                         db.Rollback();
//                         return;
//                     }
//                     errStr = db.lstErrMsg;
//                     code = stmt.Exec(count, (s, i) =>
//                         {
//                             var bindCode = s.Bind(1, i);
//                             if (bindCode != RESULT_CODE.SQLITE_OK) return bindCode;

//                             bindCode = s.Bind(2, $"This card Name is No.  编号. {i}");
//                             if (bindCode != RESULT_CODE.SQLITE_OK) return bindCode;

//                             bindCode = s.Bind(3, long.MaxValue - i);
//                             if (bindCode != RESULT_CODE.SQLITE_OK) return bindCode;

//                             bindCode = s.Bind(4, i + 0.0001);
//                             if (bindCode != RESULT_CODE.SQLITE_OK) return bindCode;

//                             var str = $"This card Name is No. {i}";
//                             var bytes = ASCIIEncoding.ASCII.GetBytes(str);
//                             return s.Bind(5, bytes);
//                         });

//                     stmt.Release();
//                     if (code != RESULT_CODE.SQLITE_OK)
//                     {
//                         db.Rollback();
//                         return;
//                     }
//                     errStr = db.lstErrMsg;
//                     code = db.Commit();
//                     if (code != RESULT_CODE.SQLITE_OK)
//                     {
//                         db.Rollback();
//                         return;
//                     }
//                     errStr = db.lstErrMsg;
//                 }

//                 // 查
//                 {
//                     code = db.Prepare(@"SELECT ID, Name, ATK, DEF, DES FROM test;", out Statement stmt);
//                     if (code != RESULT_CODE.SQLITE_OK) return;
//                     var index = 0;
//                     code = stmt.Query((stmt) =>
//                     {
//                         stmt.Get(0, out int id);
//                         UnityEngine.Debug.Assert(id == index);

//                         stmt.Get(1, out string name);
//                         UnityEngine.Debug.Assert(name == $"This card Name is No.  编号. {id}");

//                         stmt.Get(2, out long atk);
//                         UnityEngine.Debug.Assert(atk == long.MaxValue - id);

//                         stmt.Get(3, out double def);
//                         UnityEngine.Debug.Assert(def == id + 0.0001);

//                         stmt.Get(4, out byte[] des);
//                         var str = $"This card Name is No. {id}";
//                         var bytes = ASCIIEncoding.ASCII.GetBytes(str);

//                         UnityEngine.Debug.Assert(isSameBytes(bytes, des));

//                         index++;
//                     });
//                     stmt.Release();
//                     if (code != RESULT_CODE.SQLITE_OK) return;
//                     errStr = db.lstErrMsg;
//                 }

//                 // 删
//                 {
//                     code = db.Excute(@"DELETE FROM test WHERE ID % 2 = 0;");
//                     if (code != RESULT_CODE.SQLITE_OK) return;
//                     errStr = db.lstErrMsg;
//                 }

//                 // 查有无偶数
//                 {
//                     code = db.Prepare(@"SELECT COUNT(*) FROM test WHERE ID % 2 = 0;", out Statement stmt);
//                     if (code != RESULT_CODE.SQLITE_OK) return;
//                     var index = 0;
//                     code = stmt.Query((stmt) =>
//                     {
//                         stmt.Get(0, out int count);
//                         UnityEngine.Debug.Assert(count == 0);

//                         index++;
//                     });
//                     stmt.Release();
//                     if (code != RESULT_CODE.SQLITE_OK) return;
//                     UnityEngine.Debug.Assert(index == 1);
//                     errStr = db.lstErrMsg;
//                 }

//             }
//         }
//     }

//     bool isSameBytes(byte[] a, byte[] b)
//     {
//         if (a?.Length != b?.Length) return false;
//         for (var i = 0; i < a?.Length; i++)
//         {
//             if (a[i] != b[i]) return false;
//         }
//         return true;
//     }

// }

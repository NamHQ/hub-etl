//using Etl.Core.ResultHandlers;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Threading;

//namespace Etl.ConsoleApp
//{
//    public class CD028CompareResultHandler : ResultHandler
//    {
//        static readonly string[] _fields = new string[]
//        {
//                "RecordID",
//                "FileSource",
//                "ReportDate",
//                "MerchantNumber",
//                "TerminalNumber",
//                "BatchNumber",
//                "CardType",
//                "BinNumber",
//                "LastFourAccountNumber",
//                "AccountNumber",
//                "ExpirationDate",
//                "TransactionCode",
//                "TransactionDate",
//                "TransactionTime",
//                "TransactionAmount",
//                "AuthNumber",
//                "ReferenceNumber",          // Old: case "DBT/DTL ADNDA" save without trans_id
//                "InvoiceNumber",
//                "StoreNumber",
//                "CustomerCode",
//                "DebitNetworkID",
//                "ErrorCode",
//                "AuthResponse",
//                "TerminalType",
//                "PointOfSaleTerminalNumber",
//                "AVSRequest",
//                "AVSResult",
//                "MOTOIndicator",
//                "ReversalFlag",
//                "MarketData",
//                "TransactionMCC",         // Old: Parse wrong position 
//                "InterchangeRate",
//                "SubmittedInterchange",
//                "CashBackAmount",
//                "PointOfSale",
//                "Keyed",                  // Old: Forget to reset keyed="" every trans
//                "TransactionStatus",
//                "EvenAmountIndicator",
//                "ForeignCardIndicator",
//                "DuplicateAmountIndicator",
//                "CardCreatorIndicator",
//                "MatchReturnIndicator",
//                "Rejected",            // Column01
//                "TrueDepositType",     // Column02
//                "AuthInfo",            // Column03
//                "CustomerIdMethod",    // Column04
//                "AuthAmount",          // Column05
//                "TransactionId",        // Column06
//                "Cps",                 // Column07
//                "Column08",
//                "Column09",
//                "Column10",
//                "Column11",
//                "Column12",
//                "Column13",
//                "Column14",
//                "Column15",
//                "SystemNumber",
//                "PrinNumber",
//                "AuthAdndaA",
//                "AuthAdndaC",
//                "AuthAdndaD",
//                "AuthAdndaF",
//                "Mktsp1AdndaE",
//                "DbtdtlAdndaK",
//                "DbtdtlAdndaL",
//                "DbtdtlAdndaP",
//                "FirstTwelveAccountNumber",
//                "First4AccountNumber",
//                "First5AccountNumber",
//                "First6AccountNumber",
//                "First7AccountNumber",
//                "First8AccountNumber",
//                "First9AccountNumber",
//                "First10AccountNumber",
//                "First11AccountNumber",
//                "First12AccountNumber",
//                "HashedFirst12AccountNumber",
//                "HashedAccountNumber",
//                "ChipAdndA1",         // Old: when happening comments inside trans scope (IsContinueTransaction=true) then case "CPS POS ADNDA" will exit trans scope (not reach "CHIP ADNDA 1" which after  "CPS POS ADNDA")
//                "ChipAdndA2",
//                "ChipAdndA3",
//                "ChipAdndA4",
//                "ChipAdndA5",
//                "ChipAdndA6",
//                "CardInputCapability",
//                "CardInputMode",
//                "EMVChipCondition"
//        };
//        static readonly HashSet<string> _ignoredFields = new()
//        {
//            //"ReferenceNumber",          // Old: case "DBT/DTL ADNDA" save without trans_id
//            //"TransactionMCC",           // Old: Parse wrong position 
//            //"Keyed",                    // Old: Forget to reset keyed="" every trans
//            //"HashedFirst12AccountNumber",
//            //"HashedAccountNumber",
//            //"AccountNumber"
//        };

//        private int _startIndex = 0;
//        private int _ignored = 0;
//        private string _lastBatchNumber = "";
//        private int _recordId = 0;
//        private readonly Lazy<List<string[]>> _oldResults;

//        public CD028CompareResultHandler(string filePath)
//        {
//            _oldResults = new Lazy<List<string[]>>(() => GetComparedResults(filePath));
//        }

//        public void Execute(ParseResult result)
//        {
//            var newBatch = new List<IDictionary<string, object>>(result.Batch.Count);

//            foreach (var record in result.Batch)
//            {
//                var batchNumber = record.TryGetValue("BatchNumber", out object raw) ? raw as string : null;
//                if (_lastBatchNumber != batchNumber)
//                {
//                    _lastBatchNumber = batchNumber;
//                    _recordId = 0;
//                }

//                record["RecordID"] = ++_recordId;
//                record["FileSource"] = "CD028";

//                if (record.ContainsKey("Ignored") || !record.ContainsKey("AccountNumber"))
//                    _ignored++;
//                else
//                    newBatch.Add(record);
//            }

//            CompareOldResults(_startIndex, newBatch);
//            _startIndex += newBatch.Count;

//            var t = DateTime.Now.Subtract(result.StartAt);
//            Console.WriteLine($"Total: {result.TotalRecords}, Errors: {result.TotalErrors}, Ignored: {_ignored}, Spend: {t.Minutes}:{t.Seconds}s, Batch {result.Batch.Count}, thread:{Thread.CurrentThread.ManagedThreadId}");
//        }

//        private void CompareOldResults(int startIndex, List<IDictionary<string, object>> batch)
//        {
//            for (var i = 0; i < batch.Count; i++)
//            {
//                var record = batch[i];
//                var values = GetValues(record, _fields);
//                var sb = new StringBuilder();
//                var recordIndex = i + startIndex;

//                for (var j = 0; j < _fields.Length; j++)
//                    if (!_ignoredFields.Contains(_fields[j]))
//                    {
//                        if (values[j] != _oldResults.Value[recordIndex][j].Replace("\0", "").Trim())
//                            sb.AppendLine($"    {_fields[j]}:[{values[j]}] [{_oldResults.Value[recordIndex][j]}]");
//                    }

//                if (sb.Length > 0)
//                {
//                    Console.WriteLine($"{record["RecordID"]}({record["AccountNumber"]})");
//                    Console.WriteLine(sb.ToString());
//                }
//            }
//        }

//        private static List<string[]> GetComparedResults(string filePath)
//        {
//            var lines = new List<string[]>();
//            using var stream = new StreamReader(filePath);

//            stream.ReadLine(); //header
//            while (!stream.EndOfStream)
//                lines.Add(stream.ReadLine().Split("|"));

//            return lines;
//        }

//        private static string[] GetValues(IDictionary<string, object> record, string[] fields)
//        {
//            var values = new string[fields.Length];

//            for (var i = 0; i < fields.Length; i++)
//                values[i] = record.ContainsKey(fields[i]) ? record[fields[i]].ToString() : string.Empty;

//            return values;
//        }
//    }
//}

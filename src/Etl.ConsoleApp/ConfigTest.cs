using Etl.Core.Extraction;
using Etl.Core.Transformation.Fields;
using Etl.Core;
using Etl.Storage;
using Etl.Core.Load;

namespace Etl.ConsoleApp
{
    public static class ConfigTest
    {
        //public static Core.Etl CreateCD028()
        //{
        //    return new Core.Etl
        //    {
        //        ScanBatch = 5,
        //        FlushBatch = 10 * 1000,

        //        Extraction = {

        //            Comments = {
        //                new Layout { Start=@"\A1CD-028", MaxStart=1, Children={
        //                    new Layout { Start=@"\A-MRCH", MaxStart=7, Children={
        //                        new Layout { StartOffset=2 },
        //                        new Layout { Start=@"\A.{21}\d\d\/\d\d\/\d\d\s", MaxStart=1, EndOffset=-2}
        //                    } }
        //                } }
        //            },

        //            Layout = new Layout
        //            {
        //                Start = @"\A^1CD-028.+PAGE\s{6}1$",
        //                End = @"\A^1CD-028.+PAGE\s{6}1$",
        //                Children = {

        //                    new Layout { Direction = LayoutDirection.Column, Children = {
        //                        new Layout { StartOffset = 10, EndOffset = 4, DataField = "SystemNumber" },
        //                        new Layout { StartOffset = 1, EndOffset = 4, DataField = "PrinNumber" },
        //                        new Layout { StartOffset = 90, EndOffset = 8, DataField = "ReportDate" }
        //                    } },

        //                    new Layout { StartOffset = 2, Direction = LayoutDirection.Column, Children = {
        //                        new Layout { StartOffset = 59, EndOffset = 3, DataField = "CustomerCode" },
        //                    } },

        //                    new Layout { End=@"\A0END OF BATCH", EndOffset=1, Repeat=true, DataField="BATCHES", Children={
        //                        new Layout { Start = @"\A(-MRCH =)|(.{21}\d\d\/\d\d\/\d\d\s)", Children={
        //                            new Layout { Start = @"\A(-MRCH =)|(.{21}\d\d\/\d\d\/\d\d\s)", End = @"\A-MRCH =", Repeat = true, DataField = "MERCHANTS", Children = {
        //                                new Layout { Start=@"\A-MRCH =", MaxStart=1, Direction = LayoutDirection.Column, Children = {
        //                                    new Layout { StartOffset = 8, EndOffset = 16, DataField = "MerchantNumber" },
        //                                } },
        //                                new Layout { StartOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                    new Layout { Start = "TRUE DEPOSIT", StartOffset = 19, EndOffset = 2, DataField = "TrueDepositType" },
        //                                } },

        //                                new Layout { Start = @"\A.{21}\d\d\/\d\d\/\d\d\s", End = @"^.{21}\d\d\/\d\d\/\d\d\s", Repeat = true, DataField = "TRANSACTIONS", Children = {
        //                                    new Layout { Start = @"\A.{89}\d\d\/\d\d\/\d\d\s", EndOffset=1, MaxStart=1, Direction=LayoutDirection.Column, Children= {
        //                                        new Layout { EndOffset=68, Direction=LayoutDirection.Column, Repeat= true, DataField="DOUBLE TRANSACTIONS", Children= {
        //                                            new Layout { EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 1, EndOffset = 16, DataField = "AccountNumber" },
        //                                            new Layout { StartOffset = 4, EndOffset = 8, DataField = "TransactionDate" },
        //                                            new Layout { StartOffset = 1, EndOffset = 1, DataField = "TransactionCode" },
        //                                            new Layout { Start = @"[\d\.]", End = @"\s", EndOffset=1, DataField = "TransactionAmount" },
        //                                            new Layout { Start = @"\d", EndOffset=8, DataField = "ReferenceNumber" },
        //                                        } },
        //                                        } }
        //                                    } },

        //                                    new Layout { EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                        new Layout { StartOffset = 1, EndOffset = 16, DataField = "AccountNumber" },
        //                                        new Layout { StartOffset = 4, EndOffset = 8, DataField = "TransactionDate" },
        //                                        new Layout { StartOffset = 1, EndOffset = 1, DataField = "TransactionCode" },
        //                                        new Layout { Start = @"[\d\.]", End = @"\s", EndOffset=1, DataField = "TransactionAmount" },
        //                                        new Layout { Start = @"\d", EndOffset=8, DataField = "ReferenceNumber" },
        //                                        } },
        //                                    new Layout { Start=@"PTS EDIT PEND.{0,4}$", EndOffset = 1, MaxStart=4, Direction = LayoutDirection.Column, Children = {
        //                                        new Layout { Start="PTS EDIT PENDED", EndOffset = 4, DataField = "Ignored" },
        //                                        } },

        //                                    new Layout { Start=@"\A..DBT\/DTL ADNDA", Children = {
        //                                        new Layout { EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 17, EndOffset=35, DataField = "DebitNetworkID" },
        //                                        } },
        //                                        new Layout { Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 83, EndOffset=6, DataField = "DbtdtlAdndaP" },
        //                                            } },
        //                                        new Layout { EndOffset=1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 23, EndOffset=6, DataField = "TransactionTime" },
        //                                            new Layout { StartOffset = 8, EndOffset=20, DataField = "AccountNumber" },
        //                                            new Layout { StartOffset = 26, EndOffset=6, DataField = "AuthNumber" },
        //                                            } },
        //                                    } },

        //                                    new Layout { Start = @"\A..AUTH ADNDA", Children = {
        //                                        new Layout { Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 17, DataField = "AuthInfo" },
        //                                        } },
        //                                        new Layout { EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 17, EndOffset=1, DataField = "AuthAdndaA" },
        //                                            new Layout { EndOffset = 6, DataField = "AuthNumber" },
        //                                            new Layout { EndOffset = 2, DataField = "AuthAdndaC" },
        //                                            new Layout { EndOffset = 1, DataField = "AuthAdndaD" },
        //                                            new Layout { StartOffset = 5, EndOffset=1, DataField = "AuthAdndaF" },
        //                                            new Layout { Start=@"\s\d{4}\s*$", StartOffset=1, EndOffset=4, DataField="TransactionMCC" },
        //                                        } },
        //                                        new Layout { Start = @"\A..POS ADNDA:",  EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 12, DataField = "_PosAdndA" },
        //                                        } },
        //                                        new Layout { Start = @"\A..MKT SP1", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 17, EndOffset = 1, DataField = "AVSResult" },
        //                                            new Layout { StartOffset = 5, EndOffset = 12, DataField = "_Mktsp1AdndaE" },
        //                                        } },
        //                                        new Layout { Start = @"\A..MKT SP2", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 17, EndOffset=41, DataField = "MarketData" },
        //                                        } },
        //                                        new Layout { Start = @"\A..CPS POS ADNDA:", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 17, EndOffset=15, DataField = "TransactionId" },
        //                                            new Layout { EndOffset=1, DataField = "Cps" },
        //                                            new Layout { StartOffset = 15, EndOffset=2, DataField = "AuthResponse" },
        //                                            new Layout { StartOffset = 15, EndOffset=1, DataField = "EMVChipCondition" },
        //                                            new Layout { StartOffset = 10, EndOffset=4, DataField = "ExpirationDate" },
        //                                        } },                                            new Layout { Start = @"\A..CHIP ADNDA 1", Children = {
        //                                        new Layout { Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 15, DataField = "ChipAdndA1" },
        //                                        } },
        //                                        new Layout { Start = @"\A..CHIP ADNDA 2", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 15, DataField = "ChipAdndA2" },
        //                                        } },
        //                                        new Layout { Start = @"\A..CHIP ADNDA 3", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 15, DataField = "ChipAdndA3" },
        //                                        } },
        //                                        new Layout { Start = @"\A..CHIP ADNDA 4", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 15, DataField = "ChipAdndA4" },
        //                                        } },
        //                                        new Layout { Start = @"\A..CHIP ADNDA 5",  EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 15, DataField = "ChipAdndA5" },
        //                                        } },
        //                                        new Layout { Start = @"\A..CHIP ADNDA 6",  EndOffset = 1, Direction = LayoutDirection.Column, Children = {
        //                                            new Layout { StartOffset = 15, DataField = "ChipAdndA6" },
        //                                            } },
        //                                        } },
        //                                    }}
        //                                } },
        //                            } },

        //                            new Layout { StartOffset=-1, Direction = LayoutDirection.Column, Children = {
        //                                new Layout { StartOffset = 50, EndOffset = 6, DataField = "BatchNumber" },
        //                                new Layout { StartOffset = 8, EndOffset = 24, DataField = "BatchAmount" },
        //                                new Layout { StartOffset = 7, DataField = "BatchCount" },
        //                            } },
        //                        } },
        //                    } },
        //                }
        //            },
        //        },

        //        Transformation = {
        //            Fields= {
        //                new ArrayField {  ParserField="BATCHES", Flat=true, Fields={
        //                    new ArrayField { ParserField="MERCHANTS", Flat=true, Fields={
        //                        new ArrayField { ParserField="TRANSACTIONS", Flat=true, IgnoreParserFields={ "_PosAdndA", "_Mktsp1AdndaE", }, Fields={
        //                            new ArrayField { ParserField="DOUBLE TRANSACTIONS", Flat=true, Fields={
        //                                new StringField { Field="BinNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=6} },
        //                                new StringField { Field="LastFourAccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { Start=@"\d{4}$", EndOffset=4} },
        //                                new StringField { Field="First4AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=4} },
        //                                new StringField { Field="First5AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=5} },
        //                                new StringField { Field="First6AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=6} },
        //                                new StringField { Field="First7AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=7} },
        //                                new StringField { Field="First8AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=8} },
        //                                new StringField { Field="First9AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=9} },
        //                                new StringField { Field="First10AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=10} },
        //                                new StringField { Field="First11AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=11} },
        //                                new StringField { Field="First12AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12} },
        //                                new HashField { Field="HashedFirst12AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12}},
        //                                new EncryptField { Field="FirstTwelveAccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12} },
        //                                new HashField { Field="HashedAccountNumber", ParserField="AccountNumber"},
        //                                new EncryptField { Field="AccountNumber" },
        //                            } },

        //                            new StringField { Field="BinNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=6} },
        //                            new StringField { Field="LastFourAccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { Start=@"\d{4}$", EndOffset=4} },
        //                            new StringField { Field="First4AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=4} },
        //                            new StringField { Field="First5AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=5} },
        //                            new StringField { Field="First6AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=6} },
        //                            new StringField { Field="First7AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=7} },
        //                            new StringField { Field="First8AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=8} },
        //                            new StringField { Field="First9AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=9} },
        //                            new StringField { Field="First10AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=10} },
        //                            new StringField { Field="First11AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=11} },
        //                            new StringField { Field="First12AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12} },
        //                            new HashField { Field="HashedFirst12AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12}},
        //                            new EncryptField { Field="FirstTwelveAccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12} },
        //                            new HashField { Field="HashedAccountNumber", ParserField="AccountNumber"},
        //                            new EncryptField { Field="AccountNumber" },

        //                            new StringField { Field="PointOfSale", ParserField="AuthAdndaC" },
        //                            new StringField { Field="CustomerIdMethod", ParserField="AuthAdndaD" },
        //                            new StringField { Field="MOTOIndicator", ParserField="AuthAdndaF" },
        //                            new StringField { Field="Keyed", ParserField="AuthAdndaC", ModifyAction=new CSharpAction {
        //                                Code= "return V == \"01\" ? \"Y\" : null;"
        //                            }},

        //                            new StringField { Field="TransactionTime", ParserField="TransactionTime", ModifyAction=new CSharpAction{
        //                                Code= "return V == null || V.Length < 6 ? null : V.Substring(0, 2) + \":\" + V.Substring(2, 2) + \":\" + V.Substring(4, 2);"
        //                            }},

        //                            new StringField { Field="CardInputCapability", ParserField="_PosAdndA", ModifyAction=new SubStringAction{ StartOffset=28, EndOffset=1} },
        //                            new StringField { Field="CardInputMode", ParserField="_PosAdndA", ModifyAction=new SubStringAction{ StartOffset=36, EndOffset=1} },

        //                            new StringField { Field="Mktsp1AdndaE", ParserField="_Mktsp1AdndaE", ModifyAction=new CSharpAction{
        //                                Code= "return V != null && double.TryParse(V, out double a) && a != 0 ? (a / 100).ToString() : null;"
        //                            }},
        //                            new StringField { Field="AuthAmount", ParserField="_Mktsp1AdndaE", ModifyAction=new CSharpAction{
        //                                Code= "return V != null && double.TryParse(V, out double a) ? (a / 100).ToString() : null;"
        //                            }},

        //                            new StringField { Field="InvoiceNumber", ParserField="MarketData", ModifyAction=new CSharpAction{
        //                                Code="return !string.IsNullOrEmpty(V) && (V[0] == '1' || V[0] == '5') ? V.Substring(1) : null;"
        //                            }},
        //                        }}
        //                        } },
        //                    }
        //                },
        //            },
        //            Massage = {
        //                GlobalVariables = "int _recordId; string _lastBatchNumber;",
        //                Code=
        //                    @"var newB = new List<IDictionary<string, object>>(B.Count);
        //                    foreach (var record in B)
        //                    {
        //                        var batchNumber = record.GetText(""BatchNumber"");
        //                        if (_lastBatchNumber != batchNumber)
        //                        {
        //                            _lastBatchNumber = batchNumber;
        //                            _recordId = 0;
        //                        }
        //                        if (record.ContainsKey(""Ignored"") || !record.ContainsKey(""AccountNumber""))
        //                            continue;
        //                        record[""RecordID""] = ++_recordId;
        //                        record[""FileSource""] = ""CD028"";
        //                        newB.Add(record);
        //                    }
        //                    return newB;"
        //            }
        //        },

        //        Loaders =
        //        {
        //            new ConsoleLoader {},

        //            new CsvLoader { OutPath="$path/$name.cards", Delimiter=",", Fields={
        //                "HashedAccountNumber", "AccountNumber"
        //            } },

        //            new CsvLoader { OutPath="$path/$name.trans", Delimiter="|", Fields = {
        //                "RecordID", "FileSource", "ReportDate", "MerchantNumber", "TerminalNumber",
        //                "BatchNumber", "CardType", "BinNumber", "LastFourAccountNumber", "AccountNumber",
        //                "ExpirationDate", "TransactionCode", "TransactionDate", "TransactionTime", "TransactionAmount",
        //                // ReferenceNumber Old: case "DBT/DTL ADNDA" save without trans_id
        //                "AuthNumber", "ReferenceNumber", "InvoiceNumber", "StoreNumber", "CustomerCode",
        //                "DebitNetworkID", "ErrorCode", "AuthResponse", "TerminalType", "PointOfSaleTerminalNumber",
        //                "AVSRequest", "AVSResult", "MOTOIndicator", "ReversalFlag", "MarketData",
        //                // TransactionMCC Old: Parse wrong position 
        //                "TransactionMCC", "InterchangeRate", "SubmittedInterchange", "CashBackAmount", "PointOfSale",
        //                // Keyed Old: Forget to reset keyed="" every trans
        //                "Keyed", "TransactionStatus", "EvenAmountIndicator", "ForeignCardIndicator", "DuplicateAmountIndicator",
        //                "CardCreatorIndicator", "MatchReturnIndicator", "Rejected", "TrueDepositType", "AuthInfo",
        //                "CustomerIdMethod", "AuthAmount", "TransactionId", "Cps",
        //                "Column08", "Column09", "Column10", "Column11", "Column12", "Column13", "Column14", "Column15",
        //                "SystemNumber", "PrinNumber",  "AuthAdndaA", "AuthAdndaC", "AuthAdndaD", "AuthAdndaF", "Mktsp1AdndaE", "DbtdtlAdndaK", "DbtdtlAdndaL", "DbtdtlAdndaP",
        //                "FirstTwelveAccountNumber", "First4AccountNumber", "First5AccountNumber", "First6AccountNumber", "First7AccountNumber","First8AccountNumber",
        //                "First9AccountNumber", "First10AccountNumber", "First11AccountNumber", "First12AccountNumber", "HashedFirst12AccountNumber", "HashedAccountNumber",
        //                // ChipAdndA1 Old: when happening comments inside trans scope (IsContinueTransaction=true) then case "CPS POS ADNDA" will exit trans scope (not reach "CHIP ADNDA 1" which after  "CPS POS ADNDA")
        //                "ChipAdndA1", "ChipAdndA2", "ChipAdndA3", "ChipAdndA4", "ChipAdndA5", "ChipAdndA6", "CardInputCapability", "CardInputMode", "EMVChipCondition"
        //                }
        //            },

        //            new MongoDbLoader
        //            {
        //                CollectionName = "Test"
        //            }
        //        }
        //    };
        //}

        //public static Core.Etl CreateSD430BConfig()
        //{
        //    return new Core.Etl
        //    {

        //        Extraction = {
        //            //Comments = { @"\A1SD-430B.+PAGE\s{6}[^1].+", "" },

        //            Layout = new Layout
        //            {
        //                Id = "0",
        //                Start = @"\A1SD-430B",
        //                End = @"\A1SD-430B",
        //                Children ={
        //                new Layout {  Direction= LayoutDirection.Column, Children={
        //                    new Layout { StartOffset=10, EndOffset=17, DataField="System No"},
        //                    new Layout { Start="-FC-", StartOffset=5, EndOffset=13, DataField="FC Date"}
        //                    } },
        //                new Layout { Start=@"\A0FDR\sMRCH\sNR", End=@"\A0FDR\sMRCH\sNR", Repeat=true, DataField="MERCH DETAILS", Children= {
        //                    new Layout { Id="test", EndOffset=2, Direction= LayoutDirection.Column, Children ={
        //                        new Layout{  StartOffset=1, EndOffset=17, Children={
        //                            new Layout { StartOffset=1, DataField="MRCH NR"}
        //                            } },
        //                        new Layout{  EndOffset=16 , Children={
        //                            new Layout { StartOffset=1, DataField="SERVAGT"}
        //                            } },
        //                        new Layout{  EndOffset=11, Children={
        //                            new Layout { StartOffset=1, DataField="SE"}
        //                            } },
        //                        new Layout{ EndOffset=26, Children={
        //                            new Layout { StartOffset=1, DataField="MRCH NAME"}
        //                            } },
        //                        new Layout{ EndOffset=31 , Children={
        //                            new Layout { StartOffset=1, DataField="ADDRESS"}
        //                            } },
        //                        new Layout{  EndOffset=13, Children={
        //                            new Layout { StartOffset=1, DataField="CITY"}
        //                            } },
        //                        new Layout{  EndOffset=4 , Children={
        //                            new Layout { StartOffset=1, DataField="ST"}
        //                            } },
        //                        new Layout{  Children={
        //                            new Layout { StartOffset=1, DataField="ZIP"}
        //                            } },
        //                        }},
        //                    new Layout { EndOffset=2, Direction= LayoutDirection.Column, Children ={
        //                        new Layout{ StartOffset=1, EndOffset=16, Children={
        //                            new Layout { StartOffset=1, DataField="CARDHOLDER"}
        //                            } },
        //                        new Layout{ EndOffset=18 , Children={
        //                            new Layout { StartOffset=1, DataField="ORIG CARDHOLDER"}
        //                            } },
        //                        new Layout{ EndOffset=9, Children={
        //                            new Layout { StartOffset=1, DataField="TRAN DT"}
        //                            } },
        //                        new Layout{ EndOffset=9, Children={
        //                            new Layout { StartOffset=1, DataField="ADJ DT"}
        //                            } },
        //                        new Layout{  EndOffset=18, Children={
        //                            new Layout { StartOffset=1, DataField="CB AMT"}
        //                            } },
        //                        new Layout{ EndOffset=20 , Children={
        //                            new Layout { StartOffset=1, DataField="ROC AMT"}
        //                            } },
        //                        new Layout{ EndOffset=18, Children={
        //                            new Layout { StartOffset=1, DataField="SOC AMT"}
        //                            } },
        //                        new Layout{ EndOffset=14 , Children={
        //                            new Layout { StartOffset=1, DataField="FRGN AMT"}
        //                            } },
        //                        new Layout{ Children={
        //                            new Layout { StartOffset=1, DataField="FRGN CURR"}
        //                            } },
        //                        }},
        //                    new Layout {  EndOffset=2, Direction= LayoutDirection.Column, Children ={
        //                        new Layout{ StartOffset=1, EndOffset=7, Children={
        //                            new Layout { StartOffset=1, DataField="ADJ NR"}
        //                            } },
        //                        new Layout{ EndOffset=12 , Children={
        //                            new Layout { StartOffset=1, DataField="RES NR"}
        //                            } },
        //                        new Layout{  EndOffset=21, Children={
        //                            new Layout { StartOffset=1, DataField="REFERENCE NR"}
        //                            } },
        //                        new Layout{  EndOffset=8, Children={
        //                            new Layout { StartOffset=1, DataField="ROC INV"}
        //                            } },
        //                        new Layout{ EndOffset=8, Children={
        //                            new Layout { StartOffset=1, DataField="SOC INV"}
        //                            } },
        //                        new Layout{ EndOffset=5 , Children={
        //                            new Layout { StartOffset=1, DataField="SUPP"}
        //                            } },
        //                        new Layout{ Children={
        //                            new Layout { StartOffset=1, DataField="TRIUMPH SEQ IDT"}
        //                            } },
        //                        }},
        //                    new Layout { StartOffset=1, Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=6, EndOffset=20, DataField="FOLIO"},
        //                        new Layout{ StartOffset=7, EndOffset=12, DataField="ORDR NR"},
        //                        new Layout{ StartOffset=2, DataField="DT"}
        //                        }},
        //                    new Layout { StartOffset=1, Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=8, EndOffset=17, DataField="AIR TKT"},
        //                        new Layout{ StartOffset=7, DataField="AIR SEQ"},
        //                    }},
        //                    new Layout { StartOffset=1, Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=8, EndOffset=22, DataField="CANC NR"},
        //                        new Layout{ StartOffset=7, DataField="CANC DT"},
        //                    }},
        //                    new Layout { StartOffset=2, Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=12, EndOffset=4, DataField="IND FORM CD"},
        //                        new Layout{ StartOffset=12, DataField="IND REF NR"},
        //                    }},
        //                    new Layout { StartOffset=1, Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=12, DataField="LOC REF NR"},
        //                    }},
        //                    new Layout { StartOffset=2, Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=12, EndOffset=13, DataField="TRACKING ID"},
        //                        new Layout{ StartOffset=8, EndOffset=8, DataField="FILE SEQ"},
        //                        new Layout{ StartOffset=8, EndOffset=6, DataField="BATCH NR"},
        //                        new Layout{ StartOffset=6, DataField="INV DT"}
        //                        }},
        //                    new Layout { StartOffset=1, Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=7, EndOffset=19, DataField="LABEL1"},
        //                        new Layout{ StartOffset=4, EndOffset=32, DataField="DATA1"},
        //                        new Layout{ StartOffset=5, EndOffset=21, DataField="LABEL2"},
        //                        new Layout{ StartOffset=4, DataField="DATA2"}
        //                        }},
        //                    new Layout { Start=@"\A\sPASS\sNAME\s" , Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=10, EndOffset=12, DataField="PASS NAME"},
        //                        new Layout{ StartOffset=15, EndOffset=3, DataField="PASS FIRST NAME"},
        //                        new Layout{ StartOffset=11, EndOffset=3, DataField="PASS M NAME"},
        //                        new Layout{ StartOffset=14, EndOffset=7, DataField="PASS LAST NAME"},
        //                        new Layout{ StartOffset=12, EndOffset=3, DataField="SE PROC DATE"},
        //                        new Layout{ StartOffset=8, EndOffset=3, DataField="RTNED DATE"},
        //                        new Layout{ StartOffset=17, DataField="CRED RECPT NUMBER"}
        //                        }},
        //                    new Layout { StartOffset=2 , Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=12, EndOffset=14, DataField="RTN TO NAME"},
        //                        new Layout{ StartOffset=9, EndOffset=8, DataField="RTN TO ST"},
        //                        new Layout{ StartOffset=11, End="ASSURED", DataField="CRD DEPOSIT"},
        //                        new Layout{ StartOffset=14, End="RES", DataField="ASSURED RESERV"},
        //                        new Layout{ StartOffset=10, End="REASON", DataField="RES CANCEL"},
        //                        new Layout{ StartOffset=18, End=@"CANCEL\sZONE", DataField="REASON CANCEL DATE"},
        //                        new Layout{ StartOffset=11, DataField="CANCEL ZONE"}
        //                        }},
        //                    new Layout { StartOffset=2 , Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=12, End="RESERV", DataField="RESERV MADE"},
        //                        new Layout{ StartOffset=10, EndOffset=11, DataField="RESERV LOC"},
        //                        new Layout{ StartOffset=12, EndOffset=3, DataField="RESV MADE ON"},
        //                        new Layout{ StartOffset=17, EndOffset=3, DataField="RENT AGREE NUMBER"},
        //                        new Layout{ StartOffset=10, EndOffset=11, DataField="MERCH TYPE"},
        //                        new Layout{ StartOffset=8, EndOffset=3, DataField="MRCH RTN"},
        //                        new Layout{ StartOffset=8, DataField="RTN NAME"}
        //                        }},
        //                     new Layout { StartOffset=2 , Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=9, End="RTN", DataField="RTN DATE"},
        //                        new Layout{ StartOffset=8, End="RTN", DataField="RTN SHIP"},
        //                        new Layout{ StartOffset=10, EndOffset=42, DataField="RTN REASON"},
        //                        new Layout{ StartOffset=13, DataField="STORE CR RECV"},
        //                        }},
        //                     new Layout { StartOffset=2 , Direction=LayoutDirection.Column, Children = {
        //                        new Layout{ StartOffset=21, DataField="FINANCIAL"},
        //                        }},
        //                }},
        //                }
        //            },
        //        },

        //        Transformation = {
        //            Fields = {
        //            new DateField { Field="FC Date"},
        //            new ArrayField { Field="MERCH DETAILS", Flat=true, Fields = {
        //                new FLoatField { Field="CB AMT"},
        //                new FLoatField { Field="ROC AMT"},
        //                new FLoatField { Field="SOC AMT"},
        //                new FLoatField { Field="FRGN AMT"},
        //                //new CardField { DbField="CARDHOLDER" },
        //                new StringField { Field="CARDHOLDER_RAW", ParserField="CARDHOLDER" },
        //                new StringField { Field="CARDHOLDER_MASK", ParserField="CARDHOLDER", ModifyAction=new CSharpAction {
        //                    Code=
        //                    "var text = R[\"CARDHOLDER\"] as string;" +
        //                    "return text.Substring(0, 6) + \"\".PadLeft(text.Length - 10, '*') + text[^4..];"
        //                    }},
        //                //new CardField { DbField="ORIG CARDHOLDER" },
        //                new StringField { Field="ORIG CARDHOLDER RAW", ParserField="ORIG CARDHOLDER" },
        //                new StringField { Field="ORIG CARDHOLDER MASK", ParserField="ORIG CARDHOLDER", ModifyAction=new CSharpAction {
        //                    Code=
        //                    "var text = R[\"CARDHOLDER\"] as string;" +
        //                    "return text.Substring(0, 6) + \"\".PadLeft(text.Length - 10, '*') + text[^4..];"
        //                    }},
        //                }},
        //            }
        //        }
        //    };
        //}

        public static Core.Etl CreateDelimiterDemoConfig()
        {
            return new Core.Etl
            {
                ScanBatch = 1,
                FlushBatch = 2,
                Extraction =
                {
                    LayoutStartOffset = 3 ,
                    Layout = new Layout  { EndOffset =1, Repeat=true, Direction=LayoutDirection.Column, Children= {
                        new Layout{ End=@"\|", DataField="F1"},
                        new Layout{ StartOffset=1, End=@"\|", DataField="F2"},
                        new Layout{ StartOffset=1, End=@"\|", DataField="F3"},
                        new Layout{ StartOffset=1, End=@"\|", DataField="F4"},
                        new Layout{ StartOffset=1, DataField="F5"}
                    } }
                },
                Transformation =
                {
                    Fields =
                    {
                        new IntegerField { Field="F3", Required=true }
                    }
                },
                Loaders =
                {
                    new CsvLoader()
                }
            };
        }
    }
}

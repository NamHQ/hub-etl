using Etl.Core.Extraction;
using Etl.Core.Transformation.Fields;
using Etl.Core;
using Etl.Core.Transformation.Modification;
using Etl.Core.Load;
using Etl.Storage;

namespace Etl.ConsoleApp
{
    public static class ConfigTest
    {
        public static EtlDef CreateCD028()
        {
            return new EtlDef
            {
                ScanBatch = 5,
                FlushBatch = 10 * 1000,

                Extraction = {

                    Comments = {
                        new LayoutDef { Start=@"\A1CD-028", MaxStart=1, Children={
                            new LayoutDef { Start=@"\A-MRCH", MaxStart=7, Children={
                                new LayoutDef { StartOffset=2 },
                                new LayoutDef { Start=@"\A.{21}\d\d\/\d\d\/\d\d\s", MaxStart=1, EndOffset=-2}
                            } }
                        } }
                    },

                    Layout = new LayoutDef
                    {
                        Start = @"\A^1CD-028.+PAGE\s{6}1$",
                        End = @"\A^1CD-028.+PAGE\s{6}1$",
                        Children = {

                            new LayoutDef { Direction = LayoutDirection.Column, Children = {
                                new LayoutDef { StartOffset = 10, EndOffset = 4, DataField = "SystemNumber" },
                                new LayoutDef { StartOffset = 1, EndOffset = 4, DataField = "PrinNumber" },
                                new LayoutDef { StartOffset = 90, EndOffset = 8, DataField = "ReportDate" }
                            } },

                            new LayoutDef { StartOffset = 2, Direction = LayoutDirection.Column, Children = {
                                new LayoutDef { StartOffset = 59, EndOffset = 3, DataField = "CustomerCode" },
                            } },

                            new LayoutDef { End=@"\A0END OF BATCH", EndOffset=1, Repeat=true, DataField="BATCHES", Children={
                                new LayoutDef { Start = @"\A(-MRCH =)|(.{21}\d\d\/\d\d\/\d\d\s)", Children={
                                    new LayoutDef { Start = @"\A(-MRCH =)|(.{21}\d\d\/\d\d\/\d\d\s)", End = @"\A-MRCH =", Repeat = true, DataField = "MERCHANTS", Children = {
                                        new LayoutDef { Start=@"\A-MRCH =", MaxStart=1, Direction = LayoutDirection.Column, Children = {
                                            new LayoutDef { StartOffset = 8, EndOffset = 16, DataField = "MerchantNumber" },
                                        } },
                                        new LayoutDef { StartOffset = 1, Direction = LayoutDirection.Column, Children = {
                                            new LayoutDef { Start = "TRUE DEPOSIT", StartOffset = 19, EndOffset = 2, DataField = "TrueDepositType" },
                                        } },

                                        new LayoutDef { Start = @"\A.{21}\d\d\/\d\d\/\d\d\s", End = @"^.{21}\d\d\/\d\d\/\d\d\s", Repeat = true, DataField = "TRANSACTIONS", Children = {
                                            new LayoutDef { Start = @"\A.{89}\d\d\/\d\d\/\d\d\s", EndOffset=1, MaxStart=1, Direction=LayoutDirection.Column, Children= {
                                                new LayoutDef { EndOffset=68, Direction=LayoutDirection.Column, Repeat= true, DataField="DOUBLE TRANSACTIONS", Children= {
                                                    new LayoutDef { EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 1, EndOffset = 16, DataField = "AccountNumber" },
                                                    new LayoutDef { StartOffset = 4, EndOffset = 8, DataField = "TransactionDate" },
                                                    new LayoutDef { StartOffset = 1, EndOffset = 1, DataField = "TransactionCode" },
                                                    new LayoutDef { Start = @"[\d\.]", End = @"\s", EndOffset=1, DataField = "TransactionAmount" },
                                                    new LayoutDef { Start = @"\d", EndOffset=8, DataField = "ReferenceNumber" },
                                                } },
                                                } }
                                            } },

                                            new LayoutDef { EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                new LayoutDef { StartOffset = 1, EndOffset = 16, DataField = "AccountNumber" },
                                                new LayoutDef { StartOffset = 4, EndOffset = 8, DataField = "TransactionDate" },
                                                new LayoutDef { StartOffset = 1, EndOffset = 1, DataField = "TransactionCode" },
                                                new LayoutDef { Start = @"[\d\.]", End = @"\s", EndOffset=1, DataField = "TransactionAmount" },
                                                new LayoutDef { Start = @"\d", EndOffset=8, DataField = "ReferenceNumber" },
                                                } },
                                            new LayoutDef { Start=@"PTS EDIT PEND.{0,4}$", EndOffset = 1, MaxStart=4, Direction = LayoutDirection.Column, Children = {
                                                new LayoutDef { Start="PTS EDIT PENDED", EndOffset = 4, DataField = "Ignored" },
                                                } },

                                            new LayoutDef { Start=@"\A..DBT\/DTL ADNDA", Children = {
                                                new LayoutDef { EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 17, EndOffset=35, DataField = "DebitNetworkID" },
                                                } },
                                                new LayoutDef { Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 83, EndOffset=6, DataField = "DbtdtlAdndaP" },
                                                    } },
                                                new LayoutDef { EndOffset=1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 23, EndOffset=6, DataField = "TransactionTime" },
                                                    new LayoutDef { StartOffset = 8, EndOffset=20, DataField = "AccountNumber" },
                                                    new LayoutDef { StartOffset = 26, EndOffset=6, DataField = "AuthNumber" },
                                                    } },
                                            } },

                                            new LayoutDef { Start = @"\A..AUTH ADNDA", Children = {
                                                new LayoutDef { Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 17, DataField = "AuthInfo" },
                                                } },
                                                new LayoutDef { EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 17, EndOffset=1, DataField = "AuthAdndaA" },
                                                    new LayoutDef { EndOffset = 6, DataField = "AuthNumber" },
                                                    new LayoutDef { EndOffset = 2, DataField = "AuthAdndaC" },
                                                    new LayoutDef { EndOffset = 1, DataField = "AuthAdndaD" },
                                                    new LayoutDef { StartOffset = 5, EndOffset=1, DataField = "AuthAdndaF" },
                                                    new LayoutDef { Start=@"\s\d{4}\s*$", StartOffset=1, EndOffset=4, DataField="TransactionMCC" },
                                                } },
                                                new LayoutDef { Start = @"\A..POS ADNDA:",  EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 12, DataField = "_PosAdndA" },
                                                } },
                                                new LayoutDef { Start = @"\A..MKT SP1", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 17, EndOffset = 1, DataField = "AVSResult" },
                                                    new LayoutDef { StartOffset = 5, EndOffset = 12, DataField = "_Mktsp1AdndaE" },
                                                } },
                                                new LayoutDef { Start = @"\A..MKT SP2", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 17, EndOffset=41, DataField = "MarketData" },
                                                } },
                                                new LayoutDef { Start = @"\A..CPS POS ADNDA:", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 17, EndOffset=15, DataField = "TransactionId" },
                                                    new LayoutDef { EndOffset=1, DataField = "Cps" },
                                                    new LayoutDef { StartOffset = 15, EndOffset=2, DataField = "AuthResponse" },
                                                    new LayoutDef { StartOffset = 15, EndOffset=1, DataField = "EMVChipCondition" },
                                                    new LayoutDef { StartOffset = 10, EndOffset=4, DataField = "ExpirationDate" },
                                                } },                                            new LayoutDef { Start = @"\A..CHIP ADNDA 1", Children = {
                                                new LayoutDef { Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 15, DataField = "ChipAdndA1" },
                                                } },
                                                new LayoutDef { Start = @"\A..CHIP ADNDA 2", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 15, DataField = "ChipAdndA2" },
                                                } },
                                                new LayoutDef { Start = @"\A..CHIP ADNDA 3", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 15, DataField = "ChipAdndA3" },
                                                } },
                                                new LayoutDef { Start = @"\A..CHIP ADNDA 4", EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 15, DataField = "ChipAdndA4" },
                                                } },
                                                new LayoutDef { Start = @"\A..CHIP ADNDA 5",  EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 15, DataField = "ChipAdndA5" },
                                                } },
                                                new LayoutDef { Start = @"\A..CHIP ADNDA 6",  EndOffset = 1, Direction = LayoutDirection.Column, Children = {
                                                    new LayoutDef { StartOffset = 15, DataField = "ChipAdndA6" },
                                                    } },
                                                } },
                                            }}
                                        } },
                                    } },

                                    new LayoutDef { StartOffset=-1, Direction = LayoutDirection.Column, Children = {
                                        new LayoutDef { StartOffset = 50, EndOffset = 6, DataField = "BatchNumber" },
                                        new LayoutDef { StartOffset = 8, EndOffset = 24, DataField = "BatchAmount" },
                                        new LayoutDef { StartOffset = 7, DataField = "BatchCount" },
                                    } },
                                } },
                            } },
                        }
                    },
                },

                Transformation = {
                    Fields= {
                        new ArrayField {  ParserField="BATCHES", Flat=true, Fields={
                            new ArrayField { ParserField="MERCHANTS", Flat=true, Fields={
                                new ArrayField { ParserField="TRANSACTIONS", Flat=true, IgnoreParserFields={ "_PosAdndA", "_Mktsp1AdndaE", }, Fields={
                                    new ArrayField { ParserField="DOUBLE TRANSACTIONS", Flat=true, Fields={
                                        new StringField { Field="BinNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=6} },
                                        new StringField { Field="LastFourAccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { Start=@"\d{4}$", EndOffset=4} },
                                        new StringField { Field="First4AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=4} },
                                        new StringField { Field="First5AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=5} },
                                        new StringField { Field="First6AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=6} },
                                        new StringField { Field="First7AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=7} },
                                        new StringField { Field="First8AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=8} },
                                        new StringField { Field="First9AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=9} },
                                        new StringField { Field="First10AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=10} },
                                        new StringField { Field="First11AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=11} },
                                        new StringField { Field="First12AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12} },
                                        new HashField { Field="HashedFirst12AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12}},
                                        new EncryptField { Field="FirstTwelveAccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12} },
                                        new HashField { Field="HashedAccountNumber", ParserField="AccountNumber"},
                                        new EncryptField { Field="AccountNumber" },
                                    } },

                                    new StringField { Field="BinNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=6} },
                                    new StringField { Field="LastFourAccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { Start=@"\d{4}$", EndOffset=4} },
                                    new StringField { Field="First4AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=4} },
                                    new StringField { Field="First5AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=5} },
                                    new StringField { Field="First6AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=6} },
                                    new StringField { Field="First7AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=7} },
                                    new StringField { Field="First8AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=8} },
                                    new StringField { Field="First9AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=9} },
                                    new StringField { Field="First10AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=10} },
                                    new StringField { Field="First11AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=11} },
                                    new StringField { Field="First12AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12} },
                                    new HashField { Field="HashedFirst12AccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12}},
                                    new EncryptField { Field="FirstTwelveAccountNumber", ParserField="AccountNumber", ModifyAction=new SubStringAction { EndOffset=12} },
                                    new HashField { Field="HashedAccountNumber", ParserField="AccountNumber"},
                                    new EncryptField { Field="AccountNumber" },

                                    new StringField { Field="PointOfSale", ParserField="AuthAdndaC" },
                                    new StringField { Field="CustomerIdMethod", ParserField="AuthAdndaD" },
                                    new StringField { Field="MOTOIndicator", ParserField="AuthAdndaF" },
                                    new StringField { Field="Keyed", ParserField="AuthAdndaC", ModifyAction=new CSharpAction {
                                        Code= "return V == \"01\" ? \"Y\" : null;"
                                    }},

                                    new StringField { Field="TransactionTime", ParserField="TransactionTime", ModifyAction=new CSharpAction{
                                        Code= "return V == null || V.Length < 6 ? null : V.Substring(0, 2) + \":\" + V.Substring(2, 2) + \":\" + V.Substring(4, 2);"
                                    }},

                                    new StringField { Field="CardInputCapability", ParserField="_PosAdndA", ModifyAction=new SubStringAction{ StartOffset=28, EndOffset=1} },
                                    new StringField { Field="CardInputMode", ParserField="_PosAdndA", ModifyAction=new SubStringAction{ StartOffset=36, EndOffset=1} },

                                    new StringField { Field="Mktsp1AdndaE", ParserField="_Mktsp1AdndaE", ModifyAction=new CSharpAction{
                                        Code= "return V != null && double.TryParse(V, out double a) && a != 0 ? (a / 100).ToString() : null;"
                                    }},
                                    new StringField { Field="AuthAmount", ParserField="_Mktsp1AdndaE", ModifyAction=new CSharpAction{
                                        Code= "return V != null && double.TryParse(V, out double a) ? (a / 100).ToString() : null;"
                                    }},

                                    new StringField { Field="InvoiceNumber", ParserField="MarketData", ModifyAction=new CSharpAction{
                                        Code="return !string.IsNullOrEmpty(V) && (V[0] == '1' || V[0] == '5') ? V.Substring(1) : null;"
                                    }},
                                }}
                                } },
                            }
                        },
                    },
                    Massage = {
                        GlobalVariables = "int _recordId; string _lastBatchNumber;",
                        Code=
                            @"var newB = new List<IDictionary<string, object>>(B.Count);
                            foreach (var record in B)
                            {
                                var batchNumber = record.GetText(""BatchNumber"");
                                if (_lastBatchNumber != batchNumber)
                                {
                                    _lastBatchNumber = batchNumber;
                                    _recordId = 0;
                                }
                                if (record.ContainsKey(""Ignored"") || !record.ContainsKey(""AccountNumber""))
                                    continue;
                                record[""RecordID""] = ++_recordId;
                                record[""FileSource""] = ""CD028"";
                                newB.Add(record);
                            }
                            return newB;"
                    }
                },

                Loaders =
                {
                    new ConsoleLoader {},

                    new CsvLoader { OutPath="$path/$name.cards", Delemiter=",", Fields={
                        "HashedAccountNumber", "AccountNumber"
                    } },

                    new CsvLoader { OutPath="$path/$name.trans", Delemiter="|", Fields = {
                        "RecordID", "FileSource", "ReportDate", "MerchantNumber", "TerminalNumber",
                        "BatchNumber", "CardType", "BinNumber", "LastFourAccountNumber", "AccountNumber",
                        "ExpirationDate", "TransactionCode", "TransactionDate", "TransactionTime", "TransactionAmount",
                        // ReferenceNumber Old: case "DBT/DTL ADNDA" save without trans_id
                        "AuthNumber", "ReferenceNumber", "InvoiceNumber", "StoreNumber", "CustomerCode",
                        "DebitNetworkID", "ErrorCode", "AuthResponse", "TerminalType", "PointOfSaleTerminalNumber",
                        "AVSRequest", "AVSResult", "MOTOIndicator", "ReversalFlag", "MarketData",
                        // TransactionMCC Old: Parse wrong position 
                        "TransactionMCC", "InterchangeRate", "SubmittedInterchange", "CashBackAmount", "PointOfSale",
                        // Keyed Old: Forget to reset keyed="" every trans
                        "Keyed", "TransactionStatus", "EvenAmountIndicator", "ForeignCardIndicator", "DuplicateAmountIndicator",
                        "CardCreatorIndicator", "MatchReturnIndicator", "Rejected", "TrueDepositType", "AuthInfo",
                        "CustomerIdMethod", "AuthAmount", "TransactionId", "Cps",
                        "Column08", "Column09", "Column10", "Column11", "Column12", "Column13", "Column14", "Column15",
                        "SystemNumber", "PrinNumber",  "AuthAdndaA", "AuthAdndaC", "AuthAdndaD", "AuthAdndaF", "Mktsp1AdndaE", "DbtdtlAdndaK", "DbtdtlAdndaL", "DbtdtlAdndaP",
                        "FirstTwelveAccountNumber", "First4AccountNumber", "First5AccountNumber", "First6AccountNumber", "First7AccountNumber","First8AccountNumber",
                        "First9AccountNumber", "First10AccountNumber", "First11AccountNumber", "First12AccountNumber", "HashedFirst12AccountNumber", "HashedAccountNumber",
                        // ChipAdndA1 Old: when happening comments inside trans scope (IsContinueTransaction=true) then case "CPS POS ADNDA" will exit trans scope (not reach "CHIP ADNDA 1" which after  "CPS POS ADNDA")
                        "ChipAdndA1", "ChipAdndA2", "ChipAdndA3", "ChipAdndA4", "ChipAdndA5", "ChipAdndA6", "CardInputCapability", "CardInputMode", "EMVChipCondition"
                        }
                    },

                    //new MongoDbLoader
                    //{
                    //    CollectionName = "Test"
                    //}
                }
            };
        }

        public static EtlDef CreateSD430BConfig()
        {
            return new EtlDef
            {

                Extraction = {
                    //Comments = { @"\A1SD-430B.+PAGE\s{6}[^1].+", "" },

                    Layout = new LayoutDef
                    {
                        Id = "0",
                        Start = @"\A1SD-430B",
                        End = @"\A1SD-430B",
                        Children ={
                        new LayoutDef {  Direction= LayoutDirection.Column, Children={
                            new LayoutDef { StartOffset=10, EndOffset=17, DataField="System No"},
                            new LayoutDef {Start="-FC-", StartOffset=5, EndOffset=13, DataField="FC Date"}
                            } },
                        new LayoutDef { Start=@"\A0FDR\sMRCH\sNR", End=@"\A0FDR\sMRCH\sNR", Repeat=true, DataField="MERCH DETAILS", Children= {
                            new LayoutDef { Id="test", EndOffset=2, Direction= LayoutDirection.Column, Children ={
                                new LayoutDef{  StartOffset=1, EndOffset=17, Children={
                                    new LayoutDef { StartOffset=1, DataField="MRCH NR"}
                                    } },
                                new LayoutDef{  EndOffset=16 , Children={
                                    new LayoutDef { StartOffset=1, DataField="SERVAGT"}
                                    } },
                                new LayoutDef{  EndOffset=11, Children={
                                    new LayoutDef { StartOffset=1, DataField="SE"}
                                    } },
                                new LayoutDef{ EndOffset=26, Children={
                                    new LayoutDef { StartOffset=1, DataField="MRCH NAME"}
                                    } },
                                new LayoutDef{ EndOffset=31 , Children={
                                    new LayoutDef { StartOffset=1, DataField="ADDRESS"}
                                    } },
                                new LayoutDef{  EndOffset=13, Children={
                                    new LayoutDef { StartOffset=1, DataField="CITY"}
                                    } },
                                new LayoutDef{  EndOffset=4 , Children={
                                    new LayoutDef { StartOffset=1, DataField="ST"}
                                    } },
                                new LayoutDef{  Children={
                                    new LayoutDef { StartOffset=1, DataField="ZIP"}
                                    } },
                                }},
                            new LayoutDef { EndOffset=2, Direction= LayoutDirection.Column, Children ={
                                new LayoutDef{ StartOffset=1, EndOffset=16, Children={
                                    new LayoutDef { StartOffset=1, DataField="CARDHOLDER"}
                                    } },
                                new LayoutDef{EndOffset=18 , Children={
                                    new LayoutDef { StartOffset=1, DataField="ORIG CARDHOLDER"}
                                    } },
                                new LayoutDef{ EndOffset=9, Children={
                                    new LayoutDef { StartOffset=1, DataField="TRAN DT"}
                                    } },
                                new LayoutDef{ EndOffset=9, Children={
                                    new LayoutDef { StartOffset=1, DataField="ADJ DT"}
                                    } },
                                new LayoutDef{  EndOffset=18, Children={
                                    new LayoutDef { StartOffset=1, DataField="CB AMT"}
                                    } },
                                new LayoutDef{ EndOffset=20 , Children={
                                    new LayoutDef { StartOffset=1, DataField="ROC AMT"}
                                    } },
                                new LayoutDef{ EndOffset=18, Children={
                                    new LayoutDef { StartOffset=1, DataField="SOC AMT"}
                                    } },
                                new LayoutDef{EndOffset=14 , Children={
                                    new LayoutDef { StartOffset=1, DataField="FRGN AMT"}
                                    } },
                                new LayoutDef{ Children={
                                    new LayoutDef { StartOffset=1, DataField="FRGN CURR"}
                                    } },
                                }},
                            new LayoutDef {  EndOffset=2, Direction= LayoutDirection.Column, Children ={
                                new LayoutDef{StartOffset=1, EndOffset=7, Children={
                                    new LayoutDef { StartOffset=1, DataField="ADJ NR"}
                                    } },
                                new LayoutDef{ EndOffset=12 , Children={
                                    new LayoutDef { StartOffset=1, DataField="RES NR"}
                                    } },
                                new LayoutDef{  EndOffset=21, Children={
                                    new LayoutDef { StartOffset=1, DataField="REFERENCE NR"}
                                    } },
                                new LayoutDef{  EndOffset=8, Children={
                                    new LayoutDef { StartOffset=1, DataField="ROC INV"}
                                    } },
                                new LayoutDef{ EndOffset=8, Children={
                                    new LayoutDef { StartOffset=1, DataField="SOC INV"}
                                    } },
                                new LayoutDef{ EndOffset=5 , Children={
                                    new LayoutDef { StartOffset=1, DataField="SUPP"}
                                    } },
                                new LayoutDef{ Children={
                                    new LayoutDef { StartOffset=1, DataField="TRIUMPH SEQ IDT"}
                                    } },
                                }},
                            new LayoutDef { StartOffset=1, Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=6, EndOffset=20, DataField="FOLIO"},
                                new LayoutDef{ StartOffset=7, EndOffset=12, DataField="ORDR NR"},
                                new LayoutDef{ StartOffset=2, DataField="DT"}
                                }},
                            new LayoutDef { StartOffset=1, Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=8, EndOffset=17, DataField="AIR TKT"},
                                new LayoutDef{ StartOffset=7, DataField="AIR SEQ"},
                            }},
                            new LayoutDef { StartOffset=1, Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=8, EndOffset=22, DataField="CANC NR"},
                                new LayoutDef{ StartOffset=7, DataField="CANC DT"},
                            }},
                            new LayoutDef { StartOffset=2, Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=12, EndOffset=4, DataField="IND FORM CD"},
                                new LayoutDef{ StartOffset=12, DataField="IND REF NR"},
                            }},
                            new LayoutDef { StartOffset=1, Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=12, DataField="LOC REF NR"},
                            }},
                            new LayoutDef { StartOffset=2, Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=12, EndOffset=13, DataField="TRACKING ID"},
                                new LayoutDef{ StartOffset=8, EndOffset=8, DataField="FILE SEQ"},
                                new LayoutDef{ StartOffset=8, EndOffset=6, DataField="BATCH NR"},
                                new LayoutDef{ StartOffset=6, DataField="INV DT"}
                                }},
                            new LayoutDef { StartOffset=1, Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=7, EndOffset=19, DataField="LABEL1"},
                                new LayoutDef{ StartOffset=4, EndOffset=32, DataField="DATA1"},
                                new LayoutDef{ StartOffset=5, EndOffset=21, DataField="LABEL2"},
                                new LayoutDef{ StartOffset=4, DataField="DATA2"}
                                }},
                            new LayoutDef {Start=@"\A\sPASS\sNAME\s" , Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=10, EndOffset=12, DataField="PASS NAME"},
                                new LayoutDef{ StartOffset=15, EndOffset=3, DataField="PASS FIRST NAME"},
                                new LayoutDef{ StartOffset=11, EndOffset=3, DataField="PASS M NAME"},
                                new LayoutDef{ StartOffset=14, EndOffset=7, DataField="PASS LAST NAME"},
                                new LayoutDef{ StartOffset=12, EndOffset=3, DataField="SE PROC DATE"},
                                new LayoutDef{ StartOffset=8, EndOffset=3, DataField="RTNED DATE"},
                                new LayoutDef{ StartOffset=17, DataField="CRED RECPT NUMBER"}
                                }},
                            new LayoutDef {StartOffset=2 , Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=12, EndOffset=14, DataField="RTN TO NAME"},
                                new LayoutDef{ StartOffset=9, EndOffset=8, DataField="RTN TO ST"},
                                new LayoutDef{ StartOffset=11, End="ASSURED", DataField="CRD DEPOSIT"},
                                new LayoutDef{ StartOffset=14, End="RES", DataField="ASSURED RESERV"},
                                new LayoutDef{ StartOffset=10, End="REASON", DataField="RES CANCEL"},
                                new LayoutDef{ StartOffset=18, End=@"CANCEL\sZONE", DataField="REASON CANCEL DATE"},
                                new LayoutDef{ StartOffset=11, DataField="CANCEL ZONE"}
                                }},
                            new LayoutDef {StartOffset=2 , Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=12, End="RESERV", DataField="RESERV MADE"},
                                new LayoutDef{ StartOffset=10, EndOffset=11, DataField="RESERV LOC"},
                                new LayoutDef{ StartOffset=12, EndOffset=3, DataField="RESV MADE ON"},
                                new LayoutDef{ StartOffset=17, EndOffset=3, DataField="RENT AGREE NUMBER"},
                                new LayoutDef{ StartOffset=10, EndOffset=11, DataField="MERCH TYPE"},
                                new LayoutDef{ StartOffset=8, EndOffset=3, DataField="MRCH RTN"},
                                new LayoutDef{ StartOffset=8, DataField="RTN NAME"}
                                }},
                             new LayoutDef { StartOffset=2 , Direction=LayoutDirection.Column, Children = {
                                new LayoutDef{ StartOffset=9, End="RTN", DataField="RTN DATE"},
                                new LayoutDef{ StartOffset=8, End="RTN", DataField="RTN SHIP"},
                                new LayoutDef{ StartOffset=10, EndOffset=42, DataField="RTN REASON"},
                                new LayoutDef{ StartOffset=13, DataField="STORE CR RECV"},
                                }},
                             new LayoutDef {StartOffset=2 , Direction=LayoutDirection.Column,Children = {
                                new LayoutDef{ StartOffset=21, DataField="FINANCIAL"},
                                }},
                        }},
                        }
                    },
                },

                Transformation = {
                    Fields = {
                    new DateField { Field="FC Date"},
                    new ArrayField { Field="MERCH DETAILS", Flat=true, Fields = {
                        new FLoatField { Field="CB AMT"},
                        new FLoatField { Field="ROC AMT"},
                        new FLoatField { Field="SOC AMT"},
                        new FLoatField { Field="FRGN AMT"},
                        //new CardField { DbField="CARDHOLDER" },
                        new StringField { Field="CARDHOLDER_RAW", ParserField="CARDHOLDER" },
                        new StringField { Field="CARDHOLDER_MASK", ParserField="CARDHOLDER", ModifyAction=new CSharpAction {
                            Code=
                            "var text = R[\"CARDHOLDER\"] as string;" +
                            "return text.Substring(0, 6) + \"\".PadLeft(text.Length - 10, '*') + text[^4..];"
                            }},
                        //new CardField { DbField="ORIG CARDHOLDER" },
                        new StringField { Field="ORIG CARDHOLDER RAW", ParserField="ORIG CARDHOLDER" },
                        new StringField { Field="ORIG CARDHOLDER MASK", ParserField="ORIG CARDHOLDER", ModifyAction=new CSharpAction {
                            Code=
                            "var text = R[\"CARDHOLDER\"] as string;" +
                            "return text.Substring(0, 6) + \"\".PadLeft(text.Length - 10, '*') + text[^4..];"
                            }},
                        }},
                    }
                }
            };
        }

        public static EtlDef CreateDelimiterDemoConfig()
        {
            return new EtlDef
            {
                ScanBatch = 1,
                FlushBatch = 2,
                Extraction =
                {
                    LayoutStartOffset = 3 ,
                    Layout = new LayoutDef  { EndOffset =1, Repeat=true, Direction=LayoutDirection.Column, Children= {
                        new LayoutDef{End=@"\|", DataField="F1"},
                        new LayoutDef{StartOffset=1, End=@"\|", DataField="F2"},
                        new LayoutDef{StartOffset=1, End=@"\|", DataField="F3"},
                        new LayoutDef{StartOffset=1, End=@"\|", DataField="F4"},
                        new LayoutDef{StartOffset=1, DataField="F5"}
                    } }
                },
                Transformation =
                {
                    Fields =
                    {
                        new IntegerField { Field="F3"}
                    }
                }

            };
        }
    }
}

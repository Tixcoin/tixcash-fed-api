using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TronNet;
using TronNet.ABI.FunctionEncoding.Attributes;
using TronNet.ABI.Model;
using TronNet.Protocol;
using static TronNet.Protocol.Wallet;
using Google.Protobuf;
using System.Security.Principal;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Ocsp;
using System.Data;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.ObjectPool;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Google.Protobuf.Collections;
using TronNet.ABI.FunctionEncoding;
using TronNet.Contracts;
using System.Numerics; 
using TronNet.Crypto;
using System.Security.Cryptography.Xml;
using TronNet.ABI;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Text;
using System.Net;
using System.Reflection.PortableExecutable;
using System.IO;
using System.Globalization;
using System.ComponentModel;
using static TronNet.Protocol.SmartContract.Types;

namespace dataAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ITronClient _tronClient;
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IGrpcChannelClient channelClient;
        private readonly IWalletClient _walletClient;
        private readonly IOptions<TronNetOptions> _options;
        private readonly IConfiguration _configuration;
        private readonly DataUtility _du;
        public WeatherForecastController(ITronClient tronClient, IOptions<TronNetOptions> options, ILogger<WeatherForecastController> logger, IGrpcChannelClient channelClient, IWalletClient walletClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _du = new DataUtility((string)configuration["ConnectionString"]);
            _logger = logger;
            _walletClient = walletClient;
            _options = options;
            _tronClient = tronClient;
            var channel = channelClient.GetProtocol();

            //GetTransaction("11091c08336ed747ae03c26175d32c90e803ca53a2d74994258d4a135048313c");
            //latestblock();
        }

        [HttpGet("GetWeatherForecast", Name = "GetWeatherForecast")]
        public object Get()
        {
            var wallet = _walletClient.GetProtocol();
            var res = Task.Run(async () => await wallet.GetNowBlock2Async(new EmptyMessage())).Result;
            return res.BlockHeader.RawData.Number;
        }

        [HttpGet("getPrice", Name = "getPrice")]
        public object getPrice()
        {
            try
            {
                var wallet = _walletClient.GetProtocol();
                var soliditywallet = _walletClient.GetSolidityProtocol();
              //  var addressBytes = _walletClient.ParseAddress(add);
                var pglist = new PaginatedMessage()
                {
                    Limit = 100
                };
                var paginatedIssue = Task.Run(async () => await soliditywallet.GetPaginatedAssetIssueListAsync(pglist)).Result;


                String URI = "https://txhapi.tixcash.org/api/account/getprice";

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (var client = new WebClient())
                {
                    client.BaseAddress = "https://txhapi.tixcash.org";
                    client.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                    client.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                    client.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                    client.Headers.Add("User-Agent", "Mozilla/ 5.0(Windows NT 10.0; Win64; x64; rv: 79.0) Gecko/20100101 Firefox/79.0");

                    client.DownloadString("/api/account/getprice/");
                }


                WebClient webClient = new WebClient();
                webClient.Headers.Add("User-Agent", "PostmanRuntime/7.26.1");
                //webClient.Headers.Add("user-agent", " Mozilla/5.0 (Windows NT 6.1; WOW64; rv:25.0) Gecko/20100101 Firefox/25.0");
                var stream = webClient.DownloadData(URI);
                //using (StreamReader sr = new StreamReader(stream))
                //{
                //    //This allows you to do one Read operation.
                //    String request = sr.ReadToEnd();
                //}



                // WebClient wc = new WebClient();

                //   wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                // string HtmlResult = wc.DownloadString("https://txhapi.tixcash.org/api/account/getprice").Result;



            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    var response = ex.Response;
                    var dataStream = response.GetResponseStream();
                    var reader = new StreamReader(dataStream);
                    var details = reader.ReadToEnd();
                }
            }
            return "";
            //var res = Task.Run(async () => await wallet.GetNowBlock2Async(new EmptyMessage())).Result;
        }

        [HttpGet("GetAddressDetails", Name = "GetAddressDetails")]
        public object GetAddressDetails(string add)
        {
            try
            {
                //add = "TBuheiAgdwg4JCs1Gq782ury9gnuBbo3NQ";
                //add = "TRTHynspM7nUso8Jc3sXMKuach3Z9g51t3";
                //add = "THqRmGMeXURWd695psCk9KPHqdAZMEcdto";

                //add = "TLqX9aduAHXEVeAxdAgdZkHVWfUUXvja6d";
                var wallet = _walletClient.GetProtocol();
                var soliditywallet = _walletClient.GetSolidityProtocol();
                var addressBytes = _walletClient.ParseAddress(add);

                var contract = Task.Run(async () => await wallet.GetContractInfoAsync(new BytesMessage
                {
                    Value = addressBytes,
                }, headers: _walletClient.GetHeaders())).Result;

                //var pglist = new PaginatedMessage()
                //{
                //    Limit = 100
                //};
                //var paginatedIssue = Task.Run(async () => await soliditywallet.GetPaginatedAssetIssueListAsync(pglist)).Result;


                //account
                var GetAccount = Task.Run(async () => await wallet.GetAccountAsync(new Account() { Address = addressBytes })).Result;
                var accct = new AccountIdentifier() { Address = addressBytes };
                //var accBalRequest = new AccountBalanceRequest();
                var GetAccountBalanceAsync = Task.Run(async () => await wallet.GetAccountBalanceAsync(new AccountBalanceRequest() { AccountIdentifier = accct, }));
                var GetAccountNetAsync = Task.Run(async () => await wallet.GetAccountNetAsync(new Account() { Address = addressBytes }));

                //var GetAssetIssueListAsync = Task.Run(async () => await wallet.GetAssetIssueListAsync(new EmptyMessage()));

                ////token
                var GetAssetIssueByAccountAsync = Task.Run(async () => await wallet.GetAssetIssueByAccountAsync(new Account() { Address = addressBytes }));
                //var GetAssetIssueListByName = Task.Run(async () => await wallet.GetAssetIssueByNameAsync(new BytesMessage() { Value = ByteString.CopyFrom(Encoding.ASCII.GetBytes("ABC")) })).Result;

                ////token2
                //var GetAssetIssueListByNameAsync2 = Task.Run(async () => await soliditywallet.GetAssetIssueListByNameAsync(new BytesMessage() { Value = ByteString.CopyFrom(Encoding.ASCII.GetBytes("ABC")) }));
                //var GetAssetIssueListByName2 = Task.Run(async () => await soliditywallet.GetAssetIssueByNameAsync(new BytesMessage() { Value = ByteString.CopyFrom(Encoding.ASCII.GetBytes("ABC")) })).Result;

                //var freezeBalance = Task.Run(async () => await wallet.FreezeBalanceAsync(new FreezeBalanceContract() { ReceiverAddress = addressBytes })).Result;
                //var freezeBalance2 = Task.Run(async () => await wallet.FreezeBalance2Async(new FreezeBalanceContract() { ReceiverAddress = addressBytes })).Result;

                //account Resource
                var GetAccountResource = Task.Run(async () => await wallet.GetAccountResourceAsync(new Account() { Address = addressBytes })).Result;


                var json = System.Text.Json.JsonSerializer.Serialize(new { GetAccount = GetAccount, GetAccountResource = GetAccountResource });
                return json;
            }
            catch (Exception ex)
            {
                return new { };

            }

            //var res = Task.Run(async () => await wallet.GetNowBlock2Async(new EmptyMessage())).Result;
        }

        [HttpGet("GetContractList", Name = "GetContractList")]
        public object GetContractList(int page = 1, int verfiedOnly = 0)
        {
            try
            {
                var dt = _du.GetDataTable("EXEC [Explorer_BLOCKS].[dbo].[sp_GetContractsByPageGroup] " + page + ", " + verfiedOnly);
                return JsonConvert.SerializeObject(dt, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return new { };
            }
            //var res = Task.Run(async () => await wallet.GetNowBlock2Async(new EmptyMessage())).Result;
        }

        [HttpGet("GetAccounts", Name = "GetAccounts")]
        public object GetAccounts(int page = 1)
        {
            try
            {
                var dt = _du.GetDataTable("EXEC [Explorer_BLOCKS].[dbo].[sp_GetAccountsByPageGroup] " + page);

                //var wallet = _walletClient.GetProtocol();
                //var soliditywallet = _walletClient.GetSolidityProtocol();

                //dynamic result = JsonConvert.DeserializeObject<object[]>(dt.Rows[0].ItemArray[2].ToString());

                //foreach (dynamic o in result)
                //{
                //    var GetAccount = Task.Run(async () => await wallet.GetAccountAsync(new Account() { Address = _walletClient.ParseAddress(Convert.ToString(o.account)) })).Result;
                //    o.txhBalance = (Convert.ToDecimal(GetAccount.Balance) / 1000000M).ToString("#.##", CultureInfo.InvariantCulture);
                //    o.percentage = (Convert.ToDecimal(o.txhBalance)/100000000000M).ToString("P", CultureInfo.InvariantCulture);

                //}
                //string st = JsonConvert.SerializeObject(result, Formatting.Indented);
                //dt.Rows[0].SetField(2, st);

                //dt.AcceptChanges();

                return JsonConvert.SerializeObject(dt, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return new { };
            }
            //var res = Task.Run(async () => await wallet.GetNowBlock2Async(new EmptyMessage())).Result;
        }

        [HttpGet("GetTokens", Name = "GetTokens")]
        public object GetTokens(int page = 1)
        {
            try
            {
                var dt = _du.GetDataTable("EXEC [Explorer_BLOCKS].[dbo].[sp_GetTokensByPageGroup] " + page);
                return JsonConvert.SerializeObject(dt, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return new { };
            }
            //var res = Task.Run(async () => await wallet.GetNowBlock2Async(new EmptyMessage())).Result;
        }

        [HttpGet("GetContractByAddress", Name = "GetContractByAddress")]
        public object GetContractByAddress(string add)
        {
            try
            {
                if (string.IsNullOrEmpty(add)) throw new Exception("");
                var dt = _du.GetDataTable("SELECT * FROM [Explorer_BLOCKS].[dbo].[Contracts] Where [account] = '" + add + "'; ");
                if (dt.Rows.Count == 0) throw new Exception("");
                return JsonConvert.SerializeObject(dt, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return new { };
            }
            //var res = Task.Run(async () => await wallet.GetNowBlock2Async(new EmptyMessage())).Result;
        }

        [HttpGet("GetContractDetails", Name = "GetContractDetails")]
        public object GetContractDetails(string add)
        {
            try
            {
                //add = "TJ86JLUrMEXYQPNXx1tyD1SzxEgPECFpmj";

                var wallet = _walletClient.GetProtocol();
                var soliditywallet = _walletClient.GetSolidityProtocol();
                var addressBytes = _walletClient.ParseAddress(add);

                var contonract = Task.Run(async () => await wallet.GetContractAsync(new BytesMessage
                {
                    Value = addressBytes,
                }, headers: _walletClient.GetHeaders())).Result;

                var contract = Task.Run(async () => await wallet.GetContractInfoAsync(new BytesMessage
                {
                    Value = addressBytes,
                }, headers: _walletClient.GetHeaders())).Result;

                //account
                var GetAccount = Task.Run(async () => await wallet.GetAccountAsync(new Account() { Address = addressBytes })).Result;
                var accct = new AccountIdentifier() { Address = addressBytes };
                var accBalRequest = new AccountBalanceRequest();
                var GetAccountBalanceAsync = Task.Run(async () => await wallet.GetAccountBalanceAsync(new AccountBalanceRequest() { AccountIdentifier = accct, }));
                var GetAccountNetAsync = Task.Run(async () => await wallet.GetAccountNetAsync(new Account() { Address = addressBytes }));
                var GetAccountResource = Task.Run(async () => await wallet.GetAccountResourceAsync(new Account() { Address = addressBytes })).Result;


                //Asset
                //var GetAssetIssueListAsync = Task.Run(async () => await wallet.GetAssetIssueListAsync(new EmptyMessage()));
                //var GetAssetIssueByAccountAsync = Task.Run(async () => await wallet.GetAssetIssueByAccountAsync(new Account() { Address = addressBytes }));
                //var GetAssetIssueListByName = Task.Run(async () => await wallet.GetAssetIssueByNameAsync(new BytesMessage() { Value = ByteString.CopyFrom(Encoding.ASCII.GetBytes("ABC")) })).Result;
                //var GetAssetIssueListByNameAsync2 = Task.Run(async () => await soliditywallet.GetAssetIssueListByNameAsync(new BytesMessage() { Value = ByteString.CopyFrom(Encoding.ASCII.GetBytes("ABC")) }));
                //var GetAssetIssueListByName2 = Task.Run(async () => await soliditywallet.GetAssetIssueByNameAsync(new BytesMessage() { Value = ByteString.CopyFrom(Encoding.ASCII.GetBytes("ABC")) })).Result;

                var qry = "SELECT [isVerified],[code],[optimization],[runs],[license],[version] FROM  [Explorer_BLOCKS].[dbo].[Contracts]  WHERE account='" + add + "' ";
                var dt = _du.GetDataTable(qry);
                Int32 isverfied = 0;
                bool optimization = false;
                Int32 runs = 0;
                string code = string.Empty;
                string license = string.Empty;
                string version = string.Empty;
                if (dt.Rows.Count > 0)
                {
                    isverfied = Convert.ToInt32(dt.Rows[0].ItemArray[0].ToString());

                    if (Convert.ToInt32(dt.Rows[0].ItemArray[0].ToString()) == 2)
                    {
                        code = dt.Rows[0].ItemArray[1].ToString();
                        optimization = Convert.ToBoolean(dt.Rows[0].ItemArray[2].ToString());
                        runs = Convert.ToInt32(dt.Rows[0].ItemArray[3].ToString());
                        license = dt.Rows[0].ItemArray[4].ToString();
                        version = dt.Rows[0].ItemArray[5].ToString();
                    }
                }


                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    ContractAddress = contract.SmartContract.ContractAddress.FromByteStringToHex().ToBase58Address(),
                    OriginAddress = contract.SmartContract.OriginAddress.FromByteStringToHex().ToBase58Address(),
                    TrxHash = contract.SmartContract.TrxHash.FromByteStringToHex().ToBase58Address(),
                    OriginEnergyLimit = contract.SmartContract.OriginEnergyLimit,
                    Abi = contract.SmartContract.Abi,
                    Name = contract.SmartContract.Name,
                    Bytecode = contract.SmartContract.Bytecode.FromByteStringToHex(),
                    CallValue = contract.SmartContract.CallValue,
                    ConsumeUserResourcePercent = contract.SmartContract.ConsumeUserResourcePercent,
                    AccountName = System.Text.Encoding.UTF8.GetString(GetAccount.AccountName.ToByteArray()),
                    GetAccount = GetAccount,
                    GetAccountResource = GetAccountResource,
                    isverified = isverfied,
                    code = code,
                    optimization = optimization,
                    runs = runs,
                    license = license,
                    version = version
                });
                return json;
            }
            catch (Exception ex)
            {
                return new { };

            }

            //var res = Task.Run(async () => await wallet.GetNowBlock2Async(new EmptyMessage())).Result;
        }

        [HttpGet("ReadContract", Name = "ReadContract")]
        public bool ReadContract()
        {
            try
            {
                var wallet = _walletClient.GetProtocol();
                var soliditywallet = _walletClient.GetSolidityProtocol();
                var addressBytes = _walletClient.ParseAddress("TG3XXyExBkPp9nzdajDZsozEu4BkaSJozs");

                var trc20Decimals = new DecimalsFunction();

                var callEncoder = new FunctionCallEncoder();
                var functionABI = ABITypedRegistry.GetFunctionABI<DecimalsFunction>();

                var contract = Task.Run(async () => await wallet.GetContractInfoAsync(new BytesMessage
                {
                    Value = addressBytes,
                }, headers: _walletClient.GetHeaders())).Result;
                var abi = contract.SmartContract.Abi.Entrys.Select(s => s.Name == "decimals");


                var encodedHex = callEncoder.EncodeRequest(trc20Decimals, functionABI.Sha3Signature);

                //0x313ce567

                var trigger = new TriggerSmartContract
                {
                    ContractAddress = ByteString.CopyFrom(addressBytes.ToByteArray()),
                    Data = ByteString.CopyFrom(encodedHex.HexToByteArray()),
                };

                var txnExt = wallet.TriggerConstantContract(trigger, headers: _walletClient.GetHeaders());




            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReadContract failed");
            }

            return false;
        }


        public FunctionABI ExtractFunctionABI(Contract contract, string methodname)
        {
            //if (FunctionAttribute.IsFunctionType(contractMessageType))
            {
                var functionAttribute = FunctionAttribute.GetAttribute(contractMessageType);
                var functionABI = new FunctionABI(functionAttribute.Name, false);
                functionABI.InputParameters = ExtractParametersFromAttributes(contractMessageType);

                if (functionAttribute.DTOReturnType != null)
                {
                    functionABI.OutputParameters = ExtractParametersFromAttributes(contractMessageType);
                }
                else if (functionAttribute.ReturnType != null)
                {
                    var parameter = new Parameter(functionAttribute.ReturnType);
                    functionABI.OutputParameters = new Parameter[] { parameter };
                }
                return functionABI;
            }
            return null;
        }
        [EnableCors("DataAPI")]
        [AcceptVerbs("POST")]
        [HttpPost("SubmitContractVerify", Name = "SubmitContractVerify")]
        public bool SubmitContractVerify(IFormCollection data)
        {
            try
            {
                var result = new StringBuilder();
                using (var reader = new StreamReader(data.Files[0].OpenReadStream()))
                {
                    while (reader.Peek() >= 0)
                        result.AppendLine(reader.ReadLine());
                }
                var code = result.ToString();

                var copiled = data["compiled"];
                var contracts = data["contracts"].ToString().Split(',');
                var acctNames = string.Empty;
                for (int i = 0; i < contracts.Length; i++)
                {
                    acctNames = string.IsNullOrEmpty(acctNames) ? "'" + contracts[i] + "'" : ",'" + contracts[i] + "'";
                }
                if (code != null)
                {
                    var qry = "UPDATE [Explorer_BLOCKS].[dbo].[Contracts] SET [code]='" + code + "', [version]='" + data["version"] + "', [license]='" + data["license"] + "',[optimization]=" + (data["optimization"] == "true" ? "1" : "0") + ",[runs]=" + data["runs"] + ", [verifiedon]=GETUTCDATE(), [isVerified]=2 WHERE account='" + data["address"] + "' AND [acctName] IN ("+ acctNames + ") ";
                    _du.ExecuteSql(qry);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubmitContractVerify failed");
            }

            return false;
        }

        [HttpGet("HomePageStats", Name = "HomePageStats")]
        public object HomePageStats()
        {
            try
            {
                var wallet = _walletClient.GetProtocol();
                var latestBlockNumber = Task.Run(async () => await wallet.GetNowBlockAsync(new EmptyMessage())).Result.BlockHeader.RawData.Number;

                var ds = _du.GetDataSet("[dbo].[getHomeStats] @latestbk=" + latestBlockNumber);

                if (ds.Tables.Count > 0)
                    return JsonConvert.SerializeObject(ds, Formatting.Indented);

            }
            catch (Exception ex)
            {
            }
            return new { };
            //var res = Task.Run(async () => await wallet.GetNowBlock2Async(new EmptyMessage())).Result;
        }

        [HttpGet("GetContractsCount", Name = "GetContractsCount")]
        public object GetContractsCount()
        {
            try
            {
                //var wallet = _walletClient.GetProtocol();
                //var latestBlockNumber = Task.Run(async () => await wallet.GetNowBlockAsync(new EmptyMessage())).Result.BlockHeader.RawData.Number;

                var ds = _du.GetDataSet("[dbo].[sp_GetContractsCount] ");

                if (ds.Tables.Count > 0)
                    return JsonConvert.SerializeObject(ds, Formatting.Indented);

            }
            catch (Exception ex)
            {
            }
            return new { };
            //var res = Task.Run(async () => await wallet.GetNowBlock2Async(new EmptyMessage())).Result;
        }

        [HttpGet("Search", Name = "Search")]
        public object Search(string key)
        {

            try
            {
                //key = "GCRctCvEse9Y6E6i5DaTjkaSwyKRe6QQP8";
                var wallet = _walletClient.GetProtocol();
                var addressBytes = _walletClient.ParseAddress(key);
                var GetAccount = Task.Run(async () => await wallet.GetAccountAsync(new Account() { Address = addressBytes })).Result;
                if (GetAccount.Address.Count() == 0) throw new Exception("invalid address");
                return new { key = key, type = "Address" };
            }
            catch
            { }

            try
            {
                //key = "7cb737cd268e7806b7bf1196927b7b79e7e0499e3fb58e176c617906dcfc563b";
                var wallet = _walletClient.GetProtocol();
                var txh = Task.Run(async () => await wallet.GetTransactionInfoByIdAsync(new BytesMessage
                {
                    Value = ByteString.CopyFrom(key.HexToByteArray()),
                }, headers: _walletClient.GetHeaders())).Result;
                if (txh.Id.Length == 0) throw new Exception("invalid transaction");

                return new { key = key, type = "Transaction" };
            }
            catch
            { }

            try
            {
                //key = "7cb737cd268e7806b7bf1196927b7b79e7e0499e3fb58e176c617906dcfc563b";
                var wallet = _walletClient.GetProtocol();
                var res1 = Task.Run(async () => await wallet.GetBlockByNum2Async(new NumberMessage() { Num = long.Parse(key) })).Result;

                if (res1.BlockHeader.RawData.Number == 0) throw new Exception("invalid block");

                return new { key = key, type = "Block" };
            }
            catch
            { }

            return new { };

        }

        [HttpGet("GetNodes", Name = "GetNodes")]
        public object GetNodes()
        {
            try
            {
                var wallet = _walletClient.GetProtocol();
                var ListNodes = Task.Run(async () => await wallet.ListNodesAsync(new EmptyMessage())).Result;
                return from s in ListNodes.Nodes.ToList()
                       select new
                       {
                           host = System.Text.Encoding.UTF8.GetString(s.Address.Host.ToByteArray()),
                           port = s.Address.Port

                       };

            }
            catch
            { }
            return new { };
        }

        [HttpGet("GetBlock", Name = "GetBlock")]
        public object GetBlock(Int64 block)
        {
            try
            {
                List<TransactionData> transactionDatas = new List<TransactionData>();
                List<TxnTransferFunction> txnTransferFunctions = new List<TxnTransferFunction>();
                List<string> internalTxnsList = new List<string>();
                //key = "GCRctCvEse9Y6E6i5DaTjkaSwyKRe6QQP8";
                var wallet = _walletClient.GetProtocol();
                var res1 = Task.Run(async () => await wallet.GetBlockByNum2Async(new NumberMessage() { Num = block })).Result;
                foreach (var txn in res1.Transactions)
                {

                    var t = getTxnData(txn.Transaction.RawData.Contract, txn.Result.Result);
                    var txnInfo = Task.Run(async () => await wallet.GetTransactionInfoByIdAsync(new BytesMessage
                    {
                        Value = txn.Txid,
                    }, headers: _walletClient.GetHeaders())).Result;
                    foreach (var it in txnInfo.InternalTransactions)
                    {

                        foreach (var v in it.CallValueInfo)
                        {
                            InternalTransactionData internalTxn = new InternalTransactionData()
                            {

                                blocknumber = res1.BlockHeader.RawData.Number,
                                blocktimestamp = res1.BlockHeader.RawData.Timestamp,
                                hash = it.Hash.ToByteArray().ToSHA256Hash().ToHex(),
                                from = it.CallerAddress.FromByteStringToHex().ToBase58Address(),
                                to = it.TransferToAddress.FromByteStringToHex().ToBase58Address(),
                                amount = v.CallValue,
                                type = "Call",

                                tokenid = v.TokenId,
                                rejected = it.Rejected,

                                methodid = t.methodid,
                                method = t.method,
                                notes = it.Note.FromByteStringToHex()
                            };

                            internalTxnsList.Add(JsonConvert.SerializeObject(internalTxn, Formatting.Indented));


                        }
                    }

                    //t.contractByteArray = txnInfo.ContractAddress;
                    if (txnInfo.ContractAddress.Count() > 0)
                        t.contract = txnInfo.ContractAddress.FromByteStringToHex().ToBase58Address();
                    t.hash = txn.Transaction.GetTxid();
                    t.resultCode = txn.Result.Code.ToString();
                    t.blocknumber = res1.BlockHeader.RawData.Number;
                    t.blocktime = res1.BlockHeader.RawData.Timestamp;
                    t.UTCDate = res1.BlockHeader.RawData.Timestamp.FromUnixTimeStampToUTCDateTime();
                    t.fee = txnInfo.Fee;
                    t.feeLimit = txn.Transaction.RawData.FeeLimit;
                    t.receipt = txnInfo.Receipt;

                    foreach (var item in t.transfers)
                    {
                        item.hash = t.hash;
                        item.blocknumber = t.blocknumber;
                        item.blocktime = t.blocktime;
                        item.result = t.result;
                        item.contract = t.contract;
                        item.UTCDate = t.UTCDate;

                        //assigning reversely
                        t.method = item.method;
                    }


                    transactionDatas.Add(t);
                    txnTransferFunctions.AddRange(t.transfers);
                    t.transfers.Clear();
                }
                //var res = Task.Run(async () => await wallet.GetBlockByNumAsync(new NumberMessage() { Num = block })).Result;
                // if (GetAccount.Address.Count() == 0) throw new Exception("invalid address");
                return new
                {
                    data = new
                    {
                        Number = res1.BlockHeader.RawData.Number,
                        WitnessAddress = res1.BlockHeader.RawData.WitnessAddress.FromByteStringToHex().ToBase58Address(),
                        Timestamp = res1.BlockHeader.RawData.Timestamp,
                        TransactionsCount = res1.Transactions.Count(),
                        Version = res1.BlockHeader.RawData.Version,
                        Blockhash = String.Join<byte>("", res1.Blockid),
                        BlockSize = res1.CalculateSize(),
                        ParentHash = String.Join<byte>("", res1.BlockHeader.RawData.ParentHash),
                        UTCDate = res1.BlockHeader.RawData.Timestamp.FromUnixTimeStampToUTCDateTime(),
                        transactions = transactionDatas,
                        transfers = txnTransferFunctions,
                        internalTxns = internalTxnsList
                    },
                    type = "Block"
                };
            }
            catch
            { }
            return new { };
        }

        [HttpGet("GetTransaction", Name = "GetTransaction")]
        public object GetTransaction(string tx)
        {
            try
            {
                //tx = "3653e0a2e4fc7d8de5be75be5c2f7cf19b60ae4562116d80a2e1a44a0205acf6"
                //tx = 3653e0a2e4fc7d8de5be75be5c2f7cf19b60ae4562116d80a2e1a44a0205acf6
                //tx = "b16852d1fd6ab07fe63e25cd44ef50889580c8299d7e2e917cc2a7a2ad893975";
                // tx = "203753700b3cfb89c1f7f7f64db340424df99c26c8a414900a04e529042b93d8";
                List<TransactionData> transactionDatas = new List<TransactionData>();
                List<TxnTransferFunction> txnTransferFunctions = new List<TxnTransferFunction>();
                List<string> internalTxnsList = new List<string>();
                //key = "GCRctCvEse9Y6E6i5DaTjkaSwyKRe6QQP8";
                var wallet = _walletClient.GetProtocol();
                var txh = Task.Run(async () => await wallet.GetTransactionInfoByIdAsync(new BytesMessage
                {
                    Value = ByteString.CopyFrom(tx.HexToByteArray()),
                }, headers: _walletClient.GetHeaders())).Result;

                if (txh.Id.Length == 0) throw new Exception("invalid transaction");

                var res1 = Task.Run(async () => await wallet.GetBlockByNum2Async(new NumberMessage() { Num = txh.BlockNumber })).Result;
                foreach (var txn in res1.Transactions)
                {
                    if (txh.Id.ToByteArray().ToHex().ToLower() != txn.Transaction.GetTxid().ToLower()) continue;

                    var t = getTxnData(txn.Transaction.RawData.Contract, txn.Result.Result);


                    var txnInfo = Task.Run(async () => await wallet.GetTransactionInfoByIdAsync(new BytesMessage
                    {
                        Value = txn.Txid,
                    }, headers: _walletClient.GetHeaders())).Result;


                    t.internalTxns = new List<InternalTransactionData>();

                    foreach (var it in txnInfo.InternalTransactions)
                    {

                        foreach (var v in it.CallValueInfo)
                        {
                            InternalTransactionData internalTxn = new InternalTransactionData()
                            {

                                blocknumber = res1.BlockHeader.RawData.Number,
                                blocktimestamp = res1.BlockHeader.RawData.Timestamp,
                                hash = it.Hash.ToByteArray().ToSHA256Hash().ToHex(),
                                from = it.CallerAddress.FromByteStringToHex().ToBase58Address(),
                                to = it.TransferToAddress.FromByteStringToHex().ToBase58Address(),
                                amount = v.CallValue,
                                type = "Call",

                                tokenid = v.TokenId,
                                rejected = it.Rejected,
                                methodid = t.methodid,
                                method = t.method,
                                notes = it.Note.FromByteStringToHex()
                            };

                            t.internalTxns.Add(internalTxn);
                            internalTxnsList.Add(JsonConvert.SerializeObject(internalTxn, Formatting.Indented));


                        }
                    }

                    // t.contractByteArray = txnInfo.ContractAddress;
                    if (txnInfo.ContractAddress.Count() > 0)
                    {
                        t.contract = txnInfo.ContractAddress.FromByteStringToHex().ToBase58Address();
                        if (t.methodid == 30) //"CreateSmartContract"
                        {
                            t.newContract.address = t.contract;
                            t.newContract.blocknumber = res1.BlockHeader.RawData.Number;
                            t.newContract.blocktime = res1.BlockHeader.RawData.Timestamp;
                            t.newContract.UTCDate = res1.BlockHeader.RawData.Timestamp.FromUnixTimeStampToUTCDateTime();
                            var q = JsonConvert.SerializeObject(t.newContract, Formatting.Indented);
                        }
                    }

                    t.hash = txn.Transaction.GetTxid();
                    t.resultCode = txn.Result.Code.ToString();
                    t.blocknumber = res1.BlockHeader.RawData.Number;
                    t.blocktime = res1.BlockHeader.RawData.Timestamp;
                    t.UTCDate = res1.BlockHeader.RawData.Timestamp.FromUnixTimeStampToUTCDateTime();
                    t.fee = txnInfo.Fee;
                    t.feeLimit = txn.Transaction.RawData.FeeLimit;
                    t.receipt = txnInfo.Receipt;

                    if (t.transfers != null)
                        foreach (var item in t.transfers)
                        {
                            item.hash = t.hash;
                            item.blocknumber = t.blocknumber;
                            item.blocktime = t.blocktime;
                            item.result = t.result;

                            item.contract = t.contract;
                            item.UTCDate = t.UTCDate;

                            //assigning reversely
                            t.method = item.method;
                        }


                    transactionDatas.Add(t);
                }
                //var res = Task.Run(async () => await wallet.GetBlockByNumAsync(new NumberMessage() { Num = block })).Result;
                // if (GetAccount.Address.Count() == 0) throw new Exception("invalid address");
                return new
                {
                    data = new
                    {
                        transactions = transactionDatas
                    },
                    type = "Transaction"
                };
            }
            catch (Exception ex)
            { return new { ex = ex }; }
            return new { };
        }

        private long GetDecimals(Wallet.WalletClient wallet, byte[] contractAddressBytes)
        {
            var trc20Decimals = new DecimalsFunction();

            var callEncoder = new FunctionCallEncoder();
            var functionABI = ABITypedRegistry.GetFunctionABI<DecimalsFunction>();

            var encodedHex = callEncoder.EncodeRequest(trc20Decimals, functionABI.Sha3Signature);

            var trigger = new TriggerSmartContract
            {
                ContractAddress = ByteString.CopyFrom(contractAddressBytes),
                Data = ByteString.CopyFrom(encodedHex.HexToByteArray()),
            };

            var txnExt = wallet.TriggerConstantContract(trigger, headers: _walletClient.GetHeaders());

            if (txnExt.Result.Result)
            {

                var result = txnExt.ConstantResult[0].ToByteArray().ToHex();

                return new FunctionCallDecoder().DecodeOutput<long>(result, new Parameter("uint8", "d"));
            }
            else return 0;
        }

        private TransactionData getTxnData(RepeatedField<Transaction.Types.Contract> c, bool result)
        {
            if (c.Count() > 1) throw new Exception("multiple c");

            var wallet = _walletClient.GetProtocol();
            //TransactionData txnData = new TransactionData();
            TxnTransferFunction txnTransfer = new TxnTransferFunction();

            BigInteger quotient;
            BigInteger remainder;

            txnTransfer.methodid = (int)c[0].Type;
            txnTransfer.method = c[0].Type.ToString();

            switch (c[0].Type)
            {
                case Transaction.Types.Contract.Types.ContractType.TransferContract:
                    var tc = c[0].Parameter.Unpack<TransferContract>();
                    try
                    {

                        txnTransfer.from = tc.OwnerAddress.FromByteStringToHex().ToBase58Address();
                        txnTransfer.to = tc.ToAddress.FromByteStringToHex().ToBase58Address();
                        txnTransfer.tokenamount = tc.Amount;
                        txnTransfer.tokendecimal = 6;

                        quotient = BigInteger.DivRem(txnTransfer.tokenamount, BigInteger.Pow(10, (int)txnTransfer.tokendecimal), out remainder);
                        txnTransfer.tokenamountInDecimal = decimal.Parse(decimal.Parse(quotient.ToString() + "." + remainder.ToString().PadLeft((int)txnTransfer.tokendecimal, '0')).ToString("G29"));

                    }
                    catch { }
                    return new TransactionData()
                    {
                        amount = txnTransfer.tokenamountInDecimal,
                        from = tc.OwnerAddress.FromByteStringToHex().ToBase58Address(),
                        to = tc.ToAddress.FromByteStringToHex().ToBase58Address(),
                        transfers = new List<TxnTransferFunction>() { txnTransfer },
                        methodid = txnTransfer.methodid

                    };

                case Transaction.Types.Contract.Types.ContractType.TriggerSmartContract:
                    var tsc = c[0].Parameter.Unpack<TriggerSmartContract>();

                    if (tsc.ContractAddress.Count() > 0)
                        txnTransfer.tokendecimal = GetDecimals(wallet, tsc.ContractAddress.ToByteArray());

                    try
                    {
                        var decodedData = tsc.Data.Length > 8 ? new FunctionCallDecoder().DecodeFunctionInput<TransferFunction>(tsc.Data.FromByteStringToHex()) : new TransferFunction();

                        if (decodedData.FromAddress != null)
                            txnTransfer.from = ByteString.CopyFrom(decodedData.FromAddress.Replace("0x", "26").HexToByteArray()).FromByteStringToHex().ToBase58Address();
                        if (decodedData.To != null)
                            txnTransfer.to = ByteString.CopyFrom(decodedData.To.Replace("0x", "26").HexToByteArray()).FromByteStringToHex().ToBase58Address();

                        if (string.IsNullOrEmpty(txnTransfer.from))
                        {
                            txnTransfer.from = tsc.OwnerAddress.FromByteStringToHex().ToBase58Address();
                        }

                        if (string.IsNullOrEmpty(txnTransfer.to))
                        {
                            txnTransfer.from = tsc.ContractAddress.FromByteStringToHex().ToBase58Address();
                        }

                        txnTransfer.tokenamount = decodedData.TokenAmount;
                    }
                    catch { }

                    quotient = BigInteger.DivRem(txnTransfer.tokenamount, BigInteger.Pow(10, (int)txnTransfer.tokendecimal), out remainder);
                    txnTransfer.tokenamountInDecimal = decimal.Parse(decimal.Parse(quotient.ToString() + "." + remainder.ToString().PadLeft((int)txnTransfer.tokendecimal, '0')).ToString("G29"));

                    return new TransactionData()
                    {
                        transfers = new List<TxnTransferFunction>() { txnTransfer },
                        from = tsc.OwnerAddress.FromByteStringToHex().ToBase58Address(),
                        to = tsc.ContractAddress.FromByteStringToHex().ToBase58Address(),
                        methodid = txnTransfer.methodid
                    };
                // return //new TransactionData() { tokenid = tsc.TokenId, from = tsc.OwnerAddress.FromByteStringToHex().ToBase58Address(), tokenAddress = ByteString.CopyFrom(decodedData.To == null ? ByteString.Empty.ToByteArray() :decodedData.To.Replace("0x", "26").HexToByteArray()), to = tsc.ContractAddress.FromByteStringToHex().ToBase58Address(), }; //  result ? decodedData.TokenAmount : 0 - will chk how to decode contract method
                case Transaction.Types.Contract.Types.ContractType.UnDelegateResourceContract:
                    var udrc = c[0].Parameter.Unpack<UnDelegateResourceContract>();
                    return new TransactionData() { from = udrc.OwnerAddress.FromByteStringToHex().ToBase58Address(), to = udrc.ReceiverAddress.FromByteStringToHex().ToBase58Address(), amount = udrc.Balance, methodid = txnTransfer.methodid };
                case Transaction.Types.Contract.Types.ContractType.DelegateResourceContract:
                    var drc = c[0].Parameter.Unpack<DelegateResourceContract>();
                    return new TransactionData() { from = drc.OwnerAddress.FromByteStringToHex().ToBase58Address(), to = drc.ReceiverAddress.FromByteStringToHex().ToBase58Address(), amount = drc.Balance, methodid = txnTransfer.methodid };
                case Transaction.Types.Contract.Types.ContractType.CreateSmartContract:
                    var csc = c[0].Parameter.Unpack<CreateSmartContract>();

                    return new TransactionData()
                    {
                        from = csc.OwnerAddress.FromByteStringToHex().ToBase58Address(),
                        to = csc.NewContract.OriginAddress.FromByteStringToHex().ToBase58Address(),
                        amount = csc.CallTokenValue,
                        methodid = txnTransfer.methodid,
                        method = txnTransfer.method,
                        newContract = new Contract()
                        {
                            name = csc.NewContract.Name,
                            originaddress = csc.OwnerAddress.FromByteStringToHex().ToBase58Address()
                        }
                    };
                case Transaction.Types.Contract.Types.ContractType.AccountPermissionUpdateContract:
                    var apuc = c[0].Parameter.Unpack<AccountPermissionUpdateContract>();
                    return new TransactionData() { from = apuc.OwnerAddress.FromByteStringToHex().ToBase58Address(), methodid = txnTransfer.methodid };
                case Transaction.Types.Contract.Types.ContractType.FreezeBalanceV2Contract:
                    var fbv2c = c[0].Parameter.Unpack<FreezeBalanceV2Contract>();
                    return new TransactionData() { from = fbv2c.OwnerAddress.FromByteStringToHex().ToBase58Address(), methodid = txnTransfer.methodid };



                default:
                    return new TransactionData() { methodid = txnTransfer.methodid };
                    //                    throw new Exception("ContractType not defined");

            }

        }

       

        #region Account Auth
        [EnableCors("DataAPI")]
        [HttpPost("Register", Name = "Register")]
        public string Register(IFormCollection data)
        {
            try
            {
                var email = data["email"];
                var password = data["password"];
                var mobile = data["mobile"];
                var dt = _du.GetDataTable("SELECT TOP 1 1 FROM [dbo].[Login] Where ([emailid]='" + email + "' OR [mobile]='" + mobile + "') ");
                if (dt.Rows.Count > 0) return "User Already exists.";

                var qry = " INSERT INTO [dbo].[Login] ([emailid],[mobile],[pass],[active]) VALUES ( '" + email + "','" + mobile + "','" + password + "',1); ";
                return _du.ExecuteSql(qry) > 0 ? "succeeded" : "Registration failed";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed");
                return "Registration failed.";
            }
        }

        [EnableCors("DataAPI")]
        [HttpPost("Login", Name = "Login")]
        public string Login(IFormCollection data)
        {
            try
            {
                var email = data["email"];
                var mobile = data["mobile"];
                var password = data["password"];
                var token = data["token"];

                var dt = _du.GetDataTable("SELECT TOP 1 1 FROM [dbo].[Login] Where ([emailid]='" + email + "' OR [mobile]='" + mobile + "')  AND [pass] = '" + password + "' AND [active]=1 ");

                if (dt.Rows.Count > 0)
                {
                    var qry = " DELETE FROM [LoginNow] Where ([emailid] = '" + email + "' OR [mobile] = '" + mobile + "') ; INSERT INTO [dbo].[LoginNow] ([emailid],[mobile],[token]) SELECT emailid, mobile, '" + token + "' FROM [Login] Where ( [emailid]= '" + email + "' OR [mobile]= '" + mobile + "')  AND [pass] = '" + password + "' AND [active] = 1 ";
                    return (_du.ExecuteSql(qry) > 0) ? "succeeded" : "Login failed.";
                }
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }

            return "Login failed.";
        }

        [HttpGet("AuthRequest", Name = "AuthRequest")]
        public object AuthRequest()
        {
            return true;
        }

        [HttpGet("GetAuthAPIKey", Name = "GetAuthAPIKey")]
        public object GetAuthAPIKey(string email)
        {
            try
            {
                var dt = _du.GetDataTable("[dbo].[sp_GetAPIKEY] @emailid=" + email);

                if (dt.Rows.Count > 0)
                    return JsonConvert.SerializeObject(dt, Formatting.Indented);

            }
            catch (Exception ex)
            { return new { ex = ex }; }
            return new { };
        }
        [EnableCors("DataAPI")]
        [HttpPost("AuthAddApiKey", Name = "AuthAddApiKey")]
        public string AuthAddApiKey(IFormCollection data)
        {
            try
            {
                var email = data["email"];

                if (!string.IsNullOrEmpty(email))
                {
                    var qry = string.Format("IF (EXISTS(SELECT 1 FROM [dbo].[Login] Where [emailid] = '{0}')) BEGIN INSERT INTO [dbo].[Login_APIKey] ([loginId]) VALUES ((SELECT TOP 1 id FROm [dbo].[Login] Where [emailid] = '{0}')) END;", email);
                    return (_du.ExecuteSql(qry) > 0) ? "succeeded" : "failed to generate.";
                }
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }

            return "failed to generate.";
        }

        [EnableCors("DataAPI")]
        [HttpPost("AuthUpdatePass", Name = "AuthUpdatePass")]
        public string AuthUpdatePass(IFormCollection data)
        {
            try
            {
                var email = data["email"];
                var password = data["password"];

                if (!string.IsNullOrEmpty(email))
                {
                    var dt = _du.GetDataTable("SELECT TOP 1 1 FROM [dbo].[Login] Where ([emailid]='" + email + "') ");
                    if (dt.Rows.Count == 0) return "User not exists.";

                    var qry = " UPDATE [dbo].[Login] SET [pass] = '" + password + "', active = 1 Where emailid = '" + email + "'; ";
                    return _du.ExecuteSql(qry) > 0 ? "succeeded" : "failed to update";

                }

            
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }

            return "failed to update.";
        }

        [HttpGet("GetAuthFavAdd", Name = "GetAuthFavAdd")]
        public object GetAuthFavAdd(string email)
        {
            try
            {
                var dt = _du.GetDataTable("[dbo].[sp_GetFavAdd] @emailid=" + email);

                if (dt.Rows.Count > 0)
                    return JsonConvert.SerializeObject(dt, Formatting.Indented);

            }
            catch (Exception ex)
            { return new { ex = ex }; }
            return new { };
        }
        [EnableCors("DataAPI")]
        [HttpPost("AuthAddFavAddress", Name = "AuthAddFavAddress")]
        public string AuthAddFavAddress(IFormCollection data)
        {
            try
            {
                var email = data["email"];
                var favAdd = data["favAdd"];

                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(favAdd))
                {
                    var qry = string.Format("IF(EXISTS(SELECT 1 FROm [dbo].[Login] Where [emailid] = '{0}')) BEGIN INSERT INTO [dbo].[Login_FavAdd] ([loginId] ,[favAddress]) VALUES ((SELECT TOP 1 id FROm [dbo].[Login] Where [emailid] = '{0}'),'{1}') END;", email, favAdd);
                    return (_du.ExecuteSql(qry) > 0) ? "succeeded" : "failed to update.";
                }
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }

            return "failed to update.";
        }
        #endregion

    }

    public class InternalTransactionData
    {
        public string hash { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string type { get; set; }
        public string tokenid { get; set; }
        public decimal amount { get; set; }
        public bool rejected { get; set; }
        public string INOUT { get; set; }
        public Int64 methodid { get; set; }
        public string method { get; set; }
        public string notes { get; set; }
        public long blocknumber { get; set; }
        public long blocktimestamp { get; set; }


    }
    public class Contract
    {
        public Contract()
        {
            token = new Token();
        }
        public string address { get; set; }
        public string originaddress { get; set; }
        public string name { get; set; }
        public string numberofCalls { get; set; }
        public decimal balance { get; set; }
        public string version { get; set; }
        public string license { get; set; }
        public string verifiedOn { get; set; }
        public string setting { get; set; }
        public Int64 blocknumber { get; set; }
        public Int64 blocktime { get; set; }
        public Token token { get; set; }
        public DateTime UTCDate { get; set; }
    }
    public class Token
    {

        public string name { get; set; }
        public string numberofCalls { get; set; }
        public Int64 tkndecimal { get; set; }
        public decimal supply { get; set; }
        public decimal cursupply { get; set; }
        public decimal marketcap { get; set; }
        public decimal curmarketcap { get; set; }
        public string version { get; set; }
        public string license { get; set; }
        public Int64 blocknumber { get; set; }
        public Int64 blocktime { get; set; }

    }
    public class TransactionData
    {
        public TransactionData()
        {

            newContract = new Contract();
            transfers = new List<TxnTransferFunction>();
            internalTxns = new List<InternalTransactionData>();

        }
        public Contract newContract { get; set; }

        public List<InternalTransactionData> internalTxns { get; set; }
        public List<TxnTransferFunction> transfers { get; set; }
        public string hash { get; set; }
        public string from { get; set; }
        public Int64 blocknumber { get; set; }
        public Int64 blocktime { get; set; }
        public Int64 fee { get; set; }
        public Int64 feeLimit { get; set; }
        public ResourceReceipt receipt { get; internal set; }
        public string to { get; set; }
        public string contract { get; set; }

        public decimal amount { get; set; }
        public Int64 methodid { get; set; }
        public string method { get; set; }
        public bool result { get; set; }
        public string resultCode { get; set; }

        public DateTime UTCDate { get; set; }
    }
    public class ContractHead
    {
        public string address { get; set; }
        public string code { get; set; }
        public string license { get; set; }
        public string version { get; set; }
        public string setting { get; set; }
        public string name { get; set; }
        public string runs { get; set; }
        public string compiler { get; set; }
        public bool optimization { get; set; }
        public IFormFile filedata { get; set; }

    }
    public class TxnTransferFunction
    {
        public string hash { get; set; }
        public string token { get; set; }
        public BigInteger tokenamount { get; set; }
        public Int64 tokendecimal { get; set; }
        public decimal tokenamountInDecimal { get; set; }
        public string from { get; set; }
        public Int64 blocknumber { get; set; }
        public Int64 blocktime { get; set; }
        public string to { get; set; }
        public string contract { get; set; }
        public string Type { get; set; }

        public Int64 methodid { get; set; }
        public string method { get; set; }
        public bool result { get; set; }
        public DateTime UTCDate { get; set; }


    }
    public static class DateTimeExtensions
    {
        //public static DateTime FromUnixTimeStampToDateTime(this string unixTimeStamp)
        //{

        //    return TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(long.Parse(unixTimeStamp)).UtcDateTime, TimeZoneInfo.Local);
        //}

        public static DateTime FromUnixTimeStampToUTCDateTime(this long unixTimeStamp)
        {

            return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).UtcDateTime;
        }
    }
}
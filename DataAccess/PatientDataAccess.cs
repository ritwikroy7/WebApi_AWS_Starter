using System;
using System.Collections.Generic;
using WebApi_AWS_Starter.Models;
using Microsoft.Extensions.Logging;
using Amazon.Runtime;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.Internal;
using Amazon.DynamoDBv2.DocumentModel;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebApi_AWS_Starter.DataAccess
{
    public class DataAccessException : Exception
    {
        public  DataAccessException (string ErrorMessage) : base (ErrorMessage) {}

    }    
    public interface IPatientDataAccess
    {
        Task<List<string>> CreatePatientNameCache();
        Task<PatientInfo> GetPatientInfoByNameAndIDAsync(string Name, string ID);
        Task<List<PatientInfo>> GetPatientInfoAsync(string Name);
        Task SetPatientInfoAsync(string patientInfo);
        Task SavePrescriptionAsync(string prescription);
        Task<Prescription> GetPrescriptionAsync(string ID);
        Task<List<Drug>> CreateDrugCache();
        Task<List<_Test>> CreateTestCache();
        Task SaveTestAsync(string test);
        Task SaveDrugAsync(string drug);

    }
    public class PatientDataAccess : IPatientDataAccess
    {
        BasicAWSCredentials _dynamoCredentials = new BasicAWSCredentials(
        accessKey: "",
        secretKey: "");
        private readonly ILogger<PatientDataAccess> _log;
        public PatientDataAccess(ILogger<PatientDataAccess> log)
        {
            _log=log;
        }
        //Check For Size Of Data Returned. Do/While / Paging Not Implemented Yet.
        public async Task<List<string>> CreatePatientNameCache()
        {
            //PatientNameCache _PatientNameCache= new PatientNameCache();
            List<string> _PatientNameCache = null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig();
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;

                using (var dynamoClient = new AmazonDynamoDBClient(_dynamoCredentials, dynamoConfig))
                {
                    var dynamoRequest = new ScanRequest
                    {
                        TableName = "PatientMaster",
                        ExpressionAttributeNames = new Dictionary<string, string>
                        {
                            {"#N", "Name"}
                        },
                        ProjectionExpression = "#N"
                    };

                    var dynamoResponse = dynamoClient.ScanAsync(dynamoRequest,default(CancellationToken));
                    if(dynamoResponse.Result.Items.Count!=0)
                    {
                        _PatientNameCache = new List<string>();
                        foreach(Dictionary<string, AttributeValue> dynamoItem in dynamoResponse.Result.Items)
                        {
                            _PatientNameCache.Add(Convert.ToString(dynamoItem["Name"].S));
                        }
                    }
                    else
                    {
                        throw new DataAccessException("No Record(s) Found In Database"); 
                    }
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Info From Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Info From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Info From Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return _PatientNameCache;
        }
        public async Task<PatientInfo> GetPatientInfoByNameAndIDAsync(string Name, string ID)
        {

            PatientInfo _objPatient = new PatientInfo();
            
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(_dynamoCredentials, dynamoConfig))
                {
                    var dynamoResponse = await dynamoClient.GetItemAsync(new GetItemRequest
                    {
                            TableName="PatientMaster",
                            Key = new Dictionary<string, AttributeValue>
                                    {
                                        { "Name", new AttributeValue { S = Name } },
                                        { "ID", new AttributeValue { S = ID } }
                                    }
                        
                    },default(CancellationToken));

                    if(dynamoResponse.Item.Count!=0)
                    {
                        _objPatient.Name=Convert.ToString(dynamoResponse.Item["Name"].S);
                        _objPatient.ID=Convert.ToString(dynamoResponse.Item["ID"].S);
                        if(dynamoResponse.Item.ContainsKey("Age"))
                        {
                            _objPatient.Age=Convert.ToInt32(dynamoResponse.Item["Age"].N);
                        }
                        if(dynamoResponse.Item.ContainsKey("ContactNumber"))
                        {
                            _objPatient.ContactNumber=Convert.ToString(dynamoResponse.Item["ContactNumber"].S);
                        }
                        if(dynamoResponse.Item.ContainsKey("Parity"))
                        {
                            _objPatient.Parity=Convert.ToString(dynamoResponse.Item["Parity"].S);
                        }
                        if(dynamoResponse.Item.ContainsKey("BloodGroup"))
                        {
                            _objPatient.BloodGroup=Convert.ToString(dynamoResponse.Item["BloodGroup"].S);
                        }
                        if(dynamoResponse.Item.ContainsKey("Title"))
                        {
                            _objPatient.Title=Convert.ToString(dynamoResponse.Item["Title"].S);
                        }
                        if(dynamoResponse.Item.ContainsKey("Email"))
                        {
                            _objPatient.Email=Convert.ToString(dynamoResponse.Item["Email"].S);
                        }       
                    }
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Info From Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Info From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Info From Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return _objPatient;
        }

        public async Task<List<PatientInfo>> GetPatientInfoAsync(string Name)
        {

            List<PatientInfo> _lstPatient = new List<PatientInfo>();
            QueryResponse dynamoResponse=null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(_dynamoCredentials, dynamoConfig))
                {
                    var dynamoRequest = new QueryRequest
                    {
                        TableName = "PatientMaster",
                        KeyConditionExpression = "#N = :v_Name",
                        ExpressionAttributeNames = new Dictionary<string, string>
                        {
                            {"#N", "Name"}
                        },
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                            {":v_Name", new AttributeValue { S =  Name }}}
                    };

                     dynamoResponse = await dynamoClient.QueryAsync(dynamoRequest,default(CancellationToken));

                    if(dynamoResponse.Items.Count!=0)
                    {
                        foreach(Dictionary<string, AttributeValue> dynamoItem in dynamoResponse.Items)
                        {
                            PatientInfo _objPatient = new PatientInfo();
                            _objPatient.Name=Convert.ToString(dynamoItem["Name"].S);
                            _objPatient.ID=Convert.ToString(dynamoItem["ID"].S);
                            
                            if(dynamoItem.ContainsKey("Age"))
                            {
                                _objPatient.Age=Convert.ToInt32(dynamoItem["Age"].N);
                            }
                            if(dynamoItem.ContainsKey("ContactNumber"))
                            {
                                _objPatient.ContactNumber=Convert.ToString(dynamoItem["ContactNumber"].S);
                            }
                            if(dynamoItem.ContainsKey("Parity"))
                            {
                                _objPatient.Parity=Convert.ToString(dynamoItem["Parity"].S);
                            }
                            if(dynamoItem.ContainsKey("BloodGroup"))
                            {
                                _objPatient.BloodGroup=Convert.ToString(dynamoItem["BloodGroup"].S);
                            }
                            if(dynamoItem.ContainsKey("Title"))
                            {
                                _objPatient.Title=Convert.ToString(dynamoItem["Title"].S);
                            }
                            if(dynamoItem.ContainsKey("Email"))
                            {
                                _objPatient.Email=Convert.ToString(dynamoItem["Email"].S);
                            }

                            _lstPatient.Add(_objPatient);
                        }
                    }
                    else
                    {
                        throw new DataAccessException("No Record(s) Found In Database"); 
                    }
                    
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Info From Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Info From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Patient Info From Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return _lstPatient;
        }
        public async Task SetPatientInfoAsync(string patientInfo)
        {
            Document pResponse=null;
            
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(_dynamoCredentials, dynamoConfig))
                {
                    var pTable = Table.LoadTable(dynamoClient,"PatientMaster");
                    var pItem = Document.FromJson(patientInfo);
                    pResponse = await pTable.PutItemAsync(pItem,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Saving Patient Info In Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Saving Patient Info In Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Saving Patient Info In Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }

        public async Task SavePrescriptionAsync(string prescription)
        {
            Document pResponse=null;
            //var PatientJSON = "{\n  \"Age\": 30,\n  \"BloodGroup\": \"A+\",\n  \"Date\": \"05/12/2017\",\n  \"Findings\": {\n    \"ChiefComplaints\": [\n      \"Complaint 1\",\n      \"Complaint 2\"\n    ],\n    \"Examinations\": [\n      {\n        \"Items\": [\n          \"Vulva - XXXXXXX\"\n        ],\n        \"Type\": \"P/S\"\n      },\n      {\n        \"Items\": [\n          \"Swellings - XXXXXXX\",\n          \"Hernias - XXXXXXX\"\n        ],\n        \"Type\": \"P/A\"\n      },\n      {\n        \"Items\": [\n          \"Size\",\n          \"Tumors\"\n        ],\n        \"Type\": \"P/V\"\n      },\n      {\n        \"Items\": [\n          \"Pallor - XXXXXXX\",\n          \"Glands - XXXXXXX\"\n        ],\n        \"Type\": \"Misc\"\n      }\n    ],\n    \"FamilyHistory\": [\n      \"Cervical Cancer - Grandmother\",\n      \"H/O Diabetes\"\n    ],\n    \"MedicalHistory\": [\n      \"Item 1\",\n      \"Item 2\"\n    ],\n    \"PersonalHistory\": [\n      \"Two failed preganancies\",\n      \"LMP: XXXXXXX\"\n    ]\n  },\n  \"FollowUp\": [\n    \"XXXXXXX\",\n    \"YYYYYYY\"\n  ],\n  \"ID\": \"PQRS1234XYZ\",\n  \"Name\": \"Anna Belle\",\n  \"Parity\": \"XXXXXX\",\n  \"PatientResponse\": [\n    \"Item 1\"\n  ],\n  \"Tests\": [\n    {\n      \"Items\": [\n        \"ACL\",\n        \"Haemogram\",\n        \"Uric Acid\"\n      ],\n      \"Type\": \"Blood\"\n    },\n    {\n      \"Items\": [\n        \"Whole Abdomen\",\n        \"TVS\"\n      ],\n      \"Type\": \"USG Pelvis\"\n    },\n    {\n      \"Items\": [\n        \"Chest X-Ray\",\n        \"MRI\"\n      ],\n      \"Type\": \"Misc\"\n    }\n  ],\n  \"Title\": \"Mrs\"\n}";
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(_dynamoCredentials, dynamoConfig))
                {
                    var pTable = Table.LoadTable(dynamoClient,"PrescriptionDetails");
                    var pItem = Document.FromJson(prescription);
                    pResponse = await pTable.PutItemAsync(pItem,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Saving Prescription To Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Saving Prescription To Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Saving Prescription To Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task<Prescription> GetPrescriptionAsync(string ID)
        {
            var _prescription = (Document)null;
            var prescriptionJson = (string)null;
            Prescription prescription = null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(_dynamoCredentials, dynamoConfig))
                {
                    var pTable = Table.LoadTable(dynamoClient,"PrescriptionDetails");
                    _prescription = await pTable.GetItemAsync(ID,default(CancellationToken));
                    prescriptionJson = _prescription.ToJson();
                }
                try
                {
                    prescription=JsonConvert.DeserializeObject<Prescription>(prescriptionJson);
                }
                catch(JsonException jEx)
                {
                    _log.LogError("Json Deserialization Exception: "+jEx.Message);
                    throw new DataAccessException("An Error Occured While Retrieving Prescription From Database");
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Prescription From Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Prescription From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Prescription From Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return prescription;
        }

        public async Task<List<Drug>> CreateDrugCache()
        {
            List<Drug> DrugCache = new List<Drug>();
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(_dynamoCredentials, dynamoConfig))
                {
                    var pTable = Table.LoadTable(dynamoClient,"DrugMaster");
                    ScanFilter defScanFilter = new ScanFilter();
                    Search dynamoSearch = pTable.Scan(defScanFilter);
                    List<Document> documentList = new List<Document>();
                    do
                    {
                        documentList=await dynamoSearch.GetNextSetAsync(default(CancellationToken));
                        foreach(var document in documentList)
                        {
                            Drug _drug = new Drug();
                            try
                            {
                                _drug=JsonConvert.DeserializeObject<Drug>(document.ToJson());
                                DrugCache.Add(_drug);
                            }
                            catch(JsonException jEx)
                            {
                                _log.LogError("Json Deserialization Exception: "+jEx.Message);
                                throw new DataAccessException("An Error Occured While Retrieving Drug List From Database");
                            }
                        }
                    } while(!dynamoSearch.IsDone);
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug List From Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug List From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Drug List From Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return DrugCache;
        }

        public async Task<List<_Test>> CreateTestCache()
        {
            List<_Test> TestCache = new List<_Test>();
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(_dynamoCredentials, dynamoConfig))
                {
                    var pTable = Table.LoadTable(dynamoClient,"TestMaster");
                    ScanFilter defScanFilter = new ScanFilter();
                    Search dynamoSearch = pTable.Scan(defScanFilter);
                    List<Document> documentList = new List<Document>();
                    do
                    {
                        documentList=await dynamoSearch.GetNextSetAsync(default(CancellationToken));
                        foreach(var document in documentList)
                        {
                            _Test _test = new _Test();
                            try
                            {
                                _test=JsonConvert.DeserializeObject<_Test>(document.ToJson());
                                TestCache.Add(_test);
                            }
                            catch(JsonException jEx)
                            {
                                _log.LogError("Json Deserialization Exception: "+jEx.Message);
                                throw new DataAccessException("An Error Occured While Retrieving Test List From Database");
                            }
                        }
                    } while(!dynamoSearch.IsDone);
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test List From Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test List From Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Retrieving Test List From Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return TestCache;
        }
        public async Task SaveDrugAsync(string drug)
        {
            Document pResponse=null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(_dynamoCredentials, dynamoConfig))
                {
                    var pTable = Table.LoadTable(dynamoClient,"DrugMaster");
                    var pItem = Document.FromJson(drug);
                    pResponse = await pTable.PutItemAsync(pItem,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Saving Drug To Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Saving Drug To Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Saving Drug To Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
        public async Task SaveTestAsync(string test)
        {
            Document pResponse=null;
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(_dynamoCredentials, dynamoConfig))
                {
                    var pTable = Table.LoadTable(dynamoClient,"TestMaster");
                    var pItem = Document.FromJson(test);
                    pResponse = await pTable.PutItemAsync(pItem,default(CancellationToken));
                }
            }
            catch (AmazonDynamoDBException dEx)
            {
                _log.LogError("Amazon DynamoDB Exception: "+dEx.Message);
                throw new DataAccessException("An Error Occured While Saving Test To Database"); 
            }
            catch (AmazonServiceException aEx)
            {
                _log.LogError("Amazon Service Exception: "+aEx.Message);
                throw new DataAccessException("An Error Occured While Saving Test To Database");
            }
            catch (AmazonClientException cEx)
            {
                _log.LogError("Amazon Client Exception: "+cEx.Message);
                throw new DataAccessException("An Error Occured While Saving Test To Database");
            }
            catch (Exception eEx)
            {
                _log.LogError("Unhandled Exception:  "+eEx.Message);
                throw new DataAccessException("An Unknown Error Occured");
            }
            return;
        }
    }
}
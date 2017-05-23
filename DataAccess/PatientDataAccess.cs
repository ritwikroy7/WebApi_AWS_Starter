using System;
using System.Collections.Generic;
using WebApi_AWS_Starter.Models;
using Microsoft.Extensions.Logging;
using Amazon.Runtime;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.Internal;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi_AWS_Starter.DataAccess
{
    public class DataAccessException : Exception
    {
        public  DataAccessException (string ErrorMessage) : base (ErrorMessage) {}

    }    
    public interface IPatientDataAccess
    {
        Task<PatientNameCache> CreatePatientNameCache();
        Task<PatientDetails> GetPatientAsync(string Name, string ID);
        Task<List<PatientInfo>> GetPatientInfoAsync(string Name);
        string SetPatient();

    }
    public class PatientDataAccess : IPatientDataAccess
    {
        
        private readonly ILogger<PatientDataAccess> _log;
        public PatientDataAccess(ILogger<PatientDataAccess> log)
        {
            _log=log;
        }
        public async Task<PatientNameCache> CreatePatientNameCache()
        {
            PatientNameCache _PatientNameCache= new PatientNameCache();
            
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;

                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
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
                        _PatientNameCache.PatientNames= new List<string>();
                        foreach(Dictionary<string, AttributeValue> dynamoItem in dynamoResponse.Result.Items)
                        {
                            _PatientNameCache.PatientNames.Add(Convert.ToString(dynamoItem["Name"].S));
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
        public async Task<PatientDetails> GetPatientAsync(string Name, string ID)
        {

            PatientDetails _objPatient = new PatientDetails();
            
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
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
                        _objPatient.Age=Convert.ToInt32(dynamoResponse.Item["Age"].N);
                        _objPatient.ContactNumber=Convert.ToString(dynamoResponse.Item["ContactNumber"].S);
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
            
            try
            {
                var dynamoConfig = new AmazonDynamoDBConfig(); 
                dynamoConfig.RegionEndpoint=Amazon.RegionEndpoint.USWest2;
                using (var dynamoClient = new AmazonDynamoDBClient(dynamoConfig))
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

                    var dynamoResponse = dynamoClient.QueryAsync(dynamoRequest,default(CancellationToken));

                    if(dynamoResponse.Result.Items.Count!=0)
                    {
                        foreach(Dictionary<string, AttributeValue> dynamoItem in dynamoResponse.Result.Items)
                        {
                            PatientInfo _objPatient = new PatientInfo();
                            _objPatient.Name=Convert.ToString(dynamoItem["Name"].S);
                            _objPatient.ID=Convert.ToString(dynamoItem["ID"].S);
                            _objPatient.Age=Convert.ToInt32(dynamoItem["Age"].N);
                            _objPatient.ContactNumber=Convert.ToString(dynamoItem["ContactNumber"].S);
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
        public string SetPatient()
        {
            string PatientID=null;

            try
            {

            }
            catch (AmazonDynamoDBException dEx)
            {
                
            }
            catch (AmazonServiceException aEx)
            {

            }
            catch (AmazonClientException cEx)
            {

            }
            catch (Exception eEx)
            {
                
            }

            return PatientID;
        }
    }
}
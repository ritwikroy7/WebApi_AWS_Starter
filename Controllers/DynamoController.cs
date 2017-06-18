using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebApi_AWS_Starter.DataAccess;
using WebApi_AWS_Starter.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApi_AWS_Starter.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("DynamoPolicy")]
    public class DynamoController : Controller
    {
            private readonly ILogger<DynamoController> _log;
            private readonly IPatientDataAccess _patientDataAccess;
            private IDistributedCache _distributedCache;
            public DynamoController(ILogger<DynamoController> log, IPatientDataAccess patientDataAccess, IDistributedCache distributedCache)
            {
                _log=log;
                _patientDataAccess=patientDataAccess;
                _distributedCache=distributedCache;
            }

            // GET api/dynamo/ByNameAndID
            /// <summary>
            /// This method returns the patient record for the supplied Name and ID
            /// </summary>
            /// <param name="Name">Patient Name</param>
            /// <param name="ID">Unique Patient ID</param>
            /// <returns>A Single Patient Record</returns>
            //[Authorize]
            [HttpGet("ByNameAndID")]
            [Produces(typeof(PatientInfo))]
            public async Task<IActionResult> GetPatientInfoAsync(string Name, string ID)
            {
                var _patient=(PatientInfo)null;
                try
                {
                    if (String.IsNullOrWhiteSpace(Name) || String.IsNullOrWhiteSpace(ID))
                    {
                        return BadRequest("Name And ID Cannot Be Blank. Please Supply Both Values.");
                    }                    
                    _patient=await _patientDataAccess.GetPatientInfoByNameAndIDAsync(Name, ID);
                }
                catch (DataAccessException DAX)
                {
                    return NotFound(DAX.Message);
                }
                catch (Exception EX)
                {
                    return NotFound(EX.Message);
                }
                return Ok(_patient);
            }

            // GET api/dynamo/ByName
            /// <summary>
            /// This method returns the patient record(s) for the supplied Name
            /// </summary>
            /// <param name="Name">Patient Name</param>
            /// <returns>Patient Record(s)</returns>
            //[Authorize]
            [HttpGet("ByName")]
            [Produces(typeof(List<PatientInfo>))]
            public async Task<IActionResult> GetPatientInfoAsync(string Name)
            {
                var _lstPatientInfo=(List<PatientInfo>)null;
                try
                {
                    if (String.IsNullOrWhiteSpace(Name))
                    {
                        return BadRequest("Name Cannot Be Blank.");
                    }                    
                    _lstPatientInfo=await _patientDataAccess.GetPatientInfoAsync(Name);
                }
                catch (DataAccessException DAX)
                {
                    return NotFound(DAX.Message);
                }
                catch (Exception EX)
                {
                    return NotFound(EX.Message);
                }
                return Ok(_lstPatientInfo);
            }

            // GET api/dynamo
            /// <summary>
            /// This method returns a JSON array having all patient names
            /// </summary>
            /// <returns>Names of all Patients</returns>
            //[Authorize]
            [HttpGet]
            [Produces(typeof(List<string>))]
            public async Task<IActionResult> GetPatientNamesAsync()
            {
                var _patientNames= (List<string>)null;
                var _cacheKey = "PatientCache";
                var _patientCache=_distributedCache.GetString(_cacheKey);
                if (!string.IsNullOrEmpty(Convert.ToString(_patientCache)))
                {
                    return Ok(_patientCache);
                }
                else
                {
                    try
                    {
                        _patientNames = await _patientDataAccess.CreatePatientNameCache();
                        var _cacheEntryOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                        _distributedCache.SetString(_cacheKey, JsonConvert.SerializeObject(_patientNames),_cacheEntryOptions);
                    }
                    catch (DataAccessException DAX)
                    {
                        return NotFound(DAX.Message);
                    }
                    catch (Exception EX)
                    {
                        return NotFound(EX.Message);
                    }
                    return Ok(_patientNames);
                }
            }

            // POST api/dynamo/SavePrescription
            /// <summary>
            /// This method saves a Prescription to the database
            /// </summary>
            /// <returns>No Content</returns>
            //[Authorize]
            [HttpPost("SavePrescription")]
            public async Task<IActionResult> SavePrescriptionAsync([FromBody]Prescription _prescription)
            {
                string prescription=null;
                try
                {
                    prescription = JsonConvert.SerializeObject(_prescription);
                }
                catch(JsonException jEx)
                {
                    return StatusCode(500, jEx.Message);
                }
                try
                {
                    await _patientDataAccess.SavePrescriptionAsync(prescription);
                }
                catch (DataAccessException DAX)
                {
                    return StatusCode(500, DAX.Message);
                }
                catch (Exception EX)
                {
                    return StatusCode(500, EX.Message);
                }
                return NoContent();
            }

            // GET api/dynamo/GetPrescription/{ID}
            /// <summary>
            /// This method retrieves a Prescription from the database by the Patient ID
            /// </summary>
            /// <returns>A Prescription Object</returns>
            //[Authorize]
            [HttpGet("GetPrescription/{ID}")]
            [Produces(typeof(Prescription))]
            public async Task<IActionResult> GetPrescription(string ID)
            {
                var prescription = (Prescription)null;
                try
                {                  
                    prescription=await _patientDataAccess.GetPrescriptionAsync(ID);
                }
                catch (DataAccessException DAX)
                {
                    return NotFound(DAX.Message);
                }
                catch (Exception EX)
                {
                    return NotFound(EX.Message);
                }
                return Ok(prescription);
            }

            // POST api/dynamo/SetPatientInfo
            /// <summary>
            /// This method saves Patient Info to the database
            /// </summary>
            /// <returns>No Content</returns>
            //[Authorize]
            [HttpPost("SetPatientInfo")]
            public async Task<IActionResult> SetPatientInfoAsync([FromBody]PatientInfo _patientInfo)
            {
                string patientInfo=null;
                try
                {
                    patientInfo = JsonConvert.SerializeObject(_patientInfo);
                }
                catch(JsonException jEx)
                {
                    return StatusCode(500, jEx.Message);
                }
                try
                {
                    await _patientDataAccess.SetPatientInfoAsync(patientInfo);
                }
                catch (DataAccessException DAX)
                {
                    return StatusCode(500, DAX.Message);
                }
                catch (Exception EX)
                {
                    return StatusCode(500, EX.Message);
                }
                return NoContent();
            }
            // GET api/dynamo/GetDrugList
            /// <summary>
            /// This method returns a JSON array having all drug details
            /// </summary>
            /// <returns>List of Drug Details</returns>
            //[Authorize]
            [HttpGet("GetDrugList")]
            [Produces(typeof(List<Drug>))]
            public async Task<IActionResult> GetDrugListAsync()
            {
                var _drugList= (List<Drug>)null;
                var _cacheKey = "DrugCache";
                var _drugCache=_distributedCache.GetString(_cacheKey);
                if (!string.IsNullOrEmpty(Convert.ToString(_drugCache)))
                {
                    return Ok(_drugCache);
                }
                else
                {
                    try
                    {
                        _drugList = await _patientDataAccess.CreateDrugCache();
                        var _cacheEntryOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                        _distributedCache.SetString(_cacheKey, JsonConvert.SerializeObject(_drugList),_cacheEntryOptions);
                    }
                    catch (DataAccessException DAX)
                    {
                        return NotFound(DAX.Message);
                    }
                    catch (Exception EX)
                    {
                        return NotFound(EX.Message);
                    }
                    return Ok(_drugList);
                }
            }
            // GET api/dynamo/GetTestList
            /// <summary>
            /// This method returns a JSON array having all test details
            /// </summary>
            /// <returns>List of Test Details</returns>
            //[Authorize]
            [HttpGet("GetTestList")]
            [Produces(typeof(List<_Test>))]
            public async Task<IActionResult> GetTestListAsync()
            {
                var _testList= (List<_Test>)null;
                var _cacheKey = "TestCache";
                var _testCache=_distributedCache.GetString(_cacheKey);
                if (!string.IsNullOrEmpty(Convert.ToString(_testCache)))
                {
                    return Ok(_testCache);
                }
                else
                {
                    try
                    {
                        _testList = await _patientDataAccess.CreateTestCache();
                        var _cacheEntryOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
                        _distributedCache.SetString(_cacheKey, JsonConvert.SerializeObject(_testList),_cacheEntryOptions);
                    }
                    catch (DataAccessException DAX)
                    {
                        return NotFound(DAX.Message);
                    }
                    catch (Exception EX)
                    {
                        return NotFound(EX.Message);
                    }
                    return Ok(_testList);
                }
            }
            // POST api/dynamo/SaveDrug
            /// <summary>
            /// This method saves a Drug to the database
            /// </summary>
            /// <returns>No Content</returns>
            //[Authorize]
            [HttpPost("SaveDrug")]
            public async Task<IActionResult> SaveDrugAsync([FromBody]Drug _drug)
            {
                string drug=null;
                try
                {
                    drug = JsonConvert.SerializeObject(_drug);
                }
                catch(JsonException jEx)
                {
                    return StatusCode(500, jEx.Message);
                }
                try
                {
                    await _patientDataAccess.SaveDrugAsync(drug);
                }
                catch (DataAccessException DAX)
                {
                    return StatusCode(500, DAX.Message);
                }
                catch (Exception EX)
                {
                    return StatusCode(500, EX.Message);
                }
                return NoContent();
            }
            // POST api/dynamo/SaveTest
            /// <summary>
            /// This method saves a Test to the database
            /// </summary>
            /// <returns>No Content</returns>
            //[Authorize]
            [HttpPost("SaveTest")]
            public async Task<IActionResult> SaveTestAsync([FromBody]_Test _test)
            {
                string test=null;
                try
                {
                    test = JsonConvert.SerializeObject(_test);
                }
                catch(JsonException jEx)
                {
                    return StatusCode(500, jEx.Message);
                }
                try
                {
                    await _patientDataAccess.SaveTestAsync(test);
                }
                catch (DataAccessException DAX)
                {
                    return StatusCode(500, DAX.Message);
                }
                catch (Exception EX)
                {
                    return StatusCode(500, EX.Message);
                }
                return NoContent();
            }
            // DELETE api/dynamo/DeleteCache
            /// <summary>
            /// This method deletes the cache for a given cache key
            /// </summary>
            /// <response code="204">No Content</response>
            //[Authorize]
            [HttpDelete("DeleteCache")]
            [ProducesResponseTypeAttribute(204)]
            public async Task<IActionResult> DeleteCache(string cacheKey)
            {
                if(!String.IsNullOrWhiteSpace(cacheKey))
                {
                    try
                    {
                        _distributedCache.Remove(cacheKey);
                    }
                    catch(Exception eEx)
                    {
                        return StatusCode(500, eEx.Message);
                    }
                    return NoContent();
                }
                return BadRequest("cacheKey Cannot Be Empty");
            }
    }
}
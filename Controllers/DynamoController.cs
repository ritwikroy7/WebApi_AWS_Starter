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

namespace WebApi_AWS_Starter.Controllers
{
    [Route("api/[controller]")]
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
            [Authorize]
            [HttpGet("ByNameAndID")]
            [Produces(typeof(PatientInfo))]
            public async Task<IActionResult> GetPatientAsync(string Name, string ID)
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
            [Authorize]
            [HttpGet("ByName")]
            [Produces(typeof(PatientInfo))]
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
            [Authorize]
            [HttpGet]
            [Produces(typeof(List<string>))]
            public async Task<IActionResult> GetPatientNamesAsync()
            {
                var _patientNames= (List<string>)null;
                var _cacheKey = "PatentCache";
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
            [Authorize]
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
            [Authorize]
            [HttpGet("{ID}")]
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

            // POST api/dynamo/SetPatient
            /// <summary>
            /// This method saves Patient Info to the database
            /// </summary>
            /// <returns>No Content</returns>
            [Authorize]
            [HttpPost("SetPatient")]
            public async Task<IActionResult> SetPatientInfoAsync([FromBody]PatientInfo _patientInfo)
            {
                string prescription=null;
                try
                {
                    prescription = JsonConvert.SerializeObject(_patientInfo);
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
    }
}
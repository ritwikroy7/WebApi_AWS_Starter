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
            [HttpGet("ByNameAndID")]
            [Produces(typeof(PatientDetails))]
            public async Task<IActionResult> GetPatientAsync(string Name, string ID)
            {
                var _patient=(PatientDetails)null;
                try
                {
                    if (String.IsNullOrWhiteSpace(Name) || String.IsNullOrWhiteSpace(ID))
                    {
                        return BadRequest("Name And ID Cannot Be Blank. Please Supply Both Values.");
                    }                    
                    _patient=await _patientDataAccess.GetPatientAsync(Name, ID);
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
    }
}
using System;
using System.Collections.Generic;

namespace WebApi_AWS_Starter.Models
{
    public class PatientDetails : PatientInfo
    {

        public string Title { get; set; }
        public string Email { get; set; }
        public _Investigations Investigations { get; set; }
        public _Recommendations _Recommendations { get; set; }		
    }

    public class PatientInfo
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public int Age { get; set; }
        public string ContactNumber { get; set; }
        public string BloodGroup { get; set; }
    }

    public class PatientNameCache
    {
        public List<string> PatientNames { get; set; }
    }

    public class _Investigations
    {
        public List<string> ChiefComplaints { get; set; }
        public List<string> PersonalHistory { get; set; }
        public List<string> FamilyHistory { get; set; }
        public List<string> Examinations { get; set; }
        public List<string> AdditionalFindings { get; set; }
    }

    public class _Recommendations
    {
        public List<string> Tests { get; set; }
        public List<_Medication> Medications { get; set; }
        public List<string> FollowUp { get; set; } 			
        public List<string> PatientResponse { get; set; }
        public List<string> Misc { get; set; }
    }

    public class Drug
    {
        public string TradeName { get; set; }
        public List<string> Composition { get; set; }
    }

    public class _Medication : Drug
    {
        public string Dosage;
    }
}
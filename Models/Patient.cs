using System;
using System.Collections.Generic;

namespace WebApi_AWS_Starter.Models
{
    public class Prescription
    {
        public string Date { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }
        public int Age { get; set; }
        public string BloodGroup { get; set; }
        public string Parity { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public _Findings Findings { get; set; }
        public List<_Test> Tests { get; set; }
        public List<_Medication> Medications { get; set; }
        public List<string> FollowUp { get; set; } 			
        public List<string> PatientResponse { get; set; }
    }

    public class PatientInfo
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string Title { get; set; }
        public int Age { get; set; }
        public string BloodGroup { get; set; }
        public string Parity { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        
    }

    public class PatientNameCache
    {
        public List<string> PatientNames { get; set; }
    }

    public class _Findings
    {
        public List<string> ChiefComplaints { get; set; }
        public List<string> PersonalHistory { get; set; }
        public List<string> FamilyHistory { get; set; }
        public List<_Examination> Examinations { get; set; }
        public List<string> AdditionalFindings { get; set; }
    }
    public class _Examination
    {
        public string Type { get; set; }
        public List<string> Items { get; set; }
    }
    // public class _Recommendations
    // {
    //     public List<_Test> _Tests { get; set; }
    //     public List<_Medication> Medications { get; set; }
    //     public List<string> FollowUp { get; set; } 			
    //     public List<string> PatientResponse { get; set; }
    //     public List<string> Misc { get; set; }
    // }
    public class _Test
    {
        public string Type { get; set; }
        public List<string> SubTypes { get; set; }
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
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Utilities
{
    public class ConstantVariable
    {
        public const string DAYOFF = "Sunday";

        public const double StartAMWorkingHour = 7;
        public const double EndAMWorkingHour = 11;
        public const double StartPMWorkingHour = 13;
        public const double EndPMWorkingHour = 17;

        public const string PRE_STATUS = "Preoperative";
        public const string POST_STATUS = "Postoperative";
        public const string INTRA_STATUS = "Intraoperative";
        public const string RECOVERY_STATUS = "Recovery";
        public const string FINISHED_STATUS = "Finished";

        public const string EMERGENCY_GROUP = "Emergency";


        public const string CHIEFNURSE = "ChiefNurse";
        public const string TECHNICAL = "Technical";
        public const string SUPPLYSTAFF = "MedicalSupplier";
    }
}

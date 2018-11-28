using System;

namespace Ripp3r
{
    internal class PS3_Drive : ODD
    {
        protected PS3_Drive(string driveletter, string name, string version)
            : base(driveletter)
        {
            DriveName = name;
            FWVersion = version;
        }

        public override sealed string DriveName { get; protected set; }
        public override sealed string FWVersion { get; protected set; }

        public static ODD Parse(string driveletter, string identifier)
        {
            const string ps3 = "SONY    PS-SYSTEM   ";
            if (!identifier.StartsWith(ps3)) return null;

            string id = identifier.Replace(ps3, string.Empty);
            Type driveType = Type.GetType(string.Concat(typeof(PS3_Drive).Namespace, ".PS3_", id), false, true);
            if (driveType == null)
            {
                return new PS3_Drive(driveletter, id, "0000");
            }
            try
            {
                return (ODD) Activator.CreateInstance(driveType, driveletter);
            }
            catch (Exception e)
            {
                Interaction.Instance.ReportMessage("Slap the developer. Error in constructing object: " + e, ReportType.Fail);
                return null;
            }
        }
    }

    #region Drive Classes

    // If something different needs to be done for a specific drive, create a derived class from PS3_Drive, and name it
    // PS3_<id> (e.g. PS3_302R). The constructor MUST accept one string argument.
    // From there, methods can be overriden to support different things, if required. If a drive acts like a (e.g.) 302R,
    // don't create a class, since the default PS3_Drive class can handle it perfectly fine.

    // ReSharper disable UnusedMember.Global

/* Example class
    internal class PS3_302R : PS3_Drive
    {
        public PS3_302R(string driveletter)
            : base(driveletter, "302R", "1234")
        {
        }
    }
*/

    // ReSharper restore UnusedMember.Global

    #endregion
}

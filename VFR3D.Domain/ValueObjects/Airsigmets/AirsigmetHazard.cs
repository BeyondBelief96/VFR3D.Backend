namespace VFR3D.Domain.ValueObjects.Airsigmets
{
    public class AirsigmetHazard
    {
        /// <summary>
        /// The hazard: TURB, ICE, IFR, CONVECTIVE, ASH, MTN OBSCN
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// The severity: LGT, LT-MOD, MOD (AIRMET), MOD-SEV, SEV (SIGMET)
        /// </summary>
        public string? Severity { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class AirportConfiguration : IEntityTypeConfiguration<Airport>
    {
        public void Configure(EntityTypeBuilder<Airport> builder)
        {
            builder.ToTable("airports");

            builder.HasKey(e => e.SiteNo);

            // All required string properties with specific lengths
            builder.Property(e => e.SiteNo).HasColumnType("varchar(40)").IsRequired();
            builder.Property(e => e.SiteNo).HasColumnType("varchar(9)").IsRequired();
            builder.Property(e => e.SiteTypeCode).HasColumnType("varchar(1)").IsRequired();
            builder.Property(e => e.StateCode).HasColumnType("varchar(2)");
            builder.Property(e => e.ArptId).HasColumnType("varchar(4)");
            builder.Property(e => e.City).HasColumnType("varchar(40)");
            builder.Property(e => e.CountryCode).HasColumnType("varchar(2)");
            builder.Property(e => e.RegionCode).HasColumnType("varchar(3)");
            builder.Property(e => e.AdoCode).HasColumnType("varchar(3)");
            builder.Property(e => e.StateName).HasColumnType("varchar(30)");
            builder.Property(e => e.CountyName).HasColumnType("varchar(21)");
            builder.Property(e => e.CountyAssocState).HasColumnType("varchar(2)");
            builder.Property(e => e.ArptName).HasColumnType("varchar(50)");
            builder.Property(e => e.OwnershipTypeCode).HasColumnType("varchar(2)");
            builder.Property(e => e.FacilityUseCode).HasColumnType("varchar(2)");

            // Decimal properties with precision
            builder.Property(e => e.LatDecimal).HasColumnType("decimal(10,8)");
            builder.Property(e => e.LongDecimal).HasColumnType("decimal(11,8)");
            builder.Property(e => e.Elev).HasColumnType("decimal(6,1)");
            builder.Property(e => e.MagVarn).HasColumnType("decimal(2,0)");
            builder.Property(e => e.DistCityToAirport).HasColumnType("decimal(2,0)");

            // Latitude components
            builder.Property(e => e.LatDeg).HasColumnType("int");
            builder.Property(e => e.LatMin).HasColumnType("int");
            builder.Property(e => e.LatSec).HasColumnType("decimal(6,2)");
            builder.Property(e => e.LatHemis).HasColumnType("varchar(1)");

            // Longitude components
            builder.Property(e => e.LongDeg).HasColumnType("int");
            builder.Property(e => e.LongMin).HasColumnType("int");
            builder.Property(e => e.LongSec).HasColumnType("decimal(6,2)");
            builder.Property(e => e.LongHemis).HasColumnType("varchar(1)");

            // Flags and codes
            builder.Property(e => e.SurveyMethodCode).HasColumnType("varchar(1)");
            builder.Property(e => e.ElevMethodCode).HasColumnType("varchar(1)");
            builder.Property(e => e.MagHemis).HasColumnType("varchar(1)");
            builder.Property(e => e.DirectionCode).HasColumnType("varchar(3)");
            builder.Property(e => e.RespArtccId).HasColumnType("varchar(4)");
            builder.Property(e => e.FssOnArptFlag).HasColumnType("varchar(1)");
            builder.Property(e => e.FssId).HasColumnType("varchar(4)");
            builder.Property(e => e.FssName).HasColumnType("varchar(30)");
            builder.Property(e => e.NotamId).HasColumnType("varchar(4)");
            builder.Property(e => e.NotamFlag).HasColumnType("varchar(1)");
            builder.Property(e => e.ActivationDate).HasColumnType("varchar(7)");
            builder.Property(e => e.ArptStatus).HasColumnType("varchar(2)");
            builder.Property(e => e.NaspCode).HasColumnType("varchar(7)");

            // More flags
            builder.Property(e => e.CustomsFlag).HasColumnType("varchar(1)");
            builder.Property(e => e.LndgRightsFlag).HasColumnType("varchar(1)");
            builder.Property(e => e.JointUseFlag).HasColumnType("varchar(1)");
            builder.Property(e => e.MilLndgFlag).HasColumnType("varchar(1)");
            builder.Property(e => e.InspectMethodCode).HasColumnType("varchar(1)");
            builder.Property(e => e.InspectorCode).HasColumnType("varchar(1)");

            // Additional properties
            builder.Property(e => e.FuelTypes).HasColumnType("varchar(40)");
            builder.Property(e => e.AirframeRepairSerCode).HasColumnType("varchar(5)");
            builder.Property(e => e.PwrPlantRepairSer).HasColumnType("varchar(5)");
            builder.Property(e => e.BottledOxyType).HasColumnType("varchar(8)");
            builder.Property(e => e.BulkOxyType).HasColumnType("varchar(8)");
            builder.Property(e => e.LgtSked).HasColumnType("varchar(7)");
            builder.Property(e => e.BcnLgtSked).HasColumnType("varchar(7)");
            builder.Property(e => e.TwrTypeCode).HasColumnType("varchar(12)");
            builder.Property(e => e.SegCircleMkrFlag).HasColumnType("varchar(3)");
            builder.Property(e => e.BcnLensColor).HasColumnType("varchar(3)");
            builder.Property(e => e.LndgFeeFlag).HasColumnType("varchar(1)");
            builder.Property(e => e.MedicalUseFlag).HasColumnType("varchar(1)");

            // Source and service properties
            builder.Property(e => e.ArptPsnSource).HasColumnType("varchar(16)");
            builder.Property(e => e.ArptElevSource).HasColumnType("varchar(16)");
            builder.Property(e => e.ContrFuelAvbl).HasColumnType("varchar(1)");
            builder.Property(e => e.TrnsStrgBuoyFlag).HasColumnType("varchar(1)");
            builder.Property(e => e.TrnsStrgHgrFlag).HasColumnType("varchar(1)");
            builder.Property(e => e.TrnsStrgTieFlag).HasColumnType("varchar(1)");
            builder.Property(e => e.OtherServices).HasColumnType("varchar(110)");
            builder.Property(e => e.WindIndcrFlag).HasColumnType("varchar(3)");
            builder.Property(e => e.IcaoId).HasColumnType("varchar(7)");
            builder.Property(e => e.MinOpNetwork).HasColumnType("varchar(1)");
            builder.Property(e => e.UserFeeFlag).HasColumnType("varchar(26)");
            builder.Property(e => e.Cta).HasColumnType("varchar(4)");

            // Attendance and contact information
            builder.Property(e => e.AttendanceMonth).HasColumnType("varchar(50)");
            builder.Property(e => e.AttendanceDay).HasColumnType("varchar(16)");
            builder.Property(e => e.AttendanceHours).HasColumnType("varchar(40)");
            builder.Property(e => e.ContactTitle).HasColumnType("varchar(10)");
            builder.Property(e => e.ContactName).HasColumnType("varchar(35)");
            builder.Property(e => e.ContactAddress1).HasColumnType("varchar(35)");
            builder.Property(e => e.ContactAddress2).HasColumnType("varchar(35)");
            builder.Property(e => e.ContactCity).HasColumnType("varchar(30)");
            builder.Property(e => e.ContactState).HasColumnType("varchar(2)");
            builder.Property(e => e.ContactZipCode).HasColumnType("varchar(5)");
            builder.Property(e => e.ContactZipPlusFour).HasColumnType("varchar(4)");
            builder.Property(e => e.ContactPhoneNumber).HasColumnType("varchar(16)");

            // Indexes
            builder.HasIndex(e => e.IcaoId);
            builder.HasIndex(e => e.ArptId);
            builder.HasIndex(e => e.StateCode);
            builder.HasIndex(a => new { a.StateCode, a.IcaoId });
            builder.HasIndex(a => new { a.StateCode, a.ArptId });
            builder.HasIndex(e => new { e.LatDecimal, e.LongDecimal });
        }
    }
}
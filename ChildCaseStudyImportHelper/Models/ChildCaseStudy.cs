using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ChildCaseStudyImporter.Models
{
    public class ChildCaseStudy
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public string TempChildID { get; set; }
        public string Gender { get; set; }
        public DateTime DOB { get; set; }
        public int Age { get; set; }
        public string ChildLivesWith { get; set; }
        public string BirthDateAccuracy { get; set; }
        public DateTime CompletionDate { get; set; }
        public int Sisters { get; set; }
        public int Brothers { get; set; }
        public string ChildPhoto { get; set; }
        public string StatusReason { get; set; }
        public string WorkstationID { get; set; }
        public string DateFolderName { get; set; }
        public Status Status { get; set; }
        public CareGiver CareGiver { get; set; }
        public Parent Father { get; set; }
        public Parent Mother { get; set; }
        public Project Project { get; set; }
        //siblings
        public AboutMe AboutMe { get; set; }
        public Housing Housing { get; set; }
        public School School { get; set; }
        public string AdditionalInformation { get; set; }
		public string CCHZipFileName { get; set; }
    }

    public class Status
    {
        [XmlAttribute]
        public bool Uploaded { get; set; }
        [XmlAttribute]
        public bool Valid { get; set; }
        [XmlAttribute]
        public bool Declined { get; set; }
    }

    public class CareGiver
    {
        public string WorksAs { get; set; }
        public string WorksAsDetail { get; set; }
        public string Relationship { get; set; }
        public string RelationshipDetail { get; set; }
        public string ReasonFor { get; set; }
        public string ReasonForDetail { get; set; }
    }

    public class Parent
    {
        public string Gender { get; set; }
        public string WorksAs { get; set; }
        public string WorksAsDetail { get; set; }
    }

    public class Project
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Status { get; set; }
        public string LocationID { get; set; }
    }

    public class AboutMe
    {
        public string FavoriteThingsToDo { get; set; }
        public string WhenIPlayWithMyFriends { get; set; }
        public string HelpsOutBy { get; set; }
        public string AsksGod { get; set; }
        public string WantsToBe { get; set; }
        public string TwoFavoriteThingsAndWhy { get; set; }
        public string AlsoEnjoys { get; set; }
        public string HowTheChildInteracts { get; set; }
        public string PhysicalDevelopment { get; set; }
        public string SpiritualDevelopment { get; set; }
    }

    public class Housing
    {
        public bool BambooWalls { get; set; }
        public bool WoodWalls { get; set; }
        public bool BlockWalls { get; set; }
        public bool MudWalls { get; set; }
        public bool OtherWalls { get; set; }
        public string OtherWallsDescription { get; set; }

        public bool StrawRoof { get; set; }
        public bool WoodRoof { get; set; }
        public bool TileRoof { get; set; }
        public bool TinRoof { get; set; }
        public bool OtherRoof { get; set; }
        public string OtherRoofDescription { get; set; }

        public bool ElectrictyLight { get; set; }
        public bool LampLight { get; set; }
        public bool GeneratorLight { get; set; }
        public bool NoneLight { get; set; }
        public bool OtherLight { get; set; }
        public string OtherLightDescription { get; set; }

        public bool ElectricCooking { get; set; }
        public bool GasCooking { get; set; }
        public bool WoodCooking { get; set; }
        public bool NoneCooking { get; set; }
        public bool OtherCooking { get; set; }
        public string OtherCookingDescription { get; set; }

        public bool WellWater { get; set; }
        public bool CommunityWater { get; set; }
        public bool IndoorWater { get; set; }
        public bool RiverWater { get; set; }
        public bool OtherWater { get; set; }
        public string OtherWaterDescription { get; set; }

        public string AreaDescription { get; set; }
    }

    public class School
    {
        public bool AttendsSchool { get; set; }
        public string ClassLevel { get; set; }
        public string NonAttendenceReason { get; set; }
        public string BestSubject { get; set; }
        public string VocationalOrLifeSkill { get; set; }
    }
}

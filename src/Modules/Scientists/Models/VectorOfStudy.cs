namespace Daab.Modules.Scientists.Models;

[Flags]
public enum VectorOfStudy
{
    None = 0,

    Humanities = 1 << 0,          // Humanitar elmlər
    ExactSciences = 1 << 1,       // Dəqiq elmlər
    TechnicalSciences = 1 << 2,   // Texniki elmlər
    BioMedical = 1 << 3,          // Bioloji və tibb elmləri
    AgricultureAndEarth = 1 << 4,// Kənd təsərrüfatı və yerüstü elmlər
    ArtsAndCulture = 1 << 5      // İncəsənət və mədəniyyət elmləri
}

#!/bin/bash
# Opruimscript voor overbodige views, projecten en bestanden

echo "Bezig met opruimen van overbodige mappen en bestanden..."

rm -rf Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Views
rm -rf Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Areas
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Controllers/AfspraakController.cs
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Controllers/AfspraakTypeController.cs
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Controllers/HomeController.cs
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Controllers/PatientController.cs
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Controllers/PatientPortaalController.cs
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Services/AfspraakStatusUpdaterService.cs
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Services/PatientBoekService.cs
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/Services/IPatientBoekService.cs
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/ClientApp/src/pages/LandingPage/LandingPage.tsx
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/api_dashboard_error.txt
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/db_dump.txt
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/python3
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/ScaffoldingReadMe.txt
rm -f Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/project-5d73a8d1-0e7d-4972-912-b0fb3e8ab217.json
rm -f Project/StartTestApi/StartTestApi/PatientService.cs
rm -rf Project/StartTestApi/StartTestApi/StartTestApi
rm -rf Project/StartTestApi/StartTestApi/BenchmarkSuite1
rm -rf Project/StartTestApi/StartTestApi/.vs
rm -rf CV
rm -rf ml_training/venv

echo "Opschoning voltooid!"

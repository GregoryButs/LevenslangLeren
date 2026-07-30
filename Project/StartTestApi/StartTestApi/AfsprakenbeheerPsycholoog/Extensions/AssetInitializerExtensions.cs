using System;
using System.IO;

namespace AfsprakenbeheerPsycholoog.Extensions
{
    /// <summary>
    /// Extensie voor het klaarzetten van statische afbeeldingen en logo-assets bij het opstarten van de applicatie.
    /// </summary>
    public static class AssetInitializerExtensions
    {
        public static void InitializeAssets()
        {
            try
            {
                // Copy Google credentials file if present to prevent any json deserialization encoding issues
                var credSource = Path.Combine(Directory.GetCurrentDirectory(), "project-5d73a8d1-0e7d-4972-912-b0fb3e8ab217.json");
                var credTarget = Path.Combine(Directory.GetCurrentDirectory(), "google-credentials.json");
                if (File.Exists(credSource))
                {
                    File.Copy(credSource, credTarget, true);
                }

                var sourceDir = "/home/gregory/.gemini/antigravity-ide/brain/3fa21591-4abb-4b85-b4ad-2135411620fa";
                var targetDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (Directory.Exists(sourceDir))
                {
                    Directory.CreateDirectory(targetDir);
                    var bgSource = Path.Combine(sourceDir, "therapy_room_background_1783505687059.png");
                    var bannerSource = Path.Combine(sourceDir, "relax_cup_banner_1783505700097.png");
                    
                    if (File.Exists(bgSource)) File.Copy(bgSource, Path.Combine(targetDir, "therapy_room_background.png"), true);
                    if (File.Exists(bannerSource)) File.Copy(bannerSource, Path.Combine(targetDir, "relax_cup_banner.png"), true);
                    
                    // Copy new psychologist portrait (PNG/WebP)
                    var realPortrait = Path.Combine(sourceDir, "media__1783590742689.png");
                    var clientPublicImages = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "public", "images");
                    Directory.CreateDirectory(clientPublicImages);

                    if (File.Exists(realPortrait)) 
                    {
                        File.Copy(realPortrait, Path.Combine(targetDir, "psychologist_portrait.jpg"), true);
                        File.Copy(realPortrait, Path.Combine(targetDir, "psychologist_portrait.png"), true);
                        File.Copy(realPortrait, Path.Combine(clientPublicImages, "psychologist_portrait.jpg"), true);
                        File.Copy(realPortrait, Path.Combine(clientPublicImages, "psychologist_portrait.png"), true);
                    }

                    // Copy new uploaded practice images (PNG/WebP)
                    var practiceNew1 = Path.Combine(sourceDir, "media__1783590650123.png");
                    var practiceNew2 = Path.Combine(sourceDir, "media__1783590650133.png");
                    var practiceNew3 = Path.Combine(sourceDir, "media__1783590650160.png");

                    clientPublicImages = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "public", "images");
                    Directory.CreateDirectory(clientPublicImages);

                    if (File.Exists(practiceNew1))
                    {
                        File.Copy(practiceNew1, Path.Combine(targetDir, "practice_1.png"), true);
                        File.Copy(practiceNew1, Path.Combine(clientPublicImages, "practice_1.png"), true);
                    }
                    if (File.Exists(practiceNew2))
                    {
                        File.Copy(practiceNew2, Path.Combine(targetDir, "practice_2.png"), true);
                        File.Copy(practiceNew2, Path.Combine(clientPublicImages, "practice_2.png"), true);
                    }
                    if (File.Exists(practiceNew3))
                    {
                        File.Copy(practiceNew3, Path.Combine(targetDir, "practice_3.png"), true);
                        File.Copy(practiceNew3, Path.Combine(clientPublicImages, "practice_3.png"), true);
                    }
 
                    // Copy logo with subtitle (normal logo)
                    var normalLogo = Path.Combine(sourceDir, "media__1783516420949.png");
                    if (File.Exists(normalLogo))
                    {
                        File.Copy(normalLogo, Path.Combine(targetDir, "logo_normal.png"), true);
                        
                        clientPublicImages = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "public", "images");
                        Directory.CreateDirectory(clientPublicImages);
                        File.Copy(normalLogo, Path.Combine(clientPublicImages, "logo_normal.png"), true);
                    }

                    // Copy logo without subtitle (hero welcome logo)
                    var heroLogo = Path.Combine(sourceDir, "media__1783517063003.png");
                    if (!File.Exists(heroLogo)) 
                    {
                        heroLogo = Path.Combine(sourceDir, "media__1783516845121.png"); // Fallback
                    }
                    if (File.Exists(heroLogo))
                    {
                        File.Copy(heroLogo, Path.Combine(targetDir, "logo_hero.png"), true);
                        
                        clientPublicImages = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "public", "images");
                        Directory.CreateDirectory(clientPublicImages);
                        File.Copy(heroLogo, Path.Combine(clientPublicImages, "logo_hero.png"), true);
                    }
                }

                // Copy new dark mode logos uploaded by user in conversation
                var convDir = "/home/gregory/.gemini/antigravity-ide/brain/63fac6ee-d46c-44f6-885e-2e8cf9a70d6a";
                if (Directory.Exists(convDir))
                {
                    var darkFullSource = Path.Combine(convDir, "media__1785331285861.png");
                    var darkCompactSource = Path.Combine(convDir, "media__1785331298858.png");

                    var targetImagesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    var clientPublicImagesDir = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "public", "images");
                    Directory.CreateDirectory(targetImagesDir);
                    Directory.CreateDirectory(clientPublicImagesDir);

                    if (File.Exists(darkFullSource))
                    {
                        File.Copy(darkFullSource, Path.Combine(targetImagesDir, "logo_dark_full.png"), true);
                        File.Copy(darkFullSource, Path.Combine(clientPublicImagesDir, "logo_dark_full.png"), true);
                    }
                    if (File.Exists(darkCompactSource))
                    {
                        File.Copy(darkCompactSource, Path.Combine(targetImagesDir, "logo_dark_compact.png"), true);
                        File.Copy(darkCompactSource, Path.Combine(clientPublicImagesDir, "logo_dark_compact.png"), true);
                    }
                }

                // Copy favicons to ClientApp/public/ for Vite dev server use
                var sourceFaviconIco = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "favicon.ico");
                var sourceFaviconSvg = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "favicon.svg");
                var targetFaviconDir = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "public");
                Directory.CreateDirectory(targetFaviconDir);
                
                if (File.Exists(sourceFaviconIco))
                {
                    File.Copy(sourceFaviconIco, Path.Combine(targetFaviconDir, "favicon.ico"), true);
                }
                if (File.Exists(sourceFaviconSvg))
                {
                    File.Copy(sourceFaviconSvg, Path.Combine(targetFaviconDir, "favicon.svg"), true);
                }

                // Synchronize dark mode logo files between wwwroot/images and ClientApp/public/images
                var wwwrootImagesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                var clientAppImagesDir = Path.Combine(Directory.GetCurrentDirectory(), "ClientApp", "public", "images");
                Directory.CreateDirectory(wwwrootImagesDir);
                Directory.CreateDirectory(clientAppImagesDir);

                string[] logoFiles = new[] { "logo_dark_full.svg", "logo_dark_compact.svg", "logo_dark_full.png", "logo_dark_compact.png" };
                foreach (var logoFile in logoFiles)
                {
                    var wwwrootPath = Path.Combine(wwwrootImagesDir, logoFile);
                    var clientAppPath = Path.Combine(clientAppImagesDir, logoFile);

                    if (File.Exists(clientAppPath) && !File.Exists(wwwrootPath))
                    {
                        File.Copy(clientAppPath, wwwrootPath, true);
                    }
                    else if (File.Exists(wwwrootPath) && !File.Exists(clientAppPath))
                    {
                        File.Copy(wwwrootPath, clientAppPath, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying assets: {ex.Message}");
            }
        }
    }
}

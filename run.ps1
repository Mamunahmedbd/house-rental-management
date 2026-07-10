# Convenience script to rebuild and run the application

# 1. Build the project using MSBuild
Write-Host "Building project..." -ForegroundColor Cyan
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" /t:Build /p:Configuration=Debug /verbosity:minimal

# 2. Run the application if compilation succeeded
if ($LASTEXITCODE -eq 0) {
    Write-Host "Build Succeeded! Starting application..." -ForegroundColor Green
    & "E:\my_projects\housing_rental\bin\Debug\Housing rental.exe"
} else {
    Write-Host "Build failed. Please fix compilation errors above." -ForegroundColor Red
}

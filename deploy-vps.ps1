# SofasisERP VPS Deploy Scripti
# Kullanım: D:\SofasisERP dizininde .\deploy-vps.ps1
#
# Notlar (2026-08-22 ilk deploy'da ogrenildi):
# - SSH portu 22 DEGIL, 22667 (bkz. D:\SofasisHomeAutomation\Brain\Operations\production\ufw-rules.txt)
# - Windows Compress-Archive ile olusturulan zip, bazi klasorlerin execute (x) bitini
#   kaybediyor (drw-r--r-- yerine drwxr-xr-x olmali) -- extract sonrasi chmod sart,
#   yoksa wwwroot/_content altindaki DevExpress JS/CSS dosyalari 404 doner.
# - .env dosyasindaki ConnectionStrings__ConnectionString degeri noktali virgul (;)
#   icerdigi icin duz "source .env" bash'te komut ayirici olarak yorumlanip degeri
#   kesiyor -- deger tirnaklanarak source edilmeli.

$key = "$env:USERPROFILE\.ssh\sofasis_vps"
$vps = "sofasis-admin@178.210.161.162"
$port = 22667
$publishDir = "D:\SofasisERP\src\SofasisERP.Blazor.Server\publish-output"
$zipPath = "D:\SofasisERP\src\SofasisERP.Blazor.Server\publish-output.zip"

Write-Host "1) Release publish..." -ForegroundColor Cyan
dotnet publish "D:\SofasisERP\src\SofasisERP.Blazor.Server" -c Release -o $publishDir --nologo
if ($LASTEXITCODE -ne 0) { Write-Error "Publish basarisiz, durduruluyor."; exit 1 }

Write-Host "2) Zip'leniyor..." -ForegroundColor Cyan
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipPath -Force

Write-Host "3) Servis durduruluyor..." -ForegroundColor Cyan
ssh -i $key -p $port $vps "sudo systemctl stop sofasiserp"

Write-Host "4) Zip VPS'e kopyalaniyor..." -ForegroundColor Cyan
scp -i $key -P $port $zipPath "${vps}:/tmp/publish-output.zip"

Write-Host "5) VPS'te aciliyor + izinler duzeltiliyor..." -ForegroundColor Cyan
ssh -i $key -p $port $vps @'
sudo rm -rf /opt/sofasiserp/*.dll /opt/sofasiserp/*.json /opt/sofasiserp/wwwroot /opt/sofasiserp/*.pdb 2>/dev/null
sudo unzip -oq /tmp/publish-output.zip -d /opt/sofasiserp/
sudo find /opt/sofasiserp -type d -exec chmod 755 {} \;
sudo find /opt/sofasiserp -type f -exec chmod 644 {} \;
sudo chmod 600 /opt/sofasiserp/.env
sudo chown -R sofasis-admin:sofasis-admin /opt/sofasiserp
rm -f /tmp/publish-output.zip
echo TAMAMLANDI
'@

Write-Host "6) Veritabani/rapor kayitlari guncelleniyor..." -ForegroundColor Cyan
ssh -i $key -p $port $vps @'
cd /opt/sofasiserp
set -a
source <(sed -E 's/^([A-Za-z_][A-Za-z0-9_]*)=(.*)$/\1="\2"/' .env)
set +a
dotnet SofasisERP.Blazor.Server.dll --updateDatabase --forceUpdate --silent
'@

Write-Host "7) Servis baslatiliyor..." -ForegroundColor Cyan
ssh -i $key -p $port $vps "sudo systemctl start sofasiserp"

Write-Host "8) Baslamasi bekleniyor (~20sn) ve smoke test..." -ForegroundColor Cyan
Start-Sleep -Seconds 20
try {
    $resp = Invoke-WebRequest -Uri "https://erp.sofasis.com/" -UseBasicParsing
    Write-Host "Smoke test: HTTP $($resp.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Smoke test BASARISIZ: $_" -ForegroundColor Red
}

Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
Remove-Item $publishDir -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Deploy tamamlandi." -ForegroundColor Green

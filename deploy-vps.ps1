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
# NOT (2026-08-22, ikinci deploy'da ogrenildi): bash "source .env" ile noktali virgullu
# ConnectionString degerini kirdigi icin, systemd'nin KENDI EnvironmentFile ayristiricisini
# (servisin de kullandigi, kanitlanmis calisan yontem) systemd-run ile odunc aliyoruz --
# boylece bash quoting sorunu tamamen devre disi kaliyor.
ssh -i $key -p $port $vps "sudo systemd-run --uid=sofasis-admin --gid=sofasis-admin --working-directory=/opt/sofasiserp --property=EnvironmentFile=/opt/sofasiserp/.env --setenv=ASPNETCORE_ENVIRONMENT=Production --wait --pipe --collect --unit=sofasiserp-dbupdate /usr/bin/dotnet /opt/sofasiserp/SofasisERP.Blazor.Server.dll --updateDatabase --forceUpdate --silent"

Write-Host "7) Servis baslatiliyor..." -ForegroundColor Cyan
ssh -i $key -p $port $vps "sudo systemctl start sofasiserp"

Write-Host "8) Baslamasi bekleniyor (~35sn) ve smoke test..." -ForegroundColor Cyan
# NOT (2026-08-22): gercek acilis (DevExpress model derlemesi dahil) ~22-30sn suruyor,
# 20sn'de yapilan ilk smoke test yanlislikla 502 vermisti (servis aslinda saglikliydi).
Start-Sleep -Seconds 35
try {
    $resp = Invoke-WebRequest -Uri "https://erp.sofasis.com/" -UseBasicParsing
    Write-Host "Smoke test: HTTP $($resp.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "Smoke test BASARISIZ: $_" -ForegroundColor Red
}

Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
Remove-Item $publishDir -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "Deploy tamamlandi." -ForegroundColor Green

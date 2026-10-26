# 게임 PC(Windows)에서 최신 빌드를 받아 mods 폴더에 설치합니다.
# 사용법: 이 파일을 우클릭 → "PowerShell에서 실행"
#   또는 PowerShell에 한 줄로:
#   irm https://raw.githubusercontent.com/Bigvirus0326/sts2-mod/main/install.ps1 | iex
$ErrorActionPreference = 'Stop'
$Repo = 'Bigvirus0326/sts2-mod'
$Mod  = 'SpireMod'

function Find-Sts2 {
    $cands = @()
    try {
        $steam = (Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction Stop).SteamPath
        $cands += Join-Path $steam 'steamapps\common\Slay the Spire 2'
        $vdf = Join-Path $steam 'steamapps\libraryfolders.vdf'
        if (Test-Path $vdf) {
            Select-String -Path $vdf -Pattern '"path"\s+"([^"]+)"' | ForEach-Object {
                $cands += Join-Path ($_.Matches[0].Groups[1].Value -replace '\\\\','\') 'steamapps\common\Slay the Spire 2'
            }
        }
    } catch {}
    $cands += 'C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2'
    foreach ($c in $cands) { if (Test-Path (Join-Path $c 'data_sts2_windows_x86_64')) { return $c } }
    return $null
}

$game = Find-Sts2
if (-not $game) { $game = Read-Host '슬더스2 설치 폴더를 찾지 못했어요. 경로를 붙여 넣어 주세요' }
$mods = Join-Path $game "mods\$Mod"

$tmp = Join-Path $env:TEMP "$Mod-latest.zip"
Write-Host "최신 빌드 다운로드 중..."
Invoke-WebRequest "https://github.com/$Repo/releases/download/latest/$Mod.zip" -OutFile $tmp
if (Test-Path $mods) { Remove-Item $mods -Recurse -Force }
Expand-Archive $tmp -DestinationPath (Join-Path $game 'mods') -Force
Remove-Item $tmp

Write-Host "설치 완료: $mods" -ForegroundColor Green
if (-not (Test-Path (Join-Path $game 'mods\BaseLib'))) {
    Write-Host "※ BaseLib 모드가 안 보여요. 스팀 창작마당에서 BaseLib을 구독하거나 GitHub 릴리스에서 받아 mods에 넣어 주세요." -ForegroundColor Yellow
}

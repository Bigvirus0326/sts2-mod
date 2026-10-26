# 게임 PC(Windows)에서 이 저장소의 모드들(최신 빌드)을 받아 mods 폴더에 설치합니다.
# 사용법 (PowerShell에 한 줄):
#   irm https://raw.githubusercontent.com/Bigvirus0326/sts2-mod/main/install.ps1 | iex
# 일부만 설치하려면 먼저:  $env:MODS = 'SpireMod,FireCards'
$ErrorActionPreference = 'Stop'
$Repo = 'Bigvirus0326/sts2-mod'

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
$modsRoot = Join-Path $game 'mods'
New-Item -ItemType Directory -Force $modsRoot | Out-Null

# 설치할 모드 목록: 지정이 없으면 저장소에 빌드된 모든 모드 (릴리스 태그 '<모드>-latest')
if ($env:MODS) {
    $names = $env:MODS -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
} else {
    $rels = Invoke-RestMethod "https://api.github.com/repos/$Repo/releases?per_page=100"
    $names = $rels | Where-Object { $_.tag_name -like '*-latest' } | ForEach-Object { $_.tag_name -replace '-latest$','' }
}
if (-not $names) { throw '설치할 모드를 찾지 못했어요. GitHub Actions 빌드가 끝났는지 확인해 주세요.' }

foreach ($Mod in $names) {
    $tmp = Join-Path $env:TEMP "$Mod-latest.zip"
    Write-Host "[$Mod] 다운로드 중..."
    Invoke-WebRequest "https://github.com/$Repo/releases/download/$Mod-latest/$Mod.zip" -OutFile $tmp
    $dst = Join-Path $modsRoot $Mod
    if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }
    Expand-Archive $tmp -DestinationPath $modsRoot -Force
    Remove-Item $tmp
    Write-Host "[$Mod] 설치 완료: $dst" -ForegroundColor Green
}
if (-not (Test-Path (Join-Path $game 'mods\BaseLib'))) {
    Write-Host "※ BaseLib 모드도 필요해요. 스팀 창작마당에서 구독했다면 무시하세요." -ForegroundColor Yellow
}

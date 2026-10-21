# Рендерить HTML-слайди з _src у PNG 1600x900 (headless Chrome). Використання: .\render.ps1 [маска]
param([string]$Filter = '*')
$chrome = 'C:\Program Files\Google\Chrome\Application\chrome.exe'
$src = $PSScriptRoot
$root = Split-Path $src -Parent
$map = @{
  '00-title'          = '00-first-slide\first-slide.png'
  '01-why'            = '01-why-project\problem-solution-audience.png'
  '01b-personas'      = '01-why-project\personas.png'
  '02-srs'            = '02-srs-and-plan\srs-requirements.png'
  '02b-plan'          = '02-srs-and-plan\weekly-plan.png'
  '03a-uikit'         = '03-ux-research\ui-kit.png'
  '03b-benchmark'     = '03-ux-research\benchmark-newbalance.png'
  '03c-empathy'       = '03-ux-research\empathy-map.png'
  '03d-cjm'           = '03-ux-research\customer-journey-map.png'
  '03e-ia'            = '03-ux-research\information-architecture.png'
  '04a-system'        = '04-architecture\system-architecture-and-deployment.png'
  '04b-classes'       = '04-architecture\class-diagram.png'
  '05-clean'          = '05-clean-architecture\projects-and-references.png'
  '06-patterns'       = '06-patterns\patterns.png'
  '07-er'             = '07-database\er-diagram.png'
  '08-metrics'        = '08-metrics\key-metrics.png'
  '09a-git'           = '09-team-and-scrum\git-branches-and-contribution.png'
  '09b-scrum'         = '09-team-and-scrum\scrum-sprints-and-backlog.png'
  '10-testing'        = '10-testing-and-logging\testing-and-logging.png'
  '11-tools'          = '11-tools-and-workflow\tools-and-workflow.png'
  '12-docs'           = '12-documentation\documentation.png'
  '13-security'       = '13-auth-and-security\auth-and-security.png'
  '14-portfolio'      = '14-portfolio-value\portfolio-value.png'
  '16-mobile'         = '16-live-ui-screenshots\slide-mobile.png'
  '17-catalog'        = '16-live-ui-screenshots\slide-catalog.png'
  '18-flow'           = '16-live-ui-screenshots\slide-flow.png'
  '19-admin'          = '16-live-ui-screenshots\slide-admin.png'
  '20-auth'           = '16-live-ui-screenshots\slide-auth.png'
  'm03-srs-plan'      = '02-srs-and-plan\srs-and-plan.png'
  'm05-arch'          = '04-architecture\architecture-clean-deploy.png'
  'm06-ui'            = '16-live-ui-screenshots\slide-ui-summary.png'
  'm07-quality'       = '10-testing-and-logging\quality-security-metrics.png'
  'm08-links'         = '15-contacts-and-key-idea\links.png'
  '15a-contacts'      = '15-contacts-and-key-idea\contacts-qr.png'
  '15b-keyidea'       = '15-contacts-and-key-idea\key-idea.png'
}
foreach ($name in $map.Keys | Where-Object { $_ -like $Filter } | Sort-Object) {
  $html = Join-Path $src "$name.html"
  if (-not (Test-Path -LiteralPath $html)) { Write-Host "skip $name (no html)"; continue }
  $out = Join-Path $root $map[$name]
  $url = 'file:///' + ($html -replace '\\','/')
  & $chrome --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=1 --window-size=1600,900 --virtual-time-budget=8000 --screenshot=$out $url 2>&1 | Out-Null
  Write-Host ("ok  " + $map[$name])
}

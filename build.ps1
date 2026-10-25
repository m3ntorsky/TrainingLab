# =====================================================================
# TrainingLab - Development Build and Publish on Local CS2 Server
# OS: Windows
# =====================================================================
$ErrorActionPreference = "Stop"

#region HELPER FUNCTIONS
function Write-Step {
    param([string]$Message)

    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Write-Ok {
    param([string]$Message)

    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-ErrorMessage {
    param([string]$Message)

    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Exit-OnFailure {
    param([string]$Message)

    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMessage $Message
        exit $LASTEXITCODE
    }
}
#endregion

#region VARIABLES
$SERVER_ROOT_PATH = "E:\cs2\files"
$SERVER_GAME_PATH = Join-Path $SERVER_ROOT_PATH "game\csgo"

$PLUGIN_ROOT_PATH = $PSScriptRoot
$PLUGIN_ID = "TrainingLab"
$PLUGIN_PROJECT_FILE_PATH = Join-Path $PLUGIN_ROOT_PATH "$PLUGIN_ID.csproj"
$PLUGIN_BUILD_PATH = Join-Path $PLUGIN_ROOT_PATH "build\publish"
$PLUGIN_RELEASE_PATH = Join-Path $PLUGIN_ROOT_PATH "build\release"
$PLUGIN_SERVER_CFG_PATH = Join-Path $PLUGIN_ROOT_PATH "resources\cfg"
$PLUGIN_RELEASE_CFG_PATH = Join-Path $PLUGIN_RELEASE_PATH "cfg\traininglab"
$PLUGIN_RELEASE_DLL_PATH = Join-Path $PLUGIN_RELEASE_PATH "addons\swiftlys2\plugins\$PLUGIN_ID"

# Panorama
$CS2_CLIENT_PATH = "D:\SteamLibrary\steamapps\common\Counter-Strike Global Offensive"

$PANORAMA_ADDON_ID = "traininglab"
$PANORAMA_SOURCE_PATH = Join-Path $PLUGIN_ROOT_PATH "resources\panorama"

$PANORAMA_CONTENT_PATH = Join-Path $CS2_CLIENT_PATH "content\csgo_addons\$PANORAMA_ADDON_ID\panorama"
$PANORAMA_COMPILED_PATH = Join-Path $CS2_CLIENT_PATH "game\csgo_addons\$PANORAMA_ADDON_ID\panorama"
$PANORAMA_CLIENT_PATH = Join-Path $CS2_CLIENT_PATH "game\csgo\panorama"

$PANORAMA_COMPILER_PATH = Join-Path $CS2_CLIENT_PATH "game\bin\win64\resourcecompiler.exe"

#endregion
#region BUILD PLUGIN
function Invoke-PluginBuild {
    Write-Step "Building TrainingLab plugin"

    # Validate environment
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw ".NET SDK not found."
    }

    if (-not (Test-Path $PLUGIN_PROJECT_FILE_PATH)) {
        throw "Plugin project file not found: $PLUGIN_PROJECT_FILE_PATH"
    }

    if (-not (Test-Path $SERVER_GAME_PATH)) {
        throw "CS2 game directory not found: $SERVER_GAME_PATH"
    }

    Write-Ok "Environment OK"

    # Restore and clean
    dotnet restore $PLUGIN_PROJECT_FILE_PATH --verbosity quiet
    Exit-OnFailure "Failed to restore NuGet packages."

    dotnet clean $PLUGIN_PROJECT_FILE_PATH --configuration Release --verbosity quiet
    Exit-OnFailure "Failed to clean the project."

    # Build
    if (Test-Path $PLUGIN_BUILD_PATH) {
        Remove-Item $PLUGIN_BUILD_PATH -Recurse -Force
    }

    dotnet build $PLUGIN_PROJECT_FILE_PATH --configuration Release --output $PLUGIN_BUILD_PATH --verbosity quiet
    Exit-OnFailure "Failed to build the plugin."

    Write-Ok "Plugin built successfully"

    # Prepare release
    if (Test-Path $PLUGIN_RELEASE_PATH) {
        Remove-Item $PLUGIN_RELEASE_PATH -Recurse -Force
    }

    New-Item -ItemType Directory -Path $PLUGIN_RELEASE_DLL_PATH -Force | Out-Null
    New-Item -ItemType Directory -Path $PLUGIN_RELEASE_CFG_PATH -Force | Out-Null

    Copy-Item (Join-Path $PLUGIN_SERVER_CFG_PATH "*") $PLUGIN_RELEASE_CFG_PATH -Recurse -Force
    Copy-Item (Join-Path $PLUGIN_BUILD_PATH "*") $PLUGIN_RELEASE_DLL_PATH -Recurse -Force

    Write-Ok "Plugin release prepared"

    # Deploy to server
    Copy-Item (Join-Path $PLUGIN_RELEASE_PATH "*") $SERVER_GAME_PATH -Recurse -Force

    Write-Ok "Plugin deployed to local CS2 server"
}
#endregion

#region BUILD PANORAMA
function Invoke-PanoramaBuild {
    Write-Step "Building TrainingLab Panorama"

    # Validate environment
    if (-not (Test-Path $PANORAMA_SOURCE_PATH -PathType Container)) {
        throw "Panorama source directory not found: $PANORAMA_SOURCE_PATH"
    }

    if (-not (Test-Path $PANORAMA_COMPILER_PATH -PathType Leaf)) {
        throw "Resource compiler not found: $PANORAMA_COMPILER_PATH"
    }

    $addonRoot = Split-Path $PANORAMA_CONTENT_PATH -Parent

    if (-not (Test-Path $addonRoot -PathType Container)) {
        throw "CS2 addon '$PANORAMA_ADDON_ID' not found. Create it using Workshop Tools first."
    }

    Write-Ok "Panorama environment OK"

    # Helper: copy file preserving relative path
    function Copy-PanoramaFile {
        param(
            [string]$SourceRoot,
            [string]$DestinationRoot,
            [string]$RelativePath
        )

        $source = Join-Path $SourceRoot $RelativePath
        $destination = Join-Path $DestinationRoot $RelativePath

        $directory = Split-Path $destination -Parent

        New-Item -ItemType Directory -Path $directory -Force | Out-Null
        Copy-Item -LiteralPath $source -Destination $destination -Force
    }

    # Helper: generate VTEX wrapper for raster images
    function New-PanoramaVtex {
        param([string]$RelativePath)

        $source = $RelativePath.Replace('\', '/')
        $vtexRelative = $RelativePath -replace '\.(png|tga|webp|jpg|jpeg)$', '_$1.vtex'

        $vtexPath = Join-Path $PANORAMA_CONTENT_PATH $vtexRelative

        $body = @"
<!-- dmx encoding keyvalues2_noids 1 format vtex 1 -->
"CDmeVtex"
{
    "m_inputTextureArray" "element_array"
    [
        "CDmeInputTexture"
        {
            "m_name" "string" "InputTexture0"
            "m_fileName" "string" "panorama/$source"
            "m_colorSpace" "string" "srgb"
            "m_typeString" "string" "2D"
            "m_imageProcessorArray" "element_array"
            [
            ]
        }
    ]
    "m_outputTypeString" "string" "2D"
    "m_outputFormat" "string" "RGBA8888"
    "m_nOutputMinDimension" "int" "0"
    "m_nOutputMaxDimension" "int" "0"
    "m_textureOutputChannelArray" "element_array"
    [
        "CDmeTextureOutputChannel"
        {
            "m_inputTextureArray" "string_array"
            [
                "InputTexture0"
            ]
            "m_srcChannels" "string" "rgba"
            "m_dstChannels" "string" "rgba"
            "m_mipAlgorithm" "CDmeImageProcessor"
            {
                "m_algorithm" "string" ""
                "m_stringArg" "string" ""
                "m_vFloat4Arg" "vector4" "0 0 0 0"
            }
            "m_outputColorSpace" "string" "srgb"
        }
    ]
    "m_vClamp" "vector3" "0 0 0"
    "m_bNoLod" "bool" "0"
}
"@

        $directory = Split-Path $vtexPath -Parent
        New-Item -ItemType Directory -Path $directory -Force | Out-Null

        Set-Content -LiteralPath $vtexPath -Value $body -Encoding UTF8

        return $vtexRelative
    }

    # Normalize SVG (optional)
    $normalizer = Join-Path $PLUGIN_ROOT_PATH "normalize_svg.py"

    if (Test-Path $normalizer) {
        if (Get-Command python -ErrorAction SilentlyContinue) {
            Write-Step "Normalizing SVG"

            & python $normalizer $PANORAMA_SOURCE_PATH

            Exit-OnFailure "Failed to normalize SVG files."
        }
        else {
            Write-Warning "Python not found. Skipping SVG normalization."
        }
    }

    # Sync source files
    Write-Step "Syncing Panorama sources"

    $assetExtensions = @(
        ".xml",
        ".css",
        ".js",
        ".vxml",
        ".vcss",
        ".vjs",
        ".vtex",
        ".svg",
        ".png",
        ".tga",
        ".webp",
        ".jpg",
        ".jpeg"
    )

    $rasterExtensions = @(
        ".png",
        ".tga",
        ".webp",
        ".jpg",
        ".jpeg"
    )

    $toCompile = [System.Collections.Generic.List[string]]::new()

    $files = @(
        Get-ChildItem -LiteralPath $PANORAMA_SOURCE_PATH -Recurse -File |
            Where-Object { $_.Extension -in $assetExtensions }
    )

    if ($files.Count -eq 0) {
        throw "No Panorama source files found."
    }

    foreach ($file in $files) {
        $relative = $file.FullName.Substring(
            $PANORAMA_SOURCE_PATH.Length + 1
        )

        $targetRelative = $relative.Replace('/', '\')

        $targetRelative = $targetRelative `
            -replace '^(layout|layouts)\\', 'layout\custom_game\traininglab\' `
            -replace '^styles\\', 'styles\custom_game\traininglab\' `
            -replace '^images\\', 'images\custom_game\traininglab\'

        Write-Host "[MAP] $relative -> $targetRelative"

        $destination = Join-Path $PANORAMA_CONTENT_PATH $targetRelative

        New-Item -ItemType Directory -Path (
            Split-Path $destination -Parent
        ) -Force | Out-Null

        Copy-Item -LiteralPath $file.FullName `
            -Destination $destination -Force

        if ($file.Extension -in $rasterExtensions) {
            $toCompile.Add((New-PanoramaVtex $targetRelative))
        }
        else {
            $toCompile.Add($targetRelative)
        }
    }

    $toCompile = @($toCompile | Sort-Object -Unique)

    Write-Ok "Synced $($files.Count) source files"

    # Compile resources
    Write-Step "Compiling Panorama resources"

    $compiled = 0
    $failed = 0

    foreach ($relative in $toCompile) {
        $inputPath = Join-Path $PANORAMA_CONTENT_PATH $relative

        $output = @(
            & $PANORAMA_COMPILER_PATH -f -i $inputPath 2>&1
        )

        if ($LASTEXITCODE -ne 0 -or
            ($output -join "`n") -match '([1-9]\d*) failed') {
            $failed++

            Write-ErrorMessage "Compilation failed: $relative"
            $output | ForEach-Object { Write-Host $_ }
        }
        else {
            $compiled++
        }
    }

    Write-Host "Compiled: $compiled | Failed: $failed"

    if ($failed -gt 0) {
        throw "Panorama compilation failed for $failed resource(s)."
    }

    # Copy compiled resources to local CS2 client
    Write-Step "Deploying Panorama to local CS2 client"

    $copied = 0
    $missing = 0

    foreach ($relative in $toCompile) {
        $compiledRelative = $relative `
            -replace '\.(vxml|xml)$', '.vxml_c' `
            -replace '\.(vcss|css)$', '.vcss_c' `
            -replace '\.(vjs|js)$', '.vjs_c' `
            -replace '\.svg$', '.vsvg_c' `
            -replace '\.vtex$', '.vtex_c'

        $source = Join-Path $PANORAMA_COMPILED_PATH $compiledRelative

        if (Test-Path $source -PathType Leaf) {
            Copy-PanoramaFile `
                -SourceRoot $PANORAMA_COMPILED_PATH `
                -DestinationRoot $PANORAMA_CLIENT_PATH `
                -RelativePath $compiledRelative

            $copied++
        }
        else {
            $missing++
            Write-ErrorMessage "Missing compiled file: $compiledRelative"
        }
    }

    Write-Host "Copied: $copied | Missing: $missing"

    if ($missing -gt 0) {
        throw "Panorama deployment failed. $missing compiled file(s) missing."
    }

    Write-Ok "Panorama deployed successfully"
}
#endregion

#region EXECUTION
Invoke-PluginBuild
Invoke-PanoramaBuild
#endregion


# 清理所有 DocFx 缓存
Write-Host "清理 DocFx 缓存..." -ForegroundColor Cyan

# 删除所有缓存目录
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue obj, .cache, api, _site

# 删除源代码目录中的缓存
$sourceDirs = @(
    "Assets\CommonScripts",
    "Assets\Editor",
    "Assets\GameSystem",
    "Assets\Landing\Scripts",
    "Assets\Monoplayer\Scripts",
    "Assets\Multiplayer\Scripts"
)

foreach ($dir in $sourceDirs) {
    $cachePath = Join-Path $dir "obj"
    if (Test-Path $cachePath) {
        Remove-Item -Recurse -Force $cachePath
        Write-Host "已删除源代码缓存: $cachePath" -ForegroundColor Green
    }
}

# 删除临时文件（修复$符号转义）
Remove-Item -Force -ErrorAction SilentlyContinue "~`$docfx.json"

# 使用ASCII字符确保兼容性
Write-Host "缓存清理完成!" -ForegroundColor Green
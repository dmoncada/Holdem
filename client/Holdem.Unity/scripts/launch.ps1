# Requires:
#
# Install-Module UnitySetup -Scope CurrentUser
#
# See: https://github.com/microsoft/unitysetup.powershell

$projectPath = "$(Split-Path -Parent "$PSScriptRoot")"

Start-UnityEditor -Project "$projectPath"

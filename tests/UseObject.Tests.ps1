$ErrorActionPreference = 'Stop'

$moduleName = (Get-Item ([IO.Path]::Combine($PSScriptRoot, '..', 'module', '*.psd1'))).BaseName
$manifestPath = [IO.Path]::Combine($PSScriptRoot, '..', 'output', $moduleName)

Import-Module $manifestPath

class TestDisposable : IDisposable {
    [bool] $Disposed

    [void] Dispose() {
        $this.Disposed = $true
    }
}

Describe 'Use-Object' {
    It 'Disposes an object' {
        Use-Object ($disposable = [TestDisposable]::new()) {
            $disposable.Disposed | Should -Be $false
        }
        $disposable.Disposed | Should -Be $true
    }

    It 'Takes pipeline input' {
        0..10 | Use-Object ($disposable = [TestDisposable]::new()) { $_ } |
            Should -BeExactly (0..10)
        $disposable.Disposed | Should -Be $true
    }
}
